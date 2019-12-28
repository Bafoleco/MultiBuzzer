using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace multibuzzer
{
    public partial class ServerSetup : ContentPage
    {
        public ServerSetup()
        {
            InitializeComponent();
        }

        private async void Server_Named(object sender, System.EventArgs e)
        {
            string serverName = ((Entry)sender).Text;
            var serverPage = new Server(serverName);
            await Navigation.PushModalAsync(serverPage);

        }
    }
}
