using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using PianoRoll.Model;
using PianoRoll.Util;

namespace PianoRoll
{
    public static class Settings
    {
        public static string SettingsFile = @"settings";

        // take from settings
        public static string Local;
        public static string VoicebankDirectory;
        public static string DefaultVoicebank;
        public static string DefaultVoicebankType;
        public static string AppendTool;
        public static string TransitionTool;
        public static string Resampler;
        public static string LastFile;
        public static double LastV;
        public static double LastH;
        public static string DefaultLyric;

        // in local folder
        public static string CacheFolder;
        public static string Bat;
        public static string Output;

        public static int IntervalTick = 5;
        public static double IntervalMs { get { return (int)MusicMath.TickToMillisecond(IntervalTick); } }
        public static int SkipOnRender = 1;

        public static int Resolution = 480;
        public static int Octaves = 7;


        public static void Read()
        {
            if (!File.Exists(SettingsFile)) File.Create(SettingsFile);
            string[] lines = File.ReadAllLines(SettingsFile, Encoding.ASCII);
            foreach (string line in lines)
            {
                if (line.StartsWith("Local="))
                    Local = line.Substring("Local=".Length);
                if (line.StartsWith("VoicebankDirectory="))
                    VoicebankDirectory = line.Substring("VoicebankDirectory=".Length).Replace("%Local%",Local);
                if (line.StartsWith("DefaultVoicebank="))
                    DefaultVoicebank = line.Substring("DefaultVoicebank=".Length).Replace("%Local%", Local);
                if (line.StartsWith("DefaultVoicebankType="))
                    DefaultVoicebankType = line.Substring("DefaultVoicebankType=".Length);
                if (line.StartsWith("AppendTool="))
                    AppendTool = line.Substring("AppendTool=".Length).Replace("%Local%", Local);
                if (line.StartsWith("TransitionTool="))
                    TransitionTool = line.Substring("TransitionTool=".Length).Replace("%Local%", Local);
                if (line.StartsWith("Resampler="))
                    Resampler = line.Substring("Resampler=".Length).Replace("%Local%", Local);
                if (line.StartsWith("LastFile="))
                    LastFile = line.Substring("LastFile=".Length).Replace(Local, "%Local%");
                if (line.StartsWith("LastV="))
                    if (double.TryParse(line.Substring("LastV=".Length), out double result))
                        LastV = result;
                if (line.StartsWith("LastH="))
                    if (double.TryParse(line.Substring("LastH=".Length), out double result))
                        LastH = result;
                if (line.StartsWith("DefaultLyric="))
                    DefaultLyric = line.Substring("DefaultLyric=".Length);
            }
            if (!File.Exists(LastFile)) LastFile = null;
            CacheFolder = Path.Combine(Local, @"Cache\");
            Bat = Path.Combine(Local, @"render.bat");
            Output = Path.Combine(Local, @"output.wav");
            if (!Directory.Exists(CacheFolder)) Directory.CreateDirectory(CacheFolder);
    }

        public static void Save()
        {
            if (!File.Exists(SettingsFile)) File.Create(SettingsFile);
            string[] lines = new string[]
            {
                $"Local=" + Local,
                $"VoicebankDirectory=" + VoicebankDirectory.Replace(Local, "%Local%"),
                $"DefaultVoicebank=" + DefaultVoicebank.Replace(Local, "%Local%"),
                $"DefaultVoicebankType=" + DefaultVoicebankType,
                $"AppendTool=" + AppendTool.Replace(Local, "%Local%"),
                $"TransitionTool=" + TransitionTool.Replace(Local, "%Local%"),
                $"Resampler=" + Resampler.Replace(Local, "%Local%"),
                $"LastFile=" + LastFile.Replace(Local, "%Local%"),
                $"LastV=" + LastV,
                $"LastH=" + LastH,
                $"DefaultLyric=" + DefaultLyric
            };
            File.WriteAllLines(SettingsFile, lines, Encoding.UTF8);
        }
    }
}
