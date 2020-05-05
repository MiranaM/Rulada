using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model
{
    public class RenderPartBuilder
    {
        public Part RenderPart;
        public Part SourcePart;
        public Singer Singer;

        public void BuildRenderPart(Part part)
        {
            SourcePart = part;
            Singer = SourcePart.Track.Singer;
            if (Singer == null)
                throw new Exception();
            RenderPart = new Part { Track = new Track(Singer), IsRender = true };
            SourcePart.RenderPart = RenderPart;

            var notes = RenderPart.Notes;

            var consonantsQueue = new List<string>();
            Note prevNote = null;
            RenderNote prevRenderNote = null;
            RenderNoteParent prevVowel = null;
            foreach (var note in SourcePart.Notes)
            {
                prevRenderNote = ProcessRestNotesForRenderBuild( prevNote, note, consonantsQueue, notes, prevVowel, prevRenderNote);
                if (prevRenderNote == null)
                    prevVowel = null;

                var phonemes = note.Phonemes.Split(' ');
                var vowelIndex = -1;
                for (var i = 0; i < phonemes.Length; i++)
                {
                    if (Singer.IsVowel(phonemes[i]))
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

                RenderNoteParent vowel = new RenderNoteParent(RenderPart, note);
                vowel.Length = note.Length;
                vowel.AbsoluteTime = note.AbsoluteTime;
                FillRenderNote(vowel, phonemes[vowelIndex], note);
                var lengthParent = prevVowel != null ? prevVowel : vowel;
                var newNotes = new List<RenderNote>();
                for (var i = consonantsQueue.Count - 1; i >= 0; i--)
                {
                    var phoneme = consonantsQueue[i];
                    var length = (int)Singer.GetConsonantLength(phoneme);
                    var pitchParent = i < consonantsQueue.Count - fromPrevCount && prevRenderNote != null ? prevRenderNote : note;
                    var newNote = new RenderNoteChild(RenderPart, lengthParent);
                    FillRenderNote(newNote, phoneme, pitchParent);

                    newNotes.Add(newNote);
                    if (prevVowel != null)
                        prevVowel.AddChildAsFirst(newNote);
                    else
                        vowel.AddHeadAsFirst(newNote);
                }
                consonantsQueue.Clear();

                newNotes.Reverse();
                notes.AddRange(newNotes);
                notes.Add(vowel);

                for (int i = vowelIndex + 1; i < phonemes.Length; i++)
                {
                    consonantsQueue.Add(phonemes[i]);
                }

                prevRenderNote = vowel;
                prevVowel = vowel;
                prevNote = note;
            }

            ProcessRestNotesForRenderBuild( prevNote, null, consonantsQueue, notes, prevVowel, prevRenderNote);

            ProcessOto(notes);
            ResolveLength(notes);
            ProcessEnvelope(notes);
        }

        #region private

        private void ProcessOto(List<Note> notes)
        {
            for (var index = 0; index < notes.Count; index++)
            {
                var note = (RenderNote)notes[index];
                var prev = (RenderNote)notes.ElementAtOrDefault(index - 1);
                if (prev != null && (prev.FinalPosition + prev.FinalLength > note.FinalPosition))
                    throw new Exception();
                var isConnectedLeft = prev != null && !prev.IsExhale && prev.FinalPosition + prev.FinalLength == note.FinalPosition;
                var transitioned = TransitionTool.Current.Process(note, prev, isConnectedLeft);
                var oto = Singer.FindOto(transitioned);
                if (oto.Overlap <= 0 || oto.Preutter <= 0)
                    throw new Exception();
                note.Oto = oto;
                note.RecalculatePreOvl(prev);
            }
        }

        private void ResolveLength(List<Note> notes)
        {
            RenderNoteParent prevParent = null;
            foreach (var note in notes)
            {
                if (note is RenderNoteParent parent)
                {
                    parent.ResolveLengths(Singer);

                    if (prevParent != null && parent.GetEditorNote().FinalPosition ==
                        prevParent.GetEditorNote().FinalPosition + prevParent.GetEditorNote().FinalLength)
                    {
                        prevParent.AttachNextParent(parent);
                    }
                    prevParent = parent;
                }
            }
        }

        private void ProcessEnvelope(List<Note> notes)
        {;
            for (var index = 0; index < notes.Count; index++)
            {
                var next = notes.ElementAtOrDefault(index + 1);
                var note = notes[index];
                note.CreateEnvelope(next);
                note.Envelope.Check(note, next);
            }
        }

        private RenderNote ProcessRestNotesForRenderBuild(Note prevNote, Note note, List<string> consonantsQueue, List<Note> notes, RenderNoteParent prevVowel, RenderNote prevRenderNote)
        {
            if (RenderPart.Notes.Count == 0)
                return null;
            if (prevNote != null && (note == null || prevNote.AbsoluteTime + prevNote.Length < note.AbsoluteTime))
            {
                var last = (RenderNote)RenderPart.Notes[RenderPart.Notes.Count - 1];
                foreach (var phoneme in consonantsQueue)
                {
                    var addedNote = new RenderNoteChild(RenderPart, prevVowel);
                    FillRenderNote(addedNote,  phoneme, prevVowel);
                    notes.Add(addedNote);
                    prevVowel.AddChild(addedNote);
                    last = addedNote;
                }
                consonantsQueue.Clear();
                var exhaleNote = new RenderNoteChild(RenderPart, prevVowel);
                FillRenderNote(exhaleNote, TransitionTool.Current.GetExhaleLength(last.Phonemes), prevVowel);
                exhaleNote.IsExhale = true;
                notes.Add(exhaleNote);
                prevVowel.AddChild(exhaleNote);

                if (!last.IsRender)
                    throw new Exception();
                return last;
            }

            return prevRenderNote;
        }

        private void FillRenderNote(RenderNote note, string phoneme, Note pitchParent)
        {
            note.Lyric = phoneme;
            note.Phonemes = phoneme;
            note.Intensity = pitchParent.Intensity;
            note.NoteNum = pitchParent.NoteNum;
            note.Modulation = pitchParent.Modulation;


            // report
            var prev = note.GetPrev();
            if (prev == null)
                ReportRenderBuild(" ");
            ReportRenderBuild($"{note.AbsoluteTime}\t{note.Length}\t{note}");
        }

        private void ReportRenderBuild(string message)
        {
            Console.WriteLine("Render build: " + message);
        }


        #endregion
    }

}
