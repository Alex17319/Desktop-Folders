using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
using TsudaKageyu; //IconExtractor

namespace DesktopFolders
{
	internal static class General
	{
		public static uint IndexOfFirstTruth(params bool[] conditions)
		{
			for (uint i = 0; i < conditions.Length; i++)
			{
				if (conditions[i]) return i + 1;
			}
			return 0;
		}
		
		public static void MultiAssign<T1, T2>(out T1 var1, out T2 var2, Tuple<T1, T2> values)
		{
			var1 = values.Item1;
			var2 = values.Item2;
		}
		public static void MultiAssign<T1, T2, T3>(out T1 var1, out T2 var2, out T3 var3, Tuple<T1, T2, T3> values)
		{
			var1 = values.Item1;
			var2 = values.Item2;
			var3 = values.Item3;
		}
		public static void MultiAssign<T1, T2, T3, T4>(out T1 var1, out T2 var2, out T3 var3, out T4 var4, Tuple<T1, T2, T3, T4> values)
		{
			var1 = values.Item1;
			var2 = values.Item2;
			var3 = values.Item3;
			var4 = values.Item4;
		}
		public static void MultiAssign<T1, T2, T3, T4, T5>(Tuple<T1, T2, T3, T4, T5> values, out T1 var1, out T2 var2, out T3 var3, out T4 var4, out T5 var5)
		{
			var1 = values.Item1;
			var2 = values.Item2;
			var3 = values.Item3;
			var4 = values.Item4;
			var5 = values.Item5;
		}

		/// <summary>
		/// Returns the item in an IEnumerable&lt;out T&gt; that ranks highest according to a ranker function.
		/// A parameter specifies whether to return the first or last equally ranked item.
		/// </summary>
		/// <typeparam name="T">The type of elements for the IEnumerable&lt;out T&gt;.</typeparam>
		/// <param name="source">The IEnumberable&lt;out T&gt; to return the 'best' element of.</param>
		/// <param name="ranker">A function to rank items, and so determine which one is best. Items ranked as double.NaN or double.NegativeInfinity are ignored.</param>
		/// <returns>The items that ranks highest according to the ranker function.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static T Best<T>(this IEnumerable<T> source, Func<T, double> ranker, bool getLastOfEquallyRanked = false)
		{
			if (source == null || ranker == null)
				throw new ArgumentNullException("One of the arguments is null (source = '" + source + "', ranker = '" + ranker + "')");

			double bestRank = double.NegativeInfinity;
			T bestItem = default(T); //default(T) is never returned; this just keeps the compiler happy
			bool foundItem = false;
			if (getLastOfEquallyRanked)
			{
				foreach (T item in source)
				{
					double rank = ranker(item);
					if (rank >= bestRank) { //Difference between two versions is '>' vs '>='
						foundItem = true;
						bestItem = item;
						bestRank = rank;
					}
				}
			} else
			{
				foreach (T item in source)
				{
					double rank = ranker(item);
					if (rank > bestRank) { //Difference between two versions is '>' vs '>='
						foundItem = true;
						bestItem = item;
						bestRank = rank;
					}
				}
			}
			if (!foundItem) {
				throw new InvalidOperationException("No items ranked with values other than double.NaN or double.NegativeInfinity");
			}
			return bestItem;
		}


		/// <summary>
		/// Clears a Dictionary, adds the items defined by the specified KeyValuePairs, and returns the dictionary. Useful in field/property/variable declarations.
		/// </summary>
		/// <typeparam name="TKey">The type of the Key for the Dictionary&lt;TKey, TValue&gt;</typeparam>
		/// <typeparam name="TValue">The type of the Value for the Dictionary&lt;TKey, TValue&gt;</typeparam>
		/// <param name="dict">The Dictionary&lt;TKey, TValue&gt; to clear and add items to</param>
		/// <param name="entries">The KeyValuePairs that define the items to add to the Dictionary</param>
		/// <returns>The dictionary the items were added to, allowing it to be used in declarations</returns>
		public static Dictionary<TKey, TValue> Init<TKey, TValue>(this Dictionary<TKey, TValue> dict, params KeyValuePair<TKey, TValue>[] entries)
		{
			if (dict.Count != 0) dict.Clear();
			foreach (KeyValuePair<TKey, TValue> kvp in entries)
			{
				dict.Add(kvp.Key, kvp.Value);
			}
			return dict;
		}

		public static TRes Try<TRes, TException>(Func<TRes> method, TRes def) where TException : Exception
		{
			if (method == null) return def;
			try {
				return method();
			} catch (TException) {
				return def;
			}
		}
		public static TRes Try<TRes>(Func<TRes> method, TRes def)
		{
			if (method == null) return def;
			try {
				return method();
			} catch (Exception) {
				return def;
			}
		}

		public static string RegexReplace(this string input, string pattern, string replacement)
		{
			return Regex.Replace(input, pattern, replacement);
		}

		public static ImageSource LoadImage(string path) {
			var ms = new MemoryStream(File.ReadAllBytes(path)); // Don't use using!!
			return new BitmapImage(new Uri(path));
			//This is weird, but apparently necessary to avoid a lock
		}

		public static void SaveImage(Image image, string filePath) {
			//	//Setup
			//	RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)image.Width, (int)image.Height, 96, 96, PixelFormats.Pbgra32);
			//	Image visualImage = new Image();
			//	visualImage.Source = image;
			//	BitmapEncoder encoder = new PngBitmapEncoder();
			//	
			//	//Convert ImageSource (image) to BitmapSource (renderBitmap) through rendering
			//	renderBitmap.Render(visualImage);
			//	
			//	//Add to encoder
			//	encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
			//	
			//	//Encode and save to disk
			//	using (var fileStream = new FileStream(filePath, FileMode.Create)) {
			//		encoder.Save(fileStream);
			//	}

			//Setup
			RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)image.ActualWidth, (int)image.ActualHeight, 96, 96, PixelFormats.Pbgra32);
			BitmapEncoder encoder = new PngBitmapEncoder();
			
			//Convert ImageSource (image) to BitmapSource (renderBitmap) through rendering
			renderBitmap.Render(image);
			
			//Add to encoder
			encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
			
			//Encode and save to disk
			using (var fileStream = new FileStream(filePath, FileMode.Create)) {
				encoder.Save(fileStream);
			}
		}

		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
		{
			IntPtr hBitmap = bitmap.GetHbitmap();
			BitmapSource retval;

			try
			{
				retval = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					hBitmap,
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions()
				);
			}
			finally
			{
				DeleteObject(hBitmap);
			}

			return retval;
		}

		public static BitmapSource IconToBitmapSource(System.Drawing.Icon icon)
		{
			return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
				icon.Handle,
				new Int32Rect(0, 0, icon.Width, icon.Height),
				BitmapSizeOptions.FromEmptyOptions()
			);
		}

		///<exception cref="Exception"></exception>
		public static BitmapSource ExtractIconFromEXEDLL(string exeordllPath, int iconIndex, int width, int height)
		{
			System.Drawing.Icon fullIcon = new IconExtractor(exeordllPath).GetIcon(iconIndex); //Throws if invalid path or index
			System.Drawing.Icon[] splitIcon = IconUtil.Split(fullIcon);
			System.Drawing.Icon selectedIcon;
			if (splitIcon.Count() == 0) throw new InvalidOperationException("The icon contains no sizes");
			try {
				selectedIcon = (
					splitIcon
					.Last((icon) => {
						return icon.Width == width && icon.Height == height;
					}) //Throws if no icons match
				);
			} catch (InvalidOperationException) {
				IEnumerable<System.Drawing.Icon> largerIcons = (
					splitIcon
					.Where((icon) => {
						return icon.Width >= width && icon.Height >= height;
					})
				);
				IEnumerable<System.Drawing.Icon> selectedIcons1;
				if (largerIcons.Count() == 0) {
					selectedIcons1 = splitIcon;
				} else {
					selectedIcons1 = largerIcons;
				}

				IEnumerable<System.Drawing.Icon> inProportionIcons = (
					selectedIcons1.Where((icon) => {
						return icon.Width / icon.Height == width / height;
					})
				);

				if (inProportionIcons.Count() == 0) {
					selectedIcon = inProportionIcons.Best((icon) => {
						return Math.Abs((icon.Width - width)) * -1;
					}, true);
				} else {
					selectedIcon = selectedIcons1.Best((icon) => {
						return Math.Abs(((icon.Width * icon.Height) - (width * height))) * -1;
					}, true);
				}
			}
			return BitmapToBitmapSource(IconUtil.ToBitmap(selectedIcon)); //Might throw exceptions
		}

		///<exception cref="Exception"></exception>
		public static Tuple<string, int> SplitIconLocationString(string iconLocationStr)
		{
			string iconPath = Environment.ExpandEnvironmentVariables(iconLocationStr.Substring(0, iconLocationStr.LastIndexOf(',')));
			int iconIndex = int.Parse(iconLocationStr.Substring(iconLocationStr.LastIndexOf(',') + 1), System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture);
			return new Tuple<string, int>(iconPath, iconIndex);
		}

		//	public static BitmapSource IconToBitmapSource_COMPLEX_SLOW_UNRELIABLE(System.Drawing.Icon icon, int idealWidth, int idealHeight)
		//	{
		//		Func<System.Drawing.Icon, int, int, BitmapSource> TryConvertIconToBitmapSourceInternal = 
		//			(System.Drawing.Icon icon2, int width, int height) =>
		//		{
		//			//	using (MemoryStream mem = new MemoryStream())
		//			//	{
		//			//		icon.Save(mem);
		//			//		mem.Position = 0;
		//			using (System.Drawing.Icon resized = new System.Drawing.Icon(icon, width, height))
		//			{
		//				return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
		//					resized.Handle,
		//					new Int32Rect(0, 0, resized.Width, resized.Height), //new Int32Rect(0, 0, width, height),
		//					BitmapSizeOptions.FromEmptyOptions()
		//				);
		//			}
		//			//	}
		//		};
		//	
		//		//Loop upward through a few multiples of preferred size
		//		for (int i = 0; i < 4; i++)
		//		{
		//			try {
		//				BitmapSource bitmapIcon = TryConvertIconToBitmapSourceInternal(icon, idealWidth * (i + 1), idealHeight * (i + 1));
		//				if (bitmapIcon != null) return bitmapIcon;
		//			} catch (Exception) { /*Console.WriteLine("#4: " + e.ToString());*/ }
		//		}
		//	
		//		//Try with icon's size
		//		try {
		//			BitmapSource bitmapIcon = TryConvertIconToBitmapSourceInternal(icon, icon.Width, icon.Height);
		//			if (bitmapIcon != null) return bitmapIcon;
		//		} catch { }
		//	
		//		//Try with no size
		//		try {
		//			BitmapSource bitmapIcon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
		//				icon.Handle,
		//				Int32Rect.Empty,
		//				BitmapSizeOptions.FromEmptyOptions()
		//			);
		//			if (bitmapIcon != null) return bitmapIcon;
		//		} catch { }
		//	
		//		//Throw because everything failed
		//		throw new ArgumentException("The icon could not be converted to a bitmap source with *any* size");
		//	}
		//	//	private static readonly int[] commonIconSizesInPrefOrder = new int[] {
		//	//		1024, 512, 256, 144, 128, 96, 64, 48, 40, 32, 24, 20, 16, //Windows
		//	//		200, 192, 180, 173, 152, 120, 114, 100, 99, 87, 80, 76, 75, 72, 66, 62, 60, 58, 50, 44, 29, 25, 22, 18, //Other, don't fit very well
		//	//	};

		//	public static Icon ExtractIcon_WORKS_FOR_16_AND_32_BUT_DOESNT_WORK_WELL_FOR_OHER_SIZES(string path, bool smallIcon, bool isDirectory, SHGFI otherIconOptions = 0, FILE_ATTRIBUTE otherFileAttrs = FILE_ATTRIBUTE.NORMAL)
		//	{
		//		// SHGFI_USEFILEATTRIBUTES takes the file name and attributes into account if it doesn't exist
		//		uint flags = (uint)(
		//				SHGFI.ICON
		//			| SHGFI.USEFILEATTRIBUTES
		//			| (smallIcon ? SHGFI.SMALLICON : 0)
		//			| otherIconOptions
		//		);
		//	
		//		uint attributes = (uint)(
		//				FILE_ATTRIBUTE.NORMAL
		//			| (isDirectory ? FILE_ATTRIBUTE.DIRECTORY : 0)
		//			| otherFileAttrs
		//		);
		//	
		//		SHFILEINFO shfi;
		//		if (0 != SHGetFileInfo(
		//					path,
		//					attributes,
		//					out shfi,
		//					(uint)Marshal.SizeOf(typeof(SHFILEINFO)),
		//					flags))
		//		{
		//			//	return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
		//			//				shfi.hIcon, 
		//			//				Int32Rect.Empty,
		//			//				BitmapSizeOptions.FromEmptyOptions());
		//			return System.Drawing.Icon.FromHandle(shfi.hIcon);
		//		}
		//		return null;
		//	}
		//	
		//	[StructLayout(LayoutKind.Sequential)]
		//	private struct SHFILEINFO
		//	{
		//		public IntPtr hIcon;
		//		public int iIcon;
		//		public uint dwAttributes;
		//		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		//		public string szDisplayName;
		//		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		//		public string szTypeName;
		//	}
		//	
		//	[DllImport("shell32")]
		//	private static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint flags);
		//	
		//	public enum FILE_ATTRIBUTE : uint
		//	{
		//		READONLY            = 0x00000001,
		//		HIDDEN              = 0x00000002,
		//		SYSTEM              = 0x00000004,
		//		DIRECTORY           = 0x00000010,
		//		ARCHIVE             = 0x00000020,
		//		DEVICE              = 0x00000040,
		//		NORMAL              = 0x00000080,
		//		TEMPORARY           = 0x00000100,
		//		SPARSE_FILE         = 0x00000200,
		//		REPARSE_POINT       = 0x00000400,
		//		COMPRESSED          = 0x00000800,
		//		OFFLINE             = 0x00001000,
		//		NOT_CONTENT_INDEXED = 0x00002000,
		//		ENCRYPTED           = 0x00004000,
		//		VIRTUAL             = 0x00010000,
		//	}
		//	//	private const uint FILE_ATTRIBUTE_READONLY            = 0x00000001;
		//	//	private const uint FILE_ATTRIBUTE_HIDDEN              = 0x00000002;
		//	//	private const uint FILE_ATTRIBUTE_SYSTEM              = 0x00000004;
		//	//	private const uint FILE_ATTRIBUTE_DIRECTORY           = 0x00000010;
		//	//	private const uint FILE_ATTRIBUTE_ARCHIVE             = 0x00000020;
		//	//	private const uint FILE_ATTRIBUTE_DEVICE              = 0x00000040;
		//	//	private const uint FILE_ATTRIBUTE_NORMAL              = 0x00000080;
		//	//	private const uint FILE_ATTRIBUTE_TEMPORARY           = 0x00000100;
		//	//	private const uint FILE_ATTRIBUTE_SPARSE_FILE         = 0x00000200;
		//	//	private const uint FILE_ATTRIBUTE_REPARSE_POINT       = 0x00000400;
		//	//	private const uint FILE_ATTRIBUTE_COMPRESSED          = 0x00000800;
		//	//	private const uint FILE_ATTRIBUTE_OFFLINE             = 0x00001000;
		//	//	private const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
		//	//	private const uint FILE_ATTRIBUTE_ENCRYPTED           = 0x00004000;
		//	//	private const uint FILE_ATTRIBUTE_VIRTUAL             = 0x00010000;
		//	
		//	public enum SHGFI : uint
		//	{
		//		/**<summary> get icon                      </summary>*/ ICON              = 0x000000100,
		//		/**<summary> get display name              </summary>*/ DISPLAYNAME       = 0x000000200,
		//		/**<summary> get type name                 </summary>*/ TYPENAME          = 0x000000400,
		//		/**<summary> get attributes                </summary>*/ ATTRIBUTES        = 0x000000800,
		//		/**<summary> get icon location             </summary>*/ ICONLOCATION      = 0x000001000,
		//		/**<summary> return exe type               </summary>*/ EXETYPE           = 0x000002000,
		//		/**<summary> get system icon index         </summary>*/ SYSICONINDEX      = 0x000004000,
		//		/**<summary> put a link overlay on icon    </summary>*/ LINKOVERLAY       = 0x000008000,
		//		/**<summary> show icon in selected state   </summary>*/ SELECTED          = 0x000010000,
		//		/**<summary> get only specified attributes </summary>*/ ATTR_SPECIFIED    = 0x000020000,
		//		/**<summary> get large icon                </summary>*/ LARGEICON         = 0x000000000,
		//		/**<summary> get small icon                </summary>*/ SMALLICON         = 0x000000001,
		//		/**<summary> get open icon                 </summary>*/ OPENICON          = 0x000000002,
		//		/**<summary> get shell size icon           </summary>*/ SHELLICONSIZE     = 0x000000004,
		//		/**<summary> pszPath is a pidl             </summary>*/ PIDL              = 0x000000008,
		//		/**<summary> use passed dwFileAttribute    </summary>*/ USEFILEATTRIBUTES = 0x000000010,
		//	}
		//	//	private const uint SHGFI_ICON              = 0x000000100; // get icon
		//	//	private const uint SHGFI_DISPLAYNAME       = 0x000000200; // get display name
		//	//	private const uint SHGFI_TYPENAME          = 0x000000400; // get type name
		//	//	private const uint SHGFI_ATTRIBUTES        = 0x000000800; // get attributes
		//	//	private const uint SHGFI_ICONLOCATION      = 0x000001000; // get icon location
		//	//	private const uint SHGFI_EXETYPE           = 0x000002000; // return exe type
		//	//	private const uint SHGFI_SYSICONINDEX      = 0x000004000; // get system icon index
		//	//	private const uint SHGFI_LINKOVERLAY       = 0x000008000; // put a link overlay on icon
		//	//	private const uint SHGFI_SELECTED          = 0x000010000; // show icon in selected state
		//	//	private const uint SHGFI_ATTR_SPECIFIED    = 0x000020000; // get only specified attributes
		//	//	private const uint SHGFI_LARGEICON         = 0x000000000; // get large icon
		//	//	private const uint SHGFI_SMALLICON         = 0x000000001; // get small icon
		//	//	private const uint SHGFI_OPENICON          = 0x000000002; // get open icon
		//	//	private const uint SHGFI_SHELLICONSIZE     = 0x000000004; // get shell size icon
		//	//	private const uint SHGFI_PIDL              = 0x000000008; // pszPath is a pidl
		//	//	private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010; // use passed dwFileAttribute
	}
}
