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
using ZippyShareDownloader.Html;
using ZippyShareDownloader.util;

namespace ZippyShareDownloader.View
{
    /// <summary>
    /// Interaction logic for AddLinks.xaml
    /// </summary>
    public partial class AddLinks : Window
    {
        private readonly MainViewModel _viewModel = MainViewModel.InstatnceMainViewModel;
        private static readonly ILog Log = LogManager.GetLogger(typeof(AddLinks));

        public AddLinks()
        {
            InitializeComponent();
        }

        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            Log.Debug("start -- saveOnClick");
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);
            var tab = textRange.Text.Split('\n').Select(link => link.Trim()).ToList();
            var downloadGroup = new DownloadGroup(tab, GroupName.Text,IsDecompressedAfter.IsChecked);
            _viewModel.DownloadGroups.Add(downloadGroup);
            DialogResult = true;
            Log.Debug("end -- saveOnClick");
        }

        private void CancelOnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}