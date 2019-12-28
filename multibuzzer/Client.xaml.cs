using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Plugin.BluetoothLE;
using Xamarin.Forms;
using IDevice = Plugin.BluetoothLE.IDevice;

namespace multibuzzer
{
    public partial class Client : ContentPage
    {

        IDevice server;
        IAdapter adapter;
        string serverName;

        string nickname;

        IGattCharacteristic buzzerChar;

        IGattCharacteristic lockStatusChar;
        string lockStatus;

        public Client(string serverName, string nickname)
        {
            this.serverName = serverName.Trim();
            this.nickname = nickname.Trim();
         
            adapter = CrossBleAdapter.Current;

            adapter.WhenReady().Subscribe(readyAdapter =>
            {
                Debug.WriteLine("Started when ready hopefully only once");
                adapter.Scan().Subscribe(scanResult => {
                    ProcessScan(scanResult);
                });
            });

            InitializeComponent();
            Nickname.Text = nickname;

        }

        private void ProcessScan(IScanResult scanResult)
        {
            Debug.WriteLine(scanResult.AdvertisementData.LocalName);
            Debug.WriteLine(serverName);
    
            if (scanResult.AdvertisementData.LocalName != null && scanResult.AdvertisementData.LocalName.Trim() == serverName)
            {
                server = scanResult.Device;
                adapter.StopScan();
                ConnectToServer();
            }
        }

        private void ConnectToServer()
        {
            Debug.WriteLine(server.Name.ToString());
            server.Connect();
            Server_Name.Text = "Currently connected to " + serverName;

            server.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
            {
                //printdebug(characteristic.Uuid.ToString());
                //if (characteristic.CanNotify())
                //{
                //    Debug.WriteLine("Notifiable");
                //    characteristic.EnableNotifications();
                //    characteristic.WhenNotificationReceived().Subscribe(result =>
                //    {
                //        Debug.WriteLine("Notification recieved" + Encoding.UTF8.GetString(result.Data));
                //        Status.Text = Encoding.UTF8.GetString(result.Data);
                //    });
                //}
                //characteristic.Write(Encoding.UTF8.GetBytes(nickname + "from general")).Subscribe();
            });

            //set up lock status
            server.GetKnownCharacteristics(StaticGuids.buzzerServiceGuid, StaticGuids.lockStatusGuid).Subscribe(lockStatusChar =>
            {
                Debug.WriteLine("Found Lock char");
                lockStatusChar.EnableNotifications();
                lockStatusChar.WhenNotificationReceived().Subscribe(result =>
                {
                    Debug.WriteLine("Notification recieved" + Encoding.UTF8.GetString(result.Data));
                    lockStatus = Encoding.UTF8.GetString(result.Data);
                    Status.Text = lockStatus;
                });
            });

            //set up identity management
            server.GetKnownCharacteristics(StaticGuids.buzzerServiceGuid, StaticGuids.identityCharGuid).Subscribe(identityChar =>
            {
                Debug.WriteLine("Found Identity char");
                //Debug.WriteLine("can write?" + identityChar.CanWrite());

                //identityChar.AssertWrite(false);
                identityChar.Write(Encoding.UTF8.GetBytes(nickname)).Subscribe(write =>
                {
                    Debug.WriteLine("Success??");
                }, error =>
                {
                    Debug.WriteLine(error.GetBaseException());
                });

            });

            //set up buzz char 
            server.GetKnownCharacteristics(StaticGuids.buzzerServiceGuid, StaticGuids.buzzCharGuid).Subscribe(buzzerChar =>
            {
                buzzerChar.CanWrite();
                Debug.WriteLine("Found buzzer char");
                this.buzzerChar = buzzerChar;
            });
        }

        private async void Buzz(object sender, System.EventArgs e)
        {

            if (buzzerChar != null && lockStatus == "O")
            {
                printdebug("Attempted buzz");
                buzzerChar.Write(Encoding.UTF8.GetBytes(nickname)).Subscribe(write =>
                {
                    Debug.WriteLine("Success??");
                }, error =>
                {
                    Debug.WriteLine(error.GetBaseException());
                });
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
