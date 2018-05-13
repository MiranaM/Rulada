using Microsoft.Win32;
using NAudio.Midi;
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

namespace PianoRoll.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadMidi();
        }

        private void ButtonUst_Click(object sender, RoutedEventArgs e)
        {
            LoadUST();
        }

        private void MenuItemOpenUst_Click(object sender, RoutedEventArgs e)
        {
            LoadUST();
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }

        private void LoadMidi()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MIDI Files (*.mid)|*.mid|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value)
            {
                MidiFile midiFile = new MidiFile(openFileDialog.FileName);
                this.PianoRollControl.MidiEvents = midiFile.Events;
            }
        }

        private void LoadUST()
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "UST Files (*.ust)|*.ust|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value)
            {
                Ust ust = new Ust(openFileDialog.FileName);
                this.PianoRollControl.UNotes = Ust.NotesList;
                scrollViewer.ScrollToVerticalOffset(540);
            }

        }

        private void Save()
        {

        }

        private void Exit()
        {

        }
    }
}
