using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Speedrungames_Steam_Checker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static HttpClient httpClient = new HttpClient();
        public static Dictionary<string, bool> Unfiltered = new Dictionary<string, bool>();
        public static Dictionary<string, bool> Owned = new Dictionary<string, bool>();
        public static Dictionary<string, bool> NotOwned = new Dictionary<string, bool>();
        public static string SteamWebAPIKey = ConfigurationManager.AppSettings.Get("SteamWebAPIKey");
        public static string SteamID64 = ConfigurationManager.AppSettings.Get("SteamID64");

        public MainWindow()
        {
            InitializeComponent();
            Datagrid.IsReadOnly = true;
            Datagrid.ItemsSource = Unfiltered;
        }

        public static async Task GetSteam(MainWindow wnd)
        {
            
            wnd.rbNoFilter.IsEnabled = false;
            wnd.rbNonOwned.IsEnabled = false;
            wnd.rbOwned.IsEnabled = false;
            // The actual Get method
            using (var result = await httpClient.GetAsync("http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key=" + SteamWebAPIKey + "&steamid=" + SteamID64 + "&include_appinfo=1&format=json"))
            {
                string content = await result.Content.ReadAsStringAsync();

                var jss = new JavaScriptSerializer();
                dynamic array = jss.DeserializeObject(content);
                
                var cnt = array["response"]["game_count"];

                wnd.statusLabel.Content = "Get Steam Games 0/" + cnt;
                wnd.statusProgress.Maximum = cnt;
                
                for (int i = 0; i < cnt; i++)
                {
                    wnd.statusLabel.Content = "Get Steam Games " + i + 1 + "/" + cnt;
                    wnd.statusProgress.Value = i + 1;
                    var name = array["response"]["games"][i]["name"];

                    Unfiltered.Add(name, false);
                }

                wnd.Datagrid.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
               // wnd.Datagrid.Columns[0].Header = "Game Name";
               // wnd.Datagrid.Columns[1].Header = "Do I own?";

                await GetSpeedrun(wnd);
            }
        }

        private static Action EmptyDelegate = delegate () { };

        public static async Task GetSpeedrun(MainWindow wnd)
        {
            var jss = new JavaScriptSerializer();

            wnd.statusLabel.Content = "Search Speedrun.com for Matches 0/" + Unfiltered.Count;

            for (int si = 0; si < Unfiltered.Count; si++)
            {
                wnd.statusProgress.Value = si + 1;

                HttpResponseMessage result;
                using (result = await httpClient.GetAsync("https://www.speedrun.com/api/v1/games?platform=8gej2n93&name=" + Unfiltered.ElementAt(si).Key))
                {
                    if ((int)result.StatusCode == 420)
                    {
                        wnd.statusLabel.Content = "Requests per Minute Reached, waiting for 60,5seconds\nSearch Speedrun.com for Matches " + (si + 1) + "/" + Unfiltered.Count;                     
                        wnd.statusLabel.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);

                        await Task.Delay(60500);
                        result = await httpClient.GetAsync("https://www.speedrun.com/api/v1/games?platform=8gej2n93&name=" + Unfiltered.ElementAt(si).Key);
                    }

                    wnd.statusLabel.Content = "Search Speedrun.com for Matches " + (si+1) + "/" + Unfiltered.Count;
                    string content = await result.Content.ReadAsStringAsync();

                    dynamic array = jss.DeserializeObject(content);
                    var cnts = array["data"].Length;

                    for (int i = 0; i < cnts; i++)
                    {
                        var name = array["data"][i]["names"]["international"];
                        if (Unfiltered.ElementAt(si).Key == name)
                        {
                            Unfiltered[Unfiltered.ElementAt(si).Key] = true;
                            Owned[Unfiltered.ElementAt(si).Key] = true;
                            break;
                        }
                    }

                    if (!Unfiltered[Unfiltered.ElementAt(si).Key])
                        NotOwned[Unfiltered.ElementAt(si).Key] = false;
                }
            }

            wnd.rbNoFilter.IsEnabled = true;
            wnd.rbNonOwned.IsEnabled = true;
            wnd.rbOwned.IsEnabled = true;
        }

        private void rbNoFilter_Checked(object sender, RoutedEventArgs e)
        {
            Datagrid.ItemsSource = Unfiltered;
        }

        private void rbOwned_Checked(object sender, RoutedEventArgs e)
        {
            Datagrid.ItemsSource = Owned;
        }

        private void rbNonOwned_Checked(object sender, RoutedEventArgs e)
        {
            Datagrid.ItemsSource = NotOwned;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings s = new Settings();
            s.Owner = this;
            s.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GetSteam(this);
        }
    }
}
