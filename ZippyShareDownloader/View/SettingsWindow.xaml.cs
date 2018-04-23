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
using ZippyShareDownloader.ViewModel;

namespace ZippyShareDownloader.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly MainWindowVM _viewModel = MainWindowVM.InstatnceMainViewModel;
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
