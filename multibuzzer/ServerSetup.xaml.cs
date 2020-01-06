using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace multibuzzer
{
    public partial class ServerSetup : ContentPage
    {

        public ServerSetup()
        {
            //TODO ensure colons not present in team names critical
            InitializeComponent();
            TeamNameChoice.IsVisible = false;
            Main.Padding = new Thickness(10, 50);

        }

        private async void Start(object sender, System.EventArgs e)
        {
            Debug.WriteLine("Name:" + ServerName.Text);
            if(!string.IsNullOrWhiteSpace(ServerName.Text))
            {
                if(UsingTeams.IsChecked)
                {
                    if(!string.IsNullOrWhiteSpace(OneName.Text) && !string.IsNullOrWhiteSpace(TwoName.Text))
                    {
                        List<string> names = new List<string>();
                        names.Add(OneName.Text);
                        names.Add(TwoName.Text);
                        await Navigation.PushModalAsync(new Server(ServerName.Text, names));
                    }
                }
                else
                {
                    await Navigation.PushModalAsync(new Server(ServerName.Text, new List<string>()));
                }
            }
        }

        private void IsTeamChanged(object sender, System.EventArgs e)
        {
            if (((CheckBox)sender).IsChecked)
            {
                TeamNameChoice.IsVisible = true;
            }
            else
            {
                TeamNameChoice.IsVisible = false;
            }
        }
    }
}
