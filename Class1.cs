using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Windows.Storage;
using System.IO;
using Windows.Security.Cryptography;
using System.Security.Cryptography;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.ViewManagement;
using System.Net;
using System.Threading;
using Windows.UI.Xaml.Controls;

namespace Let_s_Pick_UWP
{
    class GlobalClass
    {
        public static string current_version = "2.6.1";
        public static string latest_version = "-1";

        public static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static int RandMethod = 1;

        public static string path = string.Empty;
        public static ArrayList all_list = new ArrayList();

        public static int current_list_id = 0;
        public static ArrayList all_items = new ArrayList();
        public static ArrayList rand_sequences = new ArrayList();

        public static ArrayList s_name_list = new ArrayList();
        public static ArrayList s_value_list = new ArrayList();
        public static ArrayList s_type_list = new ArrayList();
        public static ArrayList result_list = new ArrayList();
        public static int auto_stop_tick;
        public static int auto_stop_tick_min = 30;
        public static int req_time_out = 5000;
        public static bool auto_stop = true;
        public static bool read_out = true;
        public static bool toolbar_background_on = true;
        public static bool randseed_visible = false;
        public static bool pre_release = false;
        public static bool variable_seed = false;

        public static Brush white_br = new SolidColorBrush(Windows.UI.Colors.WhiteSmoke);

        private UISettings uisetting = new UISettings();

        public static int GenerateRandomNumber()
        {
            // Generate a random number.
            int random = (int)(CryptographicBuffer.GenerateRandomNumber() % int.MaxValue);
            return random;
        }

        public static string Md5(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(str)), 4, 8);
            t2 = t2.Replace("-", "");
            return t2;
        }

        /// <summary>
        /// 获取一个颜色的人眼感知亮度，并以 0~1 之间的小数表示。
        /// </summary>
        private static double GetGrayLevel(Color color)
        {
            return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
        }

        public static bool GetReverseForegroundColor()     //false-black   true-white
        {
            if(GetGrayLevel(new UISettings().GetColorValue(UIColorType.Accent)) > 0.5)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static async void GetLatestVersion()
        {
            string url = string.Empty;
            WebRequest myWebRequest = WebRequest.Create("https://github.com/Techy-Wu/Let-s_Pick_UWP/releases/latest");
            myWebRequest.Timeout = req_time_out;
            try
            {
                WebResponse response = await myWebRequest.GetResponseAsync();
                url = response.ResponseUri.ToString();
                latest_version = url.Replace("https://github.com/Techy-Wu/Let-s_Pick_UWP/releases/tag/", "");
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static async Task GetSequencesFromRandomORG(int min, int max)
        {
            string url = "https://www.random.org/sequences/?min=" + min + "&max=" + max + "&col=1&format=plain&rnd=new";
            string htmlStr = "";

            for(int t = 1; t <= 3; t++)
            {
                try
                {
                    WebRequest request;          //实例化WebRequest对象
                    request = WebRequest.Create(url);
                    WebResponse response;      //创建WebResponse对象
                    response = await request.GetResponseAsync();
                    //Stream datastream = response.GetResponseStream();       //创建流对象
                    Stream datastream = response.GetResponseStream();
                    Encoding ec = Encoding.Default;
                    ec = Encoding.UTF8;
                    StreamReader reader = new StreamReader(datastream, ec);
                    htmlStr = reader.ReadToEnd();                  //读取网页内容  
                    reader.Close();
                    datastream.Close();
                    response.Close();

                    int j = 0;
                    using (StringReader sr = new StringReader(htmlStr))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            GlobalClass.rand_sequences.Add(line.ToString());
                        }
                    }

                    t = 4;
                }
                catch (Exception ex)
                {
                    GlobalClass.rand_sequences.Add("Error");
                    int j = 0;
                    using (StringReader sr = new StringReader(ex.Message))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            GlobalClass.rand_sequences.Add(line.ToString());
                        }
                    }
                    GlobalClass.rand_sequences.Add("Error");

                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "Request Error";
                    string error_info = string.Empty;
                    foreach (var item in GlobalClass.rand_sequences)
                    {
                        if (item.ToString() == "Error")
                            break;

                        error_info += (string)item;
                        error_info += "\n";
                    }
                    dialog.Content = error_info;
                    dialog.PrimaryButtonText = "Ok";
                    await dialog.ShowAsync();
                }
            }
        }
    }
}
