using PianoRoll.Model;
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
using System.Windows.Shapes;

namespace PianoRoll.View
{
    /// <summary>
    /// Логика взаимодействия для ProjectDialog.xaml
    /// </summary>
    public partial class ProjectDialog : Window
    {

        public ProjectDialog()
        {
            InitializeComponent();

            InitSingers();



        }

        public void InitSingers()
        {
            SingersList.Items.Clear();
            foreach (string sing in Project.Current.SingerNames)
            {                
                ListViewItem item = new ListViewItem();
                item.Content = sing;
                SingersList.Items.Add(sing);
            }
        }

        public void InitTools()
        {
            WavToolList.Items.Clear();


        }

        public void InitSounds()
        {

        }

        public void InitNames()
        {

        }
    }
}
