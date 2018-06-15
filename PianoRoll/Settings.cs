﻿using System;
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
        public static string SettingsFile = @"settings";

        // take from settings
        public static string Local;
        public static string VoicebankDirectory;
        public static string DefaultVoicebank;
        public static string DefaultVoicebankType;
        public static string WavTool;
        public static string Resampler;
        public static string LastFile;
        public static double LastV;
        public static double LastH;

        // in local folder
        public static string CacheFolder;
        public static string Bat;
        public static string Output;

        // others
        public static double Tempo = 120.0;
        public static int Resolution = 480;
        public static int BeatPerBar = 4;
        public static int BeatUnit = 4;
        //public static int IntervalTick { get { return (int) Ust.MillisecondToTick(IntervalMs); } }
        //public static double IntervalMs = 50;
        public static int IntervalTick = 5;
        public static double IntervalMs { get { return (int)Ust.TickToMillisecond(IntervalTick); } }



        public static void Read()
        {
            if (!File.Exists(SettingsFile)) File.Create(SettingsFile);
            string[] lines = File.ReadAllLines(SettingsFile);
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
                if (line.StartsWith("WavTool="))
                    WavTool = line.Substring("WavTool=".Length).Replace("%Local%", Local);
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
                $"WavTool=" + WavTool.Replace(Local, "%Local%"),
                $"Resampler=" + Resampler.Replace(Local, "%Local%"),
                $"LastFile=" + LastFile.Replace(Local, "%Local%"),
                $"LastV=" + LastV,
                $"LastH=" + LastH
            };
            File.WriteAllLines(SettingsFile, lines);
        }
    }
}
