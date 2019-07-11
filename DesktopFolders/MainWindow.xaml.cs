using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shell;
using static DesktopFolders.General;

namespace DesktopFolders
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	internal partial class MainWindow : Window
	{
		private readonly ControlHighlightManager BPin_HighlightManager;
		private readonly bool ShowExtensions = true;
		private readonly string FolderPath;
		private readonly string RootIconCacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"IconCache");
		private readonly string LocalIconCacheDir;
		private readonly string[] CommandLineArgs = Environment.GetCommandLineArgs();
		private readonly List<Tuple<string, string, MessageBoxButton, MessageBoxImage>> InitMessages
			= new List<Tuple<string, string, MessageBoxButton, MessageBoxImage>>();
		private readonly WindowChrome windowChrome = new WindowChrome();
		private readonly List<FileGridItem> FileGridItemList = new List<FileGridItem>();


		//	private readonly BackgroundWorker IconLoadWorker = new BackgroundWorker();
		//	private int iconLoadIndex = 0;
		//	//	private readonly Func<int> GetFileGridItemCount;
		//	//	private readonly Func<int, bool> GetFileGridItemIsValid;
		//	//	private readonly Func<int, string> GetFileGridItemPath;
		//	//	private readonly Action<int, ImageSource> SetFileGridItemIcon;

		private readonly List<string> Timings = new List<string>();
		private readonly Stopwatch Stopwatch = new Stopwatch();

		//	private const int sysMenuPinButtonID = 0x100;

		private readonly ReadOnlyDictionary<int, Size> sizes = new ReadOnlyDictionary<int, Size>(
			new Dictionary<int, Size>().Init(
				new KeyValuePair<int, Size>(0, new Size(3, 2)),
				new KeyValuePair<int, Size>(7, new Size(4, 2)),
				new KeyValuePair<int, Size>(9, new Size(5, 2)),
				new KeyValuePair<int, Size>(11, new Size(6, 2)),
				new KeyValuePair<int, Size>(13, new Size(6, 3)),
				new KeyValuePair<int, Size>(19, new Size(7, 3))
			)
		);

		public MainWindow()
		{
			Stopwatch.Start();

			InitializeComponent();

			Stopwatch.Stop();
			Timings.Add("1: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			this.Loaded += new RoutedEventHandler(OnLoad);

			if (CommandLineArgs.Length > 1) {
				try {
					//	//Manually parse command line arguments
					//	string rawCommandLine = Environment.CommandLine.Trim();
					//	string rawArgs;
					//	if (rawCommandLine[0] == '"') rawArgs = rawCommandLine.Substring(rawCommandLine.IndexOf('"', 1) + 1);
					//	else rawArgs = rawCommandLine.Substring(rawCommandLine.IndexOf(' ') + 1);
					//	string trimmedArgs = rawArgs.Trim();
					//	string firstArg;
					//	if (trimmedArgs[0] == '"') firstArg = trimmedArgs.Substring(1, trimmedArgs.IndexOf('"', 1) - 1);
					//	else firstArg = trimmedArgs.Substring(trimmedArgs.IndexOf(' '));
					//	
					//	string trimmedArg = firstArg.Trim('"', ' ');
					string trimmedArg = CommandLineArgs[1].Trim('"', ' ');
					string envVarExpanded = Environment.ExpandEnvironmentVariables(trimmedArg);
					if (Path.GetDirectoryName(envVarExpanded) == null) { //If it is a root, eg "C:\"
						FolderPath = envVarExpanded;
					} else {
						FolderPath = Path.GetFullPath(envVarExpanded);
					}
				} catch {
					FolderPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Desktop");
					InitMessages.Add(new Tuple<string, string, MessageBoxButton, MessageBoxImage>(
						"The target path provided for the desktop folder is invalid.", "Desktop Folders", MessageBoxButton.OK, MessageBoxImage.Exclamation
					));
				}
			} else {
				FolderPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Desktop");
				InitMessages.Add(new Tuple<string, string, MessageBoxButton, MessageBoxImage>(
					"No target path was provided for the desktop folder.", "Desktop Folders", MessageBoxButton.OK, MessageBoxImage.Exclamation
				));
			}

			Stopwatch.Stop();
			Timings.Add("2: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			//Set start size and position
			SetStartSize();
			SetStartPosition();

			Stopwatch.Stop();
			Timings.Add("3: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			//Allow the window to be dragged via the title bar.
			TitleBar.MouseDown += new MouseButtonEventHandler(
				(object sender, MouseButtonEventArgs e) => {
					if (e.ChangedButton == MouseButton.Left) DragMove();
				}
			);

			//Setup titlebar
			//	try {
			if (Path.GetDirectoryName(FolderPath) == null) { //If it is a root, eg "C:\"
				TBName.Text = FolderPath;
			} else {
				TBName.Text = Path.GetFileName(
					//Get the correctly-capitalised name
					Directory.GetDirectories(
						Path.GetDirectoryName(FolderPath),
						Path.GetFileName(FolderPath)
					).FirstOrDefault()
				);
			}
			//	} catch { } //Just leave the default value
			this.Title = TBName.Text;
			//	this.ToolTip = FolderPath;

			this.PreviewKeyDown += new KeyEventHandler(MainWindow_PreviewKeyDown);

			Stopwatch.Stop();
			Timings.Add("4: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			//Setup window chrome (allows resizing +)
			windowChrome.CaptionHeight = 0;
			windowChrome.CornerRadius = new CornerRadius(0);
			windowChrome.GlassFrameThickness = new Thickness(0, 0, 0, 0);
			windowChrome.ResizeBorderThickness = new Thickness(8);
			windowChrome.UseAeroCaptionButtons = true;
			WindowChrome.SetWindowChrome(this, windowChrome);

			Stopwatch.Stop();
			Timings.Add("5: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			//Set max height (to avoid getting too big when maximized)
			System.Drawing.Rectangle workingArea = System.Windows.Forms.Screen.GetWorkingArea(
				new System.Drawing.Rectangle(
					new System.Drawing.Point(
						(int)Math.Round(this.Left),
						(int)Math.Round(this.Top)
					),
					new System.Drawing.Size(
						(int)Math.Round(this.Width), (int)Math.Round(this.Height)
					)
				)
			);
			this.MaxHeight = workingArea.Height + 6; //When it's maximised, three pixels of each edge are cropped off for some reason
			this.MaxWidth = workingArea.Width + 6;

			//	SetupSystemMenu();

			Stopwatch.Stop();
			Timings.Add("6: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			AlwaysAboveDesktop.AddHook(Process.GetCurrentProcess(), this);
			this.Closed += (s, e) => { AlwaysAboveDesktop.RemoveHook(); };

			Stopwatch.Stop();
			Timings.Add("7: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			BPin_HighlightManager = new ControlHighlightManager(
				BPin,
				BPin,
				null,
				new ControlHighlightManager.BrushGroup(
					new ControlHighlightManager.HighlightStateBrushes(
						new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
						null, //Transparent
						null //Doesn't matter
					),
					new ControlHighlightManager.HighlightStateBrushes(
						null, //Transparent //new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
						new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
						null
					),
					new ControlHighlightManager.HighlightStateBrushes(
						new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
						new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
						null
					),
					null //Transparent, transparent, doesn't matter
				)
			);

			BPin.MouseEnter += (s, e) => { BPin_HighlightManager.Highlight(); };
			//	BPin.MouseLeave       += (s, e) => { if (!BPin.IsFocused) BPin_HighlightManager.Unhighlight(); };
			BPin.MouseLeave += (s, e) => { BPin_HighlightManager.Unhighlight(); };
			BPin.Click += (s, e) => { BPin_HighlightManager.ToggleSelect(); };
			//	BPin.GotKeyboardFocus += (s, e) => {
			//		//Only fire if the tab key caused the focus
			//		if (e.KeyboardDevice.GetKeyStates(Key.Tab) != KeyStates.None)
			//		{
			//			BPin_HighlightManager.Highlight();
			//		}
			//	};
			//	BPin.LostFocus      += (s, e) => { if (!BPin.IsMouseOver) BPin_HighlightManager.Unhighlight(); };
			BPin.PreviewKeyDown += BPin_PreviewKeyDown;

			LocalIconCacheDir = Path.Combine(
				RootIconCacheDir,
				FolderPath
				.RegexReplace(@"(\:|\\|\/)", ";")
				.RegexReplace(@"(\s|a|e|i|o|u)", "")
				.RegexReplace(@"(\<|\>|\?|\*|\|)", "-")
				.RegexReplace("\"", "-")
			//"(item1|item2|item3)" means match "item1", "item2", or "item3"
			//"\x" means escape character 'x', so if it is a letter it has a special meaning,
			//or if not it is stopped from having a special meaning. So "\s" means "whitespace character",
			//and "\|" means "pipe character with no special meaning"
			);

			Stopwatch.Stop();
			Timings.Add("8: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			SetColours();

			Stopwatch.Stop();
			Timings.Add("9: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			List<FileData> fileDataList = GetFileData(FolderPath);

			Stopwatch.Stop();
			Timings.Add("10: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			PopulateGrid(fileDataList);

			Stopwatch.Stop();
			Timings.Add("11: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			//	//Setup and start IconLoadWorker
			//	//	GetFileGridItemCount = () => { return FileGrid.Children.Count; };
			//	//	//	GetFileGridItemIcon = (i) => { return (FileGrid.Children[i] as FileGridItem)?.FileIcon; };
			//	//	GetFileGridItemIsValid = (i) => { return (FileGrid.Children[i] as FileGridItem) != null; };
			//	//	GetFileGridItemPath = (i) => { return (FileGrid.Children[i] as FileGridItem)?.FilePath; };
			//	//	SetFileGridItemIcon = (i, icon) => {
			//	//		FileGridItem fileGridItem = FileGrid.Children[i] as FileGridItem;
			//	//		if (fileGridItem != null) fileGridItem.FileIcon = icon;
			//	//	};
			//	IconLoadWorker.DoWork += LoadIcon;
			//	IconLoadWorker.RunWorkerCompleted += SwitchToNextIcon;
			//	//Find first valid FileGridItem
			//	int count = FileGrid.Children.Count;
			//	while (iconLoadIndex < count && !(FileGrid.Children[iconLoadIndex] is FileGridItem)) {
			//		iconLoadIndex++;
			//	}
			//	//Check that not at end
			//	if (iconLoadIndex < count) {
			//		//Start icon load
			//		string filePath = (FileGrid.Children[iconLoadIndex] as FileGridItem).FilePath;
			//		IconLoadWorker.RunWorkerAsync(filePath);
			//	}
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			Stopwatch.Stop();
			Timings.Add("12: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();

			foreach (var message in InitMessages)
			{
				MessageBox.Show(this, message.Item1, message.Item2, message.Item3, message.Item4);
			}

			//Try to close on lost focus (fails if pinned)
			this.Deactivated += new EventHandler((s, e2) => TryClose());

			Stopwatch.Stop();
			Timings.Add("13: " + Stopwatch.ElapsedMilliseconds);
			Stopwatch.Restart();
		}

		//	private void LoadIconsInFileGrid(object sender, DoWorkEventArgs e)
		//	{
		//		try
		//		{
		//			Thread.Sleep(5000);
		//			Console.WriteLine("#50: " + GetFileGridItemCount.Invoke());
		//			int count = GetFileGridItemCount.Invoke();
		//			for (int i = 0; i < count; i++)
		//			{ //For some reason UIElementCollection returns objects, not UIElements, when enumerated
		//				Console.WriteLine("#51");
		//				if (GetFileGridItemIsValid.Invoke(i))
		//				{
		//					string filePath = GetFileGridItemPath.Invoke(i);
		//					ImageSource icon = ExtractIcon.ExtractAssociatedIcon(filePath);
		//					SetFileGridItemIcon.Invoke(i, icon);
		//				}
		//				//	FileGridItem fileGriditem = FileGrid.Children[i] as FileGridItem;
		//				//	if (fileGriditem != null) {
		//				//		Console.WriteLine("#52: " + fileGriditem.FileCaption);
		//				//		fileGriditem.FileIcon = ExtractIcon.ExtractAssociatedIcon(fileGriditem.FilePath);
		//				//	}
		//			}
		//		} catch (Exception e2) {
		//			Console.WriteLine("#53: " + e2.ToString());
		//		}
		//		//	foreach (UIElement item in FileGrid.Children)
		//		//	{
		//		//		Console.WriteLine("#51");
		//		//		FileGridItem fileGriditem = item as FileGridItem;
		//		//		if (fileGriditem != null) {
		//		//			Console.WriteLine("#52: " + fileGriditem.FileCaption);
		//		//			fileGriditem.FileIcon = ExtractIcon.ExtractAssociatedIcon(fileGriditem.FilePath);
		//		//		}
		//		//	}
		//	}

		//	private void LoadIcon(object sender, DoWorkEventArgs e)
		//	{
		//		string filePath = (string)e.Argument;
		//		//	string filePath = IconLoadCurrentData.FilePath;
		//		Console.WriteLine("#60: " + filePath);
		//		ImageSource icon = ExtractIcon.ExtractAssociatedIcon(filePath);
		//		//	IconLoadCurrentData.Icon = icon;
		//		e.Result = icon;
		//		//	CurrentIcon.Item1 = icon;
		//		Console.WriteLine("#65: " + icon.ToString() + ", " + icon.Width + ", " + icon.Height + ", ");
		//		
		//	}
		//	//	private Tuple<System.Drawing.Icon> CurrentIcon = new Tuple<System.Drawing.Icon>(null);
		//	//	private IconLoadData IconLoadCurrentData = new IconLoadData();
		//	//	private class IconLoadData {
		//	//		public string FilePath;
		//	//		public ImageSource Icon;
		//	//	}
		//	private void SwitchToNextIcon(object sender, RunWorkerCompletedEventArgs e)
		//	{
		//		Console.WriteLine("#61");
		//		//Apply result of just-completed load
		//		//	ImageSource icon = IconLoadCurrentData.Icon;
		//		ImageSource icon = (ImageSource)e.Result;
		//		FileGridItem fileGridItem = (FileGridItem)FileGrid.Children[iconLoadIndex];
		//		//-//	fileGridItem.FileIcon = icon;
		//		lock (icon) {
		//			fileGridItem.FileIcon = ExtractIcon.defaultExeIcon;
		//			Console.WriteLine("#65: " + icon.ToString() + ", " + icon.Width + ", " + icon.Height + ", ");
		//	
		//			using (var fileStream = new FileStream(@"C:\Users\Alex\Desktop\IconsTest\1-" + iconLoadIndex + ".png", FileMode.Create))
		//			{
		//				BitmapEncoder encoder = new PngBitmapEncoder();
		//				encoder.Frames.Add(BitmapFrame.Create((BitmapSource)icon, (BitmapSource)icon));
		//				encoder.Save(fileStream);
		//			}
		//		}
		//	
		//		Console.WriteLine("#62: " + FileGrid.Children.Count);
		//		//Find next valid FileGridItem
		//		int count = FileGrid.Children.Count;
		//		iconLoadIndex++;
		//		while (iconLoadIndex < count && !(FileGrid.Children[iconLoadIndex] is FileGridItem)) {
		//			iconLoadIndex++;
		//		}
		//	
		//		Console.WriteLine("#63: " + iconLoadIndex);
		//		//Check if at end
		//		if (iconLoadIndex >= count) return;
		//	
		//		Console.WriteLine("#64");
		//		//Start next load
		//		string filePath = (FileGrid.Children[iconLoadIndex] as FileGridItem).FilePath;
		//		//	IconLoadCurrentData.FilePath = filePath;
		//		//	IconLoadWorker.RunWorkerAsync();
		//		IconLoadWorker.RunWorkerAsync(filePath);
		//	}

		public void TryClose()
		{
			if (!BPin_HighlightManager.Selected)
			{
				this.Hide();
				SaveImages();

				//	string timingMessage = "";
				//	foreach (string timing in Timings)
				//	{
				//		timingMessage += timing + "\r\n";
				//	}
				//	//-//	MessageBox.Show(timingMessage);

				try
				{
					this.Close();
				} catch (InvalidOperationException) {
					this.Show();
				}
			}
		}

		private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) && e.SystemKey == Key.Space)
			{
				SystemMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
				SystemMenu.PlacementTarget = MainWindowArea;
				//	SystemMenu.HorizontalOffset = 8;
				//	SystemMenu.VerticalOffset = 8;
				SystemMenu.IsOpen = true;
			}
		}

		private void DoPin(object sender, RoutedEventArgs e)
		{
			BPin_HighlightManager.ToggleSelect();
		}

		private void DoClose(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void DoRestore(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Normal;
		}

		private void DoMaximize(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Maximized;
		}

		private void DoToggleMaximize(object sender, MouseButtonEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized) {
				this.WindowState = WindowState.Normal;
			} else {
				this.WindowState = WindowState.Maximized;
			}
		}

		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Console.WriteLine("#44: " + FileGridScrollViewer.ComputedVerticalScrollBarVisibility);
			if (this.WindowState == WindowState.Minimized)
			{
				this.Close();
			}
		}

		private class FileData
		{
			public string Name;
			public string Path;
			public ImageSource Icon;
			public FileData() { }
			public FileData(string name, string path, BitmapImage icon)
			{
				this.Name = name;
				this.Path = path;
				this.Icon = icon;
			}
		}

		//	private void SetupSystemMenu()
		//	{
		//		//	SystemMenu SysMenu = SystemMenu.FromWindow(Process.GetCurrentProcess().MainWindowHandle);
		//		SystemMenu SysMenu = SystemMenu.FromWindow(this);
		//		SysMenu.DeleteMenu(6);
		//		SysMenu.DeleteMenu(5);
		//		SysMenu.DeleteMenu(4);
		//		SysMenu.DeleteMenu(3);
		//	}

		private void SetStartSize()
		{
			//Get file count (excluding hidden and system files
			int fileCount = new DirectoryInfo(FolderPath).EnumerateFileSystemInfos().Where(file => {
				return !file.Attributes.HasFlag(FileAttributes.System) && !file.Attributes.HasFlag(FileAttributes.Hidden);
			}).Count();

			//Get index of size for the number of files
			int i = fileCount;
			int highestSize = sizes.Best(kvp => kvp.Key).Key;
			if (i > highestSize) {
				i = highestSize;
			} else {
				while (!sizes.ContainsKey(i) && i >= 0) {
					i--;
				}
			}

			//Get size from index or get default size
			Size size;
			if (i >= 0) size = sizes[i];
			else size = new Size(7, 3);

			//Set width and height, by working out the width/height of the FileGridItems, and then adding the borders
			this.Width = (
				  //Size of items:
				  (FileGridItem.StaticWidth * size.Width)

				//Left outer stuff
				+ FileGridBorder.BorderThickness.Left
				+ FileGridBorder.Margin.Left
				+ FileGridBorder.Padding.Left
				+ FileGridScrollViewer.Padding.Left
				+ FileGridScrollViewer.BorderThickness.Left
				+ FileGridScrollViewer.Margin.Left
				+ MainWindowArea.Margin.Left
				+ ShadowLeft.MaxWidth

				//Right outer stuff:
				+ FileGridBorder.BorderThickness.Right
				+ FileGridBorder.Margin.Right
				+ 8 //The value for FileGridBorder.Padding.Right when there is no scrollbar
				+ (fileCount > highestSize ? 14 : 0)
				//^ An extra 14 if there will be a scrollbar
				//^ (This is necessary because the binding has not yet been calculated)
				+ FileGridScrollViewer.Padding.Right
				+ FileGridScrollViewer.BorderThickness.Right
				+ FileGridScrollViewer.Margin.Right
				+ MainWindowArea.Margin.Right
				+ ShadowRight.MaxWidth
			);
			this.Height = (
				  //Size of items:
				  (FileGridItem.StaticHeight * size.Height)

				//Top outer stuff:
				+ FileGridBorder.BorderThickness.Top
				+ FileGridBorder.Margin.Top
				+ FileGridBorder.Padding.Top
				+ FileGridScrollViewer.Padding.Top
				+ FileGridScrollViewer.BorderThickness.Top
				+ FileGridScrollViewer.Margin.Top
				+ TitleBar.MaxHeight
				+ MainWindowArea.Margin.Top
				+ ShadowTop.MaxHeight

				//Bottom outer stuff:
				+ FileGridBorder.BorderThickness.Bottom
				+ FileGridBorder.Margin.Bottom
				+ FileGridBorder.Padding.Bottom
				+ FileGridScrollViewer.Padding.Bottom
				+ FileGridScrollViewer.BorderThickness.Bottom
				+ FileGridScrollViewer.Margin.Bottom
				+ MainWindowArea.Margin.Bottom
				+ ShadowBottom.MaxHeight
			);
		}

		private void SetStartPosition()
		{
			//Setup
			this.WindowStartupLocation = WindowStartupLocation.Manual;

			//Get coords
			System.Drawing.Point mousePos = System.Windows.Forms.Cursor.Position;
			System.Drawing.Rectangle screenBounds = System.Windows.Forms.Screen.GetWorkingArea(mousePos);

			//Calculate X
			double startX = mousePos.X - (this.Width / 2);
			if (startX + this.Width > screenBounds.Right) startX = screenBounds.Right - this.Width;
			if (startX < screenBounds.Left) startX = screenBounds.Left;

			//Calculate Y
			//double startY = mousePos.Y - ShadowTop.Height.Value - (TitleBar.Height/2);
			double startY = mousePos.Y - ShadowTop.Height.Value + TitleBar.Height;
			if (startY + this.Height > screenBounds.Bottom) startY = screenBounds.Bottom - this.Height;
			if (startY < screenBounds.Top) startY = screenBounds.Top;

			//Apply
			this.Left = startX;
			this.Top = startY;
		}

		private void SetColours()
		{
			TitleBar.Background = new SolidColorBrush(Color.Subtract(SystemParameters.WindowGlassColor, Color.FromArgb(30, 0, 0, 0)));
		}

		private List<FileData> GetFileData(string folderPath)
		{
			List<FileData> fileDataList = new List<FileData>();

			foreach (string filePath in Directory.EnumerateDirectories(folderPath))
			{
				FileInfo fileInfo = new FileInfo(filePath); //FileInfo works with directories as well as files

				if (//If the file is not a hidden or system file
					   (!fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
					&& (!fileInfo.Attributes.HasFlag(FileAttributes.System))
				) {
					//Setup
					FileData fileData = new FileData();

					//Name
					string ext = fileInfo.Extension;
					if (ext == ".lnk" || ext == ".url" || !ShowExtensions) {
						fileData.Name = Path.GetFileNameWithoutExtension(fileInfo.FullName);
					} else {
						fileData.Name = fileInfo.Name;
					}

					//Path (for opening the file later)
					fileData.Path = filePath;

					//Icon
					fileData.Icon = LoadIcon(fileInfo);

					//Add
					fileDataList.Add(fileData);
				}
			}

			foreach (string filePath in Directory.EnumerateFiles(folderPath))
			{
				FileInfo fileInfo = new FileInfo(filePath); //FileInfo works with directories as well as files

				if (//If the file is not a hidden or system file
					   (!fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
					&& (!fileInfo.Attributes.HasFlag(FileAttributes.System))
				) {

					//Setup
					FileData fileData = new FileData();

					//Name
					string ext = fileInfo.Extension;
					if (ext == ".lnk" || ext == ".url" || !ShowExtensions) {
						fileData.Name = Path.GetFileNameWithoutExtension(fileInfo.FullName);
					} else {
						fileData.Name = fileInfo.Name;
					}

					//Path (for opening the file later)
					fileData.Path = filePath;
					//	//	fileData.Icon = ExtractIcon.ExtractAssociatedIcon(fileInfo.FullName);
					//	if (fileInfo.Extension == ".lnk") {
					//		//Get shortcut target
					//		IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
					//		IWshRuntimeLibrary.IWshShortcut link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(filePath);
					//		string shortcutTarget = link.TargetPath;
					//		
					//		//Set icon depending on extension
					//		if (Path.GetExtension(shortcutTarget) == ".exe") fileData.Icon = ExtractIcon.defaultExeIcon;
					//		else fileData.Icon = ExtractIcon.defaultFileIcon;
					//	} else {
					//		//Set icon depending on extension
					//		if (fileInfo.Extension == ".exe") fileData.Icon = ExtractIcon.defaultExeIcon;
					//		else fileData.Icon = ExtractIcon.defaultFileIcon;
					//	}
					fileData.Icon = LoadIcon(fileInfo);

					//	using (var fileStream = new FileStream(@"C:\Users\Alex\Desktop\IconsTest\1-" + fileInfo.Name + ".png", FileMode.Create))
					//	{
					//		BitmapEncoder encoder = new PngBitmapEncoder();
					//		encoder.Frames.Add(BitmapFrame.Create((BitmapSource)fileData.Icon, (BitmapSource)fileData.Icon));
					//		encoder.Save(fileStream);
					//	}
					//	//Console.WriteLine("Icon size: " + fileData.Icon.Width + ", " + fileData.Icon.Height);

					//Add
					fileDataList.Add(fileData);
				}
			}

			return fileDataList;
		}

		private ImageSource LoadIcon(FileInfo fileInfo)
		{
			Directory.CreateDirectory(RootIconCacheDir); //Only creates if it doesn't already exist
			Directory.CreateDirectory(LocalIconCacheDir); //Only creates if it doesn't already exist
			string iconPath = Path.Combine(LocalIconCacheDir, fileInfo.Name + ".png");
			if (File.Exists(iconPath))
			{
				try
				{
					//Load and return icon
					return LoadImage(iconPath);
				} catch (Exception e) {
					Console.WriteLine("#50: " + e);
					//Extract and return icon (will be saved to disk later)
					ImageSource icon = ExtractIcon.ExtractAssociatedIcon(fileInfo.FullName);
					return icon;
				}
			}
			else
			{
				//Extract and return icon (will be saved to disk later) (same as above)
				ImageSource icon = ExtractIcon.ExtractAssociatedIcon(fileInfo.FullName);
				return icon;
			}
		}

		private void PopulateGrid(List<FileData> fileDataList)
		{
			//	//Get the files somehow (to produce something like below)
			//	List<FileData> fileData = new List<FileData>(new FileData[]
			//	{
			//		new FileData("My File"                      , "txt", new BitmapImage(new Uri("pack://application:,,,/DummyFileIcon.png", UriKind.Absolute))),
			//		new FileData("My Long Named File"           , "txt", new BitmapImage(new Uri("pack://application:,,,/DummyFileIcon.png", UriKind.Absolute))),
			//		new FileData("My File With A Very Long Name", "txt", new BitmapImage(new Uri("pack://application:,,,/DummyFileIcon.png", UriKind.Absolute))),
			//	});
			//	
			//	//For testing, to make it look nicer
			//	Random rng = new Random();
			//	fileData = fileData.OrderBy(a => rng.Next()).ToList();

			foreach (FileData fileData in fileDataList)
			{
				//Setup
				FileGridItem fileGridItem = new FileGridItem(FileGridItemList, this);
				FileGridItemList.Add(fileGridItem);

				//Setup layout
				fileGridItem.Height = 109;
				fileGridItem.Width = 80;

				//Set contents
				fileGridItem.FileCaption = fileData.Name;
				fileGridItem.FileIcon = fileData.Icon;
				fileGridItem.FilePath = fileData.Path;
				Console.WriteLine("#1: '" + fileData.Icon?.ToString() + "'");

				//Add to grid
				FileGrid.Children.Add(fileGridItem);
			}
		}

		private void SaveImages()
		{
			Directory.CreateDirectory(RootIconCacheDir); //Only creates if it doesn't already exist
			Directory.CreateDirectory(LocalIconCacheDir); //Only creates if it doesn't already exist

			foreach (FileGridItem file in FileGridItemList)
			{
				string iconPath = Path.Combine(LocalIconCacheDir, Path.GetFileName(file.FilePath) + ".png");
				if (!File.Exists(iconPath))
				{
					try
					{
						//Save icon to disk
						SaveImage(file.Icon, iconPath);
					} catch (ArgumentOutOfRangeException) {
						//Give up; load every time
						Console.WriteLine("#51");
					}
				}
			}
		}

		private void BPin_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Space:
					e.Handled = true;
					BPin_HighlightManager.ToggleSelect();
					break;
				case Key.Enter:
					e.Handled = true;
					BPin_HighlightManager.ToggleSelect();
					break;
				default:
					break;
			}
		}

		private void DoViewInExplorer(object sender, RoutedEventArgs e)
		{
			if (Directory.Exists(FolderPath))
			{
				using (Process explorerProc = new Process())
				{
					explorerProc.StartInfo.FileName = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "explorer.exe");
					explorerProc.StartInfo.UseShellExecute = true;
					explorerProc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
					explorerProc.StartInfo.Arguments = "\"" + FolderPath.Trim('"', ' ') + "\"";
					explorerProc.Start();
				}
				this.TryClose();
			}
			else
			{
				MessageBox.Show(
					owner: this,
					messageBoxText: "The displayed folder does not exist or is a file",
					caption: "Cannot view in explorer",
					button: MessageBoxButton.OK,
					icon: MessageBoxImage.Error
				);
			}
		}
	}
}
