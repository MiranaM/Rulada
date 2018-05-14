using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NAudio.Wave;

namespace PianoRoll.Model
{
    class Render
    {
        public static WaveChannel32 waveChannel;
        public static WaveOutEvent player;

        public static void Play()
        {
            File.WriteAllText(Settings.Bat, "");
            //if (Directory.Exists(Settings.CacheFolder)) Directory.Delete(Settings.CacheFolder);
            //Directory.CreateDirectory(Settings.CacheFolder);)
            foreach (UNote note in Ust.NotesList)
            {
                string tempfilename = Path.Combine(Settings.CacheFolder, $"{note.UNumber.Substring(2, 4)}.wav");
                if (note.HasOto) SendToResampler(note, tempfilename);
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
            proc.Exited += new EventHandler(PlayRendered);
            proc.Start();
        }

        public static void PlayRendered(object sender, System.EventArgs e)
        {
            WaveStream output = new WaveFileReader(Settings.Output);
            waveChannel = new WaveChannel32(output);
            player = new WaveOutEvent();
            player.Init(waveChannel);
            player.Play();
        }

        public static void Stop()
        {
            player.Stop();
        }



        public static void SendToResampler(UNote note, string tempfilename)
        {
            string ops = string.Format
            (
                "{0} {1:D} \"{2}\" {3} {4:D} {5} {6} {7:D} {8:D} !{9} {10}",
                Ust.NoteNum2String(note.NoteNum),
                note.Velocity,
                "g-5", //note.Flags,
                note.Oto.Offset,
                note.RequiredLength,
                note.Oto.Consonant,
                note.Oto.Cutoff,
                note.Volume,
                0, // modulation
                note.NoteNum,
                "+c#24#+f+p+3/J/c/s/5//AA#17#" //Pitch.BuildPitchData(note)
            );
            string request = $"\"{Settings.Resampler}\" \"{Path.Combine(USinger.UPath,note.Oto.File)}\" \"{tempfilename}\" {ops} \r\n";
            File.AppendAllText(Settings.Bat, request);
        }

        public static void SendToWavtool(UNote note, string tempfilename)
        {
            string ops = string.Format
            (
                "{0} {1}@{2}{3}{4} {5} {6}",
                0, // STP
                note.Length,
                Settings.Tempo,
                note.Oto.Preutter < 0 ? "-" : "+",
                Math.Abs(note.Oto.Preutter),
                note.Envelope.p1,
                note.Envelope.p2
            );
            string opsNote;
            if (note.Lyric == "") opsNote = "";
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
                    note.Oto.Overlap,
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
