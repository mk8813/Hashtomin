using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Hashtomin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            FillComboBox();
            toggleActivate_Toggled(null, null);
            
        }

        private class clsNava
        {
            string nava;

            public string Nava
            {
                get
                {
                    return nava;
                }

                set
                {
                    nava = value;
                }
            }
        }
        void FillComboBox()
        {
            try
            {
                var nava = new[] {new { Nava = "حاج رضا انصاریان (پیش فرض)" , Audio="Hajreza_Ansarian.mp3"} ,
                    new { Nava = "محمد اصفهانی", Audio="Esfahani.mp3" }, new { Nava = "محمود کریمی", Audio="Karimi.mp3" }
                , new { Nava = "میثم مطیعی" , Audio="Motiei.mp3" }
                , new { Nava = "عطاپور", Audio="Atapoor.mp3" }
                , new { Nava = "تواشیح (جمع خوانی)", Audio="Tavashih.mp3" }};

                comboSelectSalavat.ItemsSource= nava.ToArray();
            }
            catch (Exception)
            {

               
            }
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppSettings.Values["Nava"] != null)
                {
                    comboSelectSalavat.SelectedIndex = int.Parse(AppSettings.Values["Nava"].ToString());
                }
                else
                {
                    comboSelectSalavat.SelectedIndex = 0;
                }
                if (AppSettings.Values["Time"] != null)
                {
                    comboSelectTime.SelectedIndex = int.Parse(AppSettings.Values["Time"].ToString());
                }
                else
                {
                    comboSelectTime.SelectedIndex = 0;
                }
                if (AppSettings.Values["bgTask"] != null)
                {
                    if (AppSettings.Values["bgTask"].ToString() == "1")
                    {
                        var isAlreadyRegistered = BackgroundTaskRegistration.AllTasks.Any(t => t.Value?.Name == "bgAlarm");
                        if (isAlreadyRegistered)
                        {

                            toggleActivate.IsOn = true;


                        }
                        else
                        {
                            MessageDialog msg = new MessageDialog("برنامه در پس زمینه فعال نیست، لطفا آن را مجددا فعال کنید.");
                            await msg.ShowAsync();
                        }

                    }
                    else
                    {
                        toggleActivate.IsOn = false;
                    }
                }
                else
                {
                    toggleActivate.IsOn = false;
                }
            }
            catch (Exception)
            {


            }
        }

        private void toggleActivate_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                comboSelectTime.IsEnabled = comboSelectSalavat.IsEnabled = toggleActivate.IsOn;

                if (toggleActivate.IsOn)
                {
                    lblSelectSalavat.Foreground = lblPlayTime.Foreground = new SolidColorBrush(Colors.DarkGoldenrod);
                }
                else
                {
                    lblSelectSalavat.Foreground = lblPlayTime.Foreground = new SolidColorBrush(Colors.Gray);
                }
            }
            catch (Exception)
            {

            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboSelectSalavat.SelectedIndex<0 || comboSelectTime.SelectedIndex<0)
                {
                    MessageDialog msg = new MessageDialog("نوای موردنظر یا زمان پخش انتخاب نشده است.");
                    await msg.ShowAsync();
                    return;
                    
                }

                if (toggleActivate.IsOn)
                {

                    ///////////////////////////begin task check
                    var isAlreadyRegistered = BackgroundTaskRegistration.AllTasks.Any(t => t.Value?.Name == "bgAlarm");
                    if (isAlreadyRegistered)
                    {
                        foreach (var tsk in BackgroundTaskRegistration.AllTasks)
                        {
                            if (tsk.Value.Name == "bgAlarm")
                            {
                                tsk.Value.Unregister(true);
                                break;
                            }
                        }
                    }


                    BackgroundExecutionManager.RemoveAccess();

                    var hasAccess = await BackgroundExecutionManager.RequestAccessAsync();//important for alarms

                    if (hasAccess == BackgroundAccessStatus.DeniedBySystemPolicy || hasAccess == BackgroundAccessStatus.DeniedByUser || hasAccess == BackgroundAccessStatus.Unspecified)
                    {
                        ShowNotification("دسترسی های لازم برای اجرای برنامه در پس زمینه وجود ندارد");
                        return;
                    }

                    /////////////////////begin registering Url listener ttask
                    var task = new BackgroundTaskBuilder
                    {
                        Name = "bgAlarm",
                        TaskEntryPoint = typeof(bgAlarmPlayer.BackgroundTask).ToString()
                    };
                    TimeTrigger hourlyTrigger = new TimeTrigger(30, false);

                    task.SetTrigger(hourlyTrigger);
                  //  var condition = new SystemCondition(SystemConditionType.);

                    BackgroundTaskRegistration registration = task.Register();
                    AppSettings.Values["bgTask"] = "1";//وضعیت اجرا 


                    AppSettings.Values["Nava"] = comboSelectSalavat.SelectedIndex.ToString();
                    AppSettings.Values["Time"] = comboSelectTime.SelectedIndex.ToString();
                    MessageDialog msg = new MessageDialog("تنظیمات با موفقیت ذخیره شد.");
                    await msg.ShowAsync();

                }
                else
                {
                    ///////////////////////////begin task check
                    var isAlreadyRegistered = BackgroundTaskRegistration.AllTasks.Any(t => t.Value?.Name == "bgAlarm");
                    if (isAlreadyRegistered)
                    {
                        foreach (var tsk in BackgroundTaskRegistration.AllTasks)
                        {
                            if (tsk.Value.Name == "bgAlarm")
                            {
                                tsk.Value.Unregister(true);
                                AlarmHelper.RemoveAlarms();
                             
                                AppSettings.Values["bgTask"] = "0";
                             
                                break;
                            }
                        }
                        MessageDialog msg = new MessageDialog("پخش زمانبندی شده متوقف شد.");
                        await msg.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageDialog msg = new MessageDialog(ex.Message);
                await msg.ShowAsync();
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

        private async void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                ContentDialog deleteFileDialog = new ContentDialog()
                {

                    Content = "تقدیم به ساحت مقدس آقا علی بن موسی الرضا (ع)"+Environment.NewLine+Environment.NewLine+ "طراحی و توسعه توسط مهدی خیراندیش"  + Environment.NewLine+ "نسخه 1.2" + Environment.NewLine + Environment.NewLine + "Copyright ©  2017 Mehdi Kheirandish",
                    PrimaryButtonText = "بستن",
                    SecondaryButtonText = "ارسال بازخورد"
                };

                ContentDialogResult result = await deleteFileDialog.ShowAsync();

                if (result == ContentDialogResult.Secondary)
                {
                    try
                    {
                        var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();

                        emailMessage.Subject = "بازخورد اپلیکیشن هشتمین";
                        var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient("mk8813@hotmail.com");
                        emailMessage.To.Add(emailRecipient);
                        await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);

                    }
                    catch (Exception)
                    {


                    }
                }
                  //  await new MessageDialog("طراحی و برنامه نویسی توسط مهدی خیراندیش" +Environment.NewLine + "نسخه ١،٠").ShowAsync();
            }
            catch (Exception)
            {

                
            }
        }

        private void TextBlock_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var txt = e.OriginalSource as TextBlock;
                txt.Foreground = new SolidColorBrush(Colors.Gold); 
            }
            catch (Exception)
            {

               
            }
        }

       

        private void lblAbout_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
               
                lblAbout.Foreground = new SolidColorBrush(Colors.Khaki);
            }
            catch (Exception)
            {


            }
        }

        private void lblFeedback_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
              
                lblFeedback.Foreground = new SolidColorBrush(Colors.Khaki);
            }
            catch (Exception)
            {


            }
        }

        private void lblDonate_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
               
                lblDonate.Foreground = new SolidColorBrush(Colors.Khaki);
            }
            catch (Exception)
            {


            }
        }

        private async void lblFeedback_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?PFN={Package.Current.Id.FamilyName}"));
            }
            catch (Exception)
            {


            }
        }

        private async void lblDonate_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("https://idpay.ir/mk8813"));
            }
            catch (Exception)
            {


            }
        }

        MediaElement PlayMusic = new MediaElement();

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = e.OriginalSource as Button;
                if (button.Content.ToString() == "")//current pause
                {
                 
                    button.Content = "";//play
                    try
                    {
                        PlayMusic.Stop();
                    }
                    catch (Exception)
                    {

                      
                    }

                }
                else //current play
                {
                    button.Content = "";//pause

                    StorageFile sf = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/sounds/"+button.Tag));
                    PlayMusic.SetSource(await sf.OpenAsync(FileAccessMode.Read), sf.ContentType);
                    PlayMusic.Play();
                }

            }
            catch (Exception)
            {


            }
        }

        private void Button_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = e.OriginalSource as Button;
                button.Content = "";
                try
                {
                    PlayMusic.Stop();
                }
                catch (Exception)
                {

                   
                }
            }
            catch (Exception)
            {


            }
        }

        private void comboSelectSalavat_DropDownClosed(object sender, object e)
        {
            try
            {
            
                PlayMusic.Stop();
            }
            catch (Exception)
            {

              
            }
        }
    }
}
