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
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Input;
using Windows.Storage;
using System.Collections;
using System.Windows;
using Windows.UI.ViewManagement;
using System.Threading;
using System.Threading.Tasks;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Let_s_Pick_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        DispatcherTimer timer;
        const int min_interval = 2;
        const int max_interval = 1002;
        int nowa_interval = 0;
        int temp_auto_stop_tick_conter;
        int target_number = 1;
        int lines_conter = 1;
        int seed;
        ArrayList wait_to_print = new ArrayList();
        Random ran;
        bool first_focus = true;
        ArrayList pre = new ArrayList();
        string list_name = string.Empty;

        public HomePage()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer();
            refresh_timer();
            timer.Tick += Timer_Tick;
        }

        void set_interval()
        {
            timer.Interval = new TimeSpan(nowa_interval * 1000);
        }

        void refresh_timer()
        {
            temp_auto_stop_tick_conter = GlobalClass.auto_stop_tick;
            nowa_interval = max_interval;
            set_interval();
        }

        private void Timer_Tick(object sender, object e)
        {
            int rand;
            //rand = GlobalClass.GenerateRandomNumber() % GlobalClass.all_items.Count;
            rand = ran.Next(0, GlobalClass.all_items.Count);
            Showing.Text = GlobalClass.all_items[rand].ToString();

            if (GlobalClass.all_items.Count == 1)
            {
                if (GlobalClass.pre_release == true || GlobalClass.RandMethod == 6)
                {
                    pick_one(type: 3);
                }
                else
                {
                    pick_one(type: 1);
                }
                print_luckydogs();
                timer.Stop();
                ExitProcessingButton.Begin();
                ProcessingButton.IsEnabled = false;
                ExitAccelatingButton.Begin();
                AccelatingButton.IsEnabled = false;
                EnterFinishButton.Begin();
                FinishButton.IsEnabled = true;
                ExitProgressEllipse.Begin();
                return;
            }

            if (AutoStopSwitch.IsOn == true)
            {
                temp_auto_stop_tick_conter--;
            }

            if (nowa_interval > min_interval)
            {
                nowa_interval -= 40;
                set_interval();
                return;
            }

            if (StopButton.IsEnabled == false)
            {
                ExitAccelatingButton.Begin();
                AccelatingButton.IsEnabled = false;
                EnterStopButton.Begin();
                StopButton.IsEnabled = true;
                ExitProgressEllipse.Begin();
            }

            if (temp_auto_stop_tick_conter <= 0)
            {
                if (StopButton.IsEnabled == true)
                {
                    StopButton.IsEnabled = false;
                    ExitStopButton.Begin();
                    EnterProcessingButton.Begin();
                    ProcessingButton.IsEnabled = true;
                    EnterProgressEllipse.Begin();
                }

                if (GlobalClass.pre_release == true || GlobalClass.RandMethod == 6)
                {
                    pick_one(type: 3);
                }
                else
                {
                    pick_one(type: 1);
                }
                target_number--;

                if (target_number > 0)
                {
                    nowa_interval = 1;
                    set_interval();
                    return;
                }

                print_luckydogs();

                timer.Stop();
                wait_to_print.Clear();

                if (StopButton.IsEnabled == true)
                {
                    ExitStopButton.Begin();
                    StopButton.IsEnabled = false;
                }
                ExitProcessingButton.Begin();
                ProcessingButton.IsEnabled = false;
                EnterPickButton.Begin();
                PickButton.IsEnabled = true;
                ExitProgressEllipse.Begin();
                number_change();
                TargetNumberBox.IsEnabled = true;
                UpButton.IsEnabled = true;
                DownButton.IsEnabled = true;
            }
        }

        async void pick_one(int type /*1-direct pick      2-pre pick      3-realise pre (print out)*/)
        {
            TextBlock tb = new TextBlock();
            string now = string.Empty;

            switch (type)
            {
                case 1:
                    //ResultList.Items.Add(tb);
                    tb.Text = Showing.Text.ToString();
                    tb.FontSize = 50;

                    if (GlobalClass.current_list_id == 0)
                    {
                        if (GlobalClass.s_name_list.Contains(tb.Text) && (int)GlobalClass.s_value_list[GlobalClass.s_name_list.IndexOf(tb.Text)] > lines_conter)
                        {
                            target_number++;
                            return;
                        }
                    }

                    wait_to_print.Add(tb);
                    lines_conter++;
                    GlobalClass.result_list.Add(tb.Text);
                    GlobalClass.all_items.Remove(Showing.Text.ToString());
                    break;

                case 2:
                    bool gotten = false;
                    now = string.Empty;
                    if (GlobalClass.all_items.Count == 1)
                    {
                        now = GlobalClass.all_items[0].ToString();
                    }
                    else do
                        {
                            now = GlobalClass.all_items[ran.Next(0, GlobalClass.all_items.Count)].ToString();
                            gotten = true;

                            if (GlobalClass.current_list_id == 0)
                            {
                                if (GlobalClass.s_name_list.Contains(now) == true && (int)GlobalClass.s_value_list[GlobalClass.s_name_list.IndexOf(now)] > pre.Count + 1)
                                {
                                    gotten = false;
                                }
                            }
                        } while (gotten == false);

                    pre.Add(now);
                    GlobalClass.all_items.Remove(now);
                    break;

                case 3:
                    now = pre[0].ToString();
                    tb.Text = now;
                    Thread.Sleep(50);
                    tb.FontSize = 50;
                    wait_to_print.Add(tb);
                    Showing.Text = now;
                    lines_conter++;
                    GlobalClass.result_list.Add(now);
                    GlobalClass.all_items.Remove(now);
                    pre.RemoveAt(0);
                    break;
            }

            if (type == 3 || GlobalClass.variable_seed == false)
                return;
            else
                set_print_seed();
        }

        void print_luckydogs()
        {
            for (int i = 0; i < wait_to_print.Count; i++)
            {
                ResultList.Items.Add(wait_to_print[i]);
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            read_tn();
            target_number--;
            TargetNumberBox.Text = target_number.ToString();

            if (target_number <= 0)
            {
                target_number = 1;
                TargetNumberBox.Text = "1";
            }
        }

        void read_tn()
        {
            target_number = Convert.ToInt32(TargetNumberBox.Text.ToString());
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            read_tn();
            target_number++;
            TargetNumberBox.Text = target_number.ToString();

            if (target_number > GlobalClass.all_items.Count)
            {
                target_number = GlobalClass.all_items.Count;
                TargetNumberBox.Text = GlobalClass.all_items.Count.ToString();
            }
        }

        private void TargetNumberBox_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            number_change();
        }

        void number_change()
        {
            try
            {
                read_tn();
            }
            catch (Exception ex)
            {
                TargetNumberBox.Text = "1";
                TargetNumberBox.SelectAll();
            }

            if (target_number <= 0)
            {
                target_number = 1;
                TargetNumberBox.Text = "1";
            }
            if (target_number > GlobalClass.all_items.Count)
            {
                target_number = GlobalClass.all_items.Count;
                TargetNumberBox.Text = GlobalClass.all_items.Count.ToString();
            }
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalClass.all_items.Count == 0)
                return;

            wait_to_print.Clear();

            read_tn();
            refresh_timer();
            timer.Start();

            ExitPickButton.Begin();
            PickButton.IsEnabled = false;
            EnterAccelatingButton.Begin();
            AccelatingButton.IsEnabled = true;
            EnterProgressEllipse.Begin();

            TargetNumberBox.IsEnabled = false;
            UpButton.IsEnabled = false;
            DownButton.IsEnabled = false;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //set page
            ExitAccelatingButton.Begin();
            AccelatingButton.IsEnabled = false;
            ExitStopButton.Begin();
            StopButton.IsEnabled = false;
            ExitFinishButton.Begin();
            FinishButton.IsEnabled = false;
            ExitPickButton.Begin();
            PickButton.IsEnabled = false;

            EnterProgressEllipse.Begin();
            ProgressEllipseRunning.Begin();

            set_print_seed();

            if (GlobalClass.toolbar_background_on == true)
            {
                Toolbar_Background.Visibility = Visibility.Visible;

                if(GlobalClass.GetReverseForegroundColor() == true)
                {
                    TargetNumberBox_Header.Foreground = GlobalClass.white_br;
                    TargetNumberBox.Foreground = GlobalClass.white_br;
                    AutoStopSwitch_Header.Foreground = GlobalClass.white_br;
                    AutoStopSwitch.Foreground = GlobalClass.white_br;
                    Seed.Foreground = GlobalClass.white_br;
                    Seed_value.Foreground = GlobalClass.white_br;
                    Add_Sy.Foreground = GlobalClass.white_br;
                    Remove_Sy.Foreground = GlobalClass.white_br;
                    PickButton.Foreground = GlobalClass.white_br;
                    StopButton.Foreground = GlobalClass.white_br;
                    AccelatingButton.Foreground = GlobalClass.white_br;
                    FinishButton.Foreground = GlobalClass.white_br;
                    ProcessingButton.Foreground = GlobalClass.white_br;
                    ProgressEllipse.Stroke = GlobalClass.white_br;
                }
            }

            //load list
            load_list();

            GlobalClass.result_list.Clear();
            AutoStopSwitch.IsOn = GlobalClass.auto_stop;
            Showing.Text = Showing.Text + ": " + list_name;

            ProcessingButton.Focus(FocusState.Programmatic);
        }

        void load_list()
        {
            ArrayList temp = new ArrayList();
            GlobalClass.all_items.Clear();
            try
            {
                temp = (ArrayList)GlobalClass.all_list[GlobalClass.current_list_id];
            }
            catch (Exception)
            {
                GlobalClass.current_list_id = 0;
                temp = (ArrayList)GlobalClass.all_list[GlobalClass.current_list_id];
            }

            for (int i = 1; i < temp.Count; i++)
            {
                GlobalClass.all_items.Add(temp[i].ToString());
            }

            list_name = temp[0].ToString().Remove(0, 1);
        }

        private void StopButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            temp_auto_stop_tick_conter = 0;
            StopButton.IsEnabled = false;
            ExitStopButton.Begin();
            EnterProcessingButton.Begin();
            EnterProgressEllipse.Begin();
            ProcessingButton.IsEnabled = true;
        }

        private async void Page_GotFocus(object sender, RoutedEventArgs e)
        {
            if(first_focus == false)
            {
                return;
            }

            //reload list
            load_list();

            if (GlobalClass.pre_release == true && GlobalClass.RandMethod != 6)
            {
                int items_amount = GlobalClass.all_items.Count;
                for (int i = 0; i < items_amount; i++)
                {
                    pick_one(type: 2);
                }
            }
            else if (GlobalClass.RandMethod == 6)
            {
                bool succeed;

                //request random numbers
                pre_request();
                await GlobalClass.GetSequencesFromRandomORG(1, GlobalClass.all_items.Count);
                after_request();

                //localize as pre
                if (GlobalClass.rand_sequences[0].ToString() != "Error")
                {
                    pre.Clear();
                    foreach (var item in GlobalClass.rand_sequences)
                    {
                        int index = Convert.ToInt32(item.ToString()) - 1;
                        string now = GlobalClass.all_items[index].ToString();
                        pre.Add(now);
                    }
                }
            }

            ProcessingButton.IsEnabled = false;
            ExitProcessingButton.Begin();
            EnterPickButton.Begin();
            PickButton.IsEnabled = true;

            ExitProgressEllipse.Begin();

            lines_conter = 1;
            first_focus = false;
        }

        void pre_request()
        {
            RandomNumber_Requesting.Visibility = Visibility.Visible;
            Seed_value.Visibility = Visibility.Collapsed;
            Seed.Visibility = Visibility.Collapsed;
            GlobalClass.rand_sequences.Clear();
        }

        void after_request()
        {
            RandomNumber_Requesting.Visibility = Visibility.Collapsed;
            RandomIO_Label.Visibility = Visibility.Visible;

            try
            {
                if (GlobalClass.rand_sequences[0].ToString() == "Error")
                {
                    ExitPickButton.Begin();
                    EnterFinishButton.Begin();
                    PickButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ExitPickButton.Begin();
                EnterFinishButton.Begin();
                PickButton.IsEnabled = false;
            }
        }

        void set_print_seed()
        {
            if (GlobalClass.RandMethod == 0 || GlobalClass.RandMethod == 4)
            {
                seed = Convert.ToInt32(DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Hour.ToString());
            }
            else
            {
                seed = (int)DateTime.Now.Ticks;
            }

            if (GlobalClass.RandMethod == 4 || GlobalClass.RandMethod == 5)
            {
                ran = new Random(seed);
                seed = ran.Next(int.MinValue, int.MaxValue);
            }

            if (GlobalClass.RandMethod == 2)
            {
                byte[] bytes = new byte[4];
                System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
                rng.GetBytes(bytes);
                seed = BitConverter.ToInt32(bytes, 0);
            }

            if (GlobalClass.RandMethod == 3 || GlobalClass.RandMethod == 6)
            {
                byte[] buffer = Guid.NewGuid().ToByteArray();       //生成字节数组
                seed = BitConverter.ToInt32(buffer, 0);
            }

            ran = new Random(seed);
            Seed_value.Text = seed.ToString();
            if (GlobalClass.randseed_visible == true)
            {
                SeedStack.Visibility = Visibility.Visible;
            }
            else
            {
                SeedStack.Visibility = Visibility.Collapsed;
            }
        }
    }
}
