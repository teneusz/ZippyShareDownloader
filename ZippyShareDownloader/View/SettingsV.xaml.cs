using System.Windows;
using ZippyShareDownloader.ViewModel;

namespace ZippyShareDownloader.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsV : Window
    {
        public SettingsV()
        {
            InitializeComponent();
            DataContext = new SettingsVM();
        }
    }
}
