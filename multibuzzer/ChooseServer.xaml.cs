using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace multibuzzer
{
    public partial class ChooseServer : ContentPage
    {

        private bool serverPicked = false;
        private bool nicknamePicked = false;
        string serverName;
        string nickname;
        public ChooseServer()
        {
            InitializeComponent();
        }


        private void Server_Picked(object sender, System.EventArgs e)
        {
            serverPicked = true;
            serverName = ((Entry)sender).Text;

        }

        private void Nickname_Picked(object sender, System.EventArgs e)
        {
            nicknamePicked = true;
            nickname = ((Entry)sender).Text;

        }

        private async void Start(object sender, System.EventArgs e)
        {
            if (serverPicked && nicknamePicked)
            {
                var clientPage = new Client(serverName, nickname);
                await Navigation.PushModalAsync(clientPage);
            }
        }
    }
}