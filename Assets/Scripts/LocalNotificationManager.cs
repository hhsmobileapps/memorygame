using System;
using System.Text;
using UnityEngine;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using System.Collections.Generic;
using Unity.Notifications.iOS;
using LocalNotification = UnityEngine.iOS.LocalNotification;
using System.Collections;
using UnityEngine.iOS;
#endif


public class LocalNotificationManager : MonoBehaviour
{
    public static LocalNotificationManager instance;

    private const string ChannelId = "freddyMemoryChannel";
    private const int NormalPuzzleNotificationId = 1111;

    [HideInInspector] public bool didComeFromNormalPuzzleNotification;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        try
        {
            RescheduleAllLocalNotifications();
        }
        catch (Exception e)
        {
            print("Bildirim kurulamadı: " + e.Message);
        }
    }


    private void OnApplicationPause(bool pauseStatus)
    {
        try
        {
            RescheduleAllLocalNotifications();
        }
        catch (Exception e)
        {
            print("Bildirim kurulamadı: " + e.Message);
        }
    }


    /*private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            CheckLastNotificationIntent();
        }
    }*/


    #region Initialization

    private void Initialize()
    {
#if UNITY_ANDROID 
        var c = new AndroidNotificationChannel()
        {
            Id = ChannelId,
            Name = "Freddy Memory Game",
            Importance = Importance.Default,
            Description = "Generic notifications."
        };
        AndroidNotificationCenter.RegisterNotificationChannel(c);
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
#endif

        CheckLastNotificationIntent();
        //FIXME: Dismiss for iOS displayed notifications
    }


    private void CheckLastNotificationIntent()
    {
#if UNITY_ANDROID
        AndroidNotificationIntentData intent = null;

        try
        {
            intent = AndroidNotificationCenter.GetLastNotificationIntent();
        }
        catch (Exception e)
        {
            print("EXCEPTION AndroidNotificationCenter" + " is null: " + e.Message);
        }

        if (intent == null)
        {
            return;
        }
        else
        {

        }

#elif UNITY_IOS


        var notif = iOSNotificationCenter.GetLastRespondedNotification();
       
        if(notif == null)
        {
            return;
        }
        else
        {
            //this.GetComponent<MenuController>().OnPlayClicked();
        }

#endif
    }

    #endregion


    #region Schedulers

    private void RescheduleAllLocalNotifications()
    {
        CancelNormalPuzzleNotifications();

        SetNormalPuzzleNotifications();
    }


    private void SetNormalPuzzleNotifications()
    {





        var title = "Hi! Where have you been?";
        var body = "Improve your memory further with Freddy's Memory Game";

        for (int i = 1; i < 8; i++)
        {

            var targetDate = DateTime.Now.AddDays(i);

            ScheduleNotification(title, body, targetDate, NormalPuzzleNotificationId);
        }

    }


    private void ScheduleNotification(string title, string body, DateTime deliveryTime, int notificationId)
    {
#if UNITY_ANDROID
        var notification = new AndroidNotification
        {
            Title = title,
            Text = body,
            FireTime = deliveryTime,
            SmallIcon = "icon_small",
            LargeIcon = "icon_large"
        };
        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, ChannelId, notificationId);

        //Printer.Print("NOTIFICATION KURULDU " + notificationId);

#elif UNITY_IOS
        UnityEngine.iOS.LocalNotification notification = new UnityEngine.iOS.LocalNotification
        {
            alertTitle = title, 
            alertBody = body, 
            fireDate = deliveryTime,
            userInfo = new Dictionary<string, int> {{"id", notificationId}} 
        };
        UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notification);
#endif
    }

    #endregion


    #region Cancel notifications

    private void CancelNormalPuzzleNotifications()
    {
#if UNITY_ANDROID
        for (int i = 1; i <= 7; i++)
        {
            AndroidNotificationCenter.CancelNotification(NormalPuzzleNotificationId + i);
        }
#elif UNITY_IOS
        LocalNotification[] list = UnityEngine.iOS.NotificationServices.scheduledLocalNotifications;

        if (list == null)
        {
            return;
        }
        
        foreach (var item in list)
        {
            if (item.userInfo.Contains("id") && DoesBelongToNormalNotificationIds(Convert.ToInt32(item.userInfo["id"])))
            {
                UnityEngine.iOS.NotificationServices.CancelLocalNotification(item);
            }
        }
#endif
    }



    #endregion


    #region Utility


    private static bool DoesBelongToNormalNotificationIds(int id)
    {
        for (int i = 1; i <= 7; i++)
        {
            if (id == NormalPuzzleNotificationId + i)
            {
                return true;
            }
        }

        return false;
    }


    private static string GenerateNotificationWord(string baseString, int dayNumber)
    {
        if (baseString == null)
        {
            return null;
        }

        char[] baseCharArray = baseString.ToCharArray();
        int length = baseString.Length;
        int emptyCharCount = length - (length / 2) - 1;

        if (dayNumber == 0)
        {
            switch (length)
            {
                case 2:
                    return baseCharArray[0] + "_";
                case 3:
                    return baseCharArray[0] + "_" + baseCharArray[2];
                case 4:
                    return baseCharArray[0] + "_" + baseCharArray[2] + baseCharArray[3];
            }
        }

        if (dayNumber >= emptyCharCount)
        {
            return baseString;
        }

        StringBuilder stringBuilder = new StringBuilder(baseCharArray[0] + "_" + baseCharArray[2]);
        bool isOpenAnymore = false;

        for (int i = 3; i < length - 1; i++)
        {
            if (dayNumber == 0)
            {
                stringBuilder.Append((i % 2 == 0) ? baseCharArray[i].ToString() : "_");
            }
            else
            {
                if (!isOpenAnymore)
                {
                    isOpenAnymore = ((length - i) / 2) == dayNumber;
                }

                stringBuilder.Append((i % 2 == 0 || isOpenAnymore) ? baseCharArray[i].ToString() : "_");
            }
        }

        stringBuilder.Append(baseCharArray[length - 1]);
        return stringBuilder.ToString();
    }

    #endregion


}

