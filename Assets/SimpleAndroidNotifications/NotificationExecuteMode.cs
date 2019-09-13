namespace Assets.SimpleAndroidNotifications
{
    public enum NotificationExecuteMode
    {
        /// <summary>
        /// Schedule an inexact alarm.
        /// </summary>
        Inexact = 0,
        /// <summary>
        /// Schedule an alarm to be delivered precisely at the stated time. API 19 is required, otherwise inexact alarm will be sheduled.
        /// </summary>
        Exact = 1,
        /// <summary>
        /// Like Exact, but this alarm will be allowed to execute even when the system is in low-power idle modes. API 23 is required, otherwise inexact alarm will be sheduled.
        /// </summary>
        ExactAndAllowWhileIdle = 2
    }
}
