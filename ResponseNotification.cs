using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace LocalNotifications
{
    // Delegate for handling notifications
    public delegate void NotificationDelegate();

    // List of custom ResponseNotification objects with additional events
    public class ResponseNotifications : List<ResponseNotification>
    {
        // Events for when an item is deleted or changed
        public event NotificationDelegate ItemDeleted;
        public event NotificationDelegate ItemChanged;

        protected virtual void OnItemChanged()
        {
            ItemChanged?.Invoke();
        }

        protected virtual void OnItemDeleted()
        {
            ItemDeleted?.Invoke();
        }

        // Overridden RemoveAt method with event handling for item deletion
        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            OnItemDeleted();
        }

        // Custom constructor for ResponseNotifications class
        public ResponseNotifications() { }

        // Custom constructor that sets the base list with provided data
        public ResponseNotifications(List<ResponseNotification> baseList) { SetList(baseList); }

        // Overridden Add method with event handling for item addition
        public new void Add(string Key, string Val)
        {
            // Check for duplicate entries and adjust Uid if needed
            ResponseNotification tempResponse = dupeCheck(Key, Val);
            if (tempResponse != null)
            {
                tempResponse.PropertyChanged += NotificationResponse_PropertyChanged;
            }
            base.Add(tempResponse);
        }

        // Serialize the list to a List<object> format
        public List<object> GetList()
        {
            List<object> list = new List<object>();
            foreach (ResponseNotification item in this)
            {
                list.Add(item.Serialize());
            }
            return list;
        }

        // Helper class to wrap DateTime values for notifications
        public class TimeWrapper
        {
            private DateTime dateTime;

            public TimeWrapper(DateTime dateTime)
            {
                this.dateTime = dateTime;
            }

            public TimeWrapper()
            {
                this.dateTime = DateTime.MinValue;
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

        // Group notifications based on their RepeatInterval property
        public Dictionary<RepeatInterval, Tuple<List<ResponseNotification>, TimeWrapper>> GroupByInterval()
        {
            Dictionary<RepeatInterval, Tuple<List<ResponseNotification>, TimeWrapper>> notificationDictionary = new Dictionary<RepeatInterval, Tuple<List<ResponseNotification>, TimeWrapper>>();

            foreach (ResponseNotification notification in this)
            {
                RepeatInterval interval = notification.NotifyInterval;

                if (!notificationDictionary.ContainsKey(interval))
                {
                    notificationDictionary[interval] = Tuple.Create(new List<ResponseNotification> { }, new TimeWrapper());
                }

                notificationDictionary[interval].Item1.Add(notification);
            }
            return notificationDictionary;
        }

        // Clear the list and set it with the provided data
        public void SetList(List<ResponseNotification> inList)
        {
            base.Clear();
            foreach (ResponseNotification resNot in inList)
            {
                resNot.PropertyChanged += NotificationResponse_PropertyChanged;
                base.Add(resNot);
            }
        }

        // Event handler for property changes in ResponseNotification objects
        private void NotificationResponse_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnItemChanged();
        }

        // Check for duplicate entries and adjust Uid if needed
        private ResponseNotification dupeCheck(string Key, string Val)
        {
            ResponseNotification responseNotification = new ResponseNotification(Key, Val);
            if (this.Exists(x => x.Id == responseNotification.Id))
            {
                int dupeCount = this.Count(x => x.Id == responseNotification.Id);
                responseNotification.SetUid(responseNotification.Id + $" ({dupeCount})");
            }
            return responseNotification;
        }
    }

    // Enumeration for repeat intervals of notifications
    public enum RepeatInterval
    {
        NoRepeat = 0,
        Every5Min = 5,
        Every15Min = 15,
        Every30Min = 30,
        Every1Hr = 60
    }

    // Base class for notification data
    public class RootResponseNotification
    {
        public string Key { get; set; }
        public string Val { get; set; }
        public string Id { get; set; }
        public string Uid { get; set; }
        public RepeatInterval NotifyInterval { get; set; } = RepeatInterval.NoRepeat;
        public string DisplayName { get; set; }
        public string NotificationTitle { get; set; } = "";
        public string NotificationMessage { get; set; } = "";
    }

    // Custom ResponseNotification class inheriting from RootResponseNotification
    public class ResponseNotification : RootResponseNotification
    {
        private string _Key;
        private string _Val;

        // Overridden Key and Val properties with private backing fields
        public new string Key
        {
            get { return _Key; }
        }

        public new string Val
        {
            get { return _Val; }
        }

        private string _Id;
        // Overridden Id property with custom logic for setting the value
        public new string Id
        {
            get { return _Id; }
            set
            {
                if (_Id == null)
                {
                    _Id = $"{Key.Trim()} : {Val}";
                }
                else
                {
                    _Id = value;
                }
            }
        }

        private string _Uid = null;

        // Overridden Uid property with custom logic for setting the value
        public new string Uid
        {
            get
            {
                if (_Uid == null)
                {
                    return Id;
                }
                else
                {
                    return _Uid;
                }
            }
        }

        private string _DisplayName = null;

        // Overridden DisplayName property with custom logic for setting the value
        public new string DisplayName
        {
            get
            {
                if (_DisplayName == null)
                {
                    return Id;
                }
                else
                {
                    return _DisplayName;
                }
            }
        }

        // Calculate the repeat interval in milliseconds for the notification
        public int RepeatMs
        {
            get { return (int)this.NotifyInterval * 60 * 1000; }
        }

        // Event handler for property changes in ResponseNotification objects
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Get the display string for the RepeatInterval property
        public string GetIntervalDisplay()
        {
            switch (this.NotifyInterval)
            {
                case RepeatInterval.NoRepeat:
                    return "Does not repeat";
                case RepeatInterval.Every5Min:
                    return "Every 5 Minutes";
                case RepeatInterval.Every15Min:
                    return "Every 15 Minutes";
                case RepeatInterval.Every30Min:
                    return "Every 30 Minutes";
                case RepeatInterval.Every1Hr:
                    return "Every Hour";
                default:
                    return "ERROR";
            }
        }

        // Consume data from the base class and set properties in the derived class
        public ResponseNotification ConsumeRoot(RootResponseNotification root)
        {
            Type parentType = typeof(RootResponseNotification);
            Type childType = typeof(ResponseNotification);

            // Get all public properties of the parent class
            PropertyInfo[] properties = parentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Copy the property values from the parent instance to the child instance
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(root);
                PropertyInfo childProperty = childType.GetProperty(property.Name);

                if (childProperty != null && childProperty.CanWrite)
                {
                    // The property has a public setter, so set the value directly
                    childProperty.SetValue(this, value);
                }
                else
                {
                    // The property doesn't have a public setter, so try to set the value through a private property
                    FieldInfo privateField = childType.GetField("_" + property.Name, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (privateField != null)
                    {
                        privateField.SetValue(this, value);
                    }
                }
            }
            return this;
        }

        // Serialize the ResponseNotification object to the base class format
        public RootResponseNotification Serialize()
        {
            return this;
        }

        // Default constructor
        public ResponseNotification() { }

        // Constructor with Key and Val parameters
        public ResponseNotification(string Key, string Val)
        {
            this._Key = Key;
            this._Val = Val;
            this.Id = $"{Key.Trim()} : {Val}";
        }

        // Set the NotifyInterval property and trigger property changed event
        public void SetInterval(RepeatInterval interval)
        {
            this.NotifyInterval = interval;
            OnPropertyChanged(nameof(NotifyInterval));
        }

        public void SetFields(string displayName = "", string notificationTitle = "", string notificationMessage = "")
        {
            if (displayName != "")
            {
                SetDisplayName(displayName);
            }
            if (notificationTitle != "")
            {
                SetNotificationTitle(notificationTitle);
            }
            if (notificationMessage != "")
            {
                SetNotificationMessage(notificationMessage);
            }
            OnPropertyChanged("NotifyFields");
        }

        // Set the DisplayName property and trigger property changed event
        private void SetDisplayName(string Name)
        {
            this._DisplayName = Name;
            //OnPropertyChanged(nameof(DisplayName));
        }

        // Set the NotificationTitle property and trigger property changed event
        private void SetNotificationTitle(string Title)
        {
            this.NotificationTitle = Title;
            //OnPropertyChanged(nameof(NotificationTitle));
        }

        // Set the NotificationMessage property and trigger property changed event
        private void SetNotificationMessage(string Message)
        {
            this.NotificationMessage = Message;
            //OnPropertyChanged(nameof(NotificationMessage));
        }

        // Set the Uid property and trigger property changed event
        public void SetUid(string Uid)
        {
            this._Uid = Uid;
            OnPropertyChanged(nameof(_Uid));
        }
    }
}
