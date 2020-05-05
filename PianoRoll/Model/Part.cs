using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using PianoRoll.Control;
using PianoRoll.Model.Pitch;
using PianoRoll.Util;
using PianoRoll.View;

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
            for (var index = 0; index < Notes.Count; index++)
            {
                var note = Notes[index];
                var prevNote = Notes.ElementAtOrDefault(index - 1);
                var nextNote = Notes.ElementAtOrDefault(index + 1);
                PitchController.Current.BuildPitchData(note, prevNote, nextNote);
            }
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
    }
}