using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace PianoRoll.View
{
    /// <summary>
    ///     Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly List<string> ResamplerList = new List<string>();
        private readonly List<string> WavToolList = new List<string>();
        private List<string> VoicebankList = new List<string>();
        private readonly Dictionary<string, string> VoicebankPaths = new Dictionary<string, string>();

        public SettingsWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            InitLists();
            Resamplers.ItemsSource = ResamplerList;
            WavTools.ItemsSource = WavToolList;
            Resamplers.SelectedIndex = 1;
            WavTools.SelectedIndex = 1;
            VoicePath.Text = Settings.Current.VoicebankDirectory;
            InitVoicebanks();
        }

        private void InitLists()
        {
            // Process the list of files found in the directory.
            var fileEntries = Directory.GetFiles(Path.GetDirectoryName(Settings.Current.Resampler));
            foreach (var fileName in fileEntries)
                if (fileName.EndsWith(".exe"))
                {
                    if (fileName.Contains("wav") || fileName.Contains("Wav"))
                        WavToolList.Add(Path.GetFileNameWithoutExtension(fileName));
                    else
                        ResamplerList.Add(Path.GetFileNameWithoutExtension(fileName));
                }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.Current.Resampler = Path.Combine(Path.GetDirectoryName(Settings.Current.Resampler),
                Resamplers.SelectedItem + ".exe");
            Settings.Current.AppendTool = Path.Combine(Path.GetDirectoryName(Settings.Current.Resampler),
                WavTools.SelectedItem + ".exe");
            Close();
        }

        private void InitVoicebanks()
        {
            VoicebankList = new List<string>();
            foreach (var path in Directory.GetDirectories(Settings.Current.VoicebankDirectory))
            {
                var charpath = Path.Combine(path, "character.txt");
                var hasChar = false;
                if (File.Exists(charpath))
                {
                    var lines = File.ReadAllLines(charpath);
                    foreach (var line in lines)
                        if (line.Contains("name="))
                        {
                            var name = line.Substring("name=".Length);
                            VoicebankList.Add(name);
                            VoicebankPaths[name] = path;
                            hasChar = true;
                        }
                }

                if (!hasChar)
                {
                    var name = Path.GetFileName(path);
                    VoicebankList.Add(name);
                    VoicebankPaths[name] = path;
                }
            }

            Voicebanks.ItemsSource = VoicebankList;
        }

        private void OKVoice_Click(object sender, RoutedEventArgs e)
        {
            //string selected = (string)Voicebanks.SelectedItem;
            //Singer.Load(System.IO.Path.Combine(Settings.VoicebankDirectory, VoicebankPaths[selected]));
            //this.DialogResult = true;            
            //this.Close();
        }

        private void VoicePath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "WAV Files (*.wav)|*.wav|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value)
            {
                var VoiceBankPath = Path.GetDirectoryName(openFileDialog.FileName);
                VoicePath.Text = VoiceBankPath;
                InitVoicebanks();
            }
        }
    }
}