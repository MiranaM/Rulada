using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using PianoRoll.Model;

namespace PianoRoll
{
    class Settings
    {
        public static string VoiceDir = @"D:\DISCS\YandexDisk\Heiden\UTAU\_voicebanks\";
        public static string WavTool = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Rulada\wavtool.exe");
        public static string Resampler = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Rulada\m4.exe");

        public static string ToolsDirectory = @"D:\UTAU\_resampler";

        public static string CacheFolder = Path.Combine(Path.GetTempPath(), @"Rulada\Cache\");
        public static string Bat = Path.Combine(Path.GetTempPath(), @"Rulada\render.bat");
        public static string Output = Path.Combine(Path.GetTempPath(), @"Rulada\output.wav");
        //public static string Output = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Rulada\output.wav");
        public static double Tempo = 120.0;
        public static int Resolution = 480;
        public static int BeatPerBar = 4;
        public static int BeatUnit = 4;
        public static int IntervalTick { get { return (int) Ust.MillisecondToTick(IntervalMs); } }
        public static double IntervalMs = 5;
        //public static int IntervalTick = 5;
        //public static double IntervalMs { get { return (int)Ust.TickToMillisecond(IntervalTick); } }
    }
}
