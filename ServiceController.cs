using Android.App;
using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;
using Json.Net;
using Platform = Xamarin.Essentials.Platform;
using Java.Lang;
using System.Threading.Tasks;
using System.Linq;
using AndroidX.Core.App;

namespace LocalNotifications
{
    // Static class to store the state of notifications
    public static class NotificationState
    {
        public static Dictionary<ResponseNotification, bool> OldDict = new Dictionary<ResponseNotification, bool>();
        public static Dictionary<ResponseNotification, bool> Dict = new Dictionary<ResponseNotification, bool>();
    }

    // Helper class to wrap DateTime values for notifications
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

    // ServiceController class responsible for managing background notifications
    [Service]
    public class ServiceController : Service
    {
        private static readonly string CHANNEL_ID = "LocNetNot";
        private static NotificationManager notificationManager;
        private static Handler handler;
        private static Action runnable;
        private bool isServiceRunning;
        private static Dictionary<RepeatInterval, Tuple<List<ResponseNotification>, ResponseNotifications.TimeWrapper>> intervalGroups;
        private List<Intent> notifyServices;

        // OnCreate method executed when the service is created
        public override void OnCreate()
        {
            base.OnCreate();
            CreateNotificationChannel();
            handler = new Handler(Looper.MainLooper);
            isServiceRunning = false;
        }

        // Create a runnable with the specified RepeatInterval parameter
        private static IRunnable CreateRunnable(RepeatInterval paramStr)
        {
            IRunnable aRunnable = new RunnableParam(paramStr);
            return aRunnable;
        }

        // Inner class to hold the RepeatInterval parameter for the runnable
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

        // OnStartCommand method executed when the service is started
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!isServiceRunning)
            {
                // Initialize the dictionary for notification state
                foreach (ResponseNotification response in MainActivity.ResponseNotifications)
                {
                    NotificationState.Dict[response] = false;
                    NotificationState.OldDict[response] = false;
                }

                isServiceRunning = true;

                // Group response notifications based on the repeat interval
                intervalGroups = MainActivity.ResponseNotifications.GroupByInterval();
                notifyServices = new List<Intent>();

                // Find the minimum non-zero repeat interval
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
            //handler.RemoveCallbacks(this.);
            isServiceRunning = false;
        }

        // Create a notification with the specified title and message
        private static void CreateNotification(string Title, string Message)
        {
            // Instantiate the builder and set notification elements, including
            // the pending intent:
            NotificationCompat.Builder builder = new NotificationCompat.Builder(Platform.CurrentActivity, CHANNEL_ID)
                .SetContentTitle(Title)
                .SetContentText(Message)
                .SetSmallIcon(Resource.Drawable.ic_notification_alert);

            // Build the notification:
            Notification notification = builder.Build();

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);
        }

        // Create a notification channel for Android Oreo and above
        private void CreateNotificationChannel()
        {
            var name = Resources.GetString(Resource.String.channel_name);
            var description = GetString(Resource.String.channel_description);
            var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Default)
            {
                Description = description
            };

            notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        // Check and notify for the specified repeat interval
        private static void NotifyCheck(RepeatInterval interval)
        {
            Dictionary<ResponseNotification, bool> tempDict = NotificationState.Dict;
            List<KeyValuePair<ResponseNotification, bool>> tempNewDict = NotificationState.Dict.ToList().Where(x => x.Key.NotifyInterval == interval).ToList();
            MainActivity.LogDebug($"Notification check for the {System.Enum.GetName(typeof(RepeatInterval), tempNewDict[0].Key.NotifyInterval)} interval");
            // This code will be executed at the specified interval
            foreach (KeyValuePair<ResponseNotification, bool> KeyVal in tempNewDict)
            {
                if (KeyVal.Value == true)
                {
                    MainActivity.LogDebug($"Found match for: {KeyVal.Key.Uid}");
                    if (KeyVal.Key.NotificationTitle != "" || KeyVal.Key.NotificationMessage != "")
                    {
                        CreateNotification(KeyVal.Key.NotificationTitle, KeyVal.Key.NotificationMessage);
                    } else
                    {
                        CreateNotification($"{KeyVal.Key.Uid} | Notification", $"{KeyVal.Key.Key} is {KeyVal.Key.Val}");
                    }
                    
                }
                if (KeyVal.Key.NotifyInterval == RepeatInterval.NoRepeat)
                {
                    tempDict[KeyVal.Key] = false;
                }
            }
            NotificationState.Dict = tempDict;
        }

        // Perform background work for the specified repeat interval
        // This method is responsible for performing background work based on the specified repeat interval.
        // The `interval` parameter determines the current repeat interval.
        private static async void DoWork(RepeatInterval interval)
        {
            // Calculate the timer interval in milliseconds based on the provided repeat `interval`.
            int TimerInterval = ((int)interval) * 60 * 1000;

            // Delay the initial execution of the method by 5000 milliseconds (5 seconds).
            // This is to provide a short delay before starting the background work.
            await Task.Delay(5000);

            // Check if there is a valid internet connection (MainActivity.validConnection) to proceed with the background task.
            if (MainActivity.ValidConnection)
            {
                // Log a debug message to indicate that the background get request is running.
                MainActivity.LogDebug("Running background get request");

                // Perform an HTTP GET request using the `apiController.GetRequest()` method.
                // This method retrieves data from an API endpoint.
                string response = await MainActivity.apiController.GetRequest();

                // Log the response received from the API to aid in debugging.
                MainActivity.LogDebug($"Got response {response}");

                // Check if the response contains the string "ERROR: ", which would indicate an error occurred during the API request.
                if (!response.Contains("ERROR: "))
                {
                    // Deserialize the JSON response (a dictionary of key-value pairs) into a `Dictionary<string, string>` object.
                    Dictionary<string, string> values = JsonNet.Deserialize<Dictionary<string, string>>(response);

                    // Create temporary dictionaries to store the old and current notification states.
                    Dictionary<ResponseNotification, bool> tempOldDict = NotificationState.OldDict;
                    Dictionary<ResponseNotification, bool> tempDict = NotificationState.Dict;

                    // Create a temporary list of key-value pairs representing the current notification state.
                    List<KeyValuePair<ResponseNotification, bool>> tempNewDict = NotificationState.Dict.ToList();

                    // Iterate through the list of key-value pairs representing the current notification state.
                    foreach (KeyValuePair<ResponseNotification, bool> KeyVal in tempNewDict)
                    {
                        // Check if the key (notification identifier) exists in the received API response.
                        if (values.ContainsKey(KeyVal.Key.Key))
                        {
                            // Compare the value received from the API with the expected value for the notification.
                            if (values[KeyVal.Key.Key].ToString() == KeyVal.Key.Val.ToString())
                            {
                                // If the received value matches the expected value:

                                // Check if the notification has a repeat interval of `NoRepeat`.
                                if (KeyVal.Key.NotifyInterval == RepeatInterval.NoRepeat)
                                {
                                    // If the notification has `NoRepeat` interval and it was not previously notified (tempOldDict == false),
                                    // update the temporary current notification state (tempDict) to indicate that the notification should be shown.
                                    // Also, update the temporary old notification state (tempOldDict) to indicate that this notification has been notified.
                                    if (tempOldDict[KeyVal.Key] == false)
                                    {
                                        tempDict[KeyVal.Key] = true;
                                        tempOldDict[KeyVal.Key] = true;
                                    }
                                }
                                else if (KeyVal.Key.NotifyInterval != RepeatInterval.NoRepeat)
                                {
                                    // If the notification has a repeat interval other than `NoRepeat`, update the temporary current notification state (tempDict)
                                    // to indicate that the notification should be shown (regardless of whether it was previously notified).
                                    tempDict[KeyVal.Key] = true;
                                }
                            }
                            else
                            {
                                // If the received value does not match the expected value:

                                // Check if the notification has a repeat interval of `NoRepeat`.
                                if (KeyVal.Key.NotifyInterval == RepeatInterval.NoRepeat)
                                {
                                    // If the notification has `NoRepeat` interval and it was previously notified (tempOldDict == true),
                                    // update the temporary old notification state (tempOldDict) to indicate that this notification has not been notified anymore.
                                    // This allows `NoRepeat` notifications to be shown again if the value changes back to the expected value.
                                    if (tempOldDict[KeyVal.Key] == true)
                                    {
                                        tempOldDict[KeyVal.Key] = false;
                                    }
                                }
                                else if (KeyVal.Key.NotifyInterval != RepeatInterval.NoRepeat)
                                {
                                    // If the notification has a repeat interval other than `NoRepeat`, update the temporary current notification state (tempDict)
                                    // to indicate that the notification should not be shown (regardless of whether it was previously notified).
                                    tempDict[KeyVal.Key] = false;
                                }
                            }
                        }
                    }

                    // Update the global static dictionaries (NotificationState.OldDict and NotificationState.Dict) with the temporary dictionaries.
                    NotificationState.OldDict = tempOldDict;
                    NotificationState.Dict = tempDict;

                    // Iterate through the dictionary of interval groups and check if the notifications need to be shown for the respective intervals.
                    foreach (KeyValuePair<RepeatInterval, Tuple<List<ResponseNotification>, ResponseNotifications.TimeWrapper>> item in intervalGroups.ToList())
                    {
                        // Get the current time.
                        DateTime currentTime = DateTime.Now;

                        // Calculate the interval duration in milliseconds based on the current repeat interval.
                        int intervalMS = ((int)item.Key) * 60 * 1000;

                        // Calculate the time passed since the last notification check for the current repeat interval.
                        double timepassedMS = (currentTime - item.Value.Item2.Get()).TotalMilliseconds;

                        // Get the minimum date time from the interval group. If it is DateTime.MinValue, it means the group has not been checked before.
                        DateTime minDateTime = item.Value.Item2.Get();

                        // Check if the time passed since the last notification check exceeds the interval duration or if the group has not been checked before.
                        if (timepassedMS >= intervalMS || minDateTime == DateTime.MinValue)
                        {
                            // Perform the notification check for the current interval and update the time for the next check.
                            NotifyCheck(item.Key);
                            intervalGroups[item.Key].Item2.SetNow();
                        }
                    }
                }
            }
            else
            {
                // If there is no valid internet connection, log a debug message indicating the issue.
                // Set the timer interval to 15 minutes (Every15Min) for the next execution attempt.
                MainActivity.LogDebug("No valid connection, ServiceController retry in 15 minutes");
                TimerInterval = ((int)RepeatInterval.Every15Min) * 60 * 1000;
            }

            // Schedule the next execution of the method using the `handler.PostDelayed` method.
            // The `CreateRunnable` method returns a `Runnable` with the specified repeat `interval` parameter.
            // The `TimerInterval` parameter determines the delay until the next execution of the method in milliseconds.
            handler.PostDelayed(CreateRunnable(interval), TimerInterval);
        }

    }
}
