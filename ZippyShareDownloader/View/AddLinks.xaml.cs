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
using Prism.Commands;
using ZippyShareDownloader.Entity;
using ZippyShareDownloader.Html;
using ZippyShareDownloader.util;
using ZippyShareDownloader.ViewModel;

namespace ZippyShareDownloader.View
{
    /// <summary>
    /// Interaction logic for AddLinks.xaml
    /// </summary>
    public partial class AddLinks : Window
    {
        public AddLinks()
        {
            InitializeComponent();
            DataContext = new ViewModel.AddLinksVM();
        }
    }
}