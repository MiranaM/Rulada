namespace PianoRoll.Model
{
    public class TransitionTool
    {
        #region singleton base

        private static TransitionTool current;
        private TransitionTool()
        {

        }

        public static TransitionTool Current
        {
            get
            {
                if (current == null)
                {
                    current = new TransitionTool();
                }
                return current;
            }
        }

        #endregion

        private bool IsDefault;

        public void Load(string dir)
        {
            if (dir == "Default")
                Default();
            else
                Open(dir);
        }

        private void Default()
        {
            IsDefault = true;
        }

        private void Open(string dir)
        {
        }

        public string Process(RenderNote note, RenderNote prev, bool isConnectedLeft)
        {
            var editorNote = note.GetEditorNote();
            if (IsDefault)
            {
                if (!editorNote.IsConnectedLeft() && note.Lyric[0] != '-')
                    return "-" + note.Lyric;
                return note.Lyric;
            }

            if (note.Part.Track.Singer.VoicebankType == "Arpasing RUS" && !note.Phonemes.Contains("-"))
            {
                if (!isConnectedLeft)
                {
                    return "- " + note.Phonemes;
                }

                var prevph = prev.Phonemes;
                return $"{prevph} {note.Phonemes}";
            }

            return note.Lyric;
        }

        public string GetExhaleLength(string phoneme)
        {
            // TODO: do right way
            return phoneme + " -";
        }
    }
}