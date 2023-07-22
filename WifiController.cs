using Android.Content;
using Android.Net.Wifi;
using Android.Net.Wifi.P2p;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalNotifications
{
    public class WifiScanReceiver : BroadcastReceiver
    {
        private readonly WifiManager mWifiManager;

        public WifiScanReceiver(WifiManager wifiManager)
        {
            mWifiManager = wifiManager;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string Action = intent.Action;
            if (Action.Equals(WifiManager.ScanResultsAvailableAction))
            {
                IList<ScanResult> mScanResults = mWifiManager.ScanResults;
                Dictionary<string, Tuple<string, bool>> wifiListTemp = new();
                foreach (ScanResult result in mScanResults)
                {
                    if (!MainActivity.WifiList.ContainsKey(result.Bssid) && !(result.Ssid.Length == 0))
                    {
                        wifiListTemp.Add(result.Bssid, Tuple.Create(result.Ssid, false));
                    }
                }
                WifiInfo info = MainActivity.wifiManager.ConnectionInfo;
                MainActivity.currentBSSID = info.BSSID;
                wifiListTemp.ToList().ForEach(wifi => { MainActivity.WifiList.Add(wifi.Key, wifi.Value); });
            }
        }
    }
}