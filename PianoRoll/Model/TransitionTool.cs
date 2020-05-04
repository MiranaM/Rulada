namespace PianoRoll.Model
{
    public static class TransitionTool
    {
        private static bool IsDefault;

        public static void Load(string dir)
        {
            if (dir == "Default")
                Default();
            else
                Open(dir);
        }

        private static void Default()
        {
            IsDefault = true;
        }

        private static void Open(string dir)
        {
        }

        public static string Process(Note note)
        {
            if (IsDefault)
            {
                if (!note.IsConnectedLeft() && note.Lyric[0] != '-')
                    return "-" + note.Lyric;
                return note.Lyric;
            }

            if (note.Part.Track.Singer.VoicebankType == "Arpasing RUS" && !note.Phonemes.Contains("-"))
            {
                if (!note.IsConnectedLeft())
                {
                    return "- " + note.Phonemes;
                }

                var prevph = note.GetPrev().Phonemes;
                return $"{prevph} {note.Phonemes}";
            }

            return note.Lyric;
        }

        public static string GetRest(string phoneme)
        {
            // TODO: do right way
            return phoneme + " -";
        }
    }
}