using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Messaging;

public class UserNotificationsChangedMessage : BaseMessage
{
    public readonly bool Enabled;

    public UserNotificationsChangedMessage(bool enabled) {
        Enabled = enabled;
    }
}
