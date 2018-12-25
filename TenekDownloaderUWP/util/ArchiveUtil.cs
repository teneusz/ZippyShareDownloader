using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace TenekDownloader.util
{
    public static class ArchiveUtil
    {
        public static void UnpackArchive(string inFile, string outFile, Object extractingEventHandler)
        {
//            var unzipTask = new Task(() =>
//            {
//                try
//                {
//                    SevenZipBase.SetLibraryPath(Properties.Settings.Default.SevenZipDll);
//                    using (var tmp = new SevenZipExtractor(inFile))
//                    {
//                        foreach (var data in tmp.ArchiveFileData)
//                        {
//                            tmp.Extracting += extractingEventHandler;
//                            tmp.ExtractFiles(outFile, data.Index);
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Log.Debug("Error while extracting file <"+inFile+">",ex);
//                }
//            });

//            unzipTask.Start();
            
        }
    }
}
