using System.Windows;
using System.Windows.Controls;
using PianoRoll.Model;

namespace PianoRoll.View
{
    /// <summary>
    ///     Логика взаимодействия для ProjectDialog.xaml
    /// </summary>
    public partial class ProjectDialog : Window
    {
        public ProjectDialog()
        {
            InitializeComponent();

            InitSingers();
            InitTitle();
        }

        public void InitSingers()
        {
            SingersList.Items.Clear();
            foreach (var sing in Project.Current.SingerNames)
            {
                var item = new ListViewItem();
                item.Content = sing;
                SingersList.Items.Add(sing);
            }
        }

        public void InitTools()
        {
            WavToolList.Items.Clear();
        }

        public void InitSounds() //ДОДЕЛАЙ МЕНЯ
        {
            //SoundList.Items.Clear();

            //foreach (SoundTrack st in Project.Current.SoundTracks)
            //{
            //    ListViewItem item = new ListViewItem();
            //    item.Content =
            //}
        }

        public void InitTitle()
        {
            //CoverAuthor.Content = Project.Current.Title;
            //SongAuthor;
            SongName.Content = Project.Current.Title;
        }
    }
}