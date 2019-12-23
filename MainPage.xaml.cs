using ac_notification_listener_helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ac_notification_listener
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        TextBlock currentFolderLocationText;
        TextBlock appNameText;
        TextBlock filterText;
        TextBox inputFilter;
        TextBox inputAppName;
        Button requestButton;


        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(600, 500);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            currentFolderLocationText = (TextBlock)this.FindName("TextCurrentFileLocation");
            appNameText = (TextBlock)this.FindName("AppNameText");
            filterText = (TextBlock)this.FindName("FilterText");
            inputFilter = (TextBox)this.FindName("InputFilter");
            inputAppName = (TextBox)this.FindName("InputAppName");
            requestButton = (Button)this.FindName("ButtonRequest");
            setRecordFile();
            FillContents();
        }



        private async void setRecordFile()
        {
            StorageFolder dataFolder;
            try
            {
                dataFolder = await KnownFolders.DocumentsLibrary.GetFolderAsync("ac-notifiation-listener-data");
            }
            catch (Exception)
            {
                dataFolder = await KnownFolders.DocumentsLibrary.CreateFolderAsync("ac-notifiation-listener-data");
            }
            this.currentFolderLocationText.Text = "Dir: " + dataFolder.Path;
            Debug.WriteLine("ANYC currentFolderLocationText " + dataFolder.Path);
            StorageFile storageFile;
            try
            {
                storageFile = await dataFolder.GetFileAsync(AcHelper.RECORD_FILE_NAME);
            }
            catch (Exception)
            {
                storageFile = await dataFolder.CreateFileAsync(AcHelper.RECORD_FILE_NAME);
            }
            AcHelper.setRecordsFile(storageFile);
        }

        private async void PickFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.FileTypeFilter.Add(".csv");
            StorageFolder outputFolder = await folderPicker.PickSingleFolderAsync();
            if (outputFolder != null)
            {
                // Application now has read/write access to the picked file
                this.currentFolderLocationText.Text = "Dir: " + outputFolder.Path;
                StorageFile storageFile;
                try
                {
                    storageFile = await outputFolder.GetFileAsync(AcHelper.RECORD_FILE_NAME);
                    Debug.WriteLine("File Found " + AcHelper.RECORD_FILE_NAME);
                }
                catch
                {
                    storageFile = await outputFolder.CreateFileAsync(AcHelper.RECORD_FILE_NAME);
                    Debug.WriteLine("Created File " + AcHelper.RECORD_FILE_NAME);
                }
                AcHelper.setRecordsFile(storageFile);


                //// Write picked file location to cache file
                //StorageFolder cacheFolder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path);
                //StorageFile pathStorageFile;
                //try
                //{
                //    pathStorageFile = await cacheFolder.GetFileAsync(cacheFileName);
                //    Debug.WriteLine("File Found " + cacheFileName);
                //}
                //catch
                //{
                //    Debug.WriteLine("File Not Found");
                //    pathStorageFile = await cacheFolder.CreateFileAsync(cacheFileName);
                //}
                //AcHelper.WriteToFileWithoutAppend(pathStorageFile, outputFolder.Path);


            }
            else
            {
                this.currentFolderLocationText.Text = "Error!";
            }
        }


        const string cacheFileAppName = @"appname.txt";
        const string cacheFileFilterName = @"filter.txt";
        private async void FillContents()
        {

            // write to file

            StorageFolder cacheFolder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path);

            // read from file
            StorageFile pathStorageFile;
            try
            {
                pathStorageFile = await cacheFolder.GetFileAsync(cacheFileAppName);
                Debug.WriteLine("File Found " + cacheFileAppName);
            }
            catch
            {
                Debug.WriteLine("File Not Found");
                pathStorageFile = await cacheFolder.CreateFileAsync(cacheFileAppName);
            }
            AcHelper.appNameText = appNameText.Text = inputAppName.Text = await Windows.Storage.FileIO.ReadTextAsync(pathStorageFile);


            try
            {
                pathStorageFile = await cacheFolder.GetFileAsync(cacheFileFilterName);
                Debug.WriteLine("File Found " + cacheFileFilterName);
            }
            catch
            {
                Debug.WriteLine("File Not Found " + cacheFileFilterName);
                pathStorageFile = await cacheFolder.CreateFileAsync(cacheFileFilterName);
            }
            AcHelper.contentFilterText = filterText.Text = inputFilter.Text = await Windows.Storage.FileIO.ReadTextAsync(pathStorageFile);
        }

        private async void UpdateContents(object sender, RoutedEventArgs e)
        {
            // Set to Text Labels
            // set to ACHELPER
            AcHelper.appNameText = appNameText.Text = inputAppName.Text;
            AcHelper.contentFilterText = filterText.Text = InputFilter.Text;

            // write to file

            StorageFolder cacheFolder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path);

            StorageFile pathStorageFile;
            try
            {
                pathStorageFile = await cacheFolder.GetFileAsync(cacheFileAppName);
                Debug.WriteLine("File Found " + cacheFileAppName);
            }
            catch
            {
                Debug.WriteLine("File Not Found");
                pathStorageFile = await cacheFolder.CreateFileAsync(cacheFileAppName);
            }
            AcHelper.WriteToFileWithoutAppend(pathStorageFile, appNameText.Text);


            try
            {
                pathStorageFile = await cacheFolder.GetFileAsync(cacheFileFilterName);
                Debug.WriteLine("File Found " + cacheFileFilterName);
            }
            catch
            {
                Debug.WriteLine("File Not Found " + cacheFileFilterName);
                pathStorageFile = await cacheFolder.CreateFileAsync(cacheFileFilterName);
            }
            AcHelper.WriteToFileWithoutAppend(pathStorageFile, filterText.Text);
        }

        async private void requestButton_Click(object sender, RoutedEventArgs e)
        {
            StartupTask startupTask = await StartupTask.GetAsync("task-start-up-ac-notification");
            switch (startupTask.State)
            {
                case StartupTaskState.Disabled:
                    // Task is disabled but can be enabled.
                    StartupTaskState newState = await startupTask.RequestEnableAsync();
                    Debug.WriteLine("Request to enable startup, result = {0}", newState);
                    break;
                case StartupTaskState.DisabledByUser:
                    // Task is disabled and user must enable it manually.
                    MessageDialog dialog = new MessageDialog(
                        "I know you don't want this app to run " +
                        "as soon as you sign in, but if you change your mind, " +
                        "you can enable this in the Startup tab in Task Manager.",
                        "TestStartup");
                    await dialog.ShowAsync();
                    break;
                case StartupTaskState.DisabledByPolicy:
                    Debug.WriteLine(
                        "Startup disabled by group policy, or not supported on this device");
                    break;
                case StartupTaskState.Enabled:
                    MessageDialog dialog1 = new MessageDialog("Enabled the program to run on startup.");
                    await dialog1.ShowAsync();
                    Debug.WriteLine("Startup is enabled.");
                    break;
            }
        }
    }
}
