using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ZippyShareDownloader.Model;
using ZippyShareDownloader.util;
using ZippyShareDownloader.View;
using ZippyShareDownloader.ViewModel;

namespace ZippyShareDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly MainWindowVM _viewModel = new MainWindowVM();

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            ConfigHelper helper = new ConfigHelper
            {
               DownloadGroups = MainWindowVM.DownloadGroups
            };
            SerializerUtils.SaveConfig(helper);
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var helper = SerializerUtils.LoadConfig();
            foreach (var group in helper.DownloadGroups)
            {
                MainWindowVM.DownloadGroups.Add(group);
                foreach (var entity in group.DownloadEntities)
                {
                    _viewModel.Downloads.Add(entity);
                    entity.DownloadGroup = group;
                }
            }
        
        }

        [Serializable]
        public class ConfigHelper
        {
           
            private ObservableCollection<DownloadGroup> _downloadGroups = new ObservableCollection<DownloadGroup>();
            public ObservableCollection<DownloadGroup> DownloadGroups
            {
                get => _downloadGroups;
                set => _downloadGroups = value;
            }

            public ConfigHelper()
            {
            }
        }
    }
}