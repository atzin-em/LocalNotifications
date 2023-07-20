using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace LocalNotifications
{
    public delegate void NotificationDelegate();
    public class ResponseNotifications : List<ResponseNotification>
    {
        public event NotificationDelegate ItemDeleted;
        public event NotificationDelegate ItemChanged;

        protected virtual void OnItemChanged()
        {
            ItemChanged.Invoke();
        }
        protected virtual void OnItemDeleted()
        {
            ItemDeleted.Invoke();
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            OnItemDeleted();
        }

        public ResponseNotifications() { }
        public ResponseNotifications(List<ResponseNotification> baseList) { SetList(baseList); }
        public new void Add(string Key, string Val)
        {
            ResponseNotification tempResponse = dupeCheck(Key, Val);
            if (tempResponse != null)
            {
                tempResponse.PropertyChanged += NotificationResponse_PropertyChanged;
            }
            base.Add(tempResponse);
        }

        public List<object> GetList()
        {
            List<object> list = new List<object>();
            foreach (ResponseNotification item in this)
            {
                list.Add(item.Serialize());
            }
            return list;
        }

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

        public void SetList(List<ResponseNotification> inList)
        {
            base.Clear();
            foreach (ResponseNotification resNot in inList)
            {
                resNot.PropertyChanged += NotificationResponse_PropertyChanged;
                base.Add(resNot);
            }
        }

        private void NotificationResponse_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnItemChanged();
        }

        private ResponseNotification dupeCheck(string Key, string Val)
        {
            ResponseNotification responseNotification = new ResponseNotification(Key, Val);
            if (this.Exists(x => x.Id == responseNotification.Id))
            {
                int dupeCount = this.Where(x => x.Id == responseNotification.Id).Count();
                responseNotification.SetUid(responseNotification.Id + $" ({dupeCount})");
            }
            return responseNotification;
        }
    }

    public enum RepeatInterval
    {
        NoRepeat = 0,
        Every5Min = 5,
        Every15Min = 15,
        Every30Min = 30,
        Every1Hr = 60
    }
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

    public class ResponseNotification : RootResponseNotification
    {
        private string _Key;
        private string _Val;

        public new string Key
        {
            get { return _Key; }
        }
        public new string Val
        {
            get { return _Val; }
        }

        private string _Id;
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

        public new string Uid
        {
            get
            {
                if (_Uid == null)
                {
                    //_Uid = Id; // Not sure if necessary
                    return Id;
                }
                else
                {
                    return _Uid;
                }
            }
        }

        private string _DisplayName = null;

        public new string DisplayName
        {
            get
            {
                if (_DisplayName == null)
                {
                    return Uid;
                }
                else
                {
                    return _DisplayName;
                }
            }
        }

        public int RepeatMs
        {
            get { return (int)this.NotifyInterval * 60 * 1000; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string GetIntervalDisplay()
        {
            if (this.NotifyInterval == RepeatInterval.NoRepeat)
            {
                return "Does not repeat";
            }
            else
            if (this.NotifyInterval == RepeatInterval.Every5Min)
            {
                return "Every 5 Minutes";
            }
            else
            if (this.NotifyInterval == RepeatInterval.Every15Min)
            {
                return "Every 15 Minutes";
            }
            else
            if (this.NotifyInterval == RepeatInterval.Every30Min)
            {
                return "Every 30 Minutes";
            }
            else
            if (this.NotifyInterval == RepeatInterval.Every1Hr)
            {
                return "Every Hour";
            }
            else
            {
                return "ERROR";
            }
        }

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

        public RootResponseNotification Serialize()
        {
            return this;
        }

        public ResponseNotification() { }
        public ResponseNotification(string Key, string Val)
        {
            this._Key = Key;
            this._Val = Val;
            this.Id = $"{Key.Trim()} : {Val}";
        }

        public void SetInterval(RepeatInterval interval)
        {
            this.NotifyInterval = interval;
            OnPropertyChanged(nameof(NotifyInterval));
        }

        public void SetDisplayName(string Name)
        {
            this._DisplayName = Name;
            OnPropertyChanged(nameof(DisplayName));
        }

        public void SetNotificationTitle(string Title)
        {
            this.NotificationTitle = Title;
            OnPropertyChanged(nameof(NotificationTitle));
        }

        public void SetNotificationMessage(string Message)
        {
            this.NotificationMessage = Message;
            OnPropertyChanged(nameof(NotificationMessage));
        }

        public void SetUid(string Uid)
        {
            this._Uid = Uid;
            OnPropertyChanged(nameof(_Uid));
        }
    }
}