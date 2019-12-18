using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ac_notification_listener
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    ///         
    [DebuggerDisplay("{DebuggerDisplay(), nq}")]

    sealed partial class App : Application
    {

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 
        IReadOnlyList<UserNotification> notifs;
        StorageFile notificationFile;
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        /// 
        
        private async void SaveNotificationsToFile()
        {
            Int32 retryAttempts = 5;
            const Int32 ERROR_ACCESS_DENIED = unchecked((Int32)0x80070005);
            const Int32 ERROR_SHARING_VIOLATION = unchecked((Int32)0x80070020);

            List<string> notificationsInText = new List<string>();
            if (notifs != null && notifs.Count() != 0)
            {
                UserNotification userNotification = notifs.LastOrDefault();

                Debug.WriteLine("Start");
                string appDisplayName = userNotification.AppInfo.DisplayInfo.DisplayName;
                Debug.WriteLine(appDisplayName);
                var time = userNotification.CreationTime.DateTime;



                //var t = userNotification.AppInfo.DisplayInfo.Description;
                // Get the toast binding, if present
                NotificationBinding toastBinding = userNotification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
                //if (toastBinding != null && appDisplayName.Contains("Google Chrome"))
                //{

                    IDictionary<string,string> dictionary = toastBinding.Hints;

                    // And then get the text elements from the toast binding
                    IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();
                    // Treat the first text element as the title text
                foreach(AdaptiveNotificationText s in textElements)
                {
                    Debug.WriteLine(s.Text);
                }
                Debug.WriteLine("END");
                string titleText = textElements.FirstOrDefault()?.Text;
                        

                    // We'll treat all subsequent text elements as body text,
                    // joining them together via newlines.
                string bodyText = string.Join(",", textElements.Select(t => t.Text));
                string outstr = time.ToString() + ", " + appDisplayName + ", " + bodyText;
                notificationsInText.Add(outstr);
                //}
                if (notificationFile != null)
                {
                    // Application now has read/write access to the picked file.
                    while (retryAttempts > 0)
                    {
                        try
                        {
                            retryAttempts--;
                            await FileIO.AppendLinesAsync(notificationFile, notificationsInText);
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
                    // The operation was cancelled in the picker dialog.
                }
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
            {
                Debug.WriteLine("Windows.UI.Notifications.Management.UserNotificationListener supported");
                StartListeningNotifications();
            }

            else
            {
                // Older version of Windows, no Listener
            }
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
           // openFilePicker();
            
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            var deferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name)
            {
                case "UserNotificationChanged":
                    // Call your own method to process the new/removed notifications
                    // The next section of documentation discusses this code
                    SyncNotifications();
                    break;
            }

            deferral.Complete();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
        
        private async void SyncNotifications()
        {
            // Get the listener
            UserNotificationListener listener = UserNotificationListener.Current;

            // And request access to the user's notifications (must be called from UI thread)
            UserNotificationListenerAccessStatus notificationListenerAccessStatus = await listener.RequestAccessAsync();
            switch (notificationListenerAccessStatus)
            {
                // This means the user has granted access.
                case UserNotificationListenerAccessStatus.Allowed:

                    // Get the toast notifications
                    
                    notifs = await listener.GetNotificationsAsync(NotificationKinds.Toast);
                    Debug.WriteLine("Size of current notification buffer: " + notifs.Count());
                    if(notifs.Count() > 0)
                    {
                        var userNotification = notifs[0];
                    }

                    SaveNotificationsToFile();
                    break;

                // This means the user has denied access.
                // Any further calls to RequestAccessAsync will instantly
                // return Denied. The user must go to the Windows settings
                // and manually allow access.
                case UserNotificationListenerAccessStatus.Denied:
                    Debug.WriteLine("UserNotificationListenerAccessStatus.Denied");
                    // Show UI explaining that listener features will not
                    // work until user allows access.
                    break;

                // This means the user closed the prompt without
                // selecting either allow or deny. Further calls to
                // RequestAccessAsync will show the dialog again.
                case UserNotificationListenerAccessStatus.Unspecified:
                    Debug.WriteLine("UserNotificationListenerAccessStatus.Unspecified");
                    // Show UI that allows the user to bring up the prompt again
                    break;
            }
        }
        private async void StartListeningNotifications()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".csv");
            notificationFile = await openPicker.PickSingleFileAsync();
            BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            switch (backgroundAccessStatus)
            {
                case BackgroundAccessStatus.AlwaysAllowed:
                case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                    Debug.WriteLine("BackGroundrAccessStatus.Allowed");
                    if (!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals("UserNotificationChanged")))
                    {
                        // Specify the background task
                        var builder = new BackgroundTaskBuilder()
                        {
                            Name = "UserNotificationChanged"
                        };

                        // Set the trigger for Listener, listening to Toast Notifications
                        builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));

                        // Register the task
                        builder.Register();
                    }
                    break;
                case BackgroundAccessStatus.DeniedBySystemPolicy:
                case BackgroundAccessStatus.DeniedByUser:
                    Debug.WriteLine("BackGroundrAccessStatus.Denied");
                    break;
                case BackgroundAccessStatus.Unspecified:
                    Debug.WriteLine("BackGroundrAccessStatus.Unspecified");
                    break;
            }


        }
    }
}
