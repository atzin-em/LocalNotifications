using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using Context = Android.Content.Context;

namespace LocalNotifications
{
    public class NetworksItemCustomAdapter : BaseAdapter<string>
    {
        private Context context;
        private int layoutResourceId;
        public Dictionary<string, Tuple<string, bool>> networksList = null;
        private List<Google.Android.Material.Button.MaterialButton> buttonNetworkSelect = new List<Google.Android.Material.Button.MaterialButton>();
        public event NotificationDelegate networksListChanged;

        protected virtual void OnNetworksListChanged()
        {
            networksListChanged.Invoke();
        }
        public NetworksItemCustomAdapter(Context mContext, int layoutResourceId, Dictionary<string, Tuple<string, bool>> data)
        {
            this.context = mContext;
            this.layoutResourceId = layoutResourceId;
            this.networksList = data;

        }

        public override int Count => networksList.Count;

        public override long GetItemId(int position)
        {
            return position;
        }

        //public override Tuple<string, bool> this["dw"]
        public override string this[int position] => networksList[networksList.Keys.ToList()[position]].Item1;

        public void Update()
        {
            NotifyDataSetChanged();
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            //LinearLayout parentView = (LinearLayout)parent.view..Parent;
            if (view == null)
            {
                LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                view = inflater.Inflate(layoutResourceId, parent, false);
            }

            TextView textviewNullNet = parent.RootView.FindViewById<TextView>(Resource.Id.textviewNullNet);
            ListView listviewNetworks = parent.RootView.FindViewById<ListView>(Resource.Id.listviewNetworks);
            //LinearLayout linearLayout = ((LinearLayout)view);
            //Console.WriteLine(networksList.Count);
            //Console.WriteLine(listviewNetworks.ChildCount);
            for (int i = 0; i < listviewNetworks.ChildCount; i++)
            {
                Google.Android.Material.Button.MaterialButton networkButton = (Google.Android.Material.Button.MaterialButton)((LinearLayout)listviewNetworks.GetChildAt(i)).GetChildAt(0);
                if (networkButton.Text == "")
                {
                    networkButton.Text = this[i];
                    networkButton.Click += NetworkButton_Click;
                    bool networkEnabled = networksList[GetMACFromSSID(this[i])].Item2;
                    networkButton.SetIconResource(networkEnabled == true ? Resource.Drawable.btn_radio_on_dark : Resource.Drawable.btn_radio_off_dark);
                    networkButton.Tag = networkEnabled == true ? "true" : "false";
                    buttonNetworkSelect.Add(networkButton);
                }
            }


            if (networksList.Count <= 0)
            {
                textviewNullNet.Visibility = ViewStates.Visible;
            }
            else if (networksList.Count > 0)
            {
                textviewNullNet.Visibility = ViewStates.Gone;
            }

            return view;
        }
        private string GetMACFromSSID(string ssid)
        {
            string tempKey = networksList.First(x => x.Value.Item1 == ssid).Key;
            return tempKey;
        }

        private void NetworkButton_Click(object sender, EventArgs e)
        {
            foreach (Google.Android.Material.Button.MaterialButton network in buttonNetworkSelect)
            {
                if (((Button)sender).UniqueDrawingId == network.UniqueDrawingId)
                {
                    network.SetIconResource(network.Tag.ToString() == "false" ? Resource.Drawable.btn_radio_on_dark : Resource.Drawable.btn_radio_off_dark);
                    network.Tag = network.Tag.ToString() == "false" ? "true" : "false";
                    string tempKey = GetMACFromSSID(network.Text);
                    networksList[tempKey] = Tuple.Create(network.Text, bool.Parse(network.Tag.ToString()));
                    OnNetworksListChanged();
                }
            }
        }
    }
}