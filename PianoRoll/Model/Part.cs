using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using PianoRoll.Control;
using PianoRoll.Util;

namespace PianoRoll.Model
{
    internal enum Insert
    {
        Before,
        After
    }

    public class Part
    {
        public PartTransform PartTransform;
        public string Flags = "DP0";
        public List<Note> Notes;
        public Track Track;
        public PitchBendExpression PitchBend;
        public bool IsRender;

        public Part RenderPart;

        public Part()
        {
            Notes = new List<Note>();
        }

        public List<Note> GetSortedNotes()
        {
            return Notes.OrderBy(n => n.AbsoluteTime).ToList();
        }

        public void SortNotes()
        {
            Notes = Notes.OrderBy(n => n.AbsoluteTime).ToList();
        }

        public Note GetPrevNote(Note note)
        {
            var notes = GetSortedNotes();
            var i = notes.IndexOf(note);
            if (i <= 0) return null;
            return Notes[i - 1];
        }

        public Note GetNextNote(Note note)
        {
            var notes = GetSortedNotes();
            var i = notes.IndexOf(note);
            if (i > notes.Count - 2) return null;
            return Notes[i + 1];
        }

        public void BuildPitch()
        {
            foreach (var note in Notes)
                Pitch.Current.BuildPitchData(note);
            AveragePitch();
            PitchTrimStart();
            PitchTrimEnd();
        }

        public void BuildPartPitch()
        {
            //Pitch.BuildPitchData(PitchBend);
        }

        public void RefreshPhonemes()
        {
            // foreach (Note note in Notes) note.Lyric = note.Lyric;
        }

        public void AddNote(long startTime, int noteNum)
        {
            var note = new Note(this);
            note.NoteNum = noteNum;
            note.AbsoluteTime = startTime;
            note.Part = this;
            Notes.Add(note);
        }

        public void AddNote(long startTime, int noteNum, int length)
        {
            var note = new Note(this);
            note.NoteNum = noteNum;
            note.AbsoluteTime = startTime;
            note.Part = this;
            note.Length = length;
            Notes.Add(note);
        }

        public void DeleteNote(Note note)
        {
            Notes.Remove(note);
        }

        public void Recalculate()
        {
            foreach (var note in Notes)
                note.SubmitPosLen();
            SortNotes();
            TrimNotes();
            if (PartEditor.MustSnap)
            {
                var i = 0;
                while (i < Notes.Count)
                {
                    Notes[i].Snap();
                    i++;
                }
            }

            RecalculatePreOvl();
        }

        public void TrimNotes()
        {
            for (var index = 0; index < Notes.Count; index++)
            {
                var note = Notes[index];
                var next = note.GetNext();
                if (next != null && note.Length > next.AbsoluteTime - note.AbsoluteTime)
                    note.Length = next.AbsoluteTime - note.AbsoluteTime;
                if (note.Length <= 0)
                {
                    note.Delete();
                    index--;
                }
            }
        }

        public void RecalculatePreOvl()
        {

            foreach (var note in Notes)
                note.RecalculatePreOvl();
        }

        public void Delete(Note note)
        {
            Notes.Remove(note);
        }

        public void BuildRenderPart()
        {
            var consonantsQueue = new List<string>();
            int addedLength = 0;
            Note prevNote = null;
            Note prevVowel = null;
            var singer = Track.Singer;

            RenderPart = new Part {Track = new Track {Singer = Track.Singer}, IsRender = true};
            var notes = RenderPart.Notes;

            foreach (var note in Notes)
            {
                prevNote = ProcessRestNotesForRenderBuild(prevNote, note, consonantsQueue, notes, singer);
                if (prevNote == null)
                    prevVowel = null;

                var phonemes = note.Phonemes.Split(' ');
                var vowelIndex = -1;
                for (var i = 0; i < phonemes.Length; i++)
                {
                    if (Track.Singer.IsVowel(phonemes[i]))
                    {
                        vowelIndex = i;
                        break;
                    }
                }

                if (vowelIndex == -1)
                {
                    consonantsQueue.AddRange(phonemes);
                    continue;
                }

                var fromPrevCount = consonantsQueue.Count;
                for (var i = 0; i < vowelIndex; i++)
                {
                    consonantsQueue.Add(phonemes[i]);
                }

                var newNotes = new List<Note>();
                for (var i = consonantsQueue.Count - 1; i >= 0; i--)
                {
                    var phoneme = consonantsQueue[i];
                    var length = (int)Track.Singer.GetConsonantLength(phoneme);
                    addedLength += length;
                    var parent = i < consonantsQueue.Count - fromPrevCount && prevNote != null ? prevNote : note;
                    newNotes.Add(CreateRenderNote(RenderPart, note.AbsoluteTime - addedLength, length, phoneme, parent));
                }
                consonantsQueue.Clear();

                newNotes.Reverse();
                notes.AddRange(newNotes);
                var vowel = CreateRenderNote(RenderPart, note.AbsoluteTime, note.Length, phonemes[vowelIndex], note);
                notes.Add(vowel);

                for (int i = vowelIndex + 1; i < phonemes.Length; i++)
                {
                    consonantsQueue.Add(phonemes[i]);
                }

                if (prevVowel != null)
                {
                    if (!prevVowel.IsRender)
                        throw  new Exception();
                    // BUG: will be crash for short notes.
                    prevVowel.Length -= addedLength;
                }

                addedLength = 0;
                if (!vowel.IsRender)
                    throw new Exception();
                prevNote = vowel;
                prevVowel = vowel;
            }

            ProcessRestNotesForRenderBuild(prevNote, null, consonantsQueue, notes, singer);

            long prevAbs = 0;
            foreach (var note in notes)
            {
                var transitioned = TransitionTool.Process(note);
                note.Phoneme = Track.Singer.FindPhoneme(transitioned);

                // report
                var prev = note.GetPrev();
                var neededAbs = note.AbsoluteTime - prevAbs;
                if (prev == null)
                    ReportRenderBuild(" ");
                ReportRenderBuild($"{note.AbsoluteTime}\t{note.Length}({neededAbs})\t{note}");
                prevAbs = note.AbsoluteTime;
            }
        }

        #region private

        private Note ProcessRestNotesForRenderBuild(Note prevNote, Note note, List<string> consonantsQueue, List<Note> notes, Singer singer)
        {
            if (RenderPart.Notes.Count == 0)
                return null;
            if (prevNote != null && (note == null || prevNote.AbsoluteTime + prevNote.Length < note.AbsoluteTime))
            {
                var last = RenderPart.Notes[RenderPart.Notes.Count - 1];
                foreach (var phoneme in consonantsQueue)
                {
                    var addedNote = CreateRenderNote(RenderPart, last.AbsoluteTime + last.Length,
                        Track.Singer.GetConsonantLength(phoneme), phoneme, prevNote);
                    notes.Add(addedNote);
                    last = addedNote;
                }
                consonantsQueue.Clear();
                notes.Add(CreateRenderNote(RenderPart, last.AbsoluteTime + last.Length, singer.GetRestLength(last.Phonemes), TransitionTool.GetRest(last.Phonemes), prevNote));

                if (!last.IsRender)
                    throw new Exception();
                return last;
            }

            if (!prevNote.IsRender)
                throw new Exception();
            return prevNote;
        }

        private Note CreateRenderNote(Part renderPart, double absoluteTime, double length, string phoneme, Note parent)
        {
            var note = new Note(renderPart);
            note.Length = length;
            note.AbsoluteTime = absoluteTime;
            note.Phonemes = phoneme;
            note.Lyric = phoneme;
            note.IsRender = true;
            note.Intensity = parent.Intensity;
            note.NoteNum = parent.NoteNum;
            note.Modulation = parent.Modulation;
            return note;
        }

        private void ReportRenderBuild(string message)
        {
            Console.WriteLine("Render build: " + message);
        }

        private void AddNote(Note note)
        {
            Notes.Add(note);
            SortNotes();
        }

        private void AveragePitch()
        {
            for (var i = 0; i < Notes.Count - 1; i++)
            {
                var note = Notes[i];
                var noteNext = Notes[i + 1];
                if (note.PitchBend == null || noteNext.PitchBend == null) continue;
                Pitch.Current.AveragePitch(note, noteNext);
            }
        }

        private void PitchTrimStart()
        {
            foreach (var note in Notes)
            {
                if (note.PitchBend == null || note.PitchBend.Array == null) continue;
                var lenms = MusicMath.TickToMillisecond(note.Length) + note.Pre;
                var lentick = MusicMath.MillisecondToTick(lenms);
                var len = lentick / Settings.IntervalTick;
                if (note.PitchBend.Array.Length > len)
                {
                    int tokick = note.PitchBend.Array.Length - len;
                    note.PitchBend.Array = note.PitchBend.Array.Skip(tokick).ToArray();
                }
            }
        }

        private void PitchTrimEnd()
        {
            for (var i = 0; i < Notes.Count - 1; i++)
            {
                var note = Notes[i];
                var noteNext = Notes[i + 1];
                if (note.PitchBend == null || note.PitchBend.Array == null) continue;
                if (noteNext.PitchBend == null || noteNext.PitchBend.Array == null) continue;
                if (noteNext.Ovl >= noteNext.Pre) continue;
                var lenms = noteNext.Pre - noteNext.Ovl;
                var lentick = MusicMath.MillisecondToTick(lenms);
                var len = lentick / Settings.IntervalTick;
                if (note.PitchBend.Array.Length > len)
                {
                    var tokick = len;
                    note.PitchBend.Array = note.PitchBend.Array.Skip(tokick).ToArray();
                }
            }
        }

        #endregion
    }
}