using System;
using System.Collections.Generic;
using Xamarin.Forms.PlatformConfiguration;

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
            Main.Padding = new Thickness(10, 50);
        }


        //private void Server_Picked(object sender, System.EventArgs e)
        //{
        //    serverPicked = true;
        //    serverName = ((Entry)sender).Text;

        //}

        //private void Nickname_Picked(object sender, System.EventArgs e)
        //{
        //    nicknamePicked = true;
        //    nickname = ((Entry)sender).Text;

        //}

        private async void Start(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Server_Picked.Text) && !string.IsNullOrWhiteSpace(Nickname_Picked.Text))
            {
                var clientPage = new Client(Server_Picked.Text, Nickname_Picked.Text);
                await Navigation.PushModalAsync(clientPage);
            }
        }

    }
}
