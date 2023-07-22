using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using Context = Android.Content.Context;

namespace LocalNotifications
{
    public class DrawerItemCustomAdapter : ArrayAdapter<DataModel>
    {
        private Context mContext;
        private int layoutResourceId;
        private DataModel[] data = null;

        public DrawerItemCustomAdapter(Context mContext, int layoutResourceId, DataModel[] data)
            : base(mContext, layoutResourceId, data)
        {
            this.layoutResourceId = layoutResourceId;
            this.mContext = mContext;
            this.data = data;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View listItem = convertView;

            LayoutInflater inflater = ((Activity)mContext).LayoutInflater;
            listItem = inflater.Inflate(layoutResourceId, parent, false);

            //ImageView imageViewIcon = listItem.FindViewById<ImageView>(Resource.Id.imageViewIcon);
            TextView textViewName = listItem.FindViewById<TextView>(Resource.Id.textViewName);

            DataModel folder = data[position];

            //imageViewIcon.SetImageResource(folder.Icon);
            textViewName.Text = folder.name;

            return listItem;
        }
    }
}