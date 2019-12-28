using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace multibuzzer
{
    public partial class MainPage : ContentPage
    {
        
        public MainPage()
        {
            InitializeComponent();
        }


        private async void Start_Server(object sender, System.EventArgs e)
        {
            var serverSetupPage = new ServerSetup();
            await Navigation.PushModalAsync(serverSetupPage);
        }


        private async void Start_Client(object sender, System.EventArgs e)
        {
            var clientSetupPage = new ChooseServer();
            await Navigation.PushModalAsync(clientSetupPage);
        }
    }
}
