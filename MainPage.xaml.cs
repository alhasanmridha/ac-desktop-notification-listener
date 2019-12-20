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
            // read from file

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
            AcHelper.WriteToFileWithoutAppend(pathStorageFile, appNameText.Text);
            AcHelper.contentFilterText = filterText.Text = InputFilter.Text = await Windows.Storage.FileIO.ReadTextAsync(pathStorageFile);
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
            AcHelper.WriteToFileWithoutAppend(pathStorageFile, appNameText.Text);
        }
    }
}
