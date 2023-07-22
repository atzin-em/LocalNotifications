using System;
using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Widget;
using LocalNotifications.Resources;
using static Xamarin.Essentials.Platform;
using Android.Content;
using Android;
using Newtonsoft.Json;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Core.App;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using AndroidX.Core.View;
using Android.Views;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Android.Net.Wifi;
using Android.Views.InputMethods;

namespace LocalNotifications
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = Android.Views.SoftInput.StateAlwaysHidden)]
    public class MainActivity : AppCompatActivity
    {
        public static ApiController apiController;

        private ListView mDrawerList;
        private string[] mNavigationDrawerItemTitles;
        private DrawerLayout drawerLayout;
        private Toolbar toolbar;
        private FrameLayout displayFrame;

        private Button buttonGet;
        private static TextView textviewLog;
        private static readonly List<Tuple<DateTime, string>> logList = new();
        
        private Button buttonSelectNetworks;
        private PopupWindow popupWindow;
        public static WifiManager wifiManager;
        public static bool ValidConnection { 
            get 
            {
                if (WifiList != null && currentBSSID != null)
                {
                    if (WifiList.ContainsKey(currentBSSID))
                    {
                        return WifiList[currentBSSID].Item2;
                    } else
                    {
                        return false;
                    }
                } else
                {
                    return false;
                }
                
            } 
        }
        public static string currentBSSID = "";
        private ListView listviewNetworks;
        private static Dictionary<string, Tuple<string, bool>> _wifiList = new();
        public static Dictionary<string, Tuple<string, bool>> WifiList { 
            get { return _wifiList; }
            set { _wifiList = value; } 
        }

        private EditText inputTextApi;
        private Button buttonSubmitApi;
        private EditText inputTextApiKeyHeader;
        private EditText inputTextApiKey;
        private Button buttonSubmitApiKey;
        private EditText inputTextKey;
        private EditText inputTextValue;
        private Button buttonSubmitKeyVal;
        private static ListView listViewResponses;
        public static ResponseNotifications ResponseNotifications { 
            get 
            {
                if (listViewResponses != null)
                {
                    if (listViewResponses.Adapter != null)
                    {
                        ResponseNotifications dataStore = ((ResponsesItemCustomAdapter)listViewResponses.Adapter).responseItems;
                        return dataStore;
                    }

                }
                return new ResponseNotifications();
            } 
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            Init(this, savedInstanceState);
            GetWifi();
            SetContentView(Resource.Layout.content_main);
            apiController = new ApiController();

            displayFrame = FindViewById<FrameLayout>(Resource.Id.content_frame);
            CreateNotificationChannel();

            var intent = new Android.Content.Intent(this, typeof(ServiceController));
            StartService(intent);
            ResponseNotifications.ItemDeleted += ResponseNotifications_ItemDeleted;
            ResponseNotifications.ItemChanged += ResponseNotifications_ItemChanged;
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            SetupSideDrawer();
            mDrawerList.SetItemChecked(0, true);
            InitSetupLayout();
            InitSavedData();
        }

        private void ResponseNotifications_ItemChanged()
        {
            ResponseNotifications dataStore = ((ResponsesItemCustomAdapter)listViewResponses.Adapter).responseItems;
            StoreSetupData("ResponseNotifications", JsonConvert.SerializeObject(dataStore.GetList()));
        }

        private void ResponseNotifications_ItemDeleted()
        {
            listViewResponses.Adapter = new ResponsesItemCustomAdapter(this, ResponseNotifications);
            ResponseNotifications dataStore = ((ResponsesItemCustomAdapter)listViewResponses.Adapter).responseItems;
            StoreSetupData("ResponseNotifications", JsonConvert.SerializeObject(dataStore.GetList()));
        }

        private async void StoreSetupData(string key, string val)
        {
            await SecureStorage.SetAsync(key, val);
        }

        private async Task<string> GetSetupData(string key)
        {
            return await SecureStorage.GetAsync(key);
        }

        private ResponseNotifications AddResponseNotification(string key, string val)
        {
            ResponseNotifications.Add(key, val);
            listViewResponses.Adapter = new ResponsesItemCustomAdapter(this, ResponseNotifications);
            return ResponseNotifications;
        }

        private void InitSavedData()
        {
            listViewResponses = FindViewById<ListView>(Resource.Id.listView);
            inputTextApi.Text = GetSetupData("ApiUrl").Result;
            inputTextApiKey.Text = GetSetupData("ApiKey").Result;
            inputTextApiKeyHeader.Text = GetSetupData("ApiKeyHeader").Result;

            var wifiData = GetSetupData("WifiList").Result;
            if (wifiData != null)
            {
                WifiList = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, bool>>>(wifiData);
            }
            
            string notificationData = GetSetupData("ResponseNotifications").Result;
            List<RootResponseNotification> rootResponseNotification;
            if (notificationData != null)
            {
                rootResponseNotification = JsonConvert.DeserializeObject<List<RootResponseNotification>>(notificationData);
                List<ResponseNotification> resp = rootResponseNotification.Select(x => new ResponseNotification().ConsumeRoot(x)).ToList();
                listViewResponses.Adapter = new ResponsesItemCustomAdapter(this, new ResponseNotifications(resp));
            }
            

            if (inputTextApi.Length() > 0)
            {
                apiController.Api = inputTextApi.Text;
            }
            if (inputTextApiKey.Length() > 0 && inputTextApiKeyHeader.Length() > 0)
            {
                apiController.Key = inputTextApiKey.Text;
                apiController.KeyHeader = inputTextApiKeyHeader.Text;
            }
        }

        private void InitSetupLayout()
        {
            View content_view = LayoutInflater.Inflate(Resource.Layout.content_setup, null);
            displayFrame.RemoveAllViews();
            displayFrame.AddView(content_view);

            buttonSelectNetworks = FindViewById<Button>(Resource.Id.buttonSelectNetworks);
            buttonSelectNetworks.Click += ButtonSelectNetworks_Click;

            inputTextApi = FindViewById<EditText>(Resource.Id.inputTextApi);
            buttonSubmitApi = FindViewById<Button>(Resource.Id.buttonSubmitApi);
            buttonSubmitApi.Click += ButtonSubmitApi_Click;

            inputTextApiKeyHeader = FindViewById<EditText>(Resource.Id.inputTextApiKeyHeader);
            inputTextApiKey = FindViewById<EditText>(Resource.Id.inputTextApiKey);
            buttonSubmitApiKey = FindViewById<Button>(Resource.Id.buttonSubmitApiKey);
            buttonSubmitApiKey.Click += ButtonSubmitApiKey_Click;

            inputTextKey = FindViewById<EditText>(Resource.Id.inputTextKey);
            inputTextValue = FindViewById<EditText>(Resource.Id.inputTextValue);
            buttonSubmitKeyVal = FindViewById<Button>(Resource.Id.buttonSubmitKeyVal);
            buttonSubmitKeyVal.Click += ButtonSubmitKeyVal_Click;
        }

        private void ButtonScanNetworks_Click(object sender, EventArgs e)
        {
            WifiScan();
            Toast.MakeText(this, "Scanning for WiFi Networks", ToastLength.Short).Show();
        }

        private void GetWifi()
        {
            const int RequestLocationId = 0;

            var locationPermission = Manifest.Permission.AccessFineLocation;
            if (CheckSelfPermission(locationPermission) != Android.Content.PM.Permission.Granted)
            {
                RequestPermissions(new string[] { locationPermission }, RequestLocationId);
            }

            wifiManager = (WifiManager)GetSystemService(WifiService);
            wifiManager.SetWifiEnabled(true);
            WifiScanReceiver wifiScanReceiver = new(wifiManager);
            RegisterReceiver(wifiScanReceiver, new IntentFilter(WifiManager.ScanResultsAvailableAction));
            WifiScan();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode != 0 && grantResults[0] != Android.Content.PM.Permission.Granted)
            {
                // Handle the case when the user denies the location permission.
                Toast.MakeText(this, "Location permission denied. Unable to scan Wi-Fi networks.", ToastLength.Short).Show();
            }
        }

        private void WifiScan()
        {
            wifiManager.StartScan();
        }


        private void ButtonSelectNetworks_Click(object sender, EventArgs e)
        {
            LayoutInflater inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
            View popupView = inflater.Inflate(Resource.Menu.list_view_networks, null);

            popupWindow = new PopupWindow(popupView, ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, true);

            Button anchor = (Button)sender;
            popupWindow.ShowAsDropDown(anchor, (anchor.MeasuredWidth / 5) * -1, 0, GravityFlags.Center);

            listviewNetworks = popupView.FindViewById<ListView>(Resource.Id.listviewNetworks);
            NetworksItemCustomAdapter adapter = new(this, Resource.Menu.popup_menu_networks, WifiList);
            adapter.networksListChanged += Adapter_networksListChanged;
            listviewNetworks.Adapter = adapter;
        }

        private void Adapter_networksListChanged()
        {
            Dictionary<string, Tuple<string,bool>> savedNetworks = ((NetworksItemCustomAdapter)listviewNetworks.Adapter).networksList.Where(x => x.Value.Item2 == true).ToDictionary(kv => kv.Key, kv => kv.Value);
            StoreSetupData("WifiList", JsonConvert.SerializeObject(savedNetworks));
        }

        private void ButtonSubmitKeyVal_Click(object sender, EventArgs e)
        {
            if (inputTextKey.Text.Trim().Length > 0 && inputTextValue.Length() > 0)
            {
                ResponseNotifications dataStore = AddResponseNotification(inputTextKey.Text, inputTextValue.Text);
                StoreSetupData("ResponseNotifications", JsonConvert.SerializeObject(dataStore.GetList()));
                Toast.MakeText(this, "Added New Response Notification", ToastLength.Short).Show();
            } 
            else
            {
                Toast.MakeText(this, "Can't Add Empty Key/Val", ToastLength.Short).Show();
            }
        }

        private void ButtonSubmitApiKey_Click(object sender, EventArgs e)
        {
            string apiKeyHeader = inputTextApiKeyHeader.Text;
            string apiKey = inputTextApiKey.Text;
            if (
                ((apiController.KeyHeader.Length == 0 && inputTextApiKeyHeader.Length() == 0) || 
                (apiController.KeyHeader.Length > 0 && inputTextApiKeyHeader.Length() == 0)) && inputTextApiKey.Length() > 0)
            {
                Toast.MakeText(this, "Can't Set Secret With No Header", ToastLength.Short).Show();
            }
            else if (inputTextApiKeyHeader.Length() > 0 && inputTextApiKey.Length() == 0)
            {
                Toast.MakeText(this, "Can't Set Empty Header", ToastLength.Short).Show();
            }
            else if (
                (inputTextApiKeyHeader.Length() > 0 && inputTextApiKey.Length() > 0) || 
                (inputTextApiKeyHeader.Length() == 0 && apiController.KeyHeader.Length > 0) ||
                (inputTextApiKey.Length() == 0 && apiController.Key == "SET"))
            {
                StoreSetupData("ApiKeyHeader", apiKeyHeader);
                StoreSetupData("ApiKey", apiKey);
                apiController.Key = apiKey;
                apiController.KeyHeader = apiKeyHeader;
                Toast.MakeText(this, (apiKey.Length > 0 ? "Set Api Secret" : "Unset Api Secret"), ToastLength.Short).Show();
            } 
            else
            {
                Toast.MakeText(this, "Can't Set Empty Secret And Header", ToastLength.Short).Show();
            }
        }

        private void ButtonSubmitApi_Click(object sender, EventArgs e)
        {
            if (apiController.Api.Length == 0 && inputTextApi.Length() == 0)
            {
                Toast.MakeText(this, "Api Url Cannot Be Empty", ToastLength.Short).Show();
            } else
            {
                string apiUrl = inputTextApi.Text.Trim();
                if (!apiUrl.Contains("http://"))
                {
                    apiUrl = "http://" + apiUrl;
                    inputTextApi.Text = apiUrl;
                }
                StoreSetupData("ApiUrl", apiUrl);
                apiController.Api = apiUrl;
                Toast.MakeText(this, (apiUrl.Length > 0 ? "Set Api Url" : "Unset Api Url"), ToastLength.Short).Show();
            }
        }

        private void InitDebugLayout()
        {
            View content_view = LayoutInflater.Inflate(Resource.Layout.content_debug, null);
            displayFrame.RemoveAllViews();
            displayFrame.AddView(content_view);

            textviewLog = FindViewById<TextView>(Resource.Id.textviewLog);

            if (logList.Count > 0)
            {
                foreach (Tuple<DateTime, string> logEvent in logList)
                {
                    textviewLog.Text += " " + logEvent.Item2.ToString() + "\n>";
                }
            }

            buttonGet = FindViewById<Button>(Resource.Id.buttonGet);
            buttonGet.Click += ButtonGet_Click;
        }

        private async void ButtonGet_Click(object sender, EventArgs e)
        {
            string response = await apiController.GetRequest();
            LogDebug(response);
        }

        public static void DismissKeyboard(View view)
        {
            if (view != null)
            {
                var imm = (InputMethodManager)CurrentActivity.GetSystemService(InputMethodService);
                imm.HideSoftInputFromWindow(view.WindowToken, 0);
            }
            var currentFocus = CurrentActivity.CurrentFocus;
            if (currentFocus != null)
            {
                InputMethodManager inputMethodManager = (InputMethodManager)CurrentActivity.GetSystemService(Context.InputMethodService);
                inputMethodManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
            }
        }

        public static void LogDebug(string message)
        {
            logList.Add(Tuple.Create(DateTime.Now, message));
            if (textviewLog != null)
            {
                textviewLog.Text += " " + message + "\n>";
            }
        }

        private void SetupSideDrawer()
        {
            mDrawerList = (ListView)FindViewById(Resource.Id.nav_drawer);
            mNavigationDrawerItemTitles = Resources.GetStringArray(Resource.Array.navigation_drawer_items_array);
            DataModel[] drawerItem = new DataModel[mNavigationDrawerItemTitles.Length];

            for (int i = 0; i < mNavigationDrawerItemTitles.Length; i++)
            {
                drawerItem[i] = new DataModel(mNavigationDrawerItemTitles[i]);
            }

            DrawerItemCustomAdapter adapter = new(this, Resource.Menu.list_view_drawer, drawerItem);
            mDrawerList.Adapter = adapter;
            mDrawerList.ItemClick += MDrawerList_ItemClick;

            toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            drawerLayout = (DrawerLayout)FindViewById(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new(this, drawerLayout, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawerLayout.AddDrawerListener(toggle);
            toggle.DrawerIndicatorEnabled = true;
            toggle.SyncState();            
        }

        private void MDrawerList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            View content_view = null;
            if (e.Position == 0) 
            {
                InitSetupLayout();
                InitSavedData();
            } 
            else if (e.Position == 1) 
            {
                InitDebugLayout();
            }

            if (content_view != null)
            {
                drawerLayout.CloseDrawer(GravityCompat.Start);
            }
        }

        private void CreateNotificationChannel()
        {
            var permissionStatus = CheckSelfPermission(Manifest.Permission.PostNotifications);
            if (permissionStatus != Android.Content.PM.Permission.Granted)
            {
                // Request permission
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.PostNotifications }, 1);
            }
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }
        }
    }

   
}
