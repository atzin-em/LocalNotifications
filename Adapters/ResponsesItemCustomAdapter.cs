using Android.Content;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;

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

            itemIndex = position;
            titleTextView.Text = responseItems[position].DisplayName;
            subtitleLabel.Text = responseItems[position].Uid;

            Button deleteButton = view.FindViewById<Button>(Resource.Id.deleteButton);
            LinearLayout layoutParent = view.FindViewById<LinearLayout>(Resource.Id.contentLayout);



            if (view.HasOnClickListeners == false)
            {
                view.Click += (sender, e) =>
                {
                    layoutParent.Visibility = layoutParent.Visibility == ViewStates.Gone ? ViewStates.Visible : ViewStates.Gone;
                };
            }

            deleteButton.Click += (sender, e) =>
            {
                ResponseNotification temp = responseItems.Find(x => x.Uid == ((TextView)((LinearLayout)((Button)sender).Parent).GetChildAt(0)).Text);
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
                buttonNotificationRepeat.Click += (sender, e) =>
                {
                    ShowRepeatPopup((Google.Android.Material.Button.MaterialButton)sender, e, responseItems[position], position);
                };
            }

            EditText displaynameInput = view.FindViewById<EditText>(Resource.Id.displaynameInput);
            EditText notif_titleInput = view.FindViewById<EditText>(Resource.Id.notif_titleInput);
            EditText notif_messageInput = view.FindViewById<EditText>(Resource.Id.notif_messageInput);


            return view;
        }

        private void ShowRepeatPopup(Google.Android.Material.Button.MaterialButton sender, EventArgs e, ResponseNotification response, int position)
        {

            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            View popupView = inflater.Inflate(Resource.Menu.popup_menu_repeat, null);

            popupWindow = new PopupWindow(popupView, ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, true);
            popupWindow.ShowAsDropDown((View)sender);

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
                    //buttonNotificationRepeat.Text = repeatButton.Text;
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