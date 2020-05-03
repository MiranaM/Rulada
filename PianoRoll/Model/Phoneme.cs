namespace PianoRoll.Model
{
    public class Phoneme
    {
        public string File { set; get; }
        public string Alias { set; get; }
        public double Offset { set; get; }
        public double Consonant { set; get; }
        public double Cutoff { set; get; }
        public double Preutter { set; get; }
        public double Overlap { set; get; }

        public static Phoneme GetDefault(string alias = "")
        {
            var phoneme = new Phoneme
            {
                Alias = alias,
                Offset = 0,
                Consonant = 0,
                Cutoff = 0,
                Preutter = 0,
                Overlap = 0
            };
            return phoneme;
        }
    }
}