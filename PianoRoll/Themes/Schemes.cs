﻿using System.Windows;
using System.Windows.Media;

namespace PianoRoll.Themes
{
    public static class Schemes
    {
        public static SolidColorBrush blackNoteChannelBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#e2e0df"));

        public static SolidColorBrush noteSeparatorBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#c9cac9"));

        public static SolidColorBrush measureSeparatorBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#7aa38e"));

        public static SolidColorBrush beatSeparatorBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#c9cac9"));

        public static SolidColorBrush pitchBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#ff8c75"));

        public static SolidColorBrush pitchSecondBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#24aaed"));

        public static SolidColorBrush unknownBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#772211"));

        public static SolidColorBrush pianoBlackNote =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#556064"));

        public static SolidColorBrush pianoNoteNames =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#b5b5b5"));

        public static SolidColorBrush foreBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#90FFFF"));

        public static SolidColorBrush partBackgroundBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#334444"));

        public static SolidColorBrush partNoteBrush =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#60AAAA"));

        public static SolidColorBrush black = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#000000"));

        public static SolidColorBrush positionMarkerHead =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#888888"));

        public static SolidColorBrush positionMarkerLine =
            new SolidColorBrush((Color) ColorConverter.ConvertFromString("#77000000"));

        public static LinearGradientBrush noteBrush = new LinearGradientBrush
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