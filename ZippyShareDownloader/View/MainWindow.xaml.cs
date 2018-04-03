using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using ZippyShareDownloader.View;

namespace ZippyShareDownloader
{
    /// <inheritdoc>
    ///     <cref></cref>
    /// </inheritdoc>
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
       private readonly MainViewModel _viewModel = MainViewModel.InstatnceMainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = _viewModel;
        }

    }

    public sealed class ObservableCollectionEx<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        // this collection also reacts to changes in its components' properties

        public ObservableCollectionEx()
        {
            CollectionChanged += ObservableCollectionEx_CollectionChanged;
        }

        private void ObservableCollectionEx_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (T item in e.OldItems)
                {
                    //Removed items
                    item.PropertyChanged -= EntityViewModelPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (T item in e.NewItems)
                {
                    //Added items
                    item.PropertyChanged += EntityViewModelPropertyChanged;
                }
            }
        }

        public void EntityViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //This will get called when the property of an object inside the collection changes - note you must make it a 'reset' - dunno why
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(args);
        }
    }
}