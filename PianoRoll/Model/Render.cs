using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NAudio.Wave;
using PianoRoll.Util;
using PianoRoll.View;
using PianoRoll.Control;
using System.Diagnostics;

namespace PianoRoll.Model
{
    public static class Render
    {
        static WaveChannel32 waveChannel;
        static WaveOutEvent player;
        static WaveStream output;
        static Process bat;
        static long position = 0;
        static Part Part;
        static bool IsPlaying = false;
        public static long PlayerPosition { get { return output.Position; } }
        public static long PlayerLength { get { return output.Length; } }

        public delegate void RenderCompletedHandler();
        public static event RenderCompletedHandler OnRenderComplited;

        public static void Send(Part part)
        {
            OnRenderComplited += OnRenderCompleted_Render;
            Part = part;
            position = 0;
            Part.Recalculate();
            Part.BuildPitch();
            if (output != null) output.Close();
            if (File.Exists(Settings.Output)) File.Delete(Settings.Output);
            if (File.Exists(Settings.Output + ".dat")) File.Delete(Settings.Output + ".dat");
            if (File.Exists(Settings.Output + ".whd")) File.Delete(Settings.Output + ".whd");            

            if (!File.Exists(Settings.Bat)) File.Create(Settings.Bat);
            string delcommand = $"del \"{ Settings.CacheFolder }\\*.wav\"\r\n";
            File.WriteAllText(Settings.Bat, delcommand);
            int i = 1;
            long renderPosition = 0;
            foreach (Note note in Part.Notes)
            {
                string tempfilename = Path.Combine(Settings.CacheFolder, $"{i}");
                tempfilename += $"_{note.Lyric}_{note.NoteNum}_{note.Length}.wav";
                // Send Rest
                if (note.AbsoluteTime > renderPosition)
                {
                    long resttime = note.AbsoluteTime - renderPosition;
                    SendToAppendTool(resttime, $"{i}_Rest.wav");
                    renderPosition += resttime;
                }
                // Send
                SendToResampler(note, tempfilename);
                SendToAppendTool(note, tempfilename);
                renderPosition += note.Length;
                i++;
            }
            File.AppendAllText(Settings.Bat, $"@if not exist \"{Settings.Output}.whd\" goto E \r\n" +
                    $"@if not exist \"{Settings.Output}.dat\" goto E \r\n" +
                    $"copy /Y \"{Settings.Output}.whd\" /B + \"{Settings.Output}.dat\" /B \"{Settings.Output}\" \r\n" +
                    $"del \"{Settings.Output}.whd\" \r\n" +
                    $"del \"{Settings.Output}.dat\" \r\n" +
                    ":E");
            Process();
        }

        static void OnRenderCompleted_Render()
        {
        }

        static async void Process()
        {
            bat = new System.Diagnostics.Process();
            bat.StartInfo.FileName = Settings.Bat;
            bat.StartInfo.WorkingDirectory = Settings.CacheFolder;
            bat.EnableRaisingEvents = true;
            bat.Exited += new EventHandler(OnExited);
            await ProcessStart();
        }

        static Task<bool> ProcessStart()
        {
            return Task.Run(() =>
            {
                bat.Start();
                return true;
            });
        }

        public static void OnExited(object sender, System.EventArgs e)
        {
            if (!File.Exists(Settings.Output)) return;
            output = new WaveFileReader(Settings.Output);
            output.Position = position;
            Play();
        }
        
        public static void Play()
        {
            if (IsPlaying) Stop();
            IsPlaying = true;
            // if (output == null) output = new WaveFileReader(Settings.Output);
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
            IsPlaying = false;
            if (player != null)
            {
                position = player.GetPosition();
                player.Stop();
            }
            if (output != null) output.Close();
        }

        public static int[] TakeEach(int[] array, int each)
        {
            IsPlaying = false;
            List<int> list = new List<int>();
            for (int i = 0; i < array.Length; i++)
            {
                if (i % each == 0) list.Add(array[i]);
            }
            return list.ToArray();
        }

        public static void SendToResampler(Note note, string tempfilename)
        {
            Part Part = Project.Current.Tracks[0].Parts[0];

            string pitchBase64 = Base64.Base64EncodeInt12(TakeEach(note.PitchBend.Array, Settings.SkipOnRender));
            Phoneme phoneme = note.Phoneme;
            string request = string.Format
            (
                "\"{0}\" \"{1}\" \"{2}\" {3} {4:D} \"{5}\" {6} {7:D} {8} {9} {10:D} {11:D} !{12} {13}\r\n",
                Settings.Resampler,
                Path.Combine(Part.Track.Singer.Dir, phoneme.File),
                tempfilename,
                MusicMath.NoteNum2String(note.NoteNum - 12), 
                note.Velocity,
                Part.Flags + note.Flags,
                phoneme.Offset,
                (int)note.RequiredLength,
                phoneme.Consonant,
                phoneme.Cutoff,
                note.Intensity,
                note.Modulation,
                note.NoteNum,
                pitchBase64
            );
            File.AppendAllText(Settings.Bat, request);
        }

        /// <summary>
        /// Send Note to AppendTool
        /// </summary>
        public static void SendToAppendTool(Note note, string filename)
        {
            Part Part = Project.Current.Tracks[0].Parts[0];
            string lyric = note.Lyric;
            Note next = Part.GetNextNote(note);
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
                "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",
                note.stp, // STP,
                length, //note.RequiredLength, 
                note.Envelope.p1,
                note.Envelope.p2,
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
            string request = $"\"{Settings.AppendTool}\" \"{Settings.Output}\" \"{filename}\" {ops} \r\n";
            File.AppendAllText(Settings.Bat, request);
        }


        /// <summary>
        /// Send Pause to AppendTool
        /// </summary>
        public static void SendToAppendTool(long duration, string filename)
        {
            Part Part = Project.Current.Tracks[0].Parts[0];
            string length = $"{duration}@{Project.Tempo}+0";
            string ops = $"0 {length} 0 0";
            string request = $"\"{Settings.AppendTool}\" \"{Settings.Output}\" \"{filename}\" {ops}\r\n";
            File.AppendAllText(Settings.Bat, request);
        }

    }
}
