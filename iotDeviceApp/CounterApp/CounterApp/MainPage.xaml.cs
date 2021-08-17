using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Essentials;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CounterApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        public static string id1;
       public async void OnButtonClicked(object sender, EventArgs args)
        {
            var client = new System.Net.Http.HttpClient();
            string url = $"https://schoolprojfunctions.azurewebsites.net/api/set_passcode/{id.Text}?code=gS0CgTKRS4Ywxfo2VjQ9qgrBp7Eq3tClPDiTJsh1144IFhDHqc4now==";
            var response = await client.GetAsync(url);
            var resp = response.Content.ReadAsStringAsync().Result;
            string str = resp.ToString();
            if (str == "code_set")
            {
                id1 = id.Text;
                App.Current.MainPage = new GateMessage(id.Text);
                //App.Current.MainPage = new faceverification();
            }
            else if(str == "permit")
            {
                for (int i = 0; i < 7; i++)
                {
                    await granted.FadeTo(1, 500);
                    await granted.FadeTo(0, 500);
                }
                id.Text = string.Empty;
            }
            else
            {
                await invalid_id.FadeTo(1,1000);
                await Task.Delay(4000);
                await invalid_id.FadeTo(0,1000);
            }
        }
    }
}
