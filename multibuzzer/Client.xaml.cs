using System;
using System.Collections.Generic;
using Plugin.BluetoothLE;
using Xamarin.Forms;
using IDevice = Plugin.BluetoothLE.IDevice;

namespace multibuzzer
{
    public partial class Client : ContentPage
    {

        IDevice server;

        public Client()
        {
            
            CrossBleAdapter.Current.Scan().Subscribe(scanResult => {
                //add every scan result to screen 
                Label debugLabel = new Label();
                debugLabel.Text = scanResult.ToString(); 
                Debug.Children.Add(debugLabel);

                if(scanResult.AdvertisementData.LocalName == "MultiBuzzer")
                {
                    server = scanResult.Device;
                }
            });

            server.Connect();
            server.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
            {

                characteristic.EnableNotifications();
                characteristic.WhenNotificationReceived().Subscribe(result => {
                    Status.Text = ((char)result.Data[0]).ToString();
                });

            });


            InitializeComponent();
        }
    }
}
