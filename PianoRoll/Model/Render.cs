using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NAudio.Wave;
using PianoRoll.Util;
using PianoRoll.View;

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
            Part part = Project.Current.Tracks[0].Parts[0];
            part.RefreshPhonemes();
            part.Recalculate();
            part.BuildPitch();

            if (output != null) output.Close();
            if (File.Exists(Settings.Output)) File.Delete(Settings.Output);
            if (File.Exists(Settings.Output + ".dat")) File.Delete(Settings.Output + ".dat");
            if (File.Exists(Settings.Output + ".whd")) File.Delete(Settings.Output + ".whd");            

            if (!File.Exists(Settings.Bat)) File.Create(Settings.Bat);
            string delcommand = $"del \"{ Settings.CacheFolder }\\*.wav\"\r\n";
            File.WriteAllText(Settings.Bat, delcommand);
            int i = 1;
            foreach (Note note in part.Notes)
            {
                string tempfilename = Path.Combine(Settings.CacheFolder, $"{i}");
                tempfilename += $"_{note.Lyric}_{note.NoteNum}_{note.Length}.wav";
                if (!note.IsRest) SendToResampler(note, tempfilename);
                SendToWavtool(note, tempfilename);
                i++;
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
            if (player != null) player.Stop();
            if (output != null) output.Close();
            position = 0;
        }

        public static void Pause()
        {
            if (player != null)
            {
                position = player.GetPosition();
                player.Stop();
            }
            if (output != null) output.Close();
        }

        public static int[] TakeEach(int[] array, int each)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < array.Length; i++)
            {
                if (i % each == 0) list.Add(array[i]);
            }
            return list.ToArray();
        }

        public static void SendToResampler(Note note, string tempfilename)
        {
            Part part = Project.Current.Tracks[0].Parts[0];

            string pitchBase64 = Base64.Base64EncodeInt12(TakeEach(note.PitchBend.Array, Settings.SkipOnRender));
            string request = string.Format
            (
                "\"{0}\" \"{1}\" \"{2}\" {3} {4:D} \"{5}\" {6} {7:D} {8} {9} {10:D} {11:D} !{12} {13}\r\n",
                Settings.Resampler,
                Path.Combine(part.Singer.Dir, note.Phoneme.File),
                tempfilename,
                MusicMath.NoteNum2String(note.NoteNum), 
                note.Velocity,
                part.Flags + note.Flags,
                note.Phoneme.Offset, // + note.Phoneme.Preutter - note.Phoneme.Overlap, // offset
                (int)note.RequiredLength,
                note.Phoneme.Consonant,
                note.Phoneme.Cutoff,
                note.Intensity,
                note.Modulation,
                note.NoteNum,
                pitchBase64
            );
            File.AppendAllText(Settings.Bat, request);
        }
        
        public static void SendToWavtool(Note note, string tempfilename)
        {
            Part part = Project.Current.Tracks[0].Parts[0];
            string lyric = note.Lyric;
            //double length = Ust.TickToMillisecond(note.Length) + note.pre;
            Note next = part.GetNextNote(note);
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

            string length = $"{note.Length}@{Project.Tempo}{sign}{Math.Abs(offset).ToString("f0")}";

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
                    note.ovl, //note.Phoneme.Overlap,
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
