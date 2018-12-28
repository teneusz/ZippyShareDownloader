using TenekDownloader.link.model;

namespace TenekDownloader.link
{
	public interface ILinkInterpreter
	{
		LinkInfo LinkInfo { get; }

		void ProcessLink(string link);
	}
}