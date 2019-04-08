using System;
using System.Globalization;
using System.Windows;
using MahApps.Metro.Controls;

namespace TenekDownloader
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MainWindow()
		{
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                log.Debug(ex.Message, ex);
            }
        }
	}
}