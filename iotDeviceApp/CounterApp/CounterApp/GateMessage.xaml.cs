using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;
using ZXing.Net.Mobile.Forms;

namespace CounterApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GateMessage : ContentPage
    {
        public static Label grant;
        public static Label reject;
        ZXingBarcodeImageView barcode;
        public GateMessage(string id)
        {
            InitializeComponent();
            grant = granted;
            reject = denied;
            barcode = new ZXingBarcodeImageView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                AutomationId = "zxingBarcodeImageView",
            };
            barcode.BarcodeFormat = ZXing.BarcodeFormat.QR_CODE;
            barcode.BarcodeOptions.Width = 500;
            barcode.BarcodeOptions.Height = 500;
            barcode.BarcodeValue = $"https://schoolprojfunctions.azurewebsites.net/api/gate_access/{id}/";
            StackPage.Children.Add(barcode);
            var backButton = new Button();
            backButton.Text = "Back";
            backButton.BackgroundColor = Color.MediumAquamarine;
            backButton.Clicked += (object sender, EventArgs e) =>
            {
                App.Current.MainPage = new MainPage();
            };
            StackPage.Children.Add(backButton);
        }
        public static async void Access(string access)
        {
            if(access == "granted")
            {
                App.Current.MainPage = new faceverification();
                //for(int i = 0; i < 7; i++)
                //{
                //    await grant.FadeTo(1, 500);
                //    await grant.FadeTo(0, 500);
                //}

            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    await reject.FadeTo(1, 500);
                    await reject.FadeTo(0, 500);
                }
                App.Current.MainPage = new MainPage();
            }
        }
    }
}