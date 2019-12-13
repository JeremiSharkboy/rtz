using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.IO;
using System.Linq;

namespace rtz
{
    class Program
    {
        private static FileInfo input;
        private static DirectoryInfo tempDir;
        private static FileInfo oututZip;
        private static string tempOutput = string.Empty;

        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    //just zip current folder if args is empty
                    return ToZipCurFolder();
                }

                if (args.Length >= 2)
                {
                    tempOutput = args[1];
                }


                if (!GetInputFileInfo(args[0], out input))
                {
                    Console.WriteLine("Input rar not found");
                    return 1;
                }

                tempDir = GetCurDirPath();
                oututZip = GetOutputZipPath();

                Unrar();
                ToZip(tempDir.FullName, oututZip.FullName);
                RemoveTemp();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
                return 1;
            }

        }

        private static int ToZipCurFolder()
        {
            var di = new DirectoryInfo(Environment.CurrentDirectory);
            var fi = new FileInfo(di.Name + ".zip");

            if (fi.Exists)
                fi.Delete();

            ToZip(di.FullName, Path.Combine(di.FullName, fi.FullName));

            Console.WriteLine($"Done {fi.FullName}");

            return 1;
        }

        private static bool GetInputFileInfo(string path, out FileInfo data)
        {
            if (string.IsNullOrEmpty(path))
            {
                data = null;
                return false;
            }

            data = new FileInfo(path);
            return data.Exists;

        }

        private static DirectoryInfo GetCurDirPath()
        {
            var dirInfo = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "TEMP"));

            if (!dirInfo.Exists)
                dirInfo.Create();
            dirInfo.Attributes = FileAttributes.Hidden;

            return dirInfo;
        }

        private static FileInfo GetOutputZipPath()
        {
            var fileInf = string.IsNullOrEmpty(tempOutput) ?

                 //what if .rar does not exists at filename
                
                 new FileInfo(Path.Combine(input.DirectoryName, input.Name.Replace(".rar", string.Empty) + ".zip"))
                 :
                 new FileInfo(tempOutput);

            if (fileInf.Exists)
                fileInf.Delete();

            return fileInf;
        }

        private static void Unrar()
        {
            using (var rar = RarArchive.Open(input.FullName))
            {
                foreach (var entry in rar.Entries.Where(entry => !entry.IsDirectory))
                {
                    Console.WriteLine(entry.Key);
                    entry.WriteToDirectory(tempDir.FullName, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }

        private static void ToZip(string what, string toWhere)
        {
            var ms = new MemoryStream();
            using (var zip = ZipArchive.Create())
            {
                Console.WriteLine("Writing data...");
                zip.AddAllFromDirectory(what);
                zip.SaveTo(ms, new WriterOptions(CompressionType.Deflate)
                {
                    LeaveStreamOpen = true
                });
                var fs = new FileStream(toWhere, FileMode.CreateNew);

                ms.WriteTo(fs);
                ms.Close();
            }


        }

        private static void RemoveTemp()
        {
            if (tempDir.Exists)
            {
                tempDir.Delete(true);
            }
        }
    }
}
