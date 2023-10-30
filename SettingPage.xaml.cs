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
using Windows.Data;
using Windows.Storage;
using Windows.Globalization;
using Windows.ApplicationModel.Core;
using System.Collections;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Let_s_Pick_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        Windows.Storage.Pickers.FileOpenPicker open_Picker = new Windows.Storage.Pickers.FileOpenPicker();

        public SettingPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            open_Picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            open_Picker.FileTypeFilter.Add(".lpst");

            AutoStop.IsOn = GlobalClass.auto_stop;
            ToolBar_Background_Switch.IsOn = GlobalClass.toolbar_background_on;
            RandSeed_Visible_Switch.IsOn = GlobalClass.randseed_visible;
            TickSet.Text = GlobalClass.auto_stop_tick.ToString();
            LanguageSelector.ItemsSource = ApplicationLanguages.ManifestLanguages;
            LanguageSelector.SelectedItem = ApplicationLanguages.PrimaryLanguageOverride;
            RandMethod_Selector.SelectedIndex = GlobalClass.RandMethod;
            Pre_Caculate_Switch.IsOn = GlobalClass.pre_release;
            Variable_Seed_Switch.IsOn = GlobalClass.variable_seed;
            UpdateSearchTimeout.Text = GlobalClass.req_time_out.ToString();

            try
            {
                for (int i = 0; i < GlobalClass.all_list.Count; i++)
                {
                    ArrayList temp = new ArrayList();
                    temp = (ArrayList)GlobalClass.all_list[i];
                    CurrentListSelector.Items.Add(temp[0].ToString().Remove(0, 1));
                }
            }
            catch (Exception)
            {
            }
            CurrentListSelector.SelectedIndex = GlobalClass.current_list_id;


            if(ApplicationLanguages.PrimaryLanguageOverride == string.Empty)
            {
                LanguageSelector.Text = "(Auto)";
            }
        }

        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ApplicationLanguages.PrimaryLanguageOverride != LanguageSelector.SelectedItem.ToString())
            {
                RestartInfoStack.Visibility = Visibility.Visible;
            }
            else
            {
                RestartInfoStack.Visibility = Visibility.Collapsed;
            }
            ApplicationLanguages.PrimaryLanguageOverride = LanguageSelector.SelectedItem.ToString();
        }

        private void Page_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            try
            {
                try
                {
                    GlobalClass.auto_stop_tick = Convert.ToInt32(TickSet.Text.ToString());
                }
                catch (Exception)
                {
                    TickSet.Text = GlobalClass.auto_stop_tick_min.ToString();
                }

                GlobalClass.current_list_id = CurrentListSelector.SelectedIndex == -1 ? 0 : CurrentListSelector.SelectedIndex;
                GlobalClass.RandMethod = RandMethod_Selector.SelectedIndex;
                GlobalClass.pre_release = Pre_Caculate_Switch.IsOn;
                GlobalClass.variable_seed = Variable_Seed_Switch.IsOn;

                GlobalClass.localSettings.Values["Auto_Stop_Tick"] = GlobalClass.auto_stop_tick;
                GlobalClass.localSettings.Values["ToolBar_Background_State"] = ToolBar_Background_Switch.IsOn;
                GlobalClass.localSettings.Values["Current_List_id"] = GlobalClass.current_list_id;
                GlobalClass.localSettings.Values["RandSeed_Visible_State"] = RandSeed_Visible_Switch.IsOn;
                GlobalClass.localSettings.Values["RandMethod"] = RandMethod_Selector.SelectedIndex;
                GlobalClass.localSettings.Values["Pre_Release"] = Pre_Caculate_Switch.IsOn;
                GlobalClass.localSettings.Values["Variable_Seed"] = Variable_Seed_Switch.IsOn;
                GlobalClass.localSettings.Values["UpdateRequestTimeout"] = GlobalClass.req_time_out;
            }
            catch (Exception)
            {
            }
        }

        private void TickSet_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            try
            {
                GlobalClass.auto_stop_tick = Convert.ToInt32(TickSet.Text.ToString());
            }
            catch (Exception)
            {
                TickSet.Text = GlobalClass.auto_stop_tick_min.ToString();
            }
            if(GlobalClass.auto_stop_tick < GlobalClass.auto_stop_tick_min)
            {
                TickSet.Text = GlobalClass.auto_stop_tick_min.ToString();
                GlobalClass.auto_stop_tick = GlobalClass.auto_stop_tick_min;
            }
        }

        private void AutoStop_Toggled(object sender, RoutedEventArgs e)
        {
            GlobalClass.auto_stop = AutoStop.IsOn;
        }

        private async void Restart_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await CoreApplication.RequestRestartAsync(string.Empty);
        }

        private async void ImportSettingsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.Storage.StorageFile file = await open_Picker.PickSingleFileAsync();
            if (file != null)
            {
                string fname = file.Path;
                File.WriteAllText(GlobalClass.path + "\\slist.lpst", await Windows.Storage.FileIO.ReadTextAsync(file));
            }
        }

        private void ToolBar_Background_Switch_Toggled(object sender, RoutedEventArgs e)
        {
            GlobalClass.toolbar_background_on = ToolBar_Background_Switch.IsOn;
        }

        private void RandSeed_Visible_Switch_Toggled(object sender, RoutedEventArgs e)
        {
            GlobalClass.randseed_visible = RandSeed_Visible_Switch.IsOn;
        }

        private void RandMethod_Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int index = RandMethod_Selector.SelectedIndex;
                Pre_Caculate_Switch.Visibility = Visibility.Visible;
                Variable_Seed_Switch.Visibility = Visibility.Visible;
                if (index == 0 || index == 1 || index == 4 || index == 5)
                {
                    Pseudorandom_Notice.Visibility = Visibility.Visible;
                }
                else
                {
                    Pseudorandom_Notice.Visibility = Visibility.Collapsed;
                    if (index == 6)
                    {
                        Pre_Caculate_Switch.Visibility = Visibility.Collapsed;
                        Variable_Seed_Switch.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateSearchTimeout_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            try
            {
                GlobalClass.req_time_out = Convert.ToInt32(UpdateSearchTimeout.Text.ToString());
            }
            catch (Exception)
            {
                UpdateSearchTimeout.Text = GlobalClass.req_time_out.ToString();
            }
            if (GlobalClass.req_time_out < 1500)
            {
                GlobalClass.req_time_out = 1500;
                UpdateSearchTimeout.Text = GlobalClass.req_time_out.ToString();
            }
        }
    }
}
