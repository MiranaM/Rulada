using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace PianoRoll
{
    class Settings
    {
        public static string VoiceDir = @"D:\DISCS\YandexDisk\Heiden\UTAU\_voicebanks\";
        public static string WavTool = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Rulada\wavtool.exe");
        public static string Resampler = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Rulada\m4.exe");
        public static string CacheFolder = Path.Combine(Path.GetTempPath(), @"Rulada\Cache\");
        public static string Bat = Path.Combine(Path.GetTempPath(), @"Rulada\render.bat");
        public static string Output = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Rulada\output.wav");
        public static double Tempo = 120.0;
    }
}
