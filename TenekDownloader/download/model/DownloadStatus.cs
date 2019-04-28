namespace TenekDownloader.download.model
{
	public enum DownloadStatus
	{
		Downloading,
		Error,
		NotFound,
		Waiting,
		NotDownloading,
		Preparing,
		Completed,
		Canceled,
        Deleting,
        Paused,
        Queued,
        Pausing
    }
}