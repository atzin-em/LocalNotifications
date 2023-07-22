using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Android.Util;
using System.Text;
using static Android.Views.View;
using Xamarin.Essentials;
using Google.Android.Material.TextField;
using Android.Views.InputMethods;
using Google.Android.Material.Internal;
using Android.Text;
using TextInputEditText = Google.Android.Material.TextField.TextInputEditText;
using Android.Support.Design.Widget;
using static Android.Icu.Text.Transliterator;

namespace LocalNotifications.Resources
{
    public class ResponsesItemCustomAdapter : BaseAdapter<string>
    {

        public ResponseNotifications responseItems;
        private Context context;
        private PopupWindow popupWindow;
        private int itemIndex;
        Button buttonNotificationRepeat;
        List<Button> repeatButtons;

        public ResponsesItemCustomAdapter(Context context, ResponseNotifications item)
        {
            this.context = context;
            this.responseItems = item;
        }

        public override int Count => responseItems.Count;

        public override long GetItemId(int position)
        {
            return position;
        }

        public override string this[int position] => responseItems[position].Uid;

        public void Update()
        {
            NotifyDataSetChanged();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
            {
                LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                view = inflater.Inflate(Resource.Menu.list_view_responses, null);
            }

            //Set the text or customize the content of your list item
            TextView titleTextView = view.FindViewById<TextView>(Resource.Id.titleTextView);
            TextView subtitleLabel = view.FindViewById<TextView>(Resource.Id.subtitleLabel);

            TextInputEditText displaynameInput = view.FindViewById<TextInputEditText>(Resource.Id.displaynameInput);
            TextInputEditText notif_titleInput = view.FindViewById<TextInputEditText>(Resource.Id.notif_titleInput);
            TextInputEditText notif_messageInput = view.FindViewById<TextInputEditText>(Resource.Id.notif_messageInput);

            itemIndex = position;

            titleTextView.Text = responseItems[position].DisplayName;
            subtitleLabel.Text = responseItems[position].Uid;
            displaynameInput.Text = responseItems[position].DisplayName;
            notif_titleInput.Text = responseItems[position].NotificationTitle;
            notif_messageInput.Text = responseItems[position].NotificationMessage;

            Button deleteButton = view.FindViewById<Button>(Resource.Id.deleteButton);
            LinearLayout layoutParent = view.FindViewById<LinearLayout>(Resource.Id.contentLayout);

            displaynameInput.EditorAction += (sender, args) => {
                TextInputEditText senderObject = (TextInputEditText)sender;
                GetInputFields(view, position, senderObject);
            };

            notif_titleInput.EditorAction += (sender, args) =>
            {
                TextInputEditText senderObject = (TextInputEditText)sender;
                GetInputFields(view, position, senderObject);
            };

            notif_messageInput.EditorAction += (sender, args) =>
            {
                TextInputEditText senderObject = (TextInputEditText)sender;
                GetInputFields(view, position, senderObject);
            };


            if (view.HasOnClickListeners == false) {
                view.Click += (sender, e) =>
                {
                    layoutParent.Visibility = layoutParent.Visibility == ViewStates.Gone ? ViewStates.Visible : ViewStates.Gone;
                };               
            }

            deleteButton.Click += (sender, e) =>
            {
                Button deleteButton = (Button)sender;
                LinearLayout parentLayout = (LinearLayout)deleteButton.Parent;
                LinearLayout titleLayout = (LinearLayout)parentLayout.GetChildAt(0);
                TextView titleTextView = (TextView)titleLayout.GetChildAt(0);

                ResponseNotification temp = responseItems.Find(x => x.Id == titleTextView.Text || x.DisplayName == titleTextView.Text);
                if (temp != null)
                {
                    int itemPosition = responseItems.IndexOf(temp);
                    responseItems.RemoveAt(itemPosition);
                    NotifyDataSetChanged();
                }
            };

            buttonNotificationRepeat = view.FindViewById<Button>(Resource.Id.repeatButton);
            buttonNotificationRepeat.Text = responseItems[position].GetIntervalDisplay();
            buttonNotificationRepeat.Tag = responseItems[position].NotifyInterval.ToString();
            if (buttonNotificationRepeat.HasOnClickListeners == false)
            {
                buttonNotificationRepeat.Click += (sender, e) => {
                    ShowRepeatPopup((Google.Android.Material.Button.MaterialButton)sender, e, responseItems[position], position);
                };
            }
            return view;
        }

        private void GetInputFields(View view, int position, TextInputEditText sender)
        {
            TextInputEditText displaynameInput = view.FindViewById<TextInputEditText>(Resource.Id.displaynameInput);
            TextInputEditText notif_titleInput = view.FindViewById<TextInputEditText>(Resource.Id.notif_titleInput);
            TextInputEditText notif_messageInput = view.FindViewById<TextInputEditText>(Resource.Id.notif_messageInput);
            if (displaynameInput.Text != "")
            {
                responseItems[position].SetFields(displaynameInput.Text, notif_titleInput.Text, notif_messageInput.Text);
                //responseItems[position].SetDisplayName(displaynameInput.Text);
            }
            else
            {
                responseItems[position].SetFields(responseItems[position].Id, notif_titleInput.Text, notif_messageInput.Text);
                //responseItems[position].SetDisplayName(responseItems[position].Id);
            }
            sender.ClearFocus();
            MainActivity.DismissKeyboard(view);
        }

        private void ShowRepeatPopup(Google.Android.Material.Button.MaterialButton sender, EventArgs e, ResponseNotification response, int position)
        {
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            View popupView = inflater.Inflate(Resource.Menu.popup_menu_repeat, null);

            int anchorWidth = sender.MeasuredWidth;
            int anchorHeight = sender.MeasuredHeight;
            int dpi = (int)DeviceDisplay.MainDisplayInfo.Density;

            popupWindow = new PopupWindow(popupView, anchorWidth + (dpi * 5), anchorHeight * 6 , true);
            popupWindow.ShowAsDropDown(sender, -(dpi * 5 / 2), 0, GravityFlags.Left);

            Button btnNoRepeat = popupView.FindViewById<Button>(Resource.Id.btnNoRepeat);
            Button btnEvery5Min = popupView.FindViewById<Button>(Resource.Id.btnEvery5Min);
            Button btnEvery15Min = popupView.FindViewById<Button>(Resource.Id.btnEvery15Min);
            Button btnEvery30Min = popupView.FindViewById<Button>(Resource.Id.btnEvery30Min);
            Button btnEveryHour = popupView.FindViewById<Button>(Resource.Id.btnEveryHour);

            repeatButtons = new List<Button>() {
                btnNoRepeat, btnEvery5Min, btnEvery15Min, btnEvery30Min, btnEveryHour
            };

            foreach (Google.Android.Material.Button.MaterialButton repeatButton in repeatButtons)
            {
                int iconDrawable = Resource.Drawable.btn_radio_off_dark;
                if (repeatButton.Tag.ToString() == sender.Tag.ToString())
                {
                    iconDrawable = Resource.Drawable.btn_radio_on_dark;
                    ((Button)sender).Text = repeatButton.Text;
                    responseItems[itemIndex].SetInterval(response.NotifyInterval);
                }
                repeatButton.SetIconResource(iconDrawable);
            }

            foreach (Button repeatButton in repeatButtons)
            {
                if (repeatButton.HasOnClickListeners == false)
                {
                    repeatButton.Click += (childSender, e) => SelectRepeatOption(childSender, e, sender, position);
                }
            }
        }

        private void SelectRepeatOption(object sender, EventArgs e, object baseSender, int position)
        {
            Button senderParent = (Button)baseSender;
            Button senderButton = (Button)sender;

            foreach (Google.Android.Material.Button.MaterialButton repeatButton in repeatButtons)
            {
                int iconDrawable = Resource.Drawable.btn_radio_off_dark;
                if (repeatButton.Tag == senderButton.Tag)
                {
                    iconDrawable = Resource.Drawable.btn_radio_on_dark;
                    senderParent.Text = repeatButton.Text;
                    senderParent.Tag = repeatButton.Tag;
                    RepeatInterval repeatInterval = (RepeatInterval)Enum.Parse(typeof(RepeatInterval), repeatButton.Tag.ToString());
                    responseItems[position].SetInterval(repeatInterval);
                }
                repeatButton.SetIconResource(iconDrawable);
            }
        }
    }
}