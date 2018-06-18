using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model
{
    public static class TransitionTool
    {
        static bool IsDefault = false;

        public static void Load(string dir)
        {
            if (dir == "Default") Default();
            else Open(dir);
        }

        static void Default()
        {
            IsDefault = true;
        }

        static void Open(string dir) { }

        public static string Process(Note note)
        {
            if (IsDefault)
            {
                if (!note.IsConnectedLeft()) return "-" + note.Lyric;
                else return note.Lyric;
            }
            else return note.Lyric;
        }
    }
}
