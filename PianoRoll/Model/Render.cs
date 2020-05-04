using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using PianoRoll.Control;
using PianoRoll.Util;

namespace PianoRoll.Model
{
    public static class Render
    {
        private static WaveChannel32 waveChannel;
        private static WaveOutEvent player;
        private static WaveStream output;
        private static Process bat;
        private static long position;
        private static Part Part;
        private static bool IsPlaying;

        public static long PlayerPosition => output.Position;

        public static long PlayerLength => output.Length;

        public delegate void RenderCompletedHandler();

        public static event RenderCompletedHandler OnRenderComplited;

        public static void Send(Part part)
        {
            OnRenderComplited += OnRenderCompleted_Render;
            part.BuildRenderPart();
            Part = part.RenderPart;
            position = 0;
            Part.Recalculate();
            Part.BuildPitch();
            if (output != null) output.Close();
            if (File.Exists(Settings.Output)) File.Delete(Settings.Output);
            if (File.Exists(Settings.Output + ".dat")) File.Delete(Settings.Output + ".dat");
            if (File.Exists(Settings.Output + ".whd")) File.Delete(Settings.Output + ".whd");

            if (!File.Exists(Settings.Bat)) File.Create(Settings.Bat);
            var delcommand = $"del \"{Settings.CacheFolder}\\*.wav\"\r\n";
            File.WriteAllText(Settings.Bat, delcommand);
            var i = 1;
            long renderPosition = 0;
            foreach (var note in Part.Notes)
            {
                var tempfilename = Path.Combine(Settings.CacheFolder, $"{i}");
                tempfilename += $"_{note.Lyric.GetHashCode()}_{note.NoteNum}_{note.Length}.wav";
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

            File.AppendAllText(Settings.Bat,
                $"@if not exist \"{Settings.Output}.whd\" goto E \r\n" +
                $"@if not exist \"{Settings.Output}.dat\" goto E \r\n" +
                $"copy /Y \"{Settings.Output}.whd\" /B + \"{Settings.Output}.dat\" /B \"{Settings.Output}\" \r\n" +
                $"del \"{Settings.Output}.whd\" \r\n" + $"del \"{Settings.Output}.dat\" \r\n" + ":E");
            Process();
        }

        private static void OnRenderCompleted_Render()
        {
        }

        private static async void Process()
        {
            bat = new Process();
            bat.StartInfo.FileName = Settings.Bat;
            bat.StartInfo.WorkingDirectory = Settings.CacheFolder;
            bat.EnableRaisingEvents = true;
            bat.Exited += OnExited;
            await ProcessStart();
        }

        private static Task<bool> ProcessStart()
        {
            return Task.Run(() =>
            {
                bat.Start();
                return true;
            });
        }

        public static void OnExited(object sender, EventArgs e)
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
            if (player != null)
                player.Stop();
            if (output != null)
                output.Close();
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

            if (output != null)
                output.Close();
        }

        public static int[] TakeEach(int[] array, int each)
        {
            IsPlaying = false;
            var list = new List<int>();
            for (var i = 0; i < array.Length; i++)
                if (i % each == 0)
                    list.Add(array[i]);

            return list.ToArray();
        }

        public static void SendToResampler(Note note, string tempFilename)
        {
            var pitchBase64 = Base64.Base64EncodeInt12(TakeEach(note.PitchBend.Array, Settings.SkipOnRender));
            var phoneme = note.HasPhoneme? note.Phoneme : note.DefaultPhoneme;
            string request = string.Format(
                "\"{0}\" \"{1}\" \"{2}\" {3} {4:D} \"{5}\" {6} {7:D} {8} {9} {10:D} {11:D} !{12} {13}\r\n\r\n",
                Settings.Resampler, 
                Path.Combine(Part.Track.Singer.Dir, phoneme.File), 
                tempFilename,
                MusicMath.NoteNum2String(note.NoteNum - 12), 
                note.Velocity, 
                Part.Flags + note.Flags, 
                phoneme.Offset,
                (int) note.RequiredLength, 
                phoneme.Consonant, 
                phoneme.Cutoff, 
                note.Intensity, 
                note.Modulation,
                note.NoteNum, 
                pitchBase64);
            File.AppendAllText(Settings.Bat, request);
        }

        /// <summary>
        ///     Send Note to AppendTool
        /// </summary>
        public static void SendToAppendTool(Note note, string filename)
        {
            var next = Part.GetNextNote(note);
            var offset = note.Pre;
            if (next != null)
            {
                offset -= next.Pre;
                offset += next.Ovl;
            }

            var envelope = new Envelope(note);
            var sign = offset >= 0 ? "+" : "-";
            var length = $"{note.Length}@{Project.Tempo}{sign}{Math.Abs(offset).ToString("f0")}";
            string ops = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}", 
                note.Stp, // STP,
                length, //note.RequiredLength, 
                envelope.p1, 
                envelope.p2, 
                envelope.p3, 
                envelope.v1, 
                envelope.v2,
                envelope.v3,
                envelope.v4, 
                note.Ovl, //note.Phoneme.Overlap,
                envelope.p4, 
                envelope.p5, 
                envelope.v5);
            var request = $"\"{Settings.AppendTool}\" \"{Settings.Output}\" \"{filename}\" {ops} \r\n";
            File.AppendAllText(Settings.Bat, request);
        }

        /// <summary>
        ///     Send Pause to AppendTool
        /// </summary>
        public static void SendToAppendTool(long duration, string filename)
        {
            //var Part = Project.Current.Tracks[0].Parts[0];
            var length = $"{duration}@{Project.Tempo}+0";
            var ops = $"0 {length} 0 0";
            var request = $"\"{Settings.AppendTool}\" \"{Settings.Output}\" \"{filename}\" {ops}\r\n";
            File.AppendAllText(Settings.Bat, request);
        }
    }
}