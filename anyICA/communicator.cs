using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Nancy;
using Nancy.Extensions;
using Nancy.Hosting.Self;
using Newtonsoft.Json;

namespace anyICA
{
    public class Communicator : NancyModule
    {
        private GroceryList groceries{get; set;}


        public Communicator()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow window = (MainWindow)Application.Current.MainWindow;
                groceries = window.groceryList;
            }, DispatcherPriority.ContextIdle);
            //Get["/"] = x => { return "Hello World"; };


            Get("/getitem", parameters => {
                //do work here
                var ret = "";
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow window = (MainWindow)Application.Current.MainWindow;
                    ret = window.GetItem();
                },DispatcherPriority.ContextIdle);

                return ret;  //""; //MainWindow.GetItem();
            });
            Post("/anypush", data => {
                var jsonString = this.Request.Body.AsString();
                groceries.IngestFromAjax(jsonString);
                //dynamic list = JsonConvert.DeserializeObject(jsonString);
                return "awesome";
            }); 
        }
    }
}
