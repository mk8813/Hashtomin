using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace bgAlarmPlayer
{
    public sealed class BackgroundTask : IBackgroundTask
    {
      
        private BackgroundTaskCancellationReason cancelReason = BackgroundTaskCancellationReason.Abort;
        private volatile bool cancelRequested = false;
        ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                if (cancelRequested == false)
                {
                    //morning alarm
                    objAlarm morningAlarm = new objAlarm();
                    morningAlarm.Id = "MORNING";
                    morningAlarm.Name = "Morning Alarm";
                    morningAlarm.SingleFireTime = DateTimeOffset.Parse(DateTimeOffset.Now.ToString("MM/dd/yyyy") + " " + "08:00");//08:00

                    //night alarm
                    objAlarm nightAlarm = new objAlarm();
                    nightAlarm.Id = "NIGHT";
                    nightAlarm.Name = "Night Alarm";
                    nightAlarm.SingleFireTime = DateTimeOffset.Parse(DateTimeOffset.Now.ToString("MM/dd/yyyy") + " " + "20:00");//20:00

                    var TimeToPlay = AppSettings.Values["Time"].ToString();

                    if (DateTimeOffset.Now.TimeOfDay.Hours == 1)
                    {

                        switch (TimeToPlay)
                        {
                            case "0": // ساعت 8 و 20

                                AlarmHelper.ScheduleAlarm(morningAlarm);
                                AlarmHelper.ScheduleAlarm(nightAlarm);

                                break;

                            case "1": //ساعت 8 صبح
                                AlarmHelper.ScheduleAlarm(morningAlarm);
                                break;

                            case "2": //ساعت 8 شب
                                AlarmHelper.ScheduleAlarm(nightAlarm);
                                break;
                        }
                    }
                    else
                    {
                        switch (TimeToPlay)
                        {
                            case "0": // ساعت 8 و 20

                                AlarmHelper.EnsureScheduled(morningAlarm);
                                AlarmHelper.EnsureScheduled(nightAlarm);

                                break;

                            case "1": //ساعت 8 صبح
                                AlarmHelper.EnsureScheduled(morningAlarm);
                                break;

                            case "2": //ساعت 8 شب
                                AlarmHelper.EnsureScheduled(nightAlarm);
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

      
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            cancelRequested = true;
            cancelReason = reason;
            switch (reason)
            {

                case BackgroundTaskCancellationReason.IdleTask:
                    ShowNotification("برای جلوگیری از بسته شدن برنامه در پس زمینه، لطفا برنامه را باز و مجددا تغییرات را ثبت کنید");
                    break;
                case BackgroundTaskCancellationReason.SystemPolicy:
                    ShowNotification("برای جلوگیری از بسته شدن برنامه در پس زمینه، لطفا برنامه را باز و مجددا تغییرات را ثبت کنید");
                    break;
            }
           
        }

        private void ShowNotification(string text, bool isTemorary = false)
        {
            var xDoc = new XDocument(

new XElement("toast", new XAttribute("launch", "samplenotification"),

new XElement("visual",

new XElement("binding", new XAttribute("template", "ToastGeneric"),

new XElement("text", "هشتمین"),

new XElement("text", text)))));

            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();

            xmlDoc.LoadXml(xDoc.ToString());
            var toast = new ToastNotification(xmlDoc);
            if (isTemorary)
            {
                toast.ExpirationTime = DateTime.Now.AddSeconds(5);
            }
            var notifi = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            notifi.Show(toast);
        }


    }
}
