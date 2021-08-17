using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace CounterApp
{
    public partial class faceverification : ContentPage
    {
        private string id;
        MediaFile file;
        static string _storageConnection = "DefaultEndpointsProtocol=https;AccountName=xamarinblob;AccountKey=4bVzkyGnKQrXsJphtzmjnBy0RoyQLgRC8WEvh6ecc9RbWocEMmUXY6df5pXEJPifSDLj6MawSg0aKD2APHWXwQ==;EndpointSuffix=core.windows.net";
        static CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnection);
        static CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        static CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("images");

        public faceverification()
        {
            InitializeComponent();
            id = MainPage.id1;
        }
        private MediaFile _mediaFile;
        public string URL { get; set; }

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
            App.Current.MainPage = new MainPage();

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
            btnTakePic.Text = "Retake";
            btnUpload.IsEnabled = true;
        }

        //Upload to blob function
        private async Task<bool> UploadImage(Stream stream)
        {
            Busy();
            var account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=schoolprojstorage;AccountKey=WKYb475FL6jxRgtJ0YSAG3c8C9bxy694oJNPvMM5Sf0w4wodR2AN+XkCT8XRqMRU+T+gVMLAp3Nv1nmSMKCPBg==;EndpointSuffix=core.windows.net");
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("tmpphotos");
            await container.CreateIfNotExistsAsync();
            var name = Guid.NewGuid().ToString();
            var blockBlob = container.GetBlockBlobReference($"{id}_tmp.png");
            await blockBlob.UploadFromStreamAsync(stream);
            URL = blockBlob.Uri.OriginalString;
            bool t = await verification();
            NotBusy();
            if (t)
                await DisplayAlert("Verification succees", "The gate will now open, Have a nice day!", "OK");
            else
                await DisplayAlert("Verification failed", "Please try again", "OK");
            return true;
        }

        public void Busy()
        {
            uploadIndicator.IsVisible = true;
            uploadIndicator.IsRunning = true;
            btnTakePic.IsEnabled = false;
            btnUpload.IsEnabled = false;
        }

        public void NotBusy()
        {
            uploadIndicator.IsVisible = false;
            uploadIndicator.IsRunning = false;
            btnTakePic.IsEnabled = true;
            btnUpload.IsEnabled = true;
        }

        public async Task<bool> verification()
        {
            const string SUBSCRIPTION_KEY = "3bd6814e15dc4a95a6145cdf7dff0a7f";
            const string ENDPOINT = "https://schoolfaceverification.cognitiveservices.azure.com/";
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(SUBSCRIPTION_KEY)) { Endpoint = ENDPOINT };
            IList<DetectedFace> originalfaces = await client.Face.DetectWithUrlAsync($"https://schoolprojstorage.blob.core.windows.net/studentphotos/{id}.png", detectionModel: DetectionModel.Detection01, recognitionModel: RecognitionModel.Recognition04);
            IList<DetectedFace> tmpfaces = await client.Face.DetectWithUrlAsync($"https://schoolprojstorage.blob.core.windows.net/tmpphotos/{id}_tmp.png", detectionModel: DetectionModel.Detection01, recognitionModel: RecognitionModel.Recognition04);
            DetectedFace original = originalfaces[0];
            DetectedFace tmp = tmpfaces[0];
            VerifyResult res = await client.Face.VerifyFaceToFaceAsync(original.FaceId.Value, tmp.FaceId.Value);
            return res.IsIdentical;
        }
    }
}