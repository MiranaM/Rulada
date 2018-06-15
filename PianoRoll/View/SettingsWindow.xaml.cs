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
    public partial class SettingsWindow : Window
    {
        private List<String> ResamplerList = new List<string>();
        private List<String> WavToolList = new List<string>();
        private List<String> VoicebankList = new List<string>();
        private Dictionary<string, string> VoicebankPaths = new Dictionary<string, string>();

        public SettingsWindow()
        {
            InitializeComponent();
            Init();
        }

        void Init()
        {
            InitLists();
            Resamplers.ItemsSource = ResamplerList;
            WavTools.ItemsSource = WavToolList;
            Resamplers.SelectedIndex = 1;
            WavTools.SelectedIndex = 1;
            VoicePath.Text = Settings.VoicebankDirectory;
            InitVoicebanks();
        }

        void InitLists()
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(System.IO.Path.GetDirectoryName(Settings.Resampler));
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
            Settings.Resampler = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Settings.Resampler), Resamplers.SelectedItem.ToString() + ".exe");
            Settings.WavTool = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Settings.Resampler), WavTools.SelectedItem.ToString() + ".exe");
            this.Close();
        }

        private void InitVoicebanks ()
        {
            VoicebankList = new List<string>();
            foreach (string path in System.IO.Directory.GetDirectories(Settings.VoicebankDirectory))
            {
                string charpath = System.IO.Path.Combine(path, "character.txt");
                bool hasChar = false;
                if (File.Exists(charpath))
                {
                    string[] lines = File.ReadAllLines(charpath);
                    foreach (string line in lines)
                    {
                        if (line.Contains("name="))
                        {
                            string name = line.Substring("name=".Length);
                            VoicebankList.Add(name);
                            VoicebankPaths[name] = path;
                            hasChar = true;
                        }
                    }
                }
                if (!hasChar)
                {
                    string name = System.IO.Path.GetFileName(path);
                    VoicebankList.Add(name);
                    VoicebankPaths[name] = path;
                }
            }
            Voicebanks.ItemsSource = VoicebankList;
        }

        private void OKVoice_Click(object sender, RoutedEventArgs e)
        {
            string selected = (string)Voicebanks.SelectedItem;
            USinger.Load(System.IO.Path.Combine(Settings.VoicebankDirectory, VoicebankPaths[selected]));
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