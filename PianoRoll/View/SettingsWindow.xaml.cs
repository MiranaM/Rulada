using Microsoft.Win32;
using PianoRoll.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace PianoRoll.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private List<String> ResamplerList = new List<string>();
        private List<String> WavToolList = new List<string>();
        private List<String> VoicebankList = new List<string>();

        public Window1()
        {
            InitializeComponent();
            InitLists();
            this.Resamplers.ItemsSource = ResamplerList;
            this.WavTools.ItemsSource = WavToolList;
            Resamplers.SelectedIndex = 1;
            this.WavTools.SelectedIndex = 1;
            VoicePath.Text = Settings.VoiceDir;
            InitVoicebanks();
        }


        public void InitLists()
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(Settings.ToolsDirectory);
            foreach (string fileName in fileEntries)
            {
                if (fileName.EndsWith(".exe"))
                {
                    if ((fileName.Contains("wav")) || (fileName.Contains("Wav")))
                        WavToolList.Add(System.IO.Path.GetFileNameWithoutExtension(fileName));
                    else
                    {
                        ResamplerList.Add(System.IO.Path.GetFileNameWithoutExtension(fileName));
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.Resampler = System.IO.Path.Combine(Settings.ToolsDirectory, Resamplers.SelectedItem.ToString() + ".exe");
            Settings.WavTool = System.IO.Path.Combine(Settings.ToolsDirectory, WavTools.SelectedItem.ToString() + ".exe");
            this.Close();
        }

        private void InitVoicebanks ()
        {
            VoicebankList = System.IO.Directory.GetDirectories(@VoicePath.Text).ToList<string>();
            Voicebanks.ItemsSource = VoicebankList;
        }

        private void OKVoice_Click(object sender, RoutedEventArgs e)
        {
            Settings.VoiceBankDir = (string)Voicebanks.SelectedItem;
            Settings.VoiceDir = System.IO.Path.GetFullPath(Settings.VoiceBankDir + "\\..\\");
            Ust.uSettings["VoiceDir"] = Settings.VoiceBankDir;
            this.DialogResult = true;            
            this.Close();
        }

        private void VoicePath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "WAV Files (*.wav)|*.wav|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value)
            {
                string VoiceBankPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                VoicePath.Text = VoiceBankPath;
                InitVoicebanks();
            }
        }
    }
}