using System;
using UnityEngine;

namespace Assets.SimpleAndroidNotifications
{
    public partial class NotificationParams
    {
        /// <summary>
        /// Use random id for each new notification.
        /// </summary>
        public int Id;
        public TimeSpan Delay;
        public string Title;
        public string Message;
        public string Ticker;
        public bool Sound = true;
        public bool Vibrate = true;
        public int[] Vibration = { 1000, 1000 };
        public bool Light = true;
        public int LightOnMs = 3000;
        public int LightOffMs = 3000;
        public Color LightColor = Color.green;
        public NotificationIcon SmallIcon;
        public Color SmallIconColor;
        /// <summary>
        /// Use "" for simple notification. Use "app_icon" to use the app icon. Use custom value but first place image to "simple-android-notifications.aar/res/". To modify "aar" file just rename it to "zip" and back.
        /// </summary>
        public string LargeIcon;
        public NotificationExecuteMode ExecuteMode;
        public string CallbackData;
        public bool Repeat;
        public TimeSpan RepeatInterval;
    }
}