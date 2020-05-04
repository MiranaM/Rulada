using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PianoRoll.Model;
using PianoRoll.Util;

namespace PianoRoll.Control
{
    /// <summary>
    /// Логика взаимодействия для RenderNoteControl.xaml
    /// </summary>
    public partial class RenderNoteView : UserControl
    {
        private readonly double minheight;
        private double WidthInit;
        private double BorderWidth = 4;
        public RenderPartView PartEditor;

        public Note Note;

        public RenderNoteView(RenderPartView partEditor)
        {
            PartEditor = partEditor;
            InitializeComponent();
            minheight = PartEditor.yScale;
        }

        public void SetNote(Note note)
        {
            Note = note;
            Phoneme.Content = note.Phonemes;
            Canvas.SetLeft(this, MusicMath.GetNoteXPosition(note.AbsoluteTime));
            Width = MusicMath.MillisecondToTick(note.Length);
        }
    }
}
