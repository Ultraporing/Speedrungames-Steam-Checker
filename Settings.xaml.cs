using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Speedrungames_Steam_Checker
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            id64tb.Text = MainWindow.SteamID64;
            apitb.Text = MainWindow.SteamWebAPIKey;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UpdateSetting("SteamID64", id64tb.Text);
            MainWindow.SteamID64 = id64tb.Text;

            UpdateSetting("SteamWebAPIKey", apitb.Text);
            MainWindow.SteamWebAPIKey = apitb.Text;
        }

        private static void UpdateSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

            try
            {
                configuration.AppSettings.Settings[key].Value = value;
            }
            catch (Exception)
            {

                configuration.AppSettings.Settings.Add(key, value);
            }
            
            configuration.Save();

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
