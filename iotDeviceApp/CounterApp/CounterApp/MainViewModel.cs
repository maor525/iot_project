using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Essentials;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CounterApp
{
    public class MainViewModel
    {
        public static HubConnection connection;
        public MainViewModel()
        {
            connection = new HubConnectionBuilder()
            .WithUrl("https://schoolprojfunctions.azurewebsites.net/api")
            .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
            connection.On<Dictionary<string, string>>("GateAccess", (value) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if(value["Access"] == "granted")
                    {
                        GateMessage.Access("granted");
                    }
                    else
                    {
                        GateMessage.Access("rejected");
                    }

                });
            });
        }

    }
}