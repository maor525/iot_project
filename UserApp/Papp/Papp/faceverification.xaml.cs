using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.IO;

namespace Papp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class faceverification : ContentPage
    {
        public faceverification(string id, string pw, string user)
        {
            InitializeComponent();
            this.id = id;
            this.pw = pw;
            this.user = user;
        }

        private MediaFile _mediaFile;
        public string URL { get; set; }
        private string id;
        private string pw;
        private string user;

        //Picture choose from device
        private async void Button_Clicked(object sender, EventArgs e)
        {

            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Error", "This is not support on your device.", "OK");
                return;
            }
            else
            {
                var mediaOption = new PickMediaOptions()
                {
                    PhotoSize = PhotoSize.Medium
                };
                _mediaFile = await CrossMedia.Current.PickPhotoAsync();
                imageView.Source = ImageSource.FromStream(() => _mediaFile.GetStream());
            }
            btnUpload.IsEnabled = true;
        }

        //Upload picture button
        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            if (_mediaFile == null)
            {
                await DisplayAlert("Error", "There was an error when trying to get your image.", "OK");
                return;
            }
            else
            {
                await UploadImage(_mediaFile.GetStream());
            }
            var client = new System.Net.Http.HttpClient();
            string url = $"https://schoolprojfunctions.azurewebsites.net/api/first_delete/{id}?";
            var response = await client.GetAsync(url);
            await Navigation.PushModalAsync(new Student(id, pw, user));

        }

        //Take picture from camera
        private async void Button_Clicked_2(object sender, EventArgs e)
        {

            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":(No Camera available.)", "OK");
                return;
            }
            else
            {
                _mediaFile = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Directory = "Sample",
                    Name = "myImage.jpg"
                });


                imageView.Source = ImageSource.FromStream(() => _mediaFile.GetStream());
                var mediaOption = new PickMediaOptions()
                {
                    PhotoSize = PhotoSize.Medium
                };
            }
            btnUpload.IsEnabled = true;
        }

        //Upload to blob function
        private async Task<bool> UploadImage(Stream stream)
        {
            Busy();
            var account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=schoolprojstorage;AccountKey=WKYb475FL6jxRgtJ0YSAG3c8C9bxy694oJNPvMM5Sf0w4wodR2AN+XkCT8XRqMRU+T+gVMLAp3Nv1nmSMKCPBg==;EndpointSuffix=core.windows.net");
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("studentphotos");
            await container.CreateIfNotExistsAsync();
            var blockBlob = container.GetBlockBlobReference($"{id}.png");
            await blockBlob.UploadFromStreamAsync(stream);
            URL = blockBlob.Uri.OriginalString;
            NotBusy();
            await DisplayAlert("Uploaded", "Your image has been uploaded Successfully!", "OK");
            return true;
        }

        public void Busy()
        {
            uploadIndicator.IsVisible = true;
            uploadIndicator.IsRunning = true;
            btnSelectPic.IsEnabled = false;
            btnTakePic.IsEnabled = false;
            btnUpload.IsEnabled = false;
        }

        public void NotBusy()
        {
            uploadIndicator.IsVisible = false;
            uploadIndicator.IsRunning = false;
            btnSelectPic.IsEnabled = true;
            btnTakePic.IsEnabled = true;
            btnUpload.IsEnabled = true;
        }
    }
}