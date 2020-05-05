using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NAudio.Wave;
using PianoRoll.Control;
using PianoRoll.Util;

namespace PianoRoll.Model
{
    public class Render
    {
        #region singleton base

        private static Render current;
        private Render()
        {

        }

        public static Render Current
        {
            get
            {
                if (current == null)
                {
                    current = new Render();
                }
                return current;
            }
        }

        #endregion

        private WaveChannel32 waveChannel;
        private WaveOutEvent player;
        private WaveStream output;
        private Process bat;
        private long position;
        private Part Part;
        private bool IsPlaying;

        public RenderPartBuilder RenderPartBuilder = new RenderPartBuilder();

        public long PlayerPosition => output.Position;

        public long PlayerLength => output.Length;

        public delegate void RenderCompletedHandler();

        public event RenderCompletedHandler OnRenderComplited;

        public void Send(Part part)
        {
            OnRenderComplited += OnRenderCompleted_Render;
            RenderPartBuilder.BuildRenderPart(part);
            Part = part.RenderPart;
            position = 0;
            Part.BuildPitch();
            if (output != null)
                output.Close();
            if (File.Exists(Settings.Current.Output)) File.Delete(Settings.Current.Output);
            if (File.Exists(Settings.Current.Output + ".dat"))
                File.Delete(Settings.Current.Output + ".dat");
            if (File.Exists(Settings.Current.Output + ".whd"))
                File.Delete(Settings.Current.Output + ".whd");

            if (!File.Exists(Settings.Current.Bat)) File.Create(Settings.Current.Bat);
            var deleteCommand = $"del \"{Settings.Current.CacheFolder}\\*.wav\"\r\n";
            File.WriteAllText(Settings.Current.Bat, deleteCommand);
            var i = 1;
            long renderPosition = 0;
            foreach (var note in Part.Notes)
            {
                var renderNote = (RenderNote) note;
                var oto = renderNote.SafeOto;
                var tempFilename = Path.Combine(Settings.Current.CacheFolder, $"{i}");
                tempFilename += $"_{oto.Alias}_{renderNote.NoteNum}_{renderNote.RenderLength}.wav";
                // Send Rest
                if (renderNote.RenderPosition > renderPosition)
                {
                    long resttime = renderNote.RenderPosition - renderPosition;
                    SendToAppendTool(resttime, $"{i}_Rest.wav");
                    renderPosition += resttime;
                }

                // Send
                SendToResampler(renderNote, tempFilename);
                SendToAppendTool(renderNote, tempFilename);
                renderPosition += renderNote.RenderLength;
                i++;
            }

            File.AppendAllText(Settings.Current.Bat,
                $"@if not exist \"{Settings.Current.Output}.whd\" goto E \r\n" +
                $"@if not exist \"{Settings.Current.Output}.dat\" goto E \r\n" +
                $"copy /Y \"{Settings.Current.Output}.whd\" /B + \"{Settings.Current.Output}.dat\" /B \"{Settings.Current.Output}\" \r\n" +
                $"del \"{Settings.Current.Output}.whd\" \r\n" + $"del \"{Settings.Current.Output}.dat\" \r\n" + ":E");
            Process();
        }

        private void OnRenderCompleted_Render()
        {
        }

        private async void Process()
        {
            bat = new Process();
            bat.StartInfo.FileName = Settings.Current.Bat;
            bat.StartInfo.WorkingDirectory = Settings.Current.CacheFolder;
            bat.EnableRaisingEvents = true;
            bat.Exited += OnExited;
            await ProcessStart();
        }

        private Task<bool> ProcessStart()
        {
            return Task.Run(() =>
            {
                bat.Start();
                return true;
            });
        }

        public void OnExited(object sender, EventArgs e)
        {
            if (!File.Exists(Settings.Current.Output)) return;
            output = new WaveFileReader(Settings.Current.Output);
            output.Position = position;
            Play();
        }

        public void Play()
        {
            if (IsPlaying) Stop();
            IsPlaying = true;
            // if (output == null) output = new WaveFileReader(Settings.Output);
            waveChannel = new WaveChannel32(output);
            player = new WaveOutEvent();
            player.Init(waveChannel);
            player.Play();
        }

        public void Stop()
        {
            if (player != null)
                player.Stop();
            if (output != null)
                output.Close();
            position = 0;
        }

        public void Pause()
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

        public int[] TakeEach(int[] array, int each)
        {
            IsPlaying = false;
            var list = new List<int>();
            for (var i = 0; i < array.Length; i++)
                if (i % each == 0)
                    list.Add(array[i]);

            return list.ToArray();
        }

        public void SendToResampler(RenderNote note, string tempFilename)
        {
            var pitchBase64 = Base64.Current.Base64EncodeInt12(TakeEach(note.PitchBend.Array, Settings.Current.SkipOnRender));
            var oto = note.SafeOto;
            string request = string.Format(
                "\"{0}\" \"{1}\" \"{2}\" {3} {4:D} \"{5}\" {6} {7:D} {8} {9} {10:D} {11:D} !{12} {13}\r\n\r\n",
                Settings.Current.Resampler, 
                Path.Combine(Part.Track.Singer.Dir, oto.File), 
                tempFilename,
                MusicMath.Current.NoteNum2String(note.NoteNum - 12), 
                note.Velocity, 
                Part.Flags + note.Flags, 
                oto.Offset,
                (int) GetRequiredLength(note), 
                oto.Consonant, 
                oto.Cutoff, 
                note.Intensity, 
                note.Modulation,
                note.NoteNum, 
                pitchBase64);
            File.AppendAllText(Settings.Current.Bat, request);
        }

        private int GetRequiredLength(RenderNote note)
        {
            var next = Part.GetNextNote(note);
            var prev = Part.GetPrevNote(note);
            var len = note.FinalLength;
            double requiredLength = len + note.Pre;
            if (next != null)
            {
                requiredLength -= next.SafeOto.Preutter;
                requiredLength += next.SafeOto.Overlap;
            }

            requiredLength = Math.Ceiling((requiredLength + note.Stp + 25) / 50) * 50;
            return (int)requiredLength;
        }

        /// <summary>
        ///     Send Note to AppendTool
        /// </summary>
        public void SendToAppendTool(RenderNote note, string filename)
        {
            var next = Part.GetNextNote(note);
            var offset = note.Pre;
            if (next != null)
            {
                offset -= next.Pre;
                offset += next.Ovl;
            }

            var envelope = note.Envelope;
            var sign = offset >= 0 ? "+" : "-";
            var length = $"{note.RenderLength}@{Settings.Current.Tempo}{sign}{Math.Abs(offset).ToString("f0")}";
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
                note.Ovl, //note.Oto.Overlap,
                envelope.p4, 
                envelope.p5, 
                envelope.v5);
            var request = $"\"{Settings.Current.AppendTool}\" \"{Settings.Current.Output}\" \"{filename}\" {ops} \r\n";
            File.AppendAllText(Settings.Current.Bat, request);
        }

        /// <summary>
        ///     Send Pause to AppendTool
        /// </summary>
        public void SendToAppendTool(long duration, string filename)
        {
            //var Part = Project.Current.Tracks[0].Parts[0];
            var length = $"{duration}@{Settings.Current.Tempo}+0";
            var ops = $"0 {length} 0 0";
            var request = $"\"{Settings.Current.AppendTool}\" \"{Settings.Current.Output}\" \"{filename}\" {ops}\r\n";
            File.AppendAllText(Settings.Current.Bat, request);
        }
    }
}