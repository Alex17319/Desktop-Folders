using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using QuickZip.Tools;

namespace FileToIconTest
{
    public class Model : INotifyPropertyChanged
    {
        string _path;
        string[] _files;
        bool _showFiles =true, _showFolders = false;
        public string Path { get { return _path; } set { _path = value; OnPropertyChanged("Path"); } }
        public string[] Files { get { return _files; } set { _files = value; OnPropertyChanged("Files"); } }
        public bool ShowFiles { get { return _showFiles; } set { _showFiles = value; OnPropertyChanged("ShowFiles"); } }
        public bool ShowFolders { get { return _showFolders; } set { _showFolders = value; OnPropertyChanged("ShowFolders"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        Window1 _view;

        public Model(Window1 view)
        {
            _view = view;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(
                    this,
                    new PropertyChangedEventArgs(propertyName)
                    );
            }

            //Lazy =D
            if (propertyName == "Path" || propertyName == "ShowFiles" || propertyName == "ShowFolders")
            {
                _view.ClearCache();
                List<string> folderAndFiles = new List<string>();
                if (ShowFolders) folderAndFiles.AddRange(Directory.GetDirectories(Path).ToArray());
                if (ShowFiles) folderAndFiles.AddRange(Directory.GetFiles(Path).ToArray());

                Files = folderAndFiles.ToArray();
            }
        }
    }
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        Model model;
        public Window1()
        {
            InitializeComponent();            
            model = new Model(this);
            DataContext = model;
            model.Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            FileToIconConverter fic = this.Resources["fic"] as FileToIconConverter;
            fic.DefaultSize = 200;
            list.AddHandler(ListViewItem.PreviewMouseDownEvent, new MouseButtonEventHandler(list_MouseDown));
        }

        public void ClearCache()
        {
            FileToIconConverter fic = this.Resources["fic"] as FileToIconConverter;
            //Clear Thumbnail only, icon is not cleared.
            fic.ClearInstanceCache();
        }

        private void change_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = model.Path;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                model.Path = dlg.SelectedPath;
        }

        private void list_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                if (list.SelectedValue is string)
                {
                    string dir = list.SelectedValue as string;
                    if (Directory.Exists(dir))
                    {
                        model.Path = dir;
                        e.Handled = true;
                    }
                }
            }
        }

    }
}
