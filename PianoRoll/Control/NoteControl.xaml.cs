﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PianoRoll.Model;
using PianoRoll.Themes;
using PianoRoll.Util;

namespace PianoRoll.Control
{
    /// <summary>
    ///     Логика взаимодействия для NoteControl.xaml
    /// </summary>
    public partial class NoteControl : UserControl
    {
        private readonly double minheight;
        private double WidthInit;
        private double BorderWidth = 4;
        public DragMode dragMode;
        public PartEditor PartEditor;

        public enum DragMode
        {
            ResizeLeft,
            ResizeRight,
            Move,
            Mutual,
            None
        }

        public delegate void NoteChangedEvent();
        public delegate void NoteDeletedEvent(NoteControl noteControl);

        public event NoteChangedEvent OnNoteChanged = delegate {  };
        public event NoteDeletedEvent OnNoteDeleted = delegate {  };

        public NoteControl(PartEditor partEditor)
        {
            PartEditor = partEditor;
            InitializeComponent();
            minheight = Settings.Current.yScale;
            OnNoteChanged += OnNoteChanged_Note;
        }

        public void Delete()
        {
            OnNoteDeleted(this);
        }

        public void OnNoteChanged_Note()
        {
            PartEditor.OnPartChanged_Part();
        }

        public Note note;

        // зафиксить текст lyric, вызывается из Note
        // чтобы отправить на изменение, нужно Note.SetNewLyric(lyric),
        // чтобы также обработать текст
        public void SetText(string lyric)
        {
            Lyric.Content = lyric;
            EditLyric.Text = lyric;
            ThumbMove.IsEnabled = false;
            ToolTip = "can't find source file";
        }

        public void SetText(string lyric, string phoneme)
        {
            Lyric.Content = lyric;
            Phoneme.Content = $"[{phoneme}]";
            EditLyric.Text = lyric;
            ThumbMove.IsEnabled = true;
        }

        private void ConfirmLyric()
        {
            EditLyric.Visibility = Visibility.Hidden;
            var text = EditLyric.Text;
            if (text.Substring(0, 1) == "/")
            {
                note.Lyric = text;
                note.Phonemes = text.Substring(1);
            }
            else
            {
                var texts = text.Split(' ');
                var noteToSet = note;
                var i = 0;
                while (noteToSet != null && i < texts.Length)
                {
                    noteToSet.SetNewLyric(texts[i]);
                    i++;
                    noteToSet = noteToSet.GetNext();
                }
            }
            // possible BUG: need OnNoteChanged on all of them?
            OnNoteChanged();
        }

        private void EditLyric_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                ConfirmLyric();
        }

        private void EditLyric_LostFocus(object sender, RoutedEventArgs e)
        {
            if (EditLyric.Visibility == Visibility.Visible)
                ConfirmLyric();
        }

        private bool TryResize(double offset)
        {
            return TrySetWidth(Width + offset);
        }

        private bool TrySetWidth(double width)
        {
            if (width > 0)
            {
                Width = width;
                return true;
            }
            else
            {
                OnNoteDeleted(this);
                return false;
            }
        }

        private void ThumbResizeLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var offset = -e.HorizontalChange;
            if (TryResize(-e.HorizontalChange))
                Canvas.SetLeft(this, Canvas.GetLeft(this) - offset);
        }

        private void ThumbResizeRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            TryResize(e.HorizontalChange);
        }

        private void ThumbResize_DragCompleted(object sender,
            DragCompletedEventArgs e)
        {
            note.Length = MusicMath.Current.GetNoteLength(Width);
            OnNoteChanged();
        }

        private void ThumbResize_DragStarted(object sender,
            DragStartedEventArgs e)
        {
            WidthInit = Width;
        }

        private void ThumbMove_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var deltaHorizontal = Math.Min(e.HorizontalChange, ActualWidth - MinWidth);
            var deltaVertical = Math.Min(e.VerticalChange, ActualWidth - MinWidth);
            Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaHorizontal);
            Canvas.SetTop(this, Canvas.GetTop(this) + deltaVertical);
        }

        private void ThumbMove_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var left = Canvas.GetLeft(this);
            Canvas.SetLeft(this, left);

            var top = Canvas.GetTop(this);
            top += minheight * 0.5;
            top -= top % minheight;
            Canvas.SetTop(this, top);
            dragMode = DragMode.None;

            OnNoteChanged();
        }

        private void AddNote(Point currentMousePosition)
        {
            var x = Canvas.GetLeft(this) + currentMousePosition.X;
            var y = Canvas.GetTop(this) + currentMousePosition.Y;
            double time = MusicMath.Current.GetNoteXPosition(note.AbsoluteTime);
            note.Length = MusicMath.Current.MillisecondToTick(x - time);
            PartEditor.AddNote(x, y);
            // OnNoteChanged is already called in "add note"
        }

        private void ThumbMove_DragStarted(object sender, DragStartedEventArgs e)
        {
            WidthInit = Width;
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                AddNote(Mouse.GetPosition(this));
        }

        private void ThumbMove_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                OnNoteDeleted(this);
            }
            else
            {
                EditLyric.Visibility = Visibility.Visible;
            }
        }
    }
}