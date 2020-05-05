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
            int addedLength = 0;
            Note prevNote = null;
            Note prevVowel = null;
            foreach (var note in SourcePart.Notes)
            {
                prevNote = ProcessRestNotesForRenderBuild(prevNote, note, consonantsQueue, notes);
                if (prevNote == null)
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

                var newNotes = new List<Note>();
                for (var i = consonantsQueue.Count - 1; i >= 0; i--)
                {
                    var phoneme = consonantsQueue[i];
                    var length = (int)Singer.GetConsonantLength(phoneme);
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
                        throw new Exception();
                    // BUG: will be crash for short notes.
                    prevVowel.Length -= addedLength;
                    if (prevVowel.Length <= 0)
                        throw new Exception();
                }

                addedLength = 0;
                if (!vowel.IsRender)
                    throw new Exception();
                prevNote = vowel;
                prevVowel = vowel;
            }

            ProcessRestNotesForRenderBuild(prevNote, null, consonantsQueue, notes);

            foreach (var note in notes)
            {
                var transitioned = TransitionTool.Current.Process(note);
                var oto = Singer.FindOto(transitioned);
                if (oto.Overlap <= 0 || oto.Preutter <= 0)
                    throw new Exception();
                note.Oto = oto;
                note.RecalculatePreOvl();
            }

            foreach (var note in notes)
            {
                note.CreateEnvelope();
                note.Envelope.Check(note);
            }
        }

        #region private

        private Note ProcessRestNotesForRenderBuild(Note prevNote, Note note, List<string> consonantsQueue, List<Note> notes)
        {
            if (RenderPart.Notes.Count == 0)
                return null;
            if (prevNote != null && (note == null || prevNote.AbsoluteTime + prevNote.Length < note.AbsoluteTime))
            {
                var last = RenderPart.Notes[RenderPart.Notes.Count - 1];
                foreach (var phoneme in consonantsQueue)
                {
                    var addedNote = CreateRenderNote(RenderPart, last.AbsoluteTime + last.Length,
                        Singer.GetConsonantLength(phoneme), phoneme, prevNote);
                    notes.Add(addedNote);
                    last = addedNote;
                }
                consonantsQueue.Clear();
                notes.Add(CreateRenderNote(RenderPart, last.AbsoluteTime + last.Length, Singer.GetRestLength(last.Phonemes), TransitionTool.Current.GetRest(last.Phonemes), prevNote));

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
            if (length <= 0 || absoluteTime <= 0)
                throw new Exception();
            var note = new Note(renderPart);
            note.Length = (int)length;
            note.AbsoluteTime = (long)absoluteTime;
            note.Lyric = phoneme;
            note.Phonemes = phoneme;
            note.IsRender = true;
            note.Intensity = parent.Intensity;
            note.NoteNum = parent.NoteNum;
            note.Modulation = parent.Modulation;


            // report
            var prev = note.GetPrev();
            if (prev == null)
                ReportRenderBuild(" ");
            ReportRenderBuild($"{note.AbsoluteTime}\t{note.Length}\t{note}");

            return note;
        }

        private void ReportRenderBuild(string message)
        {
            Console.WriteLine("Render build: " + message);
        }


        #endregion
    }

}
