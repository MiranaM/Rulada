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

            foreach (Note note in Notes)
            {
                if (!note.IsRest)
                {
                    Pitch.BuildPitchData2(note);
                }
            }
            for (int i = 0; i < Notes.Count-1; i++)
            {
                Recalculate();
                Note note = Notes[i];
                Note noteNext = GetNextNote(note);
                Note notePrev = GetPrevNote(note);
                if (note.IsRest) continue;

                if (note.PitchBend == null || note.IsRest) continue;


                if (noteNext.PitchBend == null || noteNext.IsRest) continue;

                // average pitch
                if (!note.IsRest)
                {
                    AveragePitch(note, noteNext);
                }

                //remove excess pitch
                if (notePrev == null || notePrev.IsRest)
                {
                    var xPre = MusicMath.TickToMillisecond((long)note.PitchBend.Points[0].X);
                    if (xPre < -note.pre)
                    {
                        int tokick = (int)Math.Round(-(note.pre + xPre) / Settings.IntervalMs);
                        note.PitchBend.Array = note.PitchBend.Array.Skip(tokick).ToArray();
                    }
                }
            }
        }

        public void BuildPartPitch()
        {

        }

        void AveragePitch(Note note, Note noteNext)
        {
            int[] thisPitch = note.PitchBend.Array;
            int[] nextPitch = noteNext.PitchBend.Array;
            int length = (int)(noteNext.pre / Settings.IntervalMs);
            int start = thisPitch.Length - length;
            int C = (note.NoteNum - noteNext.NoteNum) * 100;
            for (int k = 0; k < length; k++)
            {
                thisPitch[k + start] = nextPitch[k] - C;
            }
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
