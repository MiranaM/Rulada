using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NAudio.Wave;
using PianoRoll.Util;

namespace PianoRoll.Model
{
    class Render
    {
        static WaveChannel32 waveChannel;
        static WaveOutEvent player;
        static WaveStream output;
        static long position = 0;

        public static void Send()
        {
            position = 0;
            USinger.NoteOtoRefresh();
            Ust.Recalculate();
            Ust.BuildPitch();

            if (output != null) output.Close();
            if (File.Exists(Settings.Output)) File.Delete(Settings.Output);
            if (File.Exists(Settings.Output + ".dat")) File.Delete(Settings.Output + ".dat");
            if (File.Exists(Settings.Output + ".whd")) File.Delete(Settings.Output + ".whd");
            

            if (!File.Exists(Settings.Bat)) File.Create(Settings.Bat);
            string delcommand = $"del \"{ Settings.CacheFolder }\\*.wav\"\r\n";
            File.WriteAllText(Settings.Bat, delcommand);
            foreach (UNote note in Ust.NotesList)
            {
                string tempfilename = Path.Combine(Settings.CacheFolder, $"{note.UNumber.Substring(2, 4)}");
                tempfilename += $"_{note.Lyric}_{note.NoteNum}_{note.Length}.wav";
                if (!note.IsRest) SendToResampler(note, tempfilename);
                SendToWavtool(note, tempfilename);
            }
            File.AppendAllText(Settings.Bat, $"@if not exist \"{Settings.Output}.whd\" goto E \r\n" +
                    $"@if not exist \"{Settings.Output}.dat\" goto E \r\n" +
                    $"copy /Y \"{Settings.Output}.whd\" /B + \"{Settings.Output}.dat\" /B \"{Settings.Output}\" \r\n" +
                    $"del \"{Settings.Output}.whd\" \r\n" +
                    $"del \"{Settings.Output}.dat\" \r\n" +
                    ":E");
           
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = Settings.Bat;
            proc.StartInfo.WorkingDirectory = Settings.CacheFolder;
            proc.EnableRaisingEvents = true;
            proc.Exited += new EventHandler(OnExited);
            proc.Start();
        }

        public static void OnExited(object sender, System.EventArgs e)
        {
            Play();
        }

        public static void Play()
        {
            if (!File.Exists(Settings.Output)) return;
            output = new WaveFileReader(Settings.Output);
            output.Position = position;
            waveChannel = new WaveChannel32(output);
            player = new WaveOutEvent();
            player.Init(waveChannel);
            player.Play();
        }

        public static void Stop()
        {
            position = 0;
            player.Stop();
            output.Close();
        }

        public static void Pause()
        {
            position = player.GetPosition();
            player.Stop();
            output.Close();
        }

        public static void SendToResampler(UNote note, string tempfilename)
        {
            string pitchBase64 = Base64.Base64EncodeInt12(note.PitchBend.Array);
            string request = string.Format
            (
                "\"{0}\" \"{1}\" \"{2}\" {3} {4:D} \"{5}\" {6} {7:D} {8} {9} {10:D} {11:D} !{12} {13}\r\n",
                Settings.Resampler,
                Path.Combine(USinger.UPath, note.Oto.File),
                tempfilename,
                Ust.NoteNum2String(note.NoteNum), 
                note.Intensity,
                Ust.Flags + note.Flags,
                note.Oto.Offset, // + note.Oto.Preutter - note.Oto.Overlap, // offset
                (int)note.RequiredLength,
                note.Oto.Consonant,
                note.Oto.Cutoff,
                note.Intensity,
                note.Modulation,
                note.NoteNum,
                pitchBase64
            );
            File.AppendAllText(Settings.Bat, request);
        }
        
        public static void SendToWavtool(UNote note, string tempfilename)
        {
            string lyric = note.Lyric;
            //double length = Ust.TickToMillisecond(note.Length) + note.pre;
            UNote next = Ust.GetNextNote(note);
            //if (next != null)
            //{
            //    length -= next.pre;
            //    length += next.ovl;
            //}

            double offset = note.pre;
            if (next != null)
            {
                offset -= next.pre;
                offset += next.ovl;
            }
            
            string sign = offset >= 0 ? "+" : "-";

            string length = $"{note.Length}@{Settings.Tempo}{sign}{Math.Abs(offset).ToString("f0")}";

            string ops = string.Format
            (
                "{0} {1} {2} {3}",
                note.stp, // STP,
                length, //note.RequiredLength, 
                note.Envelope.p1,
                note.Envelope.p2
            );
            string opsNote;
            //if (note.IsRest) opsNote = "";
            if (false) opsNote = "";
            else
            {
                opsNote = string.Format
                (
                    "{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                    note.Envelope.p3,
                    note.Envelope.v1,
                    note.Envelope.v2,
                    note.Envelope.v3,
                    note.Envelope.v4,
                    note.ovl, //note.Oto.Overlap,
                    note.Envelope.p4,
                    note.Envelope.p5,
                    note.Envelope.v5
                );
            }
            string request = $"\"{Settings.WavTool}\" \"{Settings.Output}\" \"{tempfilename}\" {ops} {opsNote} \r\n";
            File.AppendAllText(Settings.Bat, request);
        }

    }
}
