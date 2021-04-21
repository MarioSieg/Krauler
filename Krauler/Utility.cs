using System.IO;

namespace Krauler
{
    internal static class Utility
    {
        public const byte CheckStages = 10;

        public static void SetCorrectWorkingDir()
        {
            if (Directory.Exists(Config.ResourcesDir)) return;

            string temp = "../";
            for (byte i = 0; i < CheckStages; ++i)
            {
                temp += "../";
                var dir = temp + Config.ResourcesDir;
                if (Directory.Exists(dir))
                {
                    Directory.SetCurrentDirectory(temp);
                    Logger.Instance.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");
                    break;
                }
            }
        }
    }
}