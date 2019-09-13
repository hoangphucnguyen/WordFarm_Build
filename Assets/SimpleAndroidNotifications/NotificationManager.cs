using System;
using System.Linq;
using UnityEngine;

namespace Assets.SimpleAndroidNotifications
{
    public static class NotificationManager
    {
        #if UNITY_ANDROID && !UNITY_EDITOR

        private const string FullClassName = "com.hippogames.simpleandroidnotifications.Controller";
        private const string MainActivityClassName = "com.unity3d.player.UnityPlayerNativeActivity";

        #endif

        /// <summary>
        /// Schedule simple notification without app icon.
        /// </summary>
        /// <param name="smallIcon">List of build-in small icons: notification_icon_bell (default), notification_icon_clock, notification_icon_heart, notification_icon_message, notification_icon_nut, notification_icon_star, notification_icon_warning.</param>
        public static int Send(TimeSpan delay, string title, string message, Color smallIconColor, NotificationIcon smallIcon = 0, bool silent = false)
        {
            return SendCustom(new NotificationParams
            {
                Id = NotificationIdHandler.GetNotificationId(),
                Delay = delay,
                Title = title,
                Message = message,
                Ticker = message,
                Sound = !silent,
                Vibrate = !silent,
                Light = true,
                SmallIcon = smallIcon,
                SmallIconColor = smallIconColor,
                LargeIcon = "",
                ExecuteMode = NotificationExecuteMode.Inexact
            });
        }

        /// <summary>
        /// Schedule notification with app icon.
        /// </summary>
        /// <param name="smallIcon">List of build-in small icons: notification_icon_bell (default), notification_icon_clock, notification_icon_heart, notification_icon_message, notification_icon_nut, notification_icon_star, notification_icon_warning.</param>
        public static int SendWithAppIcon(TimeSpan delay, string title, string message, Color smallIconColor, NotificationIcon smallIcon = 0, bool silent = false)
        {
            return SendCustom(new NotificationParams
            {
                Id = NotificationIdHandler.GetNotificationId(),
                Delay = delay,
                Title = title,
                Message = message,
                Ticker = message,
                Sound = !silent,
                Vibrate = !silent,
                Light = true,
                SmallIcon = smallIcon,
                SmallIconColor = smallIconColor,
                LargeIcon = "app_icon",
                ExecuteMode = NotificationExecuteMode.Inexact
            });
        }

        /// <summary>
        /// Schedule customizable notification.
        /// </summary>
        public static int SendCustom(NotificationParams notificationParams)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR

            var p = notificationParams;
            var delay = (long) p.Delay.TotalMilliseconds;
            var repeatInterval = p.Repeat ? (long) p.RepeatInterval.TotalMilliseconds : 0;
            var vibration = string.Join(",", p.Vibration.Select(i => i.ToString()).ToArray());

            new AndroidJavaClass(FullClassName).CallStatic("SetNotification", p.Id, delay, p.Repeat ? 1 : 0, repeatInterval, p.Title, p.Message, p.Ticker,
                p.Sound ? 1 : 0, p.Vibrate ? 1 : 0, vibration, p.Light ? 1 : 0, p.LightOnMs, p.LightOffMs, ColotToInt(p.LightColor), p.LargeIcon, GetSmallIconName(p.SmallIcon), ColotToInt(p.SmallIconColor), (int) p.ExecuteMode, p.CallbackData, MainActivityClassName);
            
            NotificationIdHandler.AddScheduledNotificaion(p.Id);

            #else

            Debug.LogWarning("Simple Android Notifications are not supported for current platform. Build and play this scene on android device!");

            #endif

            return notificationParams.Id;
        }

        /// <summary>
        /// Cancel notification by id.
        /// </summary>
        public static void Cancel(int id)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR

            new AndroidJavaClass(FullClassName).CallStatic("CancelNotificationEx", id);

            NotificationIdHandler.RemoveScheduledNotificaion(id);

            #endif
        }

        /// <summary>
        /// Cancel all notifications.
        /// </summary>
        public static void CancelAll()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR

            new AndroidJavaClass(FullClassName).CallStatic("CancelAllNotificationsEx");

            NotificationIdHandler.RemoveAllScheduledNotificaions();

            #endif
        }

        /// <summary>
        /// Return notification callback if app was launched from notification (and null otherwise).
        /// </summary>
        public static NotificationCallback GetNotificationCallback()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR

            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var intent = currentActivity.Call<AndroidJavaObject>("getIntent");
            var hasExtra = intent.Call<bool>("hasExtra", "Notification.Id");

            if (hasExtra)
            {
                var extras = intent.Call<AndroidJavaObject>("getExtras");

                return new NotificationCallback
                {
                    Id = extras.Call<int>("getInt", "Notification.Id"),
                    Data = extras.Call<string>("getString", "Notification.CallbackData")
                };
            }

            #endif

            return null;
        }

        private static int ColotToInt(Color color)
        {
            var smallIconColor = (Color32) color;
            
            return smallIconColor.r * 65536 + smallIconColor.g * 256 + smallIconColor.b;
        }

        private static string GetSmallIconName(NotificationIcon icon)
        {
            return "anp_" + icon.ToString().ToLower();
        }
    }
}