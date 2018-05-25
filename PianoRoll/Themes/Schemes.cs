using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PianoRoll.Themes
{
    public static class Schemes
    {
        public static SolidColorBrush blackNoteChannelBrush = new SolidColorBrush(Colors.LightCyan);
        public static SolidColorBrush noteSeparatorBrush = new SolidColorBrush(Colors.DarkGray);
        public static SolidColorBrush measureSeparatorBrush = new SolidColorBrush(Colors.Black);
        public static SolidColorBrush beatSeparatorBrush = new SolidColorBrush(Colors.DarkGray);
        public static SolidColorBrush pitchBrush = new SolidColorBrush(Colors.LightCoral);
        public static SolidColorBrush unknownNote = new SolidColorBrush(Colors.DarkOrange);
        public static SolidColorBrush noteBackground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#ffaacc"));






    }
}
