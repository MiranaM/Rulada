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

        public RenderNote Note;

        public RenderNoteView(RenderPartView partEditor)
        {
            PartEditor = partEditor;
            InitializeComponent();
            minheight = PartEditor.yScale;
        }

        public void SetNote(RenderNote note)
        {
            Note = note;
            if (note.RenderPosition <= 0 || note.RenderLength <= 0)
                throw new Exception();

            if (note.HasOto)
                Phoneme.Content = note.SafeOto.Alias;
            var envelope = note.Envelope;
            var next = note.GetNext();

            var attack = MusicMath.Current.MillisecondToTick(envelope.p1);
            var preutterance = MusicMath.Current.MillisecondToTick(envelope.p2);
            var length = note.RenderLength;
            var decay = MusicMath.Current.MillisecondToTick(envelope.p3);
            var straightPreutterance = preutterance - attack;
            var cutoff = 0;
            if (next != null)
            {
                cutoff = MusicMath.Current.MillisecondToTick(next.StraightPre);
            }

            Overlap.Width = attack;
            Canvas.SetLeft(Overlap, -preutterance);
            var sustain = length + straightPreutterance - decay - cutoff;
            if (sustain >= 0)
            {
                Sustain.Width = sustain;
                Canvas.SetLeft(Sustain, -straightPreutterance);
                Sustain.Visibility = Visibility.Visible;
            }
            else
            {
                NoteMainBorder.Background = new SolidColorBrush(Color.FromArgb(140,255, 0, 0));
                Sustain.Visibility = Visibility.Hidden;
            }
            Decay.Width = decay;
            Canvas.SetLeft(Decay, length - decay - cutoff);
            
            Canvas.SetLeft(this, MusicMath.Current.GetNoteXPosition(note.FinalPosition));
            Canvas.SetTop(this, MusicMath.Current.GetNoteYPosition(note.NoteNum));
            Width = note.FinalLength * PartEditor.xScale;
        }
    }
}
