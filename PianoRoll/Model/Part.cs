using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using PianoRoll.Control;
using PianoRoll.Model.Pitch;
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
                PitchController.Current.BuildPitchData(note);
        }

        public void AddNote(long startTime, int noteNum)
        {
            var note = new Note(this);
            note.NoteNum = noteNum;
            note.AbsoluteTime = startTime;
            note.Part = this;
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
        }

        public void TrimNotes()
        {
            for (var index = 0; index < Notes.Count; index++)
            {
                var note = Notes[index];
                var next = note.GetNext();
                if (next != null && note.Length > next.AbsoluteTime - note.AbsoluteTime)
                    note.Length = (int)(next.AbsoluteTime - note.AbsoluteTime);
                if (note.Length <= 0)
                {
                    note.Delete();
                    index--;
                }
            }
        }

        public void Delete(Note note)
        {
            Notes.Remove(note);
        }


        #region private

        private void PitchTrimStart()
        {
            foreach (var note in Notes)
            {
                if (note.PitchBend == null || note.PitchBend.Array == null) continue;
                var lenms = MusicMath.Current.TickToMillisecond(note.Length) + note.Pre;
                var lentick = MusicMath.Current.MillisecondToTick(lenms);
                var len = lentick / Settings.Current.IntervalTick;
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
                if (note.PitchBend == null || note.PitchBend.Array == null)
                    continue;
                if (noteNext.PitchBend == null || noteNext.PitchBend.Array == null)
                    continue;
                if (noteNext.Ovl >= noteNext.Pre)
                    continue;
                var cutoffMs = MusicMath.Current.MillisecondToTick(noteNext.Pre);
                var cutoffTick = cutoffMs / Settings.Current.IntervalTick;
                if (note.PitchBend.Array.Length > cutoffTick)
                {
                    var toKick = cutoffTick;
                    note.PitchBend.Array = note.PitchBend.Array.Skip(toKick).ToArray();
                }
            }
        }

        #endregion
    }
}