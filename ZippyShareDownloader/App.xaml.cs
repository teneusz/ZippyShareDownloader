using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ZippyShareDownloader.Entity;
using ZippyShareDownloader.util;
using ZippyShareDownloader.View;

namespace ZippyShareDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly MainViewModel _viewModel = MainViewModel.InstatnceMainViewModel;

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            List<DownloadEntity> entities = _viewModel.Downloads.Where(entity => entity.SaveToFile).ToList();
            SerializerUtils.SaveDownloadEntities(entities);
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            foreach (var entity in SerializerUtils.LoadDownloadEntities())
                _viewModel.Downloads.Add(entity);
        }
    }
}