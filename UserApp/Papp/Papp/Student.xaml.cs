using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace Papp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Student : ContentPage
    {
        private string id;
        private string password;

        public Student(string id, string password, string name)
        {
            this.id = id;
            this.password = password;
            InitializeComponent();
            welcome.Text += name;
        }
        private async void btnScan_Clicked(object sender, EventArgs e)
        {
            var scanner = new ZXingScannerPage();

            await Navigation.PushModalAsync(new NavigationPage(scanner));
            scanner.OnScanResult += (result) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopModalAsync();
                    CheckPermission(result.Text);
                });
            };
            
        }

        /* Check if student is allowed to pass and update students' table */
        private async void CheckPermission(string url)
        {
            permissionLabel.Text = url + password;
            var client = new System.Net.Http.HttpClient();
            var response = await client.GetAsync(url+password);
            var resp = response.Content.ReadAsStringAsync().Result;
            permissionLabel.Text = resp.ToString();
            switch (resp.ToString())
            {
                case "updated":
                    {
                        permissionLabel.Text = "Gate opened.\nHave a nice day!";
                        break;
                    };
                case "expired":
                    {
                        permissionLabel.Text = "Your QR code has expired.\nPlease enter your ID to the gate device and scan again.";
                        break;
                    }
                case "schedule_err":
                    {
                        permissionLabel.Text = "Your scheduale is not allowing this pass.\n Please check with the office for special cases.";
                        break;
                    }
                case "out_err":
                    {
                        permissionLabel.Text = "Enterance is not allowed after schedule hours.\nIn special cases please call the office.\n Have a nice day!";
                        break;
                    }
                case "user_err":
                    {
                        permissionLabel.Text = "You are scanning another student's QR code.\nPlease enter your ID to the gate device and scan again.";
                        break;
                    }
            }
        }
    }
}