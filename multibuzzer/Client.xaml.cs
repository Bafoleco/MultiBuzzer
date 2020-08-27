using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Plugin.BluetoothLE;
using Plugin.SimpleAudioPlayer;
using Xamarin.Forms;
using IDevice = Plugin.BluetoothLE.IDevice;

namespace multibuzzer
{
    public partial class Client : ContentPage
    {
        //networking components
        private IDevice server;
        private IAdapter adapter;
        private string serverName;

        //TODO improve initialization lock

        //client state
        private string nickname;
        private Guid clientGuid;
        private IDisposable scanner;
        private readonly String[] teamSplitter = { "::" };
        private readonly String[] nameSplitter = { ":" };
        private string myTeam;

        //global state
        private IGattCharacteristic buzzerChar;
        private IGattCharacteristic lockStatusChar;
        private IGattCharacteristic teamChar;
        private IGattCharacteristic identityChar;
        private Dictionary<string, List<Guid>> teams;

        private char lockStatus
        {
            get { return _lockStatus; }
            set
            {
                if (value == 'U')
                {
                    BackgroundColor = Color.Green;
                }
                else if (value == 'C')
                {
                    BackgroundColor = Color.Red;
                }
                else
                {
                    BackgroundColor = Color.White;
                }
                _lockStatus = value;
            }
        }
        private char _lockStatus;
        private bool teamGame;

        public Client(string serverName, string nickname)
        {
            this.serverName = serverName.Trim();
            this.nickname = nickname.Trim();
            teams = new Dictionary<string, List<Guid>>();

            adapter = CrossBleAdapter.Current;
            adapter.WhenReady().Subscribe(_ =>
            {
                scanner = adapter.Scan().Subscribe(scanResult => {
                    ProcessScan(scanResult);
                });
            });

            InitializeComponent();
            Nickname.Text = "Name: " + nickname;
            ClientScreen.Padding = new Thickness(10, 50);

            //set up lock on loading screen
            //Game.IsVisible = false;
            //LoadLabel.Text = "0";
            //LoadLabel.PropertyChanged += (label, e) =>
            //{
            //    if (((Label)label).Text == "4")
            //    {
            //        manageTeamChoice();
            //    }
            //};
        }

        private void ProcessScan(IScanResult scanResult)
        {
            //Debug.WriteLine(scanResult.AdvertisementData.LocalName);
            //printdebug(scanResult.AdvertisementData.LocalName);

            if (scanResult.AdvertisementData.LocalName != null && scanResult.AdvertisementData.LocalName.Trim() == serverName)
            {
                server = scanResult.Device;
                scanner.Dispose();
                ConnectToServer();
            }
        }

        private void ConnectToServer()
        {
            server.Connect();
            Server_Name.Text = "Currently connected to " + server.Name;

            server.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
            {
                //Debug.WriteLine("Char" + characteristic.Uuid);
            });

            //set up identity management
            server.GetKnownCharacteristics(StaticGuids.buzzerServiceGuid, StaticGuids.identityCharGuid).Subscribe(identityChar =>
            {
                Debug.WriteLine("Found Identity char");
                this.identityChar = identityChar;

                //writing nickname allows server to create association b/w uuid and nickname
                identityChar.Write(Encoding.UTF8.GetBytes(nickname)).Subscribe(write =>
                {
                    Debug.WriteLine("Successful identity write");
                }, error =>
                {
                    Debug.WriteLine(error.GetBaseException());
                });

                identityChar.Read().Subscribe(read =>
                {
                    addOne(LoadLabel);
                    clientGuid = Guid.Parse(Encoding.UTF8.GetString(read.Data));
                    Debug.WriteLine("Found own GUID: " + clientGuid.ToString());
                }, error =>
                {
                    Debug.WriteLine(error.ToString());
                });

            });

            //set up lock status
            server.GetKnownCharacteristics(StaticGuids.buzzerServiceGuid, StaticGuids.lockStatusGuid).Subscribe(lockStatusChar =>
            {
                Debug.WriteLine("Found Lock char");
                lockStatusChar.EnableNotifications();
                lockStatusChar.WhenNotificationReceived().Subscribe(result =>
                {
                    Debug.WriteLine("Notification recieved" + Encoding.UTF8.GetString(result.Data));
                    string reply = Encoding.UTF8.GetString(result.Data);
                    char tempLockState = reply[0];
                    Debug.WriteLine(reply.Substring(1, 36));
                    Guid buzzDevice = Guid.Parse(reply.Substring(1, 36));

                    //TODO fix risk that client guid is not yet found
                    if (buzzDevice == clientGuid)
                    {
                        tempLockState = 'U';
                    }

                    lockStatus = tempLockState;
                    addOne(LoadLabel);
                });
            });

            //set up buzz char 
            server.GetKnownCharacteristics(StaticGuids.buzzerServiceGuid, StaticGuids.buzzCharGuid).Subscribe(buzzerChar =>
            {
                Debug.WriteLine("Found buzzer char");
                this.buzzerChar = buzzerChar;
                addOne(LoadLabel);
            });

            //set up team info
            server.GetKnownCharacteristics(StaticGuids.buzzerServiceGuid, StaticGuids.teamCharGuid).Subscribe(teamChar =>
            {
                Debug.WriteLine("Team char found");
                teamChar.EnableNotifications();
                teamChar.WhenNotificationReceived().Subscribe(result =>
                {
                    Debug.WriteLine("Team info recieved");
                    string teamInfoString = Encoding.UTF8.GetString(result.Data);
                    string[] teamInfo = teamInfoString.Split(teamSplitter, 2, StringSplitOptions.None);
                    foreach(string teamString in teamInfo)
                    {

                        string[] names = teamString.Split(nameSplitter, 50, StringSplitOptions.None);

                        if(!teams.ContainsKey(names[0]))
                        {
                            teams[names[0]] = new List<Guid>();
                        }
                        List<Guid> teamMembers = teams[names[0]];
                        for(int i = 1; i < names.Length; i++)
                        {
                            teamMembers.Add(Guid.Parse(names[i]));
                        }
                    }
                    addOne(LoadLabel);
                });
            });
        }

        Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("multibuzzer." + filename);
            return stream;
        }

        private async void Buzz(object sender, System.EventArgs e)
        {

            if (buzzerChar != null && lockStatus == 'O')
            {

                //play sound
                ISimpleAudioPlayer player = CrossSimpleAudioPlayer.Current;
                player.Load(GetStreamFromFile("buzz1.mp3"));
                player.Play();

                //tentatively change color to promote feeling of fast response
                lockStatus = 'U';
                //attempt buzz
                buzzerChar.Write(Encoding.UTF8.GetBytes(nickname)).Subscribe(write =>
                {
                    Debug.WriteLine("Successful buzz");
                }, error =>
                {
                    Debug.WriteLine(error.GetBaseException());
                });
            }
        }

        private void addOne(Label label)
        {
            int newTextNum = int.Parse(label.Text) + 1;
            label.Text = newTextNum.ToString();
        }

        private void printdebug(string s)
        {
            Label debug = new Label();
            debug.Text = s;
            BleDebug.Children.Add(debug);
        }

        private void manageTeamChoice()
        {
            if(!teams.ContainsKey("nt"))
            {
                Label label = new Label();
                label.Text = "You must choose a team before you can play";
                LoadUI.Children.Add(label);

                foreach (string teamName in teams.Keys)
                {
                    Button chooseTeam = new Button();
                    chooseTeam.Text = teamName;
                    chooseTeam.BackgroundColor = Color.Gray;
                    chooseTeam.TextColor = Color.Black;
                    chooseTeam.Clicked += (sender, e) =>
                    {
                        myTeam = ((Button)sender).Text;
                        identityChar.Write(Encoding.UTF8.GetBytes(":" + myTeam)).Subscribe(write =>
                        {
                            Debug.WriteLine("Successful identity write");
                            Game.IsVisible = true;
                            LoadUI.IsVisible = false;
                        }, error =>
                        {
                            Debug.WriteLine(error.GetBaseException());
                        });
                    };
                    LoadUI.Children.Add(chooseTeam);
                }
            }
        }
    }
}
