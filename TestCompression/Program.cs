using System;
using System.Reflection;
using System.Threading;
using TrrntZip;

namespace TestCompression
{
    class Program
    {
        public static int ThreadId;
        public static StatusCallback StatusCallBack;
        public static LogCallback StatusLogCallBack;
        public static zipType OutZip = zipType.zip;
        static void Main(string[] args)
        {
            ZipDirectory();
        }

        private static void ReadDirContent(DirectoryInfo diMaster, ref List<ZippedFile> files, int stripLength)
        {
            DirectoryInfo[] arrDi = diMaster.GetDirectories();
            FileInfo[] arrFi = diMaster.GetFiles();

            if (arrDi.Length == 0 && arrFi.Length == 0)
            {
                string name = (diMaster.FullName + "/").Substring(stripLength);
                if (name == "")
                    return;
                files.Add(new ZippedFile() { Name = name, Size = 0 });
                return;
            }

            foreach (DirectoryInfo di in arrDi)
                ReadDirContent(di, ref files, stripLength);

            foreach (FileInfo fi in arrFi)
                files.Add(new ZippedFile() { Name = fi.FullName.Substring(stripLength), Size = (ulong)fi.Length });

        }

        private static TrrntZipStatus ZipDirectory(PauseCancel pc = null)
        {
            var di = new DirectoryInfo(@"D:\Archive\Wallpaper");

            List<ZippedFile> zippedFiles = new List<ZippedFile>();
            ReadDirContent(di, ref zippedFiles, di.FullName.Length + 1);

            // sort them
            zipType outputType = Program.OutZip == zipType.archive ? zipType.zip : Program.OutZip;
            switch (outputType)
            {
                case zipType.zip:
                    TorrentZipCheck.CheckZipFiles(ref zippedFiles, ThreadId, StatusLogCallBack);
                    break;
                case zipType.sevenzip:
                    TorrentZipCheck.CheckSevenZipFiles(ref zippedFiles, ThreadId, StatusLogCallBack);
                    break;
            }

            var _buffer = new byte[1024 * 1024];

            StatusLogCallBack?.Invoke(ThreadId, "TorrentZipping");
            TrrntZipStatus fixedTzs = TorrentZipMake.ZipFiles(zippedFiles, di.FullName, _buffer, StatusCallBack, StatusLogCallBack, ThreadId, pc);
            return fixedTzs;

        }




    }
}
