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
using log4net;
using ZippyShareDownloader.Entity;

namespace ZippyShareDownloader.View
{
    /// <summary>
    /// Interaction logic for AddLinks.xaml
    /// </summary>
    public partial class AddLinks : Window
    {
        private readonly MainViewModel _viewModel = MainViewModel.InstatnceMainViewModel;
        private static readonly ILog log = LogManager.GetLogger(typeof(AddLinks));
        public AddLinks()
        {
            InitializeComponent();
        }

        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            log.Debug("start -- saveOnClick");
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);
            var tab = textRange.Text.Split('\n').Select(link => link.Trim()).ToList();

            foreach (var s in tab)
            {
                if (s.Length > 0)
                    _viewModel.Downloads.Add(new DownloadEntity {ServiceLink = s});
            }
            DialogResult = true;
            log.Debug("end -- saveOnClick");
        }

        private void CancelOnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}