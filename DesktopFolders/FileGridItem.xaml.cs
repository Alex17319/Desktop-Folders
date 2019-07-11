using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopFolders
{
	/// <summary>
	/// Interaction logic for FileGridItem.xaml
	/// </summary>
	internal partial class FileGridItem : UserControl
	{
		//	public bool Hovered { get; private set; } = false;
		//	public bool Selected { get; private set; } = false;
		//	public readonly Brush HoveredBackgroundBrush         = new SolidColorBrush(Color.FromArgb(50, 160, 220, 255));
		//	public readonly Brush HoveredBorderBrush             = new SolidColorBrush(Color.FromArgb(60, 220, 240, 255));
		//	public readonly Brush SelectedBackgroundBrush        = new SolidColorBrush(Color.FromArgb(80, 160, 220, 255));
		//	public readonly Brush SelectedBorderBrush            = new SolidColorBrush(Color.FromArgb(90, 220, 240, 255));
		//	public readonly Brush SelectedHoveredBackgroundBrush = new SolidColorBrush(Color.FromArgb(80, 120, 180, 255));
		//	public readonly Brush SelectedHoveredBorderBrush     = new SolidColorBrush(Color.FromArgb(90, 220, 240, 255));
		public readonly ControlHighlightManager highlightManager;
		
		private ICollection<FileGridItem> ContainingCollection;

		//I don't need data-binding for this
		public ImageSource FileIcon {
			get { return Icon.Source; }
			set { Icon.Source = value; }
		}

		//I don't need data-binding for this
		public string FileCaption
		{
			get { return Caption.Text; }
			set { Caption.Text = value; }
		}

		public string FilePath;

		public MainWindow MainWindow;

		public const int StaticHeight = 109;
		public const int StaticWidth = 76;

		public FileGridItem(ICollection<FileGridItem> containingCollection, MainWindow mainWindow)
		{
			InitializeComponent();

			this.ContainingCollection = containingCollection;
			this.MainWindow = mainWindow;

			highlightManager = new ControlHighlightManager(
				this.MainButton,
				this.MainBorder,
				null, //Will be ignored
				new ControlHighlightManager.BrushGroup(
					new ControlHighlightManager.HighlightStateBrushes(
						new SolidColorBrush(Color.FromArgb(50, 160, 220, 255)),
						new SolidColorBrush(Color.FromArgb(60, 220, 240, 255)),
						null //Doesn't matter
					),
					new ControlHighlightManager.HighlightStateBrushes(
						new SolidColorBrush(Color.FromArgb(80, 160, 220, 255)),
						new SolidColorBrush(Color.FromArgb(90, 220, 240, 255)),
						null
					),
					new ControlHighlightManager.HighlightStateBrushes(
						new SolidColorBrush(Color.FromArgb(80, 120, 180, 255)),
						new SolidColorBrush(Color.FromArgb(90, 220, 240, 255)),
						null
					),
					null //Transparent, transparent, doesn't matter
				)
			);

			//if (IsMouseOver) highlightManager.Highlight();

			MainButton.MouseEnter       += (s, e) => { highlightManager.Highlight(); };
			MainButton.MouseLeave       += (s, e) => { highlightManager.Unhighlight(); };
			//MainButton.Click            += (s, e) => { DeselectAll(); highlightManager.Select(); };
			MainButton.MouseDoubleClick += (s, e) => { LaunchFile(); };
			OuterButton.Click           += (s, e) => { if (e.OriginalSource == OuterButton) DeselectAll(); };
			MainButton.GotFocus         += (s, e) => { DeselectAll(); highlightManager.Select(); };
			MainButton.LostFocus        += (s, e) => { highlightManager.Deselect(); };
			MainButton.PreviewKeyDown   += MainButton_PreviewKeyDown;
			
			DeselectAll();
		}

		//	private void SetHover(object sender, EventArgs e)
		//	{
		//		Hovered = true;
		//		if (Selected) SetSelectedHoveredColours();
		//		else SetHoveredColours();
		//	}
		//	private void ClearHover(object sender, EventArgs e)
		//	{
		//		Hovered = false;
		//		if (Selected) SetSelectedColours();
		//		else SetNoColours();
		//	}
		//	
		//	public void Select()
		//	{
		//		//if (Selected) return;
		//		DeselectAll();
		//		Selected = true;
		//		if (Hovered) SetSelectedHoveredColours();
		//		else SetSelectedColours();
		//	}
		//	public void Deselect()
		//	{
		//		//if (!Selected) return;
		//		Selected = false;
		//		if (Hovered) SetHoveredColours();
		//		else SetNoColours();
		//	}
		//	
		//	private void SetNoColours() {
		//		MainButton.Background = Brushes.Transparent;
		//		MainBorder.BorderBrush = Brushes.Transparent;
		//	}
		//	private void SetHoveredColours() {
		//		MainButton.Background = HoveredBackgroundBrush;
		//		MainBorder.BorderBrush = HoveredBorderBrush;
		//	}
		//	private void SetSelectedColours() {
		//		MainButton.Background = SelectedBackgroundBrush;
		//		MainBorder.BorderBrush = SelectedBorderBrush;
		//	}
		//	private void SetSelectedHoveredColours() {
		//		MainButton.Background = SelectedHoveredBackgroundBrush;
		//		MainBorder.BorderBrush = SelectedHoveredBorderBrush;
		//	}

		private void MainButton_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Space:
					e.Handled = true;
					break;
				case Key.Enter:
					e.Handled = true;
					LaunchFile();
					break;
				default:
					break;
			}
		}
		
		private void DeselectAll()
		{
			foreach (FileGridItem fileGridItem in ContainingCollection)
			{
				fileGridItem.highlightManager.Deselect();
			}
		}

		private void LaunchFile()
		{
			if (File.Exists(FilePath))
			{
				using (Process proc = new Process()) {
					proc.StartInfo.FileName = FilePath;
					proc.StartInfo.UseShellExecute = true;
					proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
					proc.Start();
				}
				MainWindow.TryClose();
			}
			else if (Directory.Exists(FilePath))
			{
				using (Process explorerProc = new Process()) {
					explorerProc.StartInfo.FileName = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "explorer.exe");
					explorerProc.StartInfo.UseShellExecute = true;
					explorerProc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
					explorerProc.StartInfo.Arguments = "\"" + FilePath.Trim('"', ' ') + "\"";
					explorerProc.Start();
				}
				MainWindow.TryClose();
			}
			else
			{
				MessageBox.Show(
					MainWindow,
					"The file or folder does not exist",
					"Cannot open the file",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			}
		}
	}


	[Serializable]
	public class TypeMismatchException : Exception
	{
		public TypeMismatchException() { }
		public TypeMismatchException(string message) : base(message) { }
		public TypeMismatchException(string message, Exception inner) : base(message, inner) { }
		protected TypeMismatchException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
