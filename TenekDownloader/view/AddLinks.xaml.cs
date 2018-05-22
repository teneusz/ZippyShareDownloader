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
using TenekDownloader.download.model;

namespace TenekDownloader.view
{
    public partial class AddLinks : Window
    {
        public AddLinks()
        {
            InitializeComponent();
        }

        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);
            var tab = textRange.Text.Split('\n').Select(link => link.Trim()).ToList();
            if (IsInGroup?.IsChecked ?? false)
            {
                DownloadGroups.Add(new DownloadGroup(tab, GroupName.Text, IsDecompressedAfter.IsChecked));
            }
            else
            {
                foreach (var link in tab)
                {
                    DownloadGroups.Add(
                        new DownloadGroup(new List<string>() {link}, null, IsDecompressedAfter.IsChecked));
                }
            }

            DialogResult = true;
        }

        public List<DownloadGroup> DownloadGroups { get; set; } = new List<DownloadGroup>();

        private void CancelOnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}