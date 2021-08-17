using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Papp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Parents : ContentPage
    {
        IList<string> children_id = new List<string>();
        private string id;
        public Parents(string id, string name)
        {
            this.id = id;
            InitializeComponent();
            welcome.Text += name;
            showInfo();
        }
            void OnDateSelected(object sender, DateChangedEventArgs args)
    {
        Recalculate();
    }

    void OnSwitchToggled(object sender, ToggledEventArgs args)
    {
        Recalculate();
    }

        void Recalculate()
        {
            TimeSpan timeSpan = endDatePicker.Date - startDatePicker.Date + TimeSpan.FromDays(1);

            resultLabel.Text = String.Format("{0} day{1} between dates",
                                                timeSpan.Days, timeSpan.Days == 1 ? "" : "s");
        }
        /* Present gate passes of children */
        private async void showInfo()
        {
            var client = new System.Net.Http.HttpClient();
            var response = await client.GetAsync($"https://schoolprojfunctions.azurewebsites.net/api/getChildPresence/{id}?");
            var resp = response.Content.ReadAsStringAsync().Result;
            string[] info = resp.ToString().Split('\n');
            for (int i = 0; i < info.Length - 1; i++)
            {
                children_id.Add(info[i].Split('|')[0]);
                string child = info[i].Split('|')[1];
                picker.Items.Add(child.Split(' ')[0].Split(':')[0]);
                string[] childInfo = child.Split(':');
                string[] presence = child.Substring(childInfo[0].Length + 1).Split(',');
                if (presence.Length == 1)
                    infoLabel.Text += childInfo[0] + ":\n\t" + presence[0]+"\n";
                else
                {
                    string arrive = presence[0];
                    string exit = presence[1];
                    infoLabel.Text += childInfo[0] + ":\n\t" + arrive + "\n\t" + exit + "\n";
                }

            }
        }
        public async void OnButtonClicked(object sender, EventArgs args)
        {
            DateTime start_date, end_date;
            start_date = startDatePicker.Date;
            end_date = endDatePicker.Date;
            TimeSpan timeSpan = endDatePicker.Date - startDatePicker.Date + TimeSpan.FromDays(1);
            int span = timeSpan.Days;
            string[] attendance_arr;
            string dates = "", att;
            for (int i = 0; i < span; i++)
            {
                dates += start_date.AddDays(i).ToString().Replace("/", ".");
                if (i != span - 1)
                    dates += "|";
            }
            var client = new System.Net.Http.HttpClient();
            dates = dates.Replace(" ", "%20").Replace("|","%7C");
            string url = $"https://schoolprojfunctions.azurewebsites.net/api/getAttendance/{dates}/{children_id[picker.SelectedIndex]}";
            var response = await client.GetAsync(url);
            var resp = response.Content.ReadAsStringAsync().Result;
            att = resp.ToString();
            attendance_arr = att.Split('|');
            await Navigation.PushModalAsync(new attendance(children_id[picker.SelectedIndex],start_date,end_date, span , attendance_arr));
        }

    }
}