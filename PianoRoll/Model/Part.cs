using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PianoRoll.Util;
using System.Globalization;

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
        public Singer Singer;
        public string Flags;
        public List<Note> Notes = new List<Note>();
        public Track Track;
        public PitchBendExpression PitchBend;

        public void Recalculate() { foreach (Note note in Notes) note.Recalculate(); }
        public List<Note> GetSortedNotes() { return Notes.OrderBy(n => n.AbsoluteTime).ToList(); }

        public Part() { }

        public Note GetPrevNote(Note note)
        {
            List<Note> notes = GetSortedNotes();
            int i = notes.IndexOf(note);
            if (i == 0) return null;
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
            foreach (Note note in Notes) if (!note.IsRest) Pitch.BuildPitchData(note);
            AveragePitch();
            PitchTrimStart();
            //PitchTrimEnd();
        }

        void AveragePitch()
        {
            for (int i = 0; i < Notes.Count - 1; i++)
            {
                Note note = Notes[i];
                Note noteNext = Notes[i + 1];
                if (note.PitchBend == null || noteNext.PitchBend == null) continue;
                if (!note.IsRest) Pitch.AveragePitch(note, noteNext);
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
            foreach(Note note in Notes)
            {
                note.Lyric = note.Lyric;
                if (note.Lyric == "" || note.IsRest)
                note.Phoneme = Singer.FindPhoneme(note.Lyric);
                if (note.Phoneme != null) note.HasPhoneme = true;
                else note.HasPhoneme = false;
            }
        }
    }
}
