using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using SevenZip;

namespace ZippyShareDownloader.util
{
    public static class ArchiveUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ArchiveUtil));
        public static void UnpackArchive(string inFile, string outFile)
        {
            var unzipTask = new Task(() =>
            {
                try
                {
                    SevenZipCompressor.SetLibraryPath(Properties.Settings.Default.sevenZipPath);
                    using (var tmp = new SevenZipExtractor(inFile))
                    {
                        foreach (var data in tmp.ArchiveFileData)
                        {
                            tmp.ExtractFiles(outFile, data.Index);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug("Error while extracting file <"+inFile+">",ex);
                }
            });

            unzipTask.Start();
            
        }
    }
}
