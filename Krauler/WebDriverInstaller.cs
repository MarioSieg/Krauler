using System;
using System.IO;
using System.Linq;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;

namespace Krauler
{
    public static class WebDriverInstaller
    {
        private static readonly Lazy<string[]> WebDriverUrls = new(() => File.ReadAllLines(Config.ResourcesDir + "WebDriverInstallSources.txt"));

        private static readonly string[] TargetDriverFiles =
        {
            "?",
            "geckodriver.exe",
            "?"
        };

        public static void InstallDriver(WebDriverType type)
        {
            try
            {
                Logger.Instance.WriteLine($"Beginning installation of web driver: {type}");

                var relDir = Config.DriverDir + type + "/";
                var target = relDir + TargetDriverFiles[(int) type];
                var targetInstall = AppDomain.CurrentDomain.BaseDirectory + "/" + Path.GetFileName(target);

                Directory.CreateDirectory(relDir);
                if (Directory.Exists(relDir) && Directory.EnumerateFileSystemEntries(relDir).Any() && File.Exists(targetInstall))
                {
                    Logger.Instance.WriteLine("Web driver already installed!");
                    return;
                }

                using var client = new WebClient();

                var url = WebDriverUrls.Value[(int) type];
                var file = Config.DriverDir + Path.GetFileName(url);

                Logger.Instance.WriteLine($"Installing web driver: {type}, to: {file} from: {url}, for: {target}");

                client.DownloadFile(url, file);

                Logger.Instance.WriteLine("Unpacking driver installation...");

                using var stream = new ZipInputStream(File.OpenRead(file));

                for (ZipEntry entry; (entry = stream.GetNextEntry()) != null;)
                {
                    var directoryName = Path.GetDirectoryName(entry.Name);
                    var fileName = Path.GetFileName(entry.Name);

                    if (directoryName is {Length: > 0})
                    {
                        Directory.CreateDirectory(relDir + directoryName);
                    }

                    if (fileName == string.Empty) continue;
                    using var streamWriter = File.Create(relDir + fileName);

                    var bufSize = 2048;
                    var buf = new byte[bufSize];
                    for (; bufSize > 0; bufSize = stream.Read(buf, 0, buf.Length))
                    {
                        streamWriter.Write(buf, 0, bufSize);
                    }
                }
                File.Delete(file);
                if (!File.Exists(target))
                {
                    throw new Exception($"Target file {target} not found in drive installation!");
                }
                Logger.Instance.WriteLine($"Finalizing installation: {targetInstall}");
                File.Copy(target, targetInstall);
                Logger.Instance.WriteLine($"Installed web driver: {type}");
            }
            catch (Exception? ex)
            {
                Logger.Instance.WriteLine($"Failed to install web driver: {type}");
                while (ex != null)
                {
                    Logger.Instance.Write(ex);
                    ex = ex.InnerException;
                }
            }
        }
    }
}
