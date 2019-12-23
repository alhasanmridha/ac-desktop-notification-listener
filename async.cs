using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace ac_notification_listener_helper
{
    internal class AcHelper
    {
        private static StorageFile recordsFile;
        public static string appNameText;
        public static string contentFilterText;
        public const string RECORD_FILE_NAME = @"records.csv";
        public AcHelper()
        {

        }

        public static void setRecordsFile(StorageFile storageFile)
        {
            recordsFile = storageFile;
        }

        public static async void WriteToFileWithoutAppend(StorageFile storageFile, string record)
        {
            Int32 retryAttempts = 5;
            const Int32 ERROR_ACCESS_DENIED = unchecked((Int32)0x80070005);
            const Int32 ERROR_SHARING_VIOLATION = unchecked((Int32)0x80070020);
            if (storageFile != null)
            {
                // Application now has read/write access to the picked file.
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.WriteTextAsync(storageFile, record);
                        break;
                    }
                    catch (Exception ex) when ((ex.HResult == ERROR_ACCESS_DENIED) ||
                                               (ex.HResult == ERROR_SHARING_VIOLATION))
                    {
                        // This might be recovered by retrying, otherwise let the exception be raised.
                        // The app can decide to wait before retrying.
                    }
                }
            }
            else
            {
                Debug.WriteLine("File is null, can not save");
            }
        }

        public static async void WriteToFile(StorageFile storageFile, string record)
        {
            Int32 retryAttempts = 5;
            const Int32 ERROR_ACCESS_DENIED = unchecked((Int32)0x80070005);
            const Int32 ERROR_SHARING_VIOLATION = unchecked((Int32)0x80070020);
            try
            {
                if (record.Last() != '\n') record += "\n";
            }
            catch (Exception)
            {
                Debug.WriteLine("Record is invalid");
            }
            if (storageFile != null)
            {
                // Application now has read/write access to the picked file.
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.AppendTextAsync(storageFile, record);
                        break;
                    }
                    catch (Exception ex) when ((ex.HResult == ERROR_ACCESS_DENIED) ||
                                               (ex.HResult == ERROR_SHARING_VIOLATION))
                    {
                        // This might be recovered by retrying, otherwise let the exception be raised.
                        // The app can decide to wait before retrying.
                    }
                }
            }
            else
            {
                Debug.WriteLine("File is null, can not save");
            }
        }

        public static void ProcessNotification(UserNotification userNotification)
        {

            Debug.WriteLine("Start Saving Notification");
            if (userNotification != null)
            {
                string appDisplayName = userNotification.AppInfo.DisplayInfo.DisplayName;
                Debug.WriteLine(appDisplayName);
                var time = userNotification.CreationTime.DateTime;

                // Get the toast binding, if present
                NotificationBinding toastBinding = userNotification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
                if (toastBinding == null) return;

                //if (toastBinding != null && )
                //{


                // And then get the text elements from the toast binding
                IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();
                // Treat the first text element as the title text
                foreach (AdaptiveNotificationText s in textElements)
                {
                    Debug.WriteLine(s.Text);
                }

                string titleText = textElements.FirstOrDefault()?.Text;
                string bodyText = string.Join(" ", textElements.Skip(1).Select(t => t.Text));

                if (!isValidRecord(appDisplayName, titleText, bodyText)) return;

                List<String> temp = new List<String>();
                temp.Add(time.ToString());
                temp.Add(appDisplayName);
                temp.Add(titleText);
                temp.Add(bodyText);

                WriteToFile(recordsFile, string.Join(",", temp));
            }
            Debug.WriteLine("End Saving Notification");

        }
        private static bool isValidRecord(string appDispalyName, String titleText, string bodyText)
        {
            Debug.WriteLine("AppName " + appDispalyName);
            Debug.WriteLine("Title " + titleText);
            Debug.WriteLine("Body " + bodyText);
            bool result = false;
            try
            {
                result = appDispalyName.ToLower().Contains(appNameText.ToLower());
                String tempContents = bodyText.ToLower() + titleText.ToLower();
                result = result &&  tempContents.Contains(contentFilterText.ToLower());
            }
            catch (Exception)
            {

            }
            return result;
        }

        public static void ShowTileNotification()
        {
            //public sealed class ToastGenericAttributionText :

            ToastContent content = new ToastContent()
            {
                Launch = "AnyConnect",

                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        AppLogoOverride = new ToastGenericAppLogo
                        {
                            HintCrop = ToastGenericAppLogoCrop.Circle,
                            Source = "http://messageme.com/lei/profile.jpg"
                        },
                        Children =
                        {
                            new AdaptiveText {Text = "Recording Service" },
                            new AdaptiveText {Text = "Listening notifcations | AnyConnect" }
                        },
                        Attribution = new ToastGenericAttributionText
                        {
                            Text = "AnyConnect"
                        },
                    }
                },
                //Actions = new ToastActionsCustom()
                //{
                //    Inputs =
                //    {
                //        new ToastTextBox("tbReply")
                //        {
                //            PlaceholderContent = "Type a response"
                //        }
                //    },

                //    Buttons =
                //    {
                //        new ToastButton("reply", "reply")
                //        {
                //            ActivationType = ToastActivationType.Background,
                //            ImageUri = "Assets/QuickReply.png",
                //            TextBoxId = "tbReply"
                //        }
                //    }
                //},

                //Audio = new ToastAudio()
                //{
                //    Src = new Uri("ms-winsoundevent:Notification.IM")
                //}
            };
            XmlDocument doc = content.GetXml();

            // Generate WinRT notification
            var toast = new ToastNotification(doc);
            // Display toast
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

    }
}