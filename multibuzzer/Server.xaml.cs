using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Plugin.BluetoothLE;
using Plugin.BluetoothLE.Server;
using Xamarin.Forms;
using IGattCharacteristic = Plugin.BluetoothLE.Server.IGattCharacteristic;
using IGattService = Plugin.BluetoothLE.Server.IGattService;


namespace multibuzzer
{
    public partial class Server : ContentPage
    { 
        //services
        private List<Guid> services;
        private IGattService buzzerService;
        
        //game state
        //O = open
        //C = closed
        private string lockStatus = "O";
        private Guid buzzDevice;
        private Dictionary<Guid, string> deviceIdentities;
        private Dictionary<string, List<Guid>> teams;

        //characteristics
        private IGattCharacteristic lockStatusChar;
        private IGattCharacteristic buzzChar;
        private IGattCharacteristic identityChar;
        private IGattCharacteristic teamChar;

        //networking
        private string serverName;
        private IAdapter adapter;
        private IGattServer gattServer;

        public Server(string serverName, List<string> teamNames)
        {
            this.serverName = serverName;
            teams = new Dictionary<string, List<Guid>>();
            foreach(string teamName in teamNames)
            {
                teams[teamName] = new List<Guid>();
            }

            //initialization
            services = new List<Guid>();
            adapter = CrossBleAdapter.Current;
            lockStatus = "C";
            deviceIdentities = new Dictionary<Guid, string>();
            buzzDevice = Guid.Empty;

            adapter.WhenReadyCreateServer().Subscribe(server =>
            {
                Debug.WriteLine("Got a server");
                //when does this fire?
                gattServer = server;
                //create services
                CreateBuzzerService();
                //add characteristics
                CreateLockStatusChar();
                CreateBuzzChar();
                CreateIdentityChar();
                CreateTeamChar();
                StartServer();
            });
   
            InitializeComponent();
            Server_Name.Text = "Server Name: " + serverName;
        }

        private void CreateBuzzerService()
        {
            services.Add(StaticGuids.buzzerServiceGuid);
            buzzerService = gattServer.CreateService(StaticGuids.buzzerServiceGuid, true);
        }

        private void CreateLockStatusChar()
        {
            //create characteristic to inform connected devices about the lock out
            //status of the buzzer unit

            //lockStatusGuid = Guid.NewGuid();
            lockStatusChar = buzzerService.AddCharacteristic(StaticGuids.lockStatusGuid,
                        CharacteristicProperties.Indicate | CharacteristicProperties.Notify,
                        GattPermissions.Read | GattPermissions.Write
            );
           
            lockStatusChar.WhenDeviceSubscriptionChanged().Subscribe(e =>
            {
                Debug.WriteLine("Got subscriptions: " + e.ToString());
                Debug.WriteLine("UUID" + lockStatusChar.Uuid.ToString());
                //TODO factor this out into a method
                lockStatusChar.Broadcast(Encoding.UTF8.GetBytes(lockStatus + buzzDevice.ToString()));
            });
        }

        private void CreateBuzzChar()
        {
            buzzChar = buzzerService.AddCharacteristic(StaticGuids.buzzCharGuid,
                CharacteristicProperties.Write, GattPermissions.Read | GattPermissions.Write);
            buzzChar.WhenWriteReceived().Subscribe(writeRequest =>
            {
                if(lockStatus == "O")
                {
                    buzzDevice = writeRequest.Device.Uuid;
                    string buzzName = deviceIdentities[buzzDevice];
                    Buzz_Player.Text = buzzName;
                    lockStatus = "C";
                    lockStatusChar.Broadcast(Encoding.UTF8.GetBytes(lockStatus + buzzDevice.ToString() + buzzName));
                }               
            });
        }

        private void CreateIdentityChar()
        {
            //create characteristic which allows devices to self report nick names
            identityChar = buzzerService.AddCharacteristic(StaticGuids.identityCharGuid,
                CharacteristicProperties.Write | CharacteristicProperties.Read, GattPermissions.Read | GattPermissions.Write);
            identityChar.WhenWriteReceived().Subscribe(writeRequest =>
            {
                printdebug("identity write request");
                string identityWrite = Encoding.UTF8.GetString(writeRequest.Value);

                if(identityWrite[0] == ':')
                {
                    teams[identityWrite.Substring(1)].Add(writeRequest.Device.Uuid);
                    updateTeams();
                } else
                {
                    Debug.WriteLine("Identity provided: " + identityWrite);
                    deviceIdentities[writeRequest.Device.Uuid] = identityWrite;
                }
            });
            //inform connected devices of their own UUID
            identityChar.WhenReadReceived().Subscribe(read =>
            {
                read.Value = Encoding.UTF8.GetBytes(read.Device.Uuid.ToString());
            });
        }

        private void updateTeams()
        {
            throw new NotImplementedException();
        }

        private void CreateTeamChar()
        {
            teamChar = buzzerService.AddCharacteristic(StaticGuids.teamCharGuid,
                CharacteristicProperties.Indicate | CharacteristicProperties.Notify,
                        GattPermissions.Read | GattPermissions.Write
            );

            //when a new device is added give all team information
            teamChar.WhenDeviceSubscriptionChanged().Subscribe(e =>
            {
                teamChar.Broadcast(teamInfo(), e.Device);
            });
        }

        private void StartServer()
        {
            gattServer.AddService(buzzerService);
            //start advertising the server
            CrossBleAdapter.Current.Advertiser.Start(new AdvertisementData
            {
                LocalName = serverName,
                ServiceUuids = services
            });

        }

        private byte[] teamInfo()
        {
            if(teams.Keys.Count != 0)
            {
                string teamInfoString = "";

                foreach (string teamName in teams.Keys)
                {
                    teamInfoString += teamName;
                    foreach (Guid g in teams[teamName])
                    {
                        teamInfoString += ":" + g.ToString();
                    }
                    teamInfoString += "::";
                }
                return Encoding.UTF8.GetBytes(teamInfoString.Substring(0, teamInfoString.Length - 2));
            }
            else
            {
                return Encoding.UTF8.GetBytes("nt");
            }
        }

        private void Cleared(object sender, System.EventArgs e)
        {
            buzzDevice = Guid.Empty;
            Buzz_Player.Text = "";
            lockStatus = "O" ;
            if(lockStatusChar != null)
            {
                Debug.WriteLine("Cleared");
                lockStatusChar.Broadcast(Encoding.UTF8.GetBytes(lockStatus + buzzDevice.ToString()));
            }
        }

        private void printdebug(string s)
        {
            Label debug = new Label();
            debug.Text = s;
            BleDebug.Children.Add(debug);
        }

        private void updateTeams(string team, Guid guid)
        {
            teamChar.Broadcast(Encoding.UTF8.GetBytes(team + "::" + guid.ToString()));
        }
    }
}
