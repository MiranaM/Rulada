using System.Windows;
using System.Windows.Media;

namespace PianoRoll.Themes
{
    public class Schemes
    {
        #region singleton base

        private static Schemes current;
        private Schemes()
        {

        }

        public static Schemes Current
        {
            get
            {
                if (current == null)
                {
                    current = new Schemes();
                }
                return current;
            }
        }

        #endregion

        public SolidColorBrush blackNoteChannelBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#e2e0df"));

        public SolidColorBrush noteSeparatorBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#c9cac9"));

        public SolidColorBrush measureSeparatorBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#7aa38e"));

        public SolidColorBrush beatSeparatorBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#c9cac9"));

        public SolidColorBrush pitchBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#ff8c75"));

        public SolidColorBrush pitchSecondBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#24aaed"));

        public SolidColorBrush unknownBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#772211"));

        public SolidColorBrush pianoBlackNote =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#556064"));

        public SolidColorBrush pianoNoteNames =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#b5b5b5"));

        public SolidColorBrush foreBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#90FFFF"));

        public SolidColorBrush partBackgroundBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#334444"));

        public SolidColorBrush partNoteBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#60AAAA"));

        public SolidColorBrush black = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#000000"));

        public SolidColorBrush positionMarkerHead =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#888888"));

        public SolidColorBrush positionMarkerLine =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#77000000"));

        public LinearGradientBrush noteBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0.5, 0),
            EndPoint = new Point(0.5, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop((Color) ColorConverter.ConvertFromString("#FF434E5D"), 1),
                new GradientStop((Color) ColorConverter.ConvertFromString("#FF5D6E76"), 0)
            }
        };
    }
}