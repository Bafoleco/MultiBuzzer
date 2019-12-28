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
        private string buzzDevice;
        private Dictionary<Guid, string> deviceIdentities;

        //characteristics
        private IGattCharacteristic lockStatusChar;
        private IGattCharacteristic buzzChar;
        private IGattCharacteristic identityChar;

        //networking
        private string serverName;
        private IAdapter adapter;
        private IGattServer gattServer;

        public Server(string serverName)
        {
            this.serverName = serverName;

            //initialization
            services = new List<Guid>();
            adapter = CrossBleAdapter.Current;
            lockStatus = "C";
            buzzDevice = "";
            deviceIdentities = new Dictionary<Guid, string>();
            adapter.WhenReadyCreateServer().Subscribe(server =>
            {
                Debug.WriteLine("Got a server");
                //when does this fire?
                gattServer = server;
                //create services
                CreateBuzzerService();
                //add characteristics
                CreateBuzzChar();
                CreateIdentityChar();
                CreateLockStatusChar();
                StartServer();
            });
   
            InitializeComponent();
            Server_Name.Text = "Server Name: " + serverName;
        }

        private void CreateBuzzerService()
        {
            //create buzzer service 
            services.Add(StaticGuids.buzzerServiceGuid);
            buzzerService = gattServer.CreateService(StaticGuids.buzzerServiceGuid, true);
            Debug.WriteLine(gattServer.Services.Count);
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
                lockStatusChar.Broadcast(Encoding.UTF8.GetBytes(lockStatus));
            });
        }

        private void CreateBuzzChar()
        {
            buzzChar = buzzerService.AddCharacteristic(StaticGuids.buzzCharGuid,
                CharacteristicProperties.Write, GattPermissions.Read | GattPermissions.Write);
            buzzChar.WhenWriteReceived().Subscribe(writeRequest =>
            {
                printdebug("buzzer write request");

                Debug.WriteLine("Buzzed recieved on write");
                Buzz_Player.Text = deviceIdentities[writeRequest.Device.Uuid];
                Buzzed();
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
                string name = Encoding.UTF8.GetString(writeRequest.Value, 0, writeRequest.Value.Length);
                Debug.WriteLine("Identity provided: " + name);
                deviceIdentities[writeRequest.Device.Uuid] = name;
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

        private void Cleared(object sender, System.EventArgs e)
        {
            buzzDevice = "";
            Buzz_Player.Text = "";
            lockStatus = "O" ;
            if(lockStatusChar != null)
            {
                Debug.WriteLine("Cleared");
                lockStatusChar.Broadcast(Encoding.UTF8.GetBytes(lockStatus));
               
            }
        }

        private void Buzzed()
        {
            lockStatus = "C";
            if (lockStatusChar != null)
            {
                lockStatusChar.Broadcast(Encoding.UTF8.GetBytes(lockStatus));
            }
        }

        private void printdebug(string s)
        {
            Label debug = new Label();
            debug.Text = s;
            BleDebug.Children.Add(debug);
        }
    }
}
