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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Let_s_Pick_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();
        }

        private async void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var uriBlog = new Uri(@"https://techy-wu.github.io/Let-s_Pick_UWP/");

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uriBlog);
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            version_box.Text = GlobalClass.current_version;
            if (GlobalClass.latest_version != GlobalClass.current_version && GlobalClass.latest_version != "-1")
            {
                version_box.Text = version_box.Text + "    (Latest version: " + GlobalClass.latest_version + " ) ";
            }
        }

        private async void Version_box_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var uriBlog = new Uri(@"https://github.com/Techy-Wu/Let-s_Pick_UWP/releases/latest");

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uriBlog);
        }

        private void ShowHistory_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowHistory.Visibility = Visibility.Collapsed;
            UpdateHistory.Visibility = Visibility.Visible;
        }
    }
}
