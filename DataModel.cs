using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocalNotifications
{
    public class DataModel
    {
        public String name;

        // Constructor.
        public DataModel(String name)
        {
            this.name = name;
        }
    }
}