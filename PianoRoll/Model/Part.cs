using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PianoRoll.Util;
using System.Globalization;
using PianoRoll.Control;

namespace PianoRoll.Model
{
    enum Insert
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

        public void Recalculate() { foreach (Note note in Notes) note.Recalculate(); }
        public List<Note> GetSortedNotes() { return Notes.OrderBy(n => n.AbsoluteTime).ToList(); }

        public Part()
        {
            Notes = new List<Note>();
        }

        void AddNote(Note note)
        {
            Notes.Add(note);
            SortNotes();
        }

        public void SortNotes()
        {
            Notes = Notes.OrderBy(n => n.AbsoluteTime).ToList();
        }
        
        public void TrimNotes()
        {
            foreach (Note note in Notes)
            {
                Note next = note.GetNext();
                if (next != null && note.Length > next.AbsoluteTime - note.AbsoluteTime)
                    note.Length = next.AbsoluteTime - note.AbsoluteTime;
            }
        }

        public Note GetPrevNote(Note note)
        {
            List<Note> notes = GetSortedNotes();
            int i = notes.IndexOf(note);
            if (i <= 0) return null;
            return Notes[i - 1];
        }

        public Note GetNextNote(Note note)
        {
            List<Note> notes = GetSortedNotes();
            int i = notes.IndexOf(note);
            if (i > notes.Count - 2) return null;
            return Notes[i + 1];
        }

        public void BuildPitch()
        {
            Recalculate();
            foreach (Note note in Notes) Pitch.BuildPitchData(note);
            AveragePitch();
            PitchTrimStart();
            PitchTrimEnd();
        }

        void AveragePitch()
        {
            for (int i = 0; i < Notes.Count - 1; i++)
            {
                Note note = Notes[i];
                Note noteNext = Notes[i + 1];
                if (note.PitchBend == null || noteNext.PitchBend == null) continue;
                Pitch.AveragePitch(note, noteNext);
            }
        }

        void PitchTrimStart()
        {
            foreach (Note note in Notes)
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

        void PitchTrimEnd()
        {
            for (int i = 0; i < Notes.Count - 1; i++)
            {
                Note note = Notes[i];
                Note noteNext = Notes[i + 1];
                if (note.PitchBend == null || note.PitchBend.Array == null) continue;
                if (noteNext.PitchBend == null || noteNext.PitchBend.Array == null) continue;
                if (noteNext.ovl >= noteNext.pre) continue;
                var lenms = noteNext.pre - noteNext.ovl;
                var lentick = MusicMath.MillisecondToTick(lenms);
                var len = lentick / Settings.IntervalTick;
                if (note.PitchBend.Array.Length > len)
                {
                    int tokick = len;
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
            Note note = new Note(this);
            note.NoteNum = noteNum;
            note.AbsoluteTime = startTime;
            note.Part = this;
            Notes.Add(note);
        }

        public void AddNote(long startTime, int noteNum, int length)
        {
            Note note = new Note(this);
            note.NoteNum = noteNum;
            note.AbsoluteTime = startTime;
            note.Part = this;
            note.Length = length;
            Notes.Add(note);
        }

    }
}
