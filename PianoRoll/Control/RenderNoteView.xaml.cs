﻿using System;
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
            if (note.HasPhoneme)
                Phoneme.Content = note.Phoneme.Alias;
            var envelope = note.Envelope;
            var next = note.GetNext();

            var attack = MusicMath.Current.MillisecondToTick(envelope.p1);
            var preutterance = MusicMath.Current.MillisecondToTick(envelope.p2);
            var length = MusicMath.Current.MillisecondToTick(note.Length);
            var decay = MusicMath.Current.MillisecondToTick(envelope.p3);
            var straightPreutterance = preutterance - attack;
            var cutoff = 0;
            if (next != null)
            {
                cutoff = MusicMath.Current.MillisecondToTick(next.Pre - next.Ovl);
            }

            Overlap.Width = attack;
            Canvas.SetLeft(Overlap, -preutterance);
            Sustain.Width = length + straightPreutterance - decay - cutoff;
            Canvas.SetLeft(Sustain, -straightPreutterance);
            Decay.Width = decay;
            Canvas.SetLeft(Decay, length - decay - cutoff);
            
            Canvas.SetLeft(this, MusicMath.Current.GetNoteXPosition(note.AbsoluteTime));
            Width = note.Length * PartEditor.xScale;
        }
    }
}
