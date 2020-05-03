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
        public string Flags;
        public List<Note> Notes;
        public Track Track;
        public PartControll PartControll;
        public PitchBendExpression PitchBend;
        private List<Note> _notes;

        public List<Note> GetSortedNotes()
        {
            return Notes.OrderBy(n => n.AbsoluteTime).ToList();
        }

        public Part()
        {
            Notes = new List<Note>();
        }

        private void AddNote(Note note)
        {
            Notes.Add(note);
            SortNotes();
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
                var lenms = MusicMath.TickToMillisecond(note.Length) + note.pre;
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
                if (noteNext.ovl >= noteNext.pre) continue;
                var lenms = noteNext.pre - noteNext.ovl;
                var lentick = MusicMath.MillisecondToTick(lenms);
                var len = lentick / Settings.IntervalTick;
                if (note.PitchBend.Array.Length > len)
                {
                    var tokick = len;
                    note.PitchBend.Array = note.PitchBend.Array.Skip(tokick).ToArray();
                }
            }
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
            foreach (var note in Notes) note.SubmitPosLen();
            SortNotes();
            foreach (var note in Notes) note.Trim();
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
    }
}