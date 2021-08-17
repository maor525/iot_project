using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Papp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class attendance : ContentPage
    {
        public string att;
        public string id;
        public string dates = "";
        public attendance(string id, DateTime start_date, DateTime end_date, int span, string[] attendance_arr)
        {
            InitializeComponent();
            this.id = id;
            var table = new TableView();
            table.Intent = TableIntent.Settings;
            table.Root = new TableRoot();
            for(int i= 0; i < span; i++)
            {
                var layout = new StackLayout() { Orientation = StackOrientation.Horizontal };
                string info = attendance_arr[i];
                string[] info_arr = info.Split('?');
                layout.Children.Add(new Label()
                {
                    Text = "Entered: " + info_arr[1] + "                " + "Left: " + info_arr[2],
                    TextColor = Color.FromHex("#3b3d38"),
                    VerticalOptions = LayoutOptions.Center
                }) ;
                table.Root.Add(new TableSection(info_arr[0]){
                     new ViewCell() {View = layout}
                     });
            }
            Content = table;
        }

    }
}