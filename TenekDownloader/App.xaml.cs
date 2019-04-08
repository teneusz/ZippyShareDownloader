using System.Windows;
using System.Windows.Threading;

namespace TenekDownloader
{
	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public App()
        {
            this.Dispatcher.UnhandledException += App_DispatcherUnhandledException;
        }

        protected override void OnExit(ExitEventArgs e)
		{
            log.Debug("Exit from application");
			base.OnExit(e);
		}

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            log.Debug(e.Exception.Message, e.Exception);
            // Prevent default unhandled exception processing
            e.Handled = true;
        }
    }
}