using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Streams;
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
    sealed partial class App : Application
    {

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
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
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            var deferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name)
            {
                case "UserNotificationChanged":
                    // Call your own method to process the new/removed notifications
                    // The next section of documentation discusses this code
                    Debug.WriteLine("UserNotification changed@@@@@@@@@@@@@@@@@@@################################");


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
        public async void saveNotificationsToFile(IReadOnlyList<UserNotification> notifs)
        {
            List<string> notificationsText = new List<string>();

            foreach (UserNotification notif in notifs)
            {

                // Get the toast binding, if present
                NotificationBinding toastBinding = notif.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);

                if (toastBinding != null)
                {
                    // And then get the text elements from the toast binding
                    IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();

                    // Treat the first text element as the title text
                    string titleText = textElements.FirstOrDefault()?.Text;
                  
                    // We'll treat all subsequent text elements as body text,
                    // joining them together via newlines.
                    string bodyText = string.Join("\n", textElements.Skip(1).Select(t => t.Text));
                    notificationsText.Add(titleText+": "+bodyText);

                }

            }
            // Create sample file; replace if exists.
            StorageFolder storageFolder =
                ApplicationData.Current.LocalFolder;
            StorageFile sampleFile =
                await storageFolder.CreateFileAsync("allNotifications.txt",
                    CreationCollisionOption.ReplaceExisting);
            await FileIO.AppendLinesAsync(sampleFile, notificationsText);
           
        }
        private async void StartListeningNotifications()
        {
            // Get the listener
            UserNotificationListener listener = UserNotificationListener.Current;

            // And request access to the user's notifications (must be called from UI thread)
            UserNotificationListenerAccessStatus notificationListenerAccessStatus = await listener.RequestAccessAsync();
            BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            switch(backgroundAccessStatus)
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

            switch (notificationListenerAccessStatus)
            {
                // This means the user has granted access.
                case UserNotificationListenerAccessStatus.Allowed:

                    // Get the toast notifications
                    IReadOnlyList<UserNotification> notifs = await listener.GetNotificationsAsync(NotificationKinds.Toast);
                    Debug.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++UserNotificationListenerAccessStatus.Allowed: "+notifs.Count()+"++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

                    saveNotificationsToFile(notifs);
                    
                    //Debug.WriteLine("Size of notifications: " + notifs.Count);
                    //listener.ClearNotifications();
                    //Debug.WriteLine("Size of notifications: " + notifs.Count);

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
        /*private async void SyncNotifications(UserNotificationListener listener)
        {
            // Get all the current notifications from the platform
            IReadOnlyList<UserNotification> userNotifications = await listener.GetNotificationsAsync(NotificationKinds.Toast);

            // Obtain the notifications that our wearable currently has displayed
            IList<uint> wearableNotificationIds = GetNotificationsOnWearable();

            // Copy the currently displayed into a list of notification ID's to be removed
            var toBeRemoved = new List<uint>(wearableNotificationIds);

            // For each notification in the platform
            foreach (UserNotification userNotification in userNotifications)
            {
                // If we've already displayed this notification
                if (wearableNotificationIds.Contains(userNotification.Id))
                {
                    // We want to KEEP it displayed, so take it out of the list
                    // of notifications to remove.
                    toBeRemoved.Remove(userNotification.Id);
                }

                // Otherwise it's a new notification
                else
                {
                    // Display it on the Wearable
                    SendNotificationToWearable(userNotification);
                }
            }

            // Now our toBeRemoved list only contains notification ID's that no longer exist in the platform.
            // So we will remove all those notifications from the wearable.
            foreach (uint id in toBeRemoved)
            {
                RemoveNotificationFromWearable(id);
            }
        }*/
    }

}
