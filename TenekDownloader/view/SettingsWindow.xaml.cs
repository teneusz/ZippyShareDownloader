using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TenekDownloader.viewModel;

namespace TenekDownloader.view
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
//        public string SevenZipLibraryLocation
//        {
//            get => Properties.Settings.Default.SevenZipDll;
//            set
//            {
//                Properties.Settings.Default.SevenZipDll = value;
//                Properties.Settings.Default.Save();
//            }
//        }
//
//        public string DownloadLocation
//        {
//            get => Properties.Settings.Default.DownloadLocation;
//            set
//            {
//                Properties.Settings.Default.DownloadLocation = value;
//                Properties.Settings.Default.Save();
//            }
//        }
        public SettingsWindow()
        {

            InitializeComponent();
            DataContext = new ViewModel();
        }
    }
}
