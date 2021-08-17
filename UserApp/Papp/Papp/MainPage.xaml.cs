using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Papp
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }
        async void OnSubmit(object sender, EventArgs args)
        {
            if (!isValidID(id.Text))
            {
                label.Text = "Please enter a valid id number.";
                return;
            }
            if(password.Text == null)
            {
                label.Text = "Please enter password.";
                return;
            }

            /* Checks if the id is of a student's or of a parent   */
            var client = new System.Net.Http.HttpClient();
            string url = $"https://schoolprojfunctions.azurewebsites.net/api/login/{id.Text}&{password.Text}?";
            var response = await client.GetAsync(url);
            var resp = response.Content.ReadAsStringAsync().Result;
            string login = resp.ToString();
            Console.WriteLine(login);

            switch (login)
            {
                case "-1":
                    {
                        label.Text = "ID not found.";   
                        return;
                    }
                case "wrong password":
                    {
                        label.Text = "Wrong password.";
                        return;
                    }
            }
            string[] user = login.Split(" ".ToCharArray()[0]);
            switch (user[0])
            {
                case "student":
                    {
                        string url1 = $"https://schoolprojfunctions.azurewebsites.net/api/first_lookup/{id.Text}?";
                        var response1 = await client.GetAsync(url1);
                        var resp1 = response1.Content.ReadAsStringAsync().Result;
                        if(resp1 == "image required")
                            App.Current.MainPage = new faceverification(id.Text, password.Text, user[1]);
                        await Navigation.PushModalAsync(new Student(id.Text, password.Text, user[1]));
                        break;
                    }
                case "parent":
                    {
                        await Navigation.PushModalAsync(new Parents(id.Text, user[1]));
                        break;
                    }
            }
        }
        
        
        private bool isValidID(string id)
        {
            if (id == null)
                return false;
            if (id.Length != 9)
                return false;
            for(int i = 0; i < id.Length; i++)
            {
                if (id[i] < '0' || id[i] > '9')
                    return false;
            }
            return true;
        }
    }
}
