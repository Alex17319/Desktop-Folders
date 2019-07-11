using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

namespace DesktopFolders
{
	internal class ControlHighlightManager
	{
		public Control BackgroundControl;
		public Tuple<Control, Border, bool> BorderControl; //Tuple<Control potentialControl, Border potentialBorder, bool ControlIsUsed>
		public Control ForegroundControl;

		public BrushGroup Brushes;

		public bool Highlighted { get; private set; }
		public bool Selected { get; private set; }

		public ControlHighlightManager(Control backgroundControl, Control borderControl, Control foregroundControl, BrushGroup brushes) {
			this.BackgroundControl = backgroundControl;
			this.ForegroundControl = foregroundControl;
			this.Brushes = brushes;
			this.BorderControl = new Tuple<Control, Border, bool>(borderControl, null, true);
		}

		public ControlHighlightManager(Control backgroundControl, Border borderControl, Control foregroundControl, BrushGroup brushes) {
			this.BackgroundControl = backgroundControl;
			this.ForegroundControl = foregroundControl;
			this.Brushes = brushes;
			this.BorderControl = new Tuple<Control, Border, bool>(null, borderControl, false);
		}

		public void Highlight()
		{
			if (Highlighted) return;
			Highlighted = true;
			if (Selected) SetCombinedColours();
			else SetHighlighedColours();
		}
		public void Unhighlight()
		{
			if (!Highlighted) return;
			Highlighted = false;
			if (Selected) SetSelectedColours();
			else SetNoColours();
		}

		public void Select()
		{
			if (Selected) return;
			Selected = true;
			if (Highlighted) SetCombinedColours();
			else SetSelectedColours();
		}
		public void Deselect()
		{
			if (!Selected) return;
			Selected = false;
			if (Highlighted) SetHighlighedColours();
			else SetNoColours();
		}

		public void ToggleSelect()
		{
			if (Selected) Deselect();
			else Select();
		}

		private void SetNoColours() {
			if (BackgroundControl != null) BackgroundControl.Background = Brushes.DefaultBrushes.BackgroundBrush;
			if (ForegroundControl != null) ForegroundControl.Foreground = Brushes.DefaultBrushes.ForegroundBrush;
			if (BorderControl.Item3) {
				if (BorderControl.Item1 != null) BorderControl.Item1.BorderBrush = Brushes.DefaultBrushes.BorderBrush;
			} else {
				if (BorderControl.Item2 != null) BorderControl.Item2.BorderBrush = Brushes.DefaultBrushes.BorderBrush;
			}
		}
		private void SetHighlighedColours() {
			if (BackgroundControl != null) BackgroundControl.Background = Brushes.HighlightedBrushes.BackgroundBrush;
			if (ForegroundControl != null) ForegroundControl.Foreground = Brushes.HighlightedBrushes.ForegroundBrush;
			if (BorderControl.Item3) {
				if (BorderControl.Item1 != null) BorderControl.Item1.BorderBrush = Brushes.HighlightedBrushes.BorderBrush;
			} else {
				if (BorderControl.Item2 != null) BorderControl.Item2.BorderBrush = Brushes.HighlightedBrushes.BorderBrush;
			}
		}
		private void SetSelectedColours() {
			if (BackgroundControl != null) BackgroundControl.Background = Brushes.SelectedBrushes.BackgroundBrush;
			if (ForegroundControl != null) ForegroundControl.Foreground = Brushes.SelectedBrushes.ForegroundBrush;
			if (BorderControl.Item3) {
				if (BorderControl.Item1 != null) BorderControl.Item1.BorderBrush = Brushes.SelectedBrushes.BorderBrush;
			} else {
				if (BorderControl.Item2 != null) BorderControl.Item2.BorderBrush = Brushes.SelectedBrushes.BorderBrush;
			}
		}
		private void SetCombinedColours() {
			if (BackgroundControl != null) BackgroundControl.Background = Brushes.CombinedBrushes.BackgroundBrush;
			if (ForegroundControl != null) ForegroundControl.Foreground = Brushes.CombinedBrushes.ForegroundBrush;
			if (BorderControl.Item3) {
				if (BorderControl.Item1 != null) BorderControl.Item1.BorderBrush = Brushes.CombinedBrushes.BorderBrush;
			} else {
				if (BorderControl.Item2 != null) BorderControl.Item2.BorderBrush = Brushes.CombinedBrushes.BorderBrush;
			}
		}

		public class BrushGroup
		{
			public HighlightStateBrushes DefaultBrushes;
			public HighlightStateBrushes HighlightedBrushes;
			public HighlightStateBrushes SelectedBrushes;
			public HighlightStateBrushes CombinedBrushes;

			public BrushGroup(
				HighlightStateBrushes highlightedBrushes,
				HighlightStateBrushes selectedBrushes,
				HighlightStateBrushes combinedBrushes,
				HighlightStateBrushes defaultBrushes
			) {
				this.DefaultBrushes     = defaultBrushes     != null ? defaultBrushes     : new HighlightStateBrushes(null, null, null);
				this.HighlightedBrushes = highlightedBrushes != null ? highlightedBrushes : new HighlightStateBrushes(null, null, null);
				this.SelectedBrushes    = selectedBrushes    != null ? selectedBrushes    : new HighlightStateBrushes(null, null, null);
				this.CombinedBrushes    = combinedBrushes    != null ? combinedBrushes    : new HighlightStateBrushes(null, null, null);
			}	
		}

		public class HighlightStateBrushes
		{
			public Brush BackgroundBrush;
			public Brush BorderBrush;
			public Brush ForegroundBrush;

			public HighlightStateBrushes(Brush backgroundBrush, Brush borderBrush, Brush foregroundBrush) {
				this.BackgroundBrush = backgroundBrush != null ? backgroundBrush : System.Windows.Media.Brushes.Transparent;
				this.BorderBrush     = borderBrush     != null ? borderBrush     : System.Windows.Media.Brushes.Transparent;
				this.ForegroundBrush = foregroundBrush != null ? foregroundBrush : System.Windows.Media.Brushes.Black;
			}
		}
	}
}
