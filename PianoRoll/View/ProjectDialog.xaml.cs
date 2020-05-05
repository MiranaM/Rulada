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

        }

        public void Init(SingerManager singerManager, Project project)
        {
            InitSingers(singerManager);
            InitTitle(project);
        }

        public void InitSingers(SingerManager singerManager)
        {
            SingersList.Items.Clear();
            foreach (var sing in singerManager.SingerNames)
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

        public void InitTitle(Project project)
        {
            //CoverAuthor.Content = Project.Current.Title;
            //SongAuthor;
            SongName.Content = project.Title;
        }
    }
}