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
        public PartControl PartControl;
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
            Recalculate();
            foreach (var note in Notes) Pitch.BuildPitchData(note);
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

        public void Recalculate()
        {
            foreach (var note in Notes)
                note.SubmitPosLen();
            SortNotes();
            foreach (var note in Notes)
                note.Trim();
            if (PartEditor.MustSnap)
            {
                var i = 0;
                while (i < Notes.Count)
                {
                    Notes[i].Snap();
                    i++;
                }
            }

            foreach (var note in Notes) note.RecalculatePreOvl();
        }

        public void Delete(Note note)
        {
            Notes.Remove(note);
        }

        public void BuildRenderPart()
        {
            var consonantsQueue = new List<string>();
            double addedLength = 0;
            Note prevNote = null;
            var singer = Track.Singer;

            RenderPart = new Part {Track = new Track {Singer = Track.Singer}, IsRender = true};
            var notes = RenderPart.Notes;

            foreach (var note in Notes)
            {
                if (ProcessRestNotesForRenderBuild(prevNote, note, consonantsQueue, notes, singer))
                    prevNote = null;

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

                for (var i = vowelIndex - 1; i >= 0; i--)
                {
                    consonantsQueue.Add(phonemes[i]);
                }

                var newNotes = new List<Note>();
                foreach (var phoneme in consonantsQueue)
                {
                    var length = Track.Singer.GetConsonantLength(phoneme);
                    addedLength += length;
                    newNotes.Add(CreateRenderNote(RenderPart, note.AbsoluteTime - addedLength, length, phoneme, note));
                }
                consonantsQueue.Clear();

                newNotes.Reverse();
                notes.AddRange(newNotes);
                notes.Add(CreateRenderNote(RenderPart, note.AbsoluteTime, note.Length, phonemes[vowelIndex], note));

                for (int i = vowelIndex + 1; i < phonemes.Length; i++)
                {
                    consonantsQueue.Add(phonemes[i]);
                }

                if (prevNote != null)
                {
                    // BUG: will be crash for short notes.
                    prevNote.AbsoluteTime -= addedLength;
                }

                addedLength = 0;
                prevNote = note;
            }

            ProcessRestNotesForRenderBuild(prevNote, null, consonantsQueue, notes, singer);

            foreach (var note in notes)
            {
                var transitioned = TransitionTool.Process(note);
                note.Phoneme = Track.Singer.FindPhoneme(transitioned);
            }
        }

        #region private

        private bool ProcessRestNotesForRenderBuild(Note prevNote, Note note, List<string> consonantsQueue, List<Note> notes, Singer singer)
        {
            if (RenderPart.Notes.Count == 0)
                return true;
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
                return true;
            }

            return false;
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
                Pitch.AveragePitch(note, noteNext);
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