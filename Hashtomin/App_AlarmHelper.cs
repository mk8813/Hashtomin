using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Hashtomin
{
    public static  class AlarmHelper
    {
       
        private const int DAYS_IN_ADVANCE_TO_SCHEDULE = 5;

        public static void ScheduleAlarm(objAlarm alarm)
        {
            EnsureScheduled(alarm, checkForExisting: false);
        }

        public static void RemoveAlarms()
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            //var tag = alarm.Id;// GetTag(alarm);

            // Find all of the scheduled toasts for the alarm
            var scheduledNotifs = notifier.GetScheduledToastNotifications()
                /*.Where(i => i.Id.Equals(tag))*/;

            // Remove all of those from the schedule
            foreach (var n in scheduledNotifs)
            {
                notifier.RemoveFromSchedule(n);
            }
        }

        public static void EnsureScheduled(objAlarm alarm)
        {
            EnsureScheduled(alarm, checkForExisting: true);
        }

        private static void EnsureScheduled(objAlarm alarm, bool checkForExisting)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            IReadOnlyList<ScheduledToastNotification> existing = null;
            if (checkForExisting)
            {
                var tag = GetTag(alarm);
                existing = notifier.GetScheduledToastNotifications()
                    .Where(i => i.Tag.Equals(tag))
                    .ToList();
            }

            DateTimeOffset now = DateTimeOffset.Now;

            DateTimeOffset[] alarmTimes = GetAlarmTimesForAlarm(alarm);

            foreach (var time in alarmTimes)
            {
                if (time.AddSeconds(5) > now)
                {
                    // If the alarm isn't scheduled already
                    if (!checkForExisting || !existing.Any(i => i.DeliveryTime == time))
                    {
                        var scheduledNotif = GenerateAlarmNotification(alarm, time);
                        notifier.AddToSchedule(scheduledNotif);
                    }
                }
            }
        }

        private static string GetTag(objAlarm alarm)
        {
            // Tag needs to be 16 chars or less, so hash the Id
            return alarm.Id.GetHashCode().ToString();
        }

        private static ScheduledToastNotification GenerateAlarmNotification(objAlarm alarm, DateTimeOffset alarmTime)
        {
            // Using NuGet package Microsoft.Toolkit.Uwp.Notifications


            ToastContent content = new ToastContent()
            {
                Scenario = ToastScenario.Alarm,

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            //  new AdaptiveText()
                            //{
                            //    Text = " أَلسَّلٰامُ عَلَیکَ یٰا عَلی اِبنِ موسَی أَلرّضٰآ (ع)"
                            //},
                            new  AdaptiveText()
                            {
                                HintMaxLines=5,
                                HintAlign=AdaptiveTextAlign.Right,
                                HintStyle=AdaptiveTextStyle.Base,
                               
                                Language="fa-IR",
                                Text ="اَللّهُمَ صَلِّ عَلي علي بن مُوسَي الِّرِضا المَرُتَضي اَلاِمامِ التَّقيِّ النَّقيِّ وَ حُجَتِكَ عَلي مَن فَوقَ الاَرضِ و مَن تَحت الثَّري اَلصِدّيقِ الشَّهيدِ صَلاةً كَثيرَةً تآمَّةً زاكِيَةً مُتَواصِلَةً مِتَواتِرَةً مُتَرادِفَة كَاَفضَلِ ما صَلَّيتَ عَلي اَحَدٍ مِن اوليائِكَ."
                            }


                        }
                    }
                },

                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButtonDismiss() 
                    }
        
                },

                Audio = new ToastAudio()
                {
                    Src = new Uri("ms-appx:///Assets/sounds/" + GetCurrentNava().ToString()+".mp3")
                    
                }

            };

            
            // We can easily enable Universal Dismiss by generating a RemoteId for the alarm that will be
            // the same on both devices. We'll just use the alarm delivery time. If an alarm on one device
            // has the same delivery time as an alarm on another device, it'll be dismissed when one of the
            // alarms is dismissed.
          
           // string remoteId = (alarmTime.Ticks / 10000000 / 60).ToString(); // Minutes

            return new ScheduledToastNotification(content.GetXml(), alarmTime)
            {
                Tag = GetTag(alarm),

                // RemoteId is a 1607 feature, if you support older systems, use ApiInformation to check if property is present
              //  RemoteId = remoteId
            };
        }

        private static DateTimeOffset[] GetAlarmTimesForAlarm(objAlarm alarm)
        {
            if (alarm.IsOneTime())
            {
                return new DateTimeOffset[] { alarm.SingleFireTime };
                
            }
            return null;
            //else
            //{
            //    DateTimeOffset today = DateTimeOffset.Now.Date;
            //    List<DateTime> answer = new List<DateTime>();
            //    for (int i = 0; i < DAYS_IN_ADVANCE_TO_SCHEDULE; i++)
            //    {
            //        if (alarm.DaysOfWeek.Contains(today.DayOfWeek))
            //        {
            //            answer.Add(today.Add(alarm.TimeOfDay));
            //        }

            //        today = today.AddDays(1);
            //    }

            //    return answer.ToArray();
            //}
        }

       enum  Nava  {
           Hajreza_Ansarian=1 ,
            Esfahani=2,
            Karimi=3,
            Motiei=4,
            Atapoor=5,
            Tavashih=6

        };

        static Nava GetCurrentNava()
        {
            try
            {
                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                var nava = AppSettings.Values["Nava"].ToString();
                if (nava != null)
                {
                    switch (int.Parse(nava))
                    {
                        case 0:
                            return Nava.Hajreza_Ansarian;


                        case 1:
                            return Nava.Esfahani;

                        case 2:
                            return Nava.Karimi;

                        case 3:
                            return Nava.Motiei;

                        case 4:
                            return Nava.Atapoor;

                        case 5:
                            return Nava.Tavashih;

                        default:
                            return Nava.Hajreza_Ansarian;//default


                    }
                }
                else
                {
                    return Nava.Hajreza_Ansarian;//default

                }

            }
            catch (Exception)
            {

                return Nava.Hajreza_Ansarian;//default
            }
            
        }
    }

}
