/*
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

namespace DesktopFolders
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			TitleBar.MouseDown += new MouseButtonEventHandler(
				(object sender, MouseButtonEventArgs e) => {
					if (e.ChangedButton == MouseButton.Left) DragMove();
				}
			);
			//BPin.MouseDown += TitleBar_MouseDown;
			//BClose.MouseDown += TitleBar_MouseDown;
			//TBName.MouseDown += TitleBar_MouseDown;
			//BRename.MouseDown += TitleBar_MouseDown;
			//FileGrid.MouseDown += TitleBar_MouseDown;

			PopulateGrid();
		}

		private class FileData
		{
			public string Name;
			public string Extension;
			public BitmapImage Icon;
			public FileData(string name, string extension, BitmapImage icon)
			{
				this.Name = name;
				this.Extension = extension;
				this.Icon = icon;
			}
		}

		private void PopulateGrid()
		{
			//Get the files somehow (to produce something like below)
			List<FileData> fileData = new List<FileData>(new FileData[]
			{
				new FileData("My File Shortcut", "txt", new BitmapImage()),
				new FileData("Some File Shortcut", "png", new BitmapImage()),
				new FileData("My Folder Shortcut", "", new BitmapImage()),
			});
			//-//	BitmapImage image = new BitmapImage(new Uri("/MyProject;component/Images/down.png", UriKind.Relative));

			foreach (FileData fd in fileData)
			{
				//Setup UI elements
				Button FileButton = new Button();
				Grid FileContainer = new Grid;
				Label FileName = new Label();
				Image FileIcon = new Image();
				
				//Setup properties of UI elements
				FileButton.Background = new SolidColorBrush(Color.FromArgb(127, 255, 0, 0));
				FileButton.Foreground = Brushes.Black;
				FileButton.Width = 100;
				FileButton.Height = 100;
				FileButton.Padding = new Thickness(20);
				FileContainer.Background = new SolidColorBrush(Color.FromArgb(127, 0, 255, 0));
				FileContainer.Height = 80;
				FileContainer.Width = 80;
				FileName.Background = new SolidColorBrush(Color.FromArgb(127, 0, 0, 255));
				FileName.Margin = new Thickness(20);
				FileName.Width = 60;
				FileName.Height = 20;
				FileIcon.Height = 40;
				FileIcon.Width = 40;
				FileIcon.Margin = new Thickness(20);

				//Setup contents of UI elements
				FileName.Content = fd.Name;
				FileIcon.Source = fd.Icon;

				//Setup parents/children of UI elements
				FileContainer.Children.Add(FileName);
				FileContainer.Children.Add(FileIcon);
				//-//	FileButton.Content = FileContainer;
				FileButton.Content = FileName;
				FileGrid.Children.Add(FileButton);
			}
		}
	}
}
*/