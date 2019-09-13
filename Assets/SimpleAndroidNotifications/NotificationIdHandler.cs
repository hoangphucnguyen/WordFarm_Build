using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.SimpleAndroidNotifications
{
    public static class NotificationIdHandler
    {
        private const string PlayerPrefsKey = "NotificationHelper.Scheduled";

        public static List<int> GetScheduledNotificaions()
        {
            return PlayerPrefs.HasKey(PlayerPrefsKey)
                ? PlayerPrefs.GetString(PlayerPrefsKey).Split('|').Where(i => i != "").Select(i => int.Parse(i)).ToList()
                : new List<int>();
        }

        public static void SetScheduledNotificaions(List<int> scheduledNotificaions)
        {
            PlayerPrefs.SetString(PlayerPrefsKey, string.Join("|", scheduledNotificaions.Select(i => i.ToString()).ToArray()));
        }

        public static void AddScheduledNotificaion(int notificationId)
        {
            var scheduledNotificaions = GetScheduledNotificaions();

            scheduledNotificaions.Add(notificationId);
            SetScheduledNotificaions(scheduledNotificaions);
        }

        public static void RemoveScheduledNotificaion(int id)
        {
            var scheduledNotificaions = GetScheduledNotificaions();

            scheduledNotificaions.RemoveAll(i => i == id);
            SetScheduledNotificaions(scheduledNotificaions);
        }

        public static void RemoveAllScheduledNotificaions()
        {
            SetScheduledNotificaions(new List<int>());
        }

        public static int GetNotificationId()
        {
            var scheduled = GetScheduledNotificaions();

            while (true)
            {
                var id = Random.Range(0, int.MaxValue);

                if (!scheduled.Contains(id))
                {
                    return id;
                }
            }
        }
    }
}