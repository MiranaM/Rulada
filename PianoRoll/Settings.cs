using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NAudio.MediaFoundation;
using PianoRoll.Util;

namespace PianoRoll
{
    public class Settings
    {
        #region singleton base

        private static Settings current;
        private Settings()
        {
            Read();
        }

        public static Settings Current
        {
            get
            {
                if (current == null)
                {
                    current = new Settings();
                }
                return current;
            }
        }

        #endregion

        public string SettingsFile = @"settings";

        // take from settings
        public string Local;
        public string VoicebankDirectory;
        public string DefaultVoicebank;
        public string DefaultVoicebankType;
        public string AppendTool;
        public string TransitionTool;
        public string Resampler;
        public string LastFile;
        public double LastV;
        public double LastH;
        public string DefaultLyric;

        //Project default settings
        public double Tempo = 120;
        public int BeatPerBar = 4;
        public int BeatUnit = 8;

        public double xScale = 1;
        public double yScale = 15;

        public double MinNoteLengthTick => RESOLUTION / BeatUnit * xScale;

        public Dictionary<string, int> NotesLengths = new Dictionary<string, int>
        {
            ["L1"] = RESOLUTION * 4,  // 1
            ["L2"] = RESOLUTION * 2,  // 1/2
            ["L4"] = RESOLUTION,      // 1/4
            ["L8"] = RESOLUTION / 2,  // 1/8
            ["L16"] = RESOLUTION / 4, // 1/16
            ["L32"] = RESOLUTION / 8, // 1/32
            ["L64"] = RESOLUTION / 16 // 1/64
        };

        public int CurrentNoteLength = RESOLUTION;

        // in local folder
        public string CacheFolder;
        public string Bat;
        public string Output;

        public int IntervalTick = 5;

        public double IntervalMs => (int) MusicMath.Current.TickToMillisecond(IntervalTick);

        public int SkipOnRender = 1;

        //RESOLUTION = 1/4
        public const int RESOLUTION = 480;
        public int Octaves = 7;

        private void Read()
        {
            if (!File.Exists(SettingsFile))
                File.Create(SettingsFile);
            var lines = File.ReadAllLines(SettingsFile, Encoding.UTF8);
            foreach (var line in lines)
            {
                if (line.StartsWith("Local=") && line.Length > "Local=".Length)
                    Local = line.Substring("Local=".Length);
                if (line.StartsWith("VoicebankDirectory=") && line.Length > "VoicebankDirectory=".Length)
                    VoicebankDirectory = line.Substring("VoicebankDirectory=".Length).Replace("%Local%", Local);
                if (line.StartsWith("DefaultVoicebank=") && line.Length > "DefaultVoicebank=".Length)
                    DefaultVoicebank = line.Substring("DefaultVoicebank=".Length).Replace("%Local%", Local);
                if (line.StartsWith("DefaultVoicebankType=") && line.Length > "DefaultVoicebankType=".Length)
                    DefaultVoicebankType = line.Substring("DefaultVoicebankType=".Length);
                if (line.StartsWith("AppendTool=") && line.Length > "AppendTool=".Length)
                    AppendTool = line.Substring("AppendTool=".Length).Replace("%Local%", Local);
                if (line.StartsWith("TransitionTool=") && line.Length > "TransitionTool=".Length)
                    TransitionTool = line.Substring("TransitionTool=".Length).Replace("%Local%", Local);
                if (line.StartsWith("Resampler=") && line.Length > "Resampler=".Length)
                    Resampler = line.Substring("Resampler=".Length).Replace("%Local%", Local);
                if (line.StartsWith("LastFile=") && line.Length > "LastFile=".Length)
                    LastFile = line.Substring("LastFile=".Length).Replace(Local, "%Local%");
                if (line.StartsWith("LastV=") && line.Length > "LastV=".Length)
                    if (double.TryParse(line.Substring("LastV=".Length), out var result))
                        LastV = result;
                if (line.StartsWith("LastH=") && line.Length > "LastH=".Length)
                    if (double.TryParse(line.Substring("LastH=".Length), out var result))
                        LastH = result;
                if (line.StartsWith("DefaultLyric=") && line.Length > "DefaultLyric=".Length)
                    DefaultLyric = line.Substring("DefaultLyric=".Length);
            }
            if (VoicebankDirectory == String.Empty || DefaultVoicebank == String.Empty)
                throw new Exception("НЕ РАБОТАЕТ");

            if (!File.Exists(LastFile)) LastFile = null;
            CacheFolder = Path.Combine(Local, @"Cache\");
            Bat = Path.Combine(Local, @"render.bat");
            Output = Path.Combine(Local, @"output.wav");
            if (!Directory.Exists(CacheFolder)) Directory.CreateDirectory(CacheFolder);
        }

        public void Save()
        {
            if (!File.Exists(SettingsFile)) File.Create(SettingsFile);
            //var lines = new[]
            //{
            //    "Local=" + Local, "VoicebankDirectory=" + VoicebankDirectory.Replace(Local, "%Local%"),
            //    "DefaultVoicebank=" + DefaultVoicebank.Replace(Local, "%Local%"),
            //    "DefaultVoicebankType=" + DefaultVoicebankType,
            //    "AppendTool=" + AppendTool.Replace(Local, "%Local%"),
            //    "TransitionTool=" + TransitionTool.Replace(Local, "%Local%"),
            //    "Resampler=" + Resampler.Replace(Local, "%Local%"),
            //    "LastFile=" + LastFile.Replace(Local, "%Local%"), "LastV=" + LastV, "LastH=" + LastH,
            //    "DefaultLyric=" + DefaultLyric
            //};
            //File.WriteAllLines(SettingsFile, lines, Encoding.UTF8);
        }
    }
}