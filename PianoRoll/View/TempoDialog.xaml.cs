﻿using System.Windows;
using PianoRoll.Model;

namespace PianoRoll.View
{
    /// <summary>
    ///     Логика взаимодействия для TempoDialog.xaml
    /// </summary>
    public partial class TempoDialog : Window
    {
        public TempoDialog()
        {
            InitializeComponent();
            NewTempo.Text = Settings.Current.Tempo.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(NewTempo.Text, out var tempo)) Settings.Current.Tempo = tempo;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}