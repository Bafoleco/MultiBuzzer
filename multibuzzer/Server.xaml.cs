using System;
using System.Collections.Generic;
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


        List<Guid> services;
        private IGattServer server;
        private IGattService buzzerService;
        private Guid buzzerServiceGuid;
        private Guid lockStatusGuid;

        //O = open
        //C = closed
        private Char lockStatus;

        public Server()
        {
            services = new List<Guid>(); 
            lockStatus = 'O';

            //create buzzer service 
            server = (IGattServer) CrossBleAdapter.Current.CreateGattServer();
            buzzerServiceGuid = Guid.NewGuid();
            services.Add(buzzerServiceGuid);
            buzzerService = server.CreateService(buzzerServiceGuid, true);


            //create characteristic to inform connected devices about the lock out
            //status of the buzzer unit 
            lockStatusGuid = Guid.NewGuid();
            IGattCharacteristic lockStatusChar = buzzerService.AddCharacteristic(lockStatusGuid,
                    CharacteristicProperties.Read | CharacteristicProperties.Write,
                    GattPermissions.Read | GattPermissions.Write);

            lockStatusChar.WhenReadReceived().Subscribe(req =>
            {
                //transmit character lockStatus as 1 length byte array
                byte[] transmit = new byte[1];
                transmit[0] = (byte) lockStatus; 
                req.Value = transmit; 
            });


            //start advertising the server 
            CrossBleAdapter.Current.Advertiser.Start(new AdvertisementData
            {
                LocalName = "MultiBuzzer",
                ServiceUuids = services

            });
            InitializeComponent();
         
        }

        private async void Send_Text(object sender, System.EventArgs e)
        {
            //blank
        }
    }
}
