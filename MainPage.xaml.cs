using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections;
using Windows.Storage;
using System.Text;
using Windows.Globalization;
using Windows.UI.Popups;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.UI;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Let_s_Pick_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Windows.Storage.Pickers.FileOpenPicker open_Picker = new Windows.Storage.Pickers.FileOpenPicker();
        Windows.Storage.Pickers.FileSavePicker save_Picker = new Windows.Storage.Pickers.FileSavePicker();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ContentFrame_Loading(FrameworkElement sender, object args)
        {
            contentFrame.Navigate(typeof(HomePage));
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            GlobalClass.path = ApplicationData.Current.LocalCacheFolder.Path.ToString();
            GlobalClass.auto_stop_tick = GlobalClass.auto_stop_tick_min;

            open_Picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            open_Picker.FileTypeFilter.Add(".txt");
            

            save_Picker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });

            try
            {
                if (!File.Exists(GlobalClass.path + "\\source.txt"))
                {
                    File.WriteAllText(GlobalClass.path + "\\source.txt", "default");
                }
            }
            catch (Exception ex)
            {
                NavigationView.Header = ex.Message;
            }

            try
            {
                if (!File.Exists(GlobalClass.path + "\\slist.lpst"))
                {
                    File.WriteAllText(GlobalClass.path + "\\slist.lpst", "");
                }
            }
            catch (Exception ex)
            {
                NavigationView.Header = ex.Message;
            }

            //read tick
            try
            {
                GlobalClass.auto_stop_tick = (int)GlobalClass.localSettings.Values["Auto_Stop_Tick"];
            }
            catch (Exception)
            {
                GlobalClass.auto_stop_tick = GlobalClass.auto_stop_tick_min;
            }

            //read background setting
            try
            {
                GlobalClass.toolbar_background_on = (bool)GlobalClass.localSettings.Values["ToolBar_Background_State"];
            }
            catch (Exception)
            {
                GlobalClass.toolbar_background_on = false;
            }

            //read list id
            try
            {
                GlobalClass.current_list_id = (int)GlobalClass.localSettings.Values["Current_List_id"];
            }
            catch (Exception)
            {
                GlobalClass.current_list_id = 0;
            }

            //read random seed visibility
            try
            {
                GlobalClass.randseed_visible = (bool)GlobalClass.localSettings.Values["RandSeed_Visible_State"];
            }
            catch (Exception)
            {
                GlobalClass.randseed_visible = false;
            }

            //read rand method id
            try
            {
                GlobalClass.RandMethod = (int)GlobalClass.localSettings.Values["RandMethod"];
            }
            catch (Exception)
            {
                GlobalClass.RandMethod = 1;
            }

            //read whether retain toolbar foreground or not
            /*
            try
            {
                GlobalClass.retain_toolbar_foreground = (bool)GlobalClass.localSettings.Values["Retain_Toolbar_Foreground"];
            }
            catch (Exception)
            {
                GlobalClass.retain_toolbar_foreground = false;
            }
            */
            //replaced by auto reverse background on 20210117

            //read whether pre caculate or not
            try
            {
                GlobalClass.pre_release = (bool)GlobalClass.localSettings.Values["Pre_Release"];
            }
            catch (Exception)
            {
                GlobalClass.pre_release = false;
            }

            //read whether accept variable seed or not
            try
            {
                GlobalClass.variable_seed = (bool)GlobalClass.localSettings.Values["Variable_Seed"];
            }
            catch (Exception)
            {
                GlobalClass.variable_seed = false;
            }

            //read request timeout value
            try
            {
                GlobalClass.req_time_out = (int)GlobalClass.localSettings.Values["UpdateRequestTimeout"];
            }
            catch (Exception)
            {
                GlobalClass.req_time_out = 5000;
            }

            load_list();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.Black;

            if(GlobalClass.GetReverseForegroundColor() == true)
            {
                titletext.Foreground = GlobalClass.white_br;
                titleBar.ButtonForegroundColor = Windows.UI.Colors.WhiteSmoke;
            }

            Window.Current.SetTitleBar(titlebar);
        }

        private void Home_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //contentFrame.Navigate(typeof(HomePage));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Home.IsSelected = true;

            try
            {
                GlobalClass.GetLatestVersion();
            }
            catch (Exception)
            {
            }
            if (GlobalClass.latest_version != GlobalClass.current_version && GlobalClass.latest_version != "-1")
            {
                string text = UpdateTip.Text;

                text = text.Replace("%a", GlobalClass.latest_version);
                text = text.Replace("%b", GlobalClass.current_version);
                text = text.Replace("\\n", "\n");

                ContentDialog dialog = new ContentDialog();
                dialog.Content = text;
                dialog.PrimaryButtonText = "Ok";
                await dialog.ShowAsync();
            }
        }

        private async void Open_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.Storage.StorageFile file = await open_Picker.PickSingleFileAsync();
            if (file != null)
            {
                string fname = file.Path;
                File.WriteAllText(GlobalClass.path + "\\source.txt", await Windows.Storage.FileIO.ReadTextAsync(file));
            }
            GlobalClass.current_list_id = 0;

            load_list();

            contentFrame.Navigate(typeof(HomePage));
            Home.IsSelected = true;
        }

        void load_list()
        {
            //read each list
            GlobalClass.all_list.Clear();
            bool in_list = false;
            ArrayList temp = new ArrayList();
            int group_count = 1;
            try
            {
                string[] s = File.ReadAllLines(GlobalClass.path + "\\source.txt", Encoding.UTF8);
                foreach (var item in s)
                {
                    if (item[0] == '#' && in_list == false)
                    {
                        in_list = true;
                        string t = item;

                        if(t == "#")
                        {
                            t = "#Group" + group_count.ToString();
                        }
                        temp.Add(t);
                        group_count++;
                    }
                    else if(in_list == true)
                    {
                        if(item[0] == '#')
                        {
                            in_list = false;
                            GlobalClass.all_list.Add(temp.Clone());
                            temp.Clear();
                        }
                        else
                        {
                            temp.Add(item.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NavigationView.Header = ex.Message;
            }

            //all_list load (old method)
            if(GlobalClass.all_list.Count == 0)
            {
                GlobalClass.all_items.Clear();
                GlobalClass.all_items.Add("#(Auto)");
                try
                {
                    string[] s = File.ReadAllLines(GlobalClass.path + "\\source.txt", Encoding.UTF8);
                    foreach (var item in s)
                    {
                        GlobalClass.all_items.Add(item.ToString());
                    }
                }
                catch (Exception ex)
                {
                    NavigationView.Header = ex.Message;
                }
                GlobalClass.all_list.Add(GlobalClass.all_items.Clone());
            }

            //Spe_list
            GlobalClass.s_name_list.Clear();
            GlobalClass.s_value_list.Clear();
            try
            {
                string[] cts = File.ReadAllLines(GlobalClass.path + "\\slist.lpst", Encoding.UTF8);
                int i = 0;
                foreach (var item in cts)
                {
                    if(item != string.Empty)
                    {
                        switch (i % 2)
                        {
                            case 0:
                                //read name
                                GlobalClass.s_name_list.Add(item);
                                break;

                            case 1:
                                //read value
                                int value = 0;
                                try
                                {
                                    value = Convert.ToInt32(item.ToString());
                                }
                                catch (Exception)
                                {
                                    value = 0;
                                }
                                GlobalClass.s_value_list.Add(value);
                                break;
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                NavigationView.Header = ex.Message;
            }
        }

        private void Reload_Tapped(object sender, TappedRoutedEventArgs e)
        {
            load_list();
            contentFrame.Navigate(typeof(HomePage));
            Home.IsSelected = true;
        }

        private void Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            contentFrame.Navigate(typeof(SettingPage));
            NavigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
            Home.Visibility = Visibility.Collapsed;
            Reload.Visibility = Visibility.Collapsed;
            Save.Visibility = Visibility.Collapsed;
            Open.Visibility = Visibility.Collapsed;
            About.Visibility = Visibility.Collapsed;
            Settings.IsSelected = true;
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            load_list();
            contentFrame.Navigate(typeof(HomePage));
            NavigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            Home.Visibility = Visibility.Visible;
            Reload.Visibility = Visibility.Visible;
            Save.Visibility = Visibility.Visible;
            Open.Visibility = Visibility.Visible;
            About.Visibility = Visibility.Visible;
            Settings.Visibility = Visibility.Visible;
            Home.IsSelected = true;
        }

        private async void Save_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (GlobalClass.result_list.Count == 0)
            {
                Home.IsSelected = true;
                return;
            }

            try
            {
                if (!File.Exists(ApplicationData.Current.LocalFolder.Path + "\\Result.txt"))
                {
                    File.Create(ApplicationData.Current.LocalFolder.Path + "\\Result.txt");
                }
                File.WriteAllText(ApplicationData.Current.LocalFolder.Path + "\\Result.txt", "");

                string txt = string.Empty;
                foreach(var item in GlobalClass.result_list)
                {
                    txt += item;
                    txt += "\n";
                }
                File.WriteAllText(ApplicationData.Current.LocalFolder.Path + "\\Result.txt", txt);

                var MyFolderLauncherOptions = new FolderLauncherOptions();
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                await Launcher.LaunchFolderAsync(folder, MyFolderLauncherOptions);
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Content = ex.Message;
                dialog.PrimaryButtonText = "Ok";
                await dialog.ShowAsync();
            }

            Home.IsSelected = true;
        }

        private void About_Tapped(object sender, TappedRoutedEventArgs e)
        {
            contentFrame.Navigate(typeof(AboutPage));
            contentFrame.CanBeScrollAnchor = true;
            NavigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
            Home.Visibility = Visibility.Collapsed;
            Reload.Visibility = Visibility.Collapsed;
            Save.Visibility = Visibility.Collapsed;
            Open.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
            Settings.IsSelected = true;
        }
    }
}
