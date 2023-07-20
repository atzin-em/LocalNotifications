using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;
using Json.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalNotifications
{
    public static class NotificationState
    {
        public static Dictionary<ResponseNotification, bool> OldDict = new Dictionary<ResponseNotification, bool>();
        public static Dictionary<ResponseNotification, bool> Dict = new Dictionary<ResponseNotification, bool>();
    }

    public class TimeWrapper
    {
        private DateTime dateTime;

        public TimeWrapper(DateTime dateTime)
        {
            this.dateTime = dateTime;
        }

        public DateTime Get()
        {
            return dateTime;
        }

        public DateTime Set(DateTime dateTime)
        {
            this.dateTime = dateTime;
            return dateTime;
        }

        public DateTime SetNow()
        {
            this.dateTime = DateTime.Now;
            return dateTime;
        }
    }

    [Service]
    public class ServiceController : Service
    {
        private static Handler handler;
        private static Action runnable;
        private bool isServiceRunning;
        private static Dictionary<RepeatInterval, Tuple<List<ResponseNotification>, ResponseNotifications.TimeWrapper>> intervalGroups;
        private List<Intent> notifyServices;

        public override void OnCreate()
        {
            base.OnCreate();
            handler = new Handler(Looper.MainLooper);
            isServiceRunning = false;
        }

        private static IRunnable CreateRunnable(RepeatInterval paramStr)
        {
            IRunnable aRunnable = new RunnableParam(paramStr);
            return aRunnable;
        }

        class RunnableParam : Java.Lang.Object, IRunnable
        {
            private readonly RepeatInterval paramInterval;

            public RunnableParam(RepeatInterval paramStr)
            {
                this.paramInterval = paramStr;
            }

            public void Run()
            {
                DoWork(paramInterval);
            }
        }


        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!isServiceRunning)
            {
                foreach (ResponseNotification response in MainActivity.responseNotifications)
                {
                    NotificationState.Dict[response] = false;
                    NotificationState.OldDict[response] = false;

                }

                isServiceRunning = true;

                intervalGroups = MainActivity.responseNotifications.GroupByInterval();
                notifyServices = new List<Intent>();

                RepeatInterval MinNonZeroInterval = intervalGroups.Keys.ToList().OrderBy(x => (int)x).FirstOrDefault();
                MinNonZeroInterval = MinNonZeroInterval == RepeatInterval.NoRepeat ? RepeatInterval.Every5Min : MinNonZeroInterval;
                MainActivity.LogDebug($"Background notification service set to repeat every {(int)MinNonZeroInterval} minutes");
                handler.PostDelayed(CreateRunnable(MinNonZeroInterval), 0);
            }

            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            // This service doesn't support binding, so return null
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Stop the service and remove any pending callbacks
            handler.RemoveCallbacks(runnable);
            isServiceRunning = false;
        }

        private static void NotifyCheck(RepeatInterval interval)
        {
            Dictionary<ResponseNotification, bool> tempDict = NotificationState.Dict;
            List<KeyValuePair<ResponseNotification, bool>> tempNewDict = NotificationState.Dict.ToList().Where(x => x.Key.NotifyInterval == interval).ToList();
            MainActivity.LogDebug($"Notification check for the {System.Enum.GetName(typeof(RepeatInterval), tempNewDict[0].Key.NotifyInterval)} interval");
            // This code will executed at the specified interval
            foreach (KeyValuePair<ResponseNotification, bool> KeyVal in tempNewDict)
            {
                if (KeyVal.Value == true)
                {
                    MainActivity.LogDebug($"Found match for: {KeyVal.Key.Uid}");
                    MainActivity.CreateNotification($"{KeyVal.Key.Uid} | Notification", $"{KeyVal.Key.Key} is {KeyVal.Key.Val}");
                }
                if (KeyVal.Key.NotifyInterval == RepeatInterval.NoRepeat)
                {
                    tempDict[KeyVal.Key] = false;
                }
            }
            NotificationState.Dict = tempDict;
        }

        private static async void DoWork(RepeatInterval interval)
        {
            int TimerInterval = ((int)interval) * 60 * 1000;
            await Task.Delay(5000);
            if (MainActivity.validConnection)
            {
                MainActivity.LogDebug("Running background get request");
                string response = await MainActivity.apiController.GetRequest();
                MainActivity.LogDebug($"Got response {response}");
                if (!response.Contains("ERROR: "))
                {

                    Dictionary<string, string> values = JsonNet.Deserialize<Dictionary<string, string>>(response);
                    Dictionary<ResponseNotification, bool> tempOldDict = NotificationState.OldDict;
                    Dictionary<ResponseNotification, bool> tempDict = NotificationState.Dict;
                    List<KeyValuePair<ResponseNotification, bool>> tempNewDict = NotificationState.Dict.ToList();

                    foreach (KeyValuePair<ResponseNotification, bool> KeyVal in tempNewDict)
                    {
                        if (values.ContainsKey(KeyVal.Key.Key))
                        {
                            if (values[KeyVal.Key.Key].ToString() == KeyVal.Key.Val.ToString())
                            {
                                if (KeyVal.Key.NotifyInterval == RepeatInterval.NoRepeat)
                                {
                                    if (tempOldDict[KeyVal.Key] == false)
                                    {
                                        tempDict[KeyVal.Key] = true;
                                        tempOldDict[KeyVal.Key] = true;
                                    }
                                }
                                else
                                if (KeyVal.Key.NotifyInterval != RepeatInterval.NoRepeat)
                                {
                                    tempDict[KeyVal.Key] = true;
                                }
                            }
                            else
                            {
                                if (KeyVal.Key.NotifyInterval == RepeatInterval.NoRepeat)
                                {
                                    if (tempOldDict[KeyVal.Key] == true)
                                    {
                                        tempOldDict[KeyVal.Key] = false;
                                    }
                                }
                                else
                                if (KeyVal.Key.NotifyInterval != RepeatInterval.NoRepeat)
                                {
                                    tempDict[KeyVal.Key] = false;
                                }

                            }
                        }
                    }
                    NotificationState.OldDict = tempOldDict;
                    NotificationState.Dict = tempDict;

                    foreach (KeyValuePair<RepeatInterval, Tuple<List<ResponseNotification>, ResponseNotifications.TimeWrapper>> item in intervalGroups.ToList())
                    {
                        DateTime currentTime = DateTime.Now;
                        int intervalMS = ((int)item.Key) * 60 * 1000;

                        if ((currentTime - item.Value.Item2.Get()).Milliseconds >= intervalMS || item.Value.Item2.Get() == DateTime.MinValue)
                        {
                            NotifyCheck(item.Key);
                            intervalGroups[item.Key].Item2.SetNow();
                        }
                    }
                }
            }
            else
            {
                MainActivity.LogDebug("No valid connection, ServiceController retry in 15 minutes");
                TimerInterval = ((int)RepeatInterval.Every15Min) * 60 * 1000;
            }

            // Schedule the next execution
            handler.PostDelayed(CreateRunnable(interval), TimerInterval);
        }
    }

    [Service]
    public class NotifyRepeatService : Service
    {
        private bool isServiceRunning;
        private int intervalRepeat;

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!isServiceRunning)
            {
                isServiceRunning = true;
                intervalRepeat = intent.GetIntExtra("intervalRepeat", 0); // Default interval is 0 i.e. no repeat.
                MainActivity.LogDebug($"Initalised the {System.Enum.GetName(typeof(RepeatInterval), intervalRepeat)} service");
                intervalRepeat = intervalRepeat * 60 * 1000;
                Task.Run(() => ExecuteBackgroundTask());
            }

            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            isServiceRunning = false;
        }

        private async Task ExecuteBackgroundTask()
        {
            await Task.Delay(5000);
            while (isServiceRunning)
            {
                if (MainActivity.validConnection)
                {
                    Dictionary<ResponseNotification, bool> tempDict = NotificationState.Dict;
                    List<KeyValuePair<ResponseNotification, bool>> tempNewDict = NotificationState.Dict.ToList().Where(x => x.Key.RepeatMs == intervalRepeat).ToList();
                    MainActivity.LogDebug($"Notification check for the {System.Enum.GetName(typeof(RepeatInterval), tempNewDict[0].Key.NotifyInterval)} interval");
                    // This code will executed at the specified interval
                    foreach (KeyValuePair<ResponseNotification, bool> KeyVal in tempNewDict)
                    {
                        if (KeyVal.Value == true)
                        {
                            MainActivity.LogDebug($"Found match for: {KeyVal.Key.Uid}");
                            MainActivity.CreateNotification($"{KeyVal.Key.Uid} | Notification", $"{KeyVal.Key.Key} is {KeyVal.Key.Val}");
                        }
                        if (KeyVal.Key.NotifyInterval == RepeatInterval.NoRepeat)
                        {
                            tempDict[KeyVal.Key] = false;
                        }
                    }
                    NotificationState.Dict = tempDict;

                    if (intervalRepeat != 0)
                    {
                        await Task.Delay(intervalRepeat);
                    }
                }
                else
                {
                    MainActivity.LogDebug("No valid connection, NotifyRepeatService retry in 30 minutes");
                    await Task.Delay(((int)RepeatInterval.Every30Min) * 60 * 1000);
                }

            }
        }
    }
}