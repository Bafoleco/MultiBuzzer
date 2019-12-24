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
            var serverPage = new Server();
            await Navigation.PushModalAsync(serverPage);
        }


        private async void Start_Client(object sender, System.EventArgs e)
        {
            var clientPage = new Client();
            await Navigation.PushModalAsync(clientPage);
        }
    }
}
