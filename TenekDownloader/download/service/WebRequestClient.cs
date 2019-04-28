using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Prism.Mvvm;
using TenekDownloader.download.model;
using TenekDownloader.download.service.impl;
using TenekDownloader.download.stream;
using TenekDownloader.link;
using TenekDownloader.util;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace TenekDownloader.download.service
{
    public class WebRequestClient : BindableBase
    {
        #region Fields and Properties

        private readonly DownloadEntity _downloadEntity;

        public DownloadEntity DownloadEntity => _downloadEntity;

        private static readonly SettingsHelper SettingsHelper = new SettingsHelper();

        // Username and password for accessing the HTTP server
        public readonly NetworkCredential ServerLogin = null;

        // HTTP proxy server information
        public WebProxy Proxy = null;

        // Thread for the download process
        public Thread DownloadThread;

        // Temporary file path
        public string TempDownloadPath => _downloadEntity.LinkInfo.DownloadLocation +".tmp";
        
        // List of download speed values in the last 10 seconds
        private List<int> _downloadRates = new List<int>();

        // Average download speed in the last 10 seconds, used for calculating the time left to complete the download
        private int recentAverageRate;

        public event EventHandler AfterDownload;

        // Time left to complete the download
        public string TimeLeft
        {
            get
            {
                if (recentAverageRate > 0 && _downloadEntity.Status == DownloadStatus.Downloading && !HasError && _downloadEntity?.LinkInfo?.FileSize != null)
                {
                    double secondsLeft = (_downloadEntity.LinkInfo.FileSize
                                          - _downloadEntity.LinkInfo.DownloadedSize
                                          + _downloadEntity.LinkInfo.CachedSize) / recentAverageRate;

                    TimeSpan span = TimeSpan.FromSeconds(secondsLeft);
                    
                    return span.ToString(@"%h\:%m\:%s");
                }
                return string.Empty;
            }
        }


        // Elapsed time (doesn't include the time period when the download was paused)
        public TimeSpan ElapsedTime;

        public DateTime CompletedOn { get; private set; }

        // Time when the download was last started
        private DateTime _lastStartTime;


        // Time and size of downloaded data in the last calculaction of download speed
        private DateTime _lastNotificationTime;
        private long _lastNotificationDownloadedSize;

        // Last update time of the DataGrid item
        public DateTime LastUpdateTime { get; set; }

        // Server supports the Range header (resuming the download)
        public bool SupportsRange { get; set; }

        // There was an error during download
        public bool HasError { get; set; }

        // Open file as soon as the download is completed
        public bool OpenFileOnCompletion { get; set; }

        // Temporary file was created
        public bool TempFileCreated { get; set; }

        // Download is selected in the DataGrid
        public bool IsSelected { get; set; }

        // Download is part of a batch
        public bool IsBatch { get; set; }

        // Batch URL was checked
        public bool BatchUrlChecked { get; set; }

        // Speed limit was changed
        public bool SpeedLimitChanged { get; set; }

        private int speedUpdateCount;

        // Download buffer count per notification (DownloadProgressChanged event)
        public int BufferCountPerNotification { get; set; }

        // Buffer size
        public int BufferSize { get; set; }

        // Size of downloaded data in the cache memory
        // Maxiumum cache size
        public int MaxCacheSize { get; set; }
        
        // Used for blocking other processes when a file is being created or written to
        private static object fileLocker = new object();
        private int _downloadSpeed;

        #endregion

        #region Constructor and Events

        public WebRequestClient(DownloadEntity downloadEntity)
        {
            _downloadEntity = downloadEntity;
            BufferSize = 1024; // Buffer size is 1KB
            MaxCacheSize = SettingsHelper.MemoryCacheSize * 1024; // Default cache size is 1MB
            BufferCountPerNotification = 64;
            var interpreter = ServicesEnum.ValueOf(_downloadEntity.LinkInfo.ServiceName).CreateInstace();
            interpreter.ProcessLink(_downloadEntity.LinkInfo?.OrignalLink);
            _downloadEntity.LinkInfo = interpreter.LinkInfo;
            _downloadEntity.LinkInfo.DownloadLocation = AbstractDownloadService.ProcessDownloadLocation(_downloadEntity);
            _downloadEntity.LinkInfo.Uri = new Uri(_downloadEntity.LinkInfo.DownloadLink, UriKind.Absolute);

                SupportsRange = false;
            HasError = false;
            OpenFileOnCompletion = false;
            TempFileCreated = false;
            IsSelected = false;
            IsBatch = false;
            BatchUrlChecked = false;
            SpeedLimitChanged = false;
            speedUpdateCount = 0;
            recentAverageRate = 0;
        }

        #endregion

        #region Event Handlers

        // DownloadProgressChanged event handler
        public void DownloadProgressChangedHandler(object sender, EventArgs e)
        {
            // Update the UI every second
            if (DateTime.UtcNow > LastUpdateTime.AddSeconds(1))
            {
                CalculateDownloadSpeed();
                CalculateAverageRate();
                UpdateDownloadDisplay();
                LastUpdateTime = DateTime.UtcNow;
            }
        }

        // DownloadCompleted event handler
        public void DownloadCompletedHandler(object sender, EventArgs e)
        {
            if (!HasError)
            {
                // If the file already exists, delete it
                if (File.Exists(_downloadEntity.LinkInfo.DownloadLocation))
                {
                    File.Delete(_downloadEntity.LinkInfo.DownloadLocation);
                }

                // Convert the temporary (.tmp) file to the actual (requested) file
                if (File.Exists(TempDownloadPath))
                {
                    File.Move(TempDownloadPath, _downloadEntity.LinkInfo.DownloadLocation);
                }

                _downloadEntity.Status = DownloadStatus.Completed;
                UpdateDownloadDisplay();

                if (OpenFileOnCompletion && File.Exists(_downloadEntity.LinkInfo.DownloadLocation))
                {
                    Process.Start(_downloadEntity.LinkInfo.DownloadLocation);
                }
            }
            else
            {
                _downloadEntity.Status = DownloadStatus.Error;
                UpdateDownloadDisplay();
            }
            AfterDownload?.Invoke(null, null);

        }

        #endregion

        #region Methods

        // Check URL to get file size, set login and/or proxy server information, check if the server supports the Range header
        public void CheckUrl()
        {
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(_downloadEntity.LinkInfo.DownloadLink);
                webRequest.Method = "HEAD";
                webRequest.Timeout = 1000;

                if (ServerLogin != null)
                {
                    webRequest.PreAuthenticate = true;
                    webRequest.Credentials = ServerLogin;
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
                webRequest.Proxy = Proxy ?? WebRequest.DefaultWebProxy;

                using (WebResponse response = webRequest.GetResponse())
                {
                    foreach (var header in response.Headers.AllKeys)
                    {
                        if (header.Equals("Accept-Ranges", StringComparison.OrdinalIgnoreCase))
                        {
                            SupportsRange = true;
                        }
                    }

                    _downloadEntity.LinkInfo.FileSize = response.ContentLength;

                    if (!(_downloadEntity.LinkInfo.FileSize <= 0)) return;
                    MessageBox.Show("The requested file does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    HasError = true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error while getting the file information. Please make sure the URL is accessible.",
                                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                HasError = true;
            }
        }

        // Batch download URL check
        private void CheckBatchUrl()
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(_downloadEntity.LinkInfo.DownloadLink);
            webRequest.Method = "HEAD";

            if (ServerLogin != null)
            {
                webRequest.PreAuthenticate = true;
                webRequest.Credentials = ServerLogin;
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
            webRequest.Proxy = Proxy ?? WebRequest.DefaultWebProxy;

            using (var response = webRequest.GetResponse())
            {
                foreach (var header in response.Headers.AllKeys)
                {
                    if (header.Equals("Accept-Ranges", StringComparison.OrdinalIgnoreCase))
                    {
                        SupportsRange = true;
                    }
                }

                _downloadEntity.LinkInfo.FileSize = response.ContentLength;

                if (!(_downloadEntity.LinkInfo.FileSize <= 0)) return;
                _downloadEntity.LinkInfo.FileSize = 0;
                HasError = true;
            }
        }

        // Create temporary file
        private void CreateTempFile()
        {
            // Lock this block of code so other threads and processes don't interfere with file creation
            lock (fileLocker)
            {
                using (var fileStream = File.Create(TempDownloadPath))
                {
                    long createdSize = 0;
                    var buffer = new byte[4096];
                    while (createdSize < _downloadEntity.LinkInfo.FileSize)
                    {
                        var bufferSize = (_downloadEntity.LinkInfo.FileSize - createdSize) < 4096
                            ? (int)(_downloadEntity.LinkInfo.FileSize - createdSize) : 4096;
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
            lock (fileLocker)
            {
                using (var fileStream = new FileStream(TempDownloadPath, FileMode.Open))
                {
                    var cacheContent = new byte[cachedSize];
                    downloadCache.Seek(0, SeekOrigin.Begin);
                    downloadCache.Read(cacheContent, 0, cachedSize);
                    fileStream.Seek((long)_downloadEntity.LinkInfo.DownloadedSize, SeekOrigin.Begin);
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
            var sizeDiff = _downloadEntity.LinkInfo.DownloadedSize + _downloadEntity.LinkInfo.CachedSize - _lastNotificationDownloadedSize;

           _downloadSpeed = (int)Math.Floor(sizeDiff / timeDiff);

            _downloadRates.Add(_downloadSpeed);

            _lastNotificationDownloadedSize = (long)_downloadEntity.LinkInfo.DownloadedSize + _downloadEntity.LinkInfo.CachedSize;
            _lastNotificationTime = now;
        }

        // Calculate average download speed in the last 10 seconds
        private void CalculateAverageRate()
        {
            if (_downloadRates.Count <= 0) return;
            if (_downloadRates.Count > 10)
                _downloadRates.RemoveAt(0);

            var rateSum = 0;
            recentAverageRate = 0;
            foreach (var rate in _downloadRates)
            {
                rateSum += rate;
            }

            recentAverageRate = rateSum / _downloadRates.Count;
        }

        // Update download display (on downloadsGrid and propertiesGrid controls)
        private void UpdateDownloadDisplay()
        {
            speedUpdateCount++;
            if (speedUpdateCount == 4)
                speedUpdateCount = 0;

            _downloadEntity.LinkInfo.DownloadSpeed = _downloadSpeed;
            _downloadEntity.LinkInfo.TimeLeft = TimeLeft;

        }

        // Reset download properties to default values
        private void ResetProperties()
        {
            HasError = false;
            TempFileCreated = false;
            _downloadEntity.LinkInfo.DownloadedSize = 0;
            _downloadEntity.LinkInfo.CachedSize = 0;
            speedUpdateCount = 0;
            recentAverageRate = 0;
            _downloadRates.Clear();
            ElapsedTime = new TimeSpan();
            CompletedOn = DateTime.MinValue;
        }

        // Start or continue download
        public void Start()
        {
            if (_downloadEntity.Status != DownloadStatus.Preparing && _downloadEntity.Status != DownloadStatus.Paused &&
                _downloadEntity.Status != DownloadStatus.Queued && !HasError) return;
            CheckUrl();
            if (!SupportsRange && _downloadEntity.LinkInfo.DownloadedSize > 0)
            {
                HasError = true;
                return;
            }

            HasError = false;
            _downloadEntity.Status = DownloadStatus.Waiting;

            if (AbstractDownloadService.ActiveDownloads > SettingsHelper.MaxDownloadingCount)
            {
                _downloadEntity.Status = DownloadStatus.Queued;
                return;
            }

            // Start the download thread
            DownloadThread = new Thread(DownloadFile) {IsBackground = true};
            DownloadThread.Start();
        }

        // Pause download
        public void Pause()
        {
            if (_downloadEntity.Status == DownloadStatus.Waiting || _downloadEntity.Status == DownloadStatus.Downloading)
            {
                _downloadEntity.Status = DownloadStatus.Pausing;
            }
            if (_downloadEntity.Status == DownloadStatus.Queued)
            {
                _downloadEntity.Status = DownloadStatus.Paused;
            }
        }

        // Restart download
        public void Restart()
        {
            if (!HasError && _downloadEntity.Status != DownloadStatus.Completed) return;
            if (File.Exists(TempDownloadPath))
            {
                File.Delete(TempDownloadPath);
            }
            if (File.Exists(_downloadEntity.LinkInfo.DownloadLocation))
            {
                File.Delete(_downloadEntity.LinkInfo.DownloadLocation);
            }

            ResetProperties();
            _downloadEntity.Status = DownloadStatus.Waiting;
            UpdateDownloadDisplay();

            if (AbstractDownloadService.ActiveDownloads > SettingsHelper.MaxDownloadingCount)
            {
                _downloadEntity.Status = DownloadStatus.Queued;
                RaisePropertyChanged("StatusString");
                return;
            }

            DownloadThread = new Thread(DownloadFile);
            DownloadThread.IsBackground = true;
            DownloadThread.Start();
        }

        // Download file bytes from the HTTP response stream
        private void DownloadFile()
        {

            HttpWebRequest webRequest;
            HttpWebResponse webResponse = null;
            Stream responseStream = null;
            ThrottledStream throttledStream = null;
            MemoryStream downloadCache = null;
            speedUpdateCount = 0;
            recentAverageRate = 0;
            if (_downloadRates.Count > 0)
                _downloadRates.Clear();

            try
            {
                if (IsBatch && !BatchUrlChecked)
                {
                    CheckBatchUrl();
                    if (HasError)
                    {
                        return;
                    }
                    BatchUrlChecked = true;
                }

                if (!TempFileCreated)
                {
                    // Reserve local disk space for the file
                    CreateTempFile();
                    TempFileCreated = true;
                }

                _lastStartTime = DateTime.UtcNow;

                if (_downloadEntity.Status == DownloadStatus.Waiting)
                    _downloadEntity.Status = DownloadStatus.Downloading;

                // Create request to the server to download the file
                webRequest = (HttpWebRequest)WebRequest.Create(_downloadEntity.LinkInfo.DownloadLink);
                webRequest.Method = "GET";

                if (ServerLogin != null)
                {
                    webRequest.PreAuthenticate = true;
                    webRequest.Credentials = ServerLogin;
                }
                else
                {
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                }

                if (Proxy != null)
                {
                    webRequest.Proxy = Proxy;
                }
                else
                {
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                }

                // Set download starting point
//                webRequest.AddRange(DownloadedSize);
                // Get response from the server and the response stream
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();

                // Set a 5 second timeout, in case of internet connection break
                responseStream.ReadTimeout = 5000;

                // Set speed limit
                long maxBytesPerSecond;
                if (SettingsHelper.EnableSpeedLimit)
                {
                    maxBytesPerSecond = (SettingsHelper.SpeedLimit * 1024) / AbstractDownloadService.ActiveDownloads;
                }
                else
                {
                    maxBytesPerSecond = ThrottledStream.Infinite;
                }
                throttledStream = new ThrottledStream(responseStream, maxBytesPerSecond);

                // Create memory cache with the specified size
                downloadCache = new MemoryStream(MaxCacheSize);

                // Create 1KB buffer
                var downloadBuffer = new byte[BufferSize];

                _downloadEntity.LinkInfo.CachedSize = 0;
                var receivedBufferCount = 0;

                // Download file bytes until the download is paused or completed
                while (true)
                {
                    if (SpeedLimitChanged)
                    {
                        if (SettingsHelper.EnableSpeedLimit)
                        {
                            maxBytesPerSecond = (SettingsHelper.SpeedLimit * 1024) / AbstractDownloadService.ActiveDownloads;
                        }
                        else
                        {
                            maxBytesPerSecond = ThrottledStream.Infinite;
                        }
                        throttledStream.MaximumBytesPerSecond = maxBytesPerSecond;
                        SpeedLimitChanged = false;
                    }

                    // Read data from the response stream and write it to the buffer
                    var bytesSize = throttledStream.Read(downloadBuffer, 0, downloadBuffer.Length);

                    // If the cache is full or the download is paused or completed, write data from the cache to the temporary file
                    if (_downloadEntity.Status != DownloadStatus.Downloading || bytesSize == 0 || MaxCacheSize < _downloadEntity.LinkInfo.CachedSize + bytesSize)
                    {
                        // Write data from the cache to the temporary file
                        WriteCacheToFile(downloadCache, _downloadEntity.LinkInfo.CachedSize);

                        _downloadEntity.LinkInfo.DownloadedSize += _downloadEntity.LinkInfo.CachedSize;

                        // Reset the cache
                        downloadCache.Seek(0, SeekOrigin.Begin);
                        _downloadEntity.LinkInfo.CachedSize = 0;

                        // Stop downloading the file if the download is paused or completed
                        if (_downloadEntity.Status != DownloadStatus.Downloading || bytesSize == 0)
                        {
                            break;
                        }
                    }

                    // Write data from the buffer to the cache
                    downloadCache.Write(downloadBuffer, 0, bytesSize);
                    _downloadEntity.LinkInfo.CachedSize += bytesSize;

                    receivedBufferCount++;
                    if (receivedBufferCount == BufferCountPerNotification)
                    {
                        receivedBufferCount = 0;
                    }
                    DownloadProgressChangedHandler(null, null);
                }

                // Update elapsed time when the download is paused or completed
                ElapsedTime = ElapsedTime.Add(DateTime.UtcNow - _lastStartTime);

                // Change status
                if (_downloadEntity.Status != DownloadStatus.Deleting)
                {
                    if (_downloadEntity.Status == DownloadStatus.Pausing)
                    {
                        _downloadEntity.Status = DownloadStatus.Paused;
                        UpdateDownloadDisplay();
                    }
                    else if (_downloadEntity.Status == DownloadStatus.Queued)
                    {
                        UpdateDownloadDisplay();
                    }
                    else
                    {
                        CompletedOn = DateTime.UtcNow;
                    }

                    DownloadCompletedHandler(null,null);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Dare you motherfucker. I double dare you", MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
                HasError = true;
            }
            finally
            {
                // Close the response stream and cache, stop the thread
                responseStream?.Close();
                throttledStream?.Close();
                webResponse?.Close();
                downloadCache?.Close();
                DownloadThread?.Abort();
            }
        }

        #endregion
    }
}
