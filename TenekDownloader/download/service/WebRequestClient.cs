using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using log4net;
using log4net.Core;
using Prism.Mvvm;
using TenekDownloader.download.model;
using TenekDownloader.download.stream;
using TenekDownloader.link;
using TenekDownloader.Properties;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace TenekDownloader.download.service
{
    public class WebRequestClient : BindableBase
    {
        #region Constructor and Events

        public WebRequestClient(DownloadEntity downloadEntity)
        {
            DownloadEntity = downloadEntity;
            BufferSize = 1024; // Buffer size is 1KB
            MaxCacheSize = SettingsHelper.MemoryCacheSize * 1024; // Default cache size is 1MB
            BufferCountPerNotification = 64;
            var interpreter = ServicesEnum.ValueOf(DownloadEntity.LinkInfo.ServiceName).CreateInstace();
            interpreter.ProcessLink(DownloadEntity.LinkInfo?.OrignalLink);
            DownloadEntity.LinkInfo = interpreter.LinkInfo;
            DownloadEntity.LinkInfo.DownloadLocation = AbstractDownloadService.ProcessDownloadLocation(DownloadEntity);
            DownloadEntity.LinkInfo.Uri = new Uri(DownloadEntity.LinkInfo.DownloadLink, UriKind.Absolute);
            DownloadEntity.WebRequestClient = this;

            SupportsRange = false;
            HasError = false;
            OpenFileOnCompletion = false;
            TempFileCreated = false;
            IsBatch = false;
            BatchUrlChecked = false;
            SpeedLimitChanged = false;
            _speedUpdateCount = 0;
            _recentAverageRate = 0;
        }

        #endregion

        #region Fields and Properties

        private ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DownloadEntity DownloadEntity { get; }

        private static readonly SettingsHelper SettingsHelper = new SettingsHelper();

        // Username and password for accessing the HTTP server
        private readonly NetworkCredential _serverLogin = null;

        // HTTP proxy server information
        private readonly WebProxy _proxy = null;

        // Thread for the download process
        private Thread _downloadThread;

        // Temporary file path
        private string TempDownloadPath => DownloadEntity.LinkInfo.DownloadLocation + ".tmp";

        // List of download speed values in the last 10 seconds
        private readonly List<int> _downloadRates = new List<int>();

        // Average download speed in the last 10 seconds, used for calculating the time left to complete the download
        private int _recentAverageRate;

        public event EventHandler AfterDownload;

        // Time left to complete the download
        private string TimeLeft
        {
            get
            {
                if (_recentAverageRate <= 0 || DownloadEntity.Status != DownloadStatus.Downloading || HasError ||
                    DownloadEntity?.LinkInfo?.FileSize == null) return string.Empty;

                var secondsLeft = (DownloadEntity.LinkInfo.FileSize
                                   - DownloadEntity.LinkInfo.DownloadedSize
                                   + DownloadEntity.LinkInfo.CachedSize) / _recentAverageRate;

                var span = TimeSpan.FromSeconds(secondsLeft);

                return span.ToString(@"%h\:mm\:ss");
            }
        }


        // Elapsed time (doesn't include the time period when the download was paused)
        private TimeSpan _elapsedTime;

        // Time when the download was last started
        private DateTime _lastStartTime;


        // Time and size of downloaded data in the last calculation of download speed
        private DateTime _lastNotificationTime;
        private long _lastNotificationDownloadedSize;

        // Last update time of the DataGrid item
        private DateTime LastUpdateTime { get; set; }

        // Server supports the Range header (resuming the download)
        private bool SupportsRange { get; set; }

        // There was an error during download
        private bool HasError { get; set; }

        // Open file as soon as the download is completed
        private bool OpenFileOnCompletion { get; }

        // Temporary file was created
        private bool TempFileCreated { get; set; }

        // Download is selected in the DataGrid

        // Download is part of a batch
        private bool IsBatch { get; }

        // Batch URL was checked
        private bool BatchUrlChecked { get; set; }

        // Speed limit was changed
        private bool SpeedLimitChanged { get; set; }

        private int _speedUpdateCount;

        // Download buffer count per notification (DownloadProgressChanged event)
        private int BufferCountPerNotification { get; }

        // Buffer size
        private int BufferSize { get; }

        // Size of downloaded data in the cache memory
        // Maximum cache size
        private int MaxCacheSize { get; }

        // Used for blocking other processes when a file is being created or written to
        private static readonly object FileLocker = new object();
        private int _downloadSpeed;

        #endregion

        #region Event Handlers

        // DownloadProgressChanged event handler
        private void DownloadProgressChangedHandler()
        {
            // Update the UI every second
            if (DateTime.UtcNow <= LastUpdateTime.AddSeconds(1)) return;
            CalculateDownloadSpeed();
            CalculateAverageRate();
            UpdateDownloadDisplay();
            LastUpdateTime = DateTime.UtcNow;
        }

        // DownloadCompleted event handler
        private void DownloadCompletedHandler()
        {
            if (!HasError)
            {
                // If the file already exists, delete it
                if (File.Exists(DownloadEntity.LinkInfo.DownloadLocation))
                    File.Delete(DownloadEntity.LinkInfo.DownloadLocation);

                // Convert the temporary (.tmp) file to the actual (requested) file
                if (File.Exists(TempDownloadPath))
                    File.Move(TempDownloadPath, DownloadEntity.LinkInfo.DownloadLocation);

                DownloadEntity.Status = DownloadStatus.Completed;
                UpdateDownloadDisplay();

                if (OpenFileOnCompletion && File.Exists(DownloadEntity.LinkInfo.DownloadLocation))
                    Process.Start(DownloadEntity.LinkInfo.DownloadLocation);
            }
            else
            {
                DownloadEntity.Status = DownloadStatus.Error;
                UpdateDownloadDisplay();
            }

            AfterDownload?.Invoke(DownloadEntity, null);
        }

        #endregion

        #region Methods

        // Check URL to get file size, set login and/or proxy server information, check if the server supports the Range header
        private void CheckUrl()
        {
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(DownloadEntity.LinkInfo.DownloadLink);
                webRequest.Method = "HEAD";
                webRequest.Timeout = 1000;

                if (_serverLogin != null)
                {
                    webRequest.PreAuthenticate = true;
                    webRequest.Credentials = _serverLogin;
                }
                else
                {
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                }

                //                if (Settings.ManualProxyConfig && Settings.HttpProxy != String.Empty)
                //                {
                //                    this.Proxy = new WebProxy();
                //                    this.Proxy.Address = new Uri("http://" + Settings.HttpProxy + ":" + Settings.Default.ProxyPort);
                //                    this.Proxy.BypassProxyOnLocal = false;
                //                    if (Settings.Default.ProxyUsername != String.Empty && Settings.Default.ProxyPassword != String.Empty)
                //                    {
                //                        this.Proxy.Credentials = new NetworkCredential(Settings.Default.ProxyUsername, Settings.Default.ProxyPassword);
                //                    }
                //                }
                webRequest.Proxy = _proxy ?? WebRequest.DefaultWebProxy;

                using (var response = webRequest.GetResponse())
                {
                    foreach (var header in response.Headers.AllKeys)
                        if (header.Equals("Accept-Ranges", StringComparison.OrdinalIgnoreCase))
                            SupportsRange = true;

                    DownloadEntity.LinkInfo.FileSize = response.ContentLength;

                    if (!(DownloadEntity.LinkInfo.FileSize <= 0)) return;
                    _log.Debug("The requested file does not exists! \n" + DownloadEntity.ToString());
                   // MessageBox.Show("The requested file does not exist!", "Error", MessageBoxButton.OK,
                     //   MessageBoxImage.Error);
                    HasError = true;
                }
            }
            catch (Exception e)
            {
                DownloadEntity.Errors = DownloadEntity.Errors +Environment.NewLine+ e.Message;
                HasError = true;
            }
        }

        // Batch download URL check
        private void CheckBatchUrl()
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(DownloadEntity.LinkInfo.DownloadLink);
            webRequest.Method = "HEAD";

            if (_serverLogin != null)
            {
                webRequest.PreAuthenticate = true;
                webRequest.Credentials = _serverLogin;
            }
            else
            {
                webRequest.Credentials = CredentialCache.DefaultCredentials;
            }

            //            if (Settings.Default.ManualProxyConfig && Settings.Default.HttpProxy != String.Empty)
            //            {
            //                this.Proxy = new WebProxy();
            //                this.Proxy.Address = new Uri("http://" + Settings.Default.HttpProxy + ":" + Settings.Default.ProxyPort);
            //                this.Proxy.BypassProxyOnLocal = false;
            //                if (Settings.Default.ProxyUsername != String.Empty && Settings.Default.ProxyPassword != String.Empty)
            //                {
            //                    this.Proxy.Credentials = new NetworkCredential(Settings.Default.ProxyUsername, Settings.Default.ProxyPassword);
            //                }
            //            }
            webRequest.Proxy = _proxy ?? WebRequest.DefaultWebProxy;

            using (var response = webRequest.GetResponse())
            {
                foreach (var header in response.Headers.AllKeys)
                    if (header.Equals("Accept-Ranges", StringComparison.OrdinalIgnoreCase))
                        SupportsRange = true;

                DownloadEntity.LinkInfo.FileSize = response.ContentLength;

                if (!(DownloadEntity.LinkInfo.FileSize <= 0)) return;
                DownloadEntity.LinkInfo.FileSize = 0;
                HasError = true;
            }
        }

        // Create temporary file
        private void CreateTempFile()
        {
            // Lock this block of code so other threads and processes don't interfere with file creation
            lock (FileLocker)
            {
                using (var fileStream = File.Create(TempDownloadPath))
                {
                    long createdSize = 0;
                    var buffer = new byte[4096];
                    while (createdSize < DownloadEntity.LinkInfo.FileSize)
                    {
                        var bufferSize = DownloadEntity.LinkInfo.FileSize - createdSize < 4096
                            ? (int)(DownloadEntity.LinkInfo.FileSize - createdSize)
                            : 4096;
                        fileStream.Write(buffer, 0, bufferSize);
                        createdSize += bufferSize;
                    }
                }
            }
        }

        // Write data from the cache to the temporary file
        private void WriteCacheToFile(Stream downloadCache, int cachedSize)
        {
            // Block other threads and processes from using the file
            lock (FileLocker)
            {
                using (var fileStream = new FileStream(TempDownloadPath, FileMode.Open))
                {
                    var cacheContent = new byte[cachedSize];
                    downloadCache.Seek(0, SeekOrigin.Begin);
                    downloadCache.Read(cacheContent, 0, cachedSize);
                    fileStream.Seek((long)DownloadEntity.LinkInfo.DownloadedSize, SeekOrigin.Begin);
                    fileStream.Write(cacheContent, 0, cachedSize);
                }
            }
        }

        // Calculate download speed
        private void CalculateDownloadSpeed()
        {
            var now = DateTime.UtcNow;
            var interval = now - _lastNotificationTime;
            var timeDiff = interval.TotalSeconds;
            var sizeDiff = DownloadEntity.LinkInfo.DownloadedSize + DownloadEntity.LinkInfo.CachedSize -
                           _lastNotificationDownloadedSize;

            _downloadSpeed = (int)Math.Floor(sizeDiff / timeDiff);

            _downloadRates.Add(_downloadSpeed);

            _lastNotificationDownloadedSize =
                (long)DownloadEntity.LinkInfo.DownloadedSize + DownloadEntity.LinkInfo.CachedSize;
            _lastNotificationTime = now;
        }

        // Calculate average download speed in the last 10 seconds
        private void CalculateAverageRate()
        {
            if (_downloadRates.Count <= 0) return;
            if (_downloadRates.Count > 10)
                _downloadRates.RemoveAt(0);

            var rateSum = 0;
            _recentAverageRate = 0;
            foreach (var rate in _downloadRates) rateSum += rate;

            _recentAverageRate = rateSum / _downloadRates.Count;
        }

        // Update download display (on downloadsGrid and propertiesGrid controls)
        private void UpdateDownloadDisplay()
        {
            _speedUpdateCount++;
            if (_speedUpdateCount == 4)
                _speedUpdateCount = 0;

            DownloadEntity.LinkInfo.DownloadSpeed = _downloadSpeed;
            DownloadEntity.LinkInfo.TimeLeft = TimeLeft;
        }

        // Reset download properties to default values
        private void ResetProperties()
        {
            HasError = false;
            TempFileCreated = false;
            DownloadEntity.LinkInfo.DownloadedSize = 0;
            DownloadEntity.LinkInfo.CachedSize = 0;
            _speedUpdateCount = 0;
            _recentAverageRate = 0;
            _downloadRates.Clear();
            _elapsedTime = new TimeSpan();
        }

        // Start or continue download
        public void Start()
        {
            if (DownloadEntity.Status != DownloadStatus.Preparing && DownloadEntity.Status != DownloadStatus.Paused &&
                DownloadEntity.Status != DownloadStatus.Queued && !HasError) return;
            CheckUrl();
            if (!SupportsRange && DownloadEntity.LinkInfo.DownloadedSize > 0)
            {
                HasError = true;
                return;
            }

            HasError = false;
            DownloadEntity.Status = DownloadStatus.Waiting;

            if (AbstractDownloadService.ActiveDownloads > SettingsHelper.MaxDownloadingCount)
            {
                DownloadEntity.Status = DownloadStatus.Queued;
                return;
            }

            // Start the download thread
            _downloadThread = new Thread(DownloadFile) { IsBackground = true };
            _downloadThread.Start();
        }

        // Pause download
        public void Pause()
        {
            if (DownloadEntity.Status == DownloadStatus.Waiting || DownloadEntity.Status == DownloadStatus.Downloading)
                DownloadEntity.Status = DownloadStatus.Pausing;
            if (DownloadEntity.Status == DownloadStatus.Queued) DownloadEntity.Status = DownloadStatus.Paused;
        }

        // Restart download
        public void Restart()
        {
            if (!HasError && DownloadEntity.Status != DownloadStatus.Completed) return;
            if (File.Exists(TempDownloadPath)) File.Delete(TempDownloadPath);
            if (File.Exists(DownloadEntity.LinkInfo.DownloadLocation))
                File.Delete(DownloadEntity.LinkInfo.DownloadLocation);

            ResetProperties();
            DownloadEntity.Status = DownloadStatus.Waiting;
            UpdateDownloadDisplay();

            if (AbstractDownloadService.ActiveDownloads > SettingsHelper.MaxDownloadingCount)
            {
                DownloadEntity.Status = DownloadStatus.Queued;
                RaisePropertyChanged("StatusString");
                return;
            }

            _downloadThread = new Thread(DownloadFile) { IsBackground = true };
            _downloadThread.Start();
        }

        // Download file bytes from the HTTP response stream
        private void DownloadFile()
        {
            HttpWebResponse webResponse = null;
            Stream responseStream = null;
            ThrottledStream throttledStream = null;
            MemoryStream downloadCache = null;
            _speedUpdateCount = 0;
            _recentAverageRate = 0;
            if (_downloadRates.Count > 0)
                _downloadRates.Clear();

            try
            {
                if (IsBatch && !BatchUrlChecked)
                {
                    CheckBatchUrl();
                    if (HasError) return;
                    BatchUrlChecked = true;
                }

                if (!TempFileCreated)
                {
                    // Reserve local disk space for the file
                    CreateTempFile();
                    TempFileCreated = true;
                }

                _lastStartTime = DateTime.UtcNow;

                if (DownloadEntity.Status == DownloadStatus.Waiting)
                    DownloadEntity.Status = DownloadStatus.Downloading;

                // Create request to the server to download the file
                var webRequest = (HttpWebRequest)WebRequest.Create(DownloadEntity.LinkInfo.DownloadLink);
                webRequest.Method = "GET";

                if (_serverLogin != null)
                {
                    webRequest.PreAuthenticate = true;
                    webRequest.Credentials = _serverLogin;
                }
                else
                {
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                }

                webRequest.Proxy = _proxy ?? WebRequest.DefaultWebProxy;

                // Set download starting point
                //                webRequest.AddRange(DownloadedSize);
                // Get response from the server and the response stream
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();

                // Set a 5 second timeout, in case of internet connection break
                if (responseStream != null)
                {
                    responseStream.ReadTimeout = 5000;

                    // Set speed limit
                    long maxBytesPerSecond;
                    if (SettingsHelper.EnableSpeedLimit)
                        maxBytesPerSecond =
                            SettingsHelper.SpeedLimit * 1024 / AbstractDownloadService.ActiveDownloads;
                    else
                        maxBytesPerSecond = ThrottledStream.Infinite;

                    throttledStream = new ThrottledStream(responseStream, maxBytesPerSecond);

                    // Create memory cache with the specified size
                    downloadCache = new MemoryStream(MaxCacheSize);

                    // Create 1KB buffer
                    var downloadBuffer = new byte[BufferSize];

                    DownloadEntity.LinkInfo.CachedSize = 0;
                    var receivedBufferCount = 0;

                    // Download file bytes until the download is paused or completed
                    while (true)
                    {
                        if (SpeedLimitChanged)
                        {
                            if (SettingsHelper.EnableSpeedLimit)
                                maxBytesPerSecond = SettingsHelper.SpeedLimit * 1024 /
                                                    AbstractDownloadService.ActiveDownloads;
                            else
                                maxBytesPerSecond = ThrottledStream.Infinite;

                            throttledStream.MaximumBytesPerSecond = maxBytesPerSecond;
                            SpeedLimitChanged = false;
                        }

                        // Read data from the response stream and write it to the buffer
                        var bytesSize = throttledStream.Read(downloadBuffer, 0, downloadBuffer.Length);

                        // If the cache is full or the download is paused or completed, write data from the cache to the temporary file
                        if (DownloadEntity.Status != DownloadStatus.Downloading || bytesSize == 0 ||
                            MaxCacheSize < DownloadEntity.LinkInfo.CachedSize + bytesSize)
                        {
                            // Write data from the cache to the temporary file
                            WriteCacheToFile(downloadCache, DownloadEntity.LinkInfo.CachedSize);

                            DownloadEntity.LinkInfo.DownloadedSize += DownloadEntity.LinkInfo.CachedSize;

                            // Reset the cache
                            downloadCache.Seek(0, SeekOrigin.Begin);
                            DownloadEntity.LinkInfo.CachedSize = 0;

                            // Stop downloading the file if the download is paused or completed
                            if (DownloadEntity.Status != DownloadStatus.Downloading || bytesSize == 0) break;
                        }

                        // Write data from the buffer to the cache
                        downloadCache.Write(downloadBuffer, 0, bytesSize);
                        DownloadEntity.LinkInfo.CachedSize += bytesSize;

                        receivedBufferCount++;
                        if (receivedBufferCount == BufferCountPerNotification) receivedBufferCount = 0;

                        DownloadProgressChangedHandler();
                    }
                }

                // Update elapsed time when the download is paused or completed
                _elapsedTime = _elapsedTime.Add(DateTime.UtcNow - _lastStartTime);

                switch (DownloadEntity.Status)
                {
                    // Change status
                    case DownloadStatus.Deleting:
                        return;
                    case DownloadStatus.Pausing:
                        DownloadEntity.Status = DownloadStatus.Paused;
                        UpdateDownloadDisplay();
                        break;
                    case DownloadStatus.Queued:
                        UpdateDownloadDisplay();
                        break;
                    default:
                        _log.Debug("Unexpected downloading status: " + DownloadEntity.Status);
                        break;
                }

                DownloadCompletedHandler();
            }
            catch (Exception ex)
            {
                _log.Error("Error while downloading", ex);
                DownloadEntity.Status = DownloadStatus.Error;
                HasError = true;
            }
            finally
            {
                // Close the response stream and cache, stop the thread
                responseStream?.Close();
                throttledStream?.Close();
                webResponse?.Close();
                downloadCache?.Close();
                _downloadThread?.Abort();
            }
        }

        #endregion
    }
}