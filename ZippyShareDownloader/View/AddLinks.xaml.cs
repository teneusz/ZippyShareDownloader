using System.Windows;

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