using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleAndroidNotifications
{
    public class NotificationExample : MonoBehaviour
    {
        public Toggle Toogle;

        public void Awake()
        {
            Toogle.isOn = NotificationManager.GetNotificationCallback() != null;
        }

        public void ScheduleSimple()
        {
            NotificationManager.Send(TimeSpan.FromSeconds(5), "Simple notification", "Customize icon and color", new Color(1, 0.3f, 0.15f));
        }

        /// <summary>
        /// Note: as of API 19, all repeating alarms are inexact. If your application needs precise delivery times then it must use one-time exact alarms, rescheduling each time as described above.
        /// </summary>
        public void ScheduleNormal()
        {
            NotificationManager.SendWithAppIcon(TimeSpan.FromSeconds(5), "Notification", "Notification with app icon", new Color(0, 0.6f, 1), NotificationIcon.Message);
        }

        public void ScheduleRepeated()
        {
            var notificationParams = new NotificationParams
            {
                Id = NotificationIdHandler.GetNotificationId(),
                Delay = TimeSpan.FromSeconds(5),
                Title = "Repeated notification",
                Message = "Message",
                Ticker = "Ticker",
                Sound = true,
                Vibrate = true,
                Vibration = new[] { 500, 500, 500, 500, 500, 500 },
                Light = true,
                LightOnMs = 1000,
                LightOffMs = 1000,
                LightColor = Color.magenta,
                SmallIcon = NotificationIcon.Skull,
                SmallIconColor = new Color(0, 0.5f, 0),
                LargeIcon = "app_icon",
                ExecuteMode = NotificationExecuteMode.Exact,
                Repeat = true,
                RepeatInterval = TimeSpan.FromSeconds(5) // Don't use short intervals as repeated notifications are inexact
            };

            NotificationManager.SendCustom(notificationParams);
        }

        public void ScheduleCustom()
        {
            // TODO: Please note, that receiving callback will not work if your app was sleeping. It will only work if app was opened (not resumed) by clicking the notification.

            var notificationParams = new NotificationParams
            {
                Id = NotificationIdHandler.GetNotificationId(),
                Delay = TimeSpan.FromSeconds(5),
                Title = "Custom notification",
                Message = "Message",
                Ticker = "Ticker",
                Sound = true,
                Vibrate = true,
                Vibration = new[] { 500, 500, 500, 500, 500, 500 },
                Light = true,
                LightOnMs = 1000,
                LightOffMs = 1000,
                LightColor = Color.red,
                SmallIcon = NotificationIcon.Biohazard,
                SmallIconColor = new Color(0, 0.5f, 0),
                LargeIcon = "app_icon",
                ExecuteMode = NotificationExecuteMode.Inexact,
                CallbackData = "notification created at " + DateTime.Now
            };

            NotificationManager.SendCustom(notificationParams);
        }

        public void CancelAll()
        {
            NotificationManager.CancelAll();
        }
    }
}