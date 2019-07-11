using IWshRuntimeLibrary;
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
using System.Windows.Shell;
using TsudaKageyu; //IconExtractor
using Icon = System.Drawing.Icon;
using static DesktopFolders.General;

namespace DesktopFolders
{
	internal static class ExtractIcon
	{
		private static readonly QuickZip.Tools.FileToIconConverter ftiConverter = new QuickZip.Tools.FileToIconConverter();

		private static WshShell WshShell = new WshShell();

		public static readonly BitmapSource defaultFileIcon = new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultFileIcon.png"));
		public static readonly BitmapSource defaultExeIcon  = new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultExeIcon.png"));
		public static readonly BitmapSource defaultDirIcon  = new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultDirIcon.png"));
		
		//	static ExtractIcon() {
		//		try {
		//			defaultFileIcon = ExtractIconFromEXEDLL(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\shell32.dll"), 0, 48, 48);
		//		} catch { defaultFileIcon = new BitmapImage(); }
		//		//defaultFileIcon.Freeze();
		//		try {
		//			defaultExeIcon = ExtractIconFromEXEDLL(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\imageres.dll"), 11, 48, 48);
		//		} catch { defaultExeIcon = new BitmapImage(); }
		//		//defaultExeIcon.Freeze();
		//		try {
		//			defaultDirIcon = ExtractIconFromEXEDLL(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\shell32.dll"), 3, 48, 48);
		//		} catch { defaultDirIcon = new BitmapImage(); }
		//		//defaultDirIcon.Freeze();
		//	
		//		
		//		
		//	}
		
		public static ImageSource ExtractAssociatedIcon(string filePath)
		{
			//	if (!temp)
			//	{
			//		try
			//		{
			//			Console.WriteLine("#54");
			//			BitmapEncoder encoder = new PngBitmapEncoder();
			//			encoder.Frames.Add(BitmapFrame.Create(defaultFileIcon));
			//			using (var fileStream = new FileStream(@"C:\Users\Alex\Desktop\Test2\icon1.ico", FileMode.Create)) {
			//				encoder.Save(fileStream);
			//			}
			//			encoder = new PngBitmapEncoder();
			//			encoder.Frames.Add(BitmapFrame.Create(defaultExeIcon));
			//			using (var fileStream = new FileStream(@"C:\Users\Alex\Desktop\Test2\icon2.ico", FileMode.Create)) {
			//				encoder.Save(fileStream);
			//			}
			//			encoder = new PngBitmapEncoder();
			//			encoder.Frames.Add(BitmapFrame.Create(defaultDirIcon));
			//			using (var fileStream = new FileStream(@"C:\Users\Alex\Desktop\Test2\icon3.ico", FileMode.Create)) {
			//				encoder.Save(fileStream);
			//			}
			//			temp = true;
			//			Console.WriteLine("#55");
			//		} catch (Exception e) { Console.WriteLine(e); }
			//	}
			

			if (!System.IO.File.Exists(filePath))
			{
				if (Directory.Exists(filePath))
				{
					return defaultDirIcon;
				}
				else
				{
					return defaultExeIcon;
				}
			}

			//?//	FileInfo fileInfo = new FileInfo(filePath);
			//?//	if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
			//?//	{
			//?//		return defaultDirIcon;
			//?//	}

			//Icon
			//	System.Drawing.Icon icon;
			//	//	icon = System.Drawing.Icon.ExtractAssociatedIcon(fileInfo.FullName);
			//	icon = IconMethods.ExtractIcon(fileInfo.FullName, false, false, IconMethods.SHGFI.USEFILEATTRIBUTES);
			//	//	using (FileStream fs = File.Create(@"C:\Users\Alex\Desktop\Test\8-" + fileData.Name + ".tiff")) {
			//	//		icon.ToBitmap().Save(fs, System.Drawing.Imaging.ImageFormat.Tiff);
			//	//	}
			//	//	Console.WriteLine("#2: '" + icon?.ToString() + "'");
			//	ImageSource bitmapIcon = IconMethods.IconToBitmapSource(icon, 1000, 1000);
			//	//	ImageSource bitmapIcon = IconMethods.ExtractIcon(fileInfo.FullName, false, false, IconMethods.SHGFI.USEFILEATTRIBUTES);
			//	Console.WriteLine("#3: '" + bitmapIcon);
			//	fileData.Icon = bitmapIcon;

			if (Path.GetExtension(filePath) == ".lnk") {
				IWshShortcut link = (IWshShortcut)WshShell.CreateShortcut(filePath); //Get the shortcut
				string iconLocationStr = link.IconLocation;
				string shortcutTarget = link.TargetPath;

				//	Console.WriteLine("File: " + fileInfo.Name);

				try
				{
					//	Console.WriteLine("#1");

					//	if (iconLocationStr.StartsWith(",")) { //The icon path is invalid
					//		throw new Exception("Go to catch");
					//	}

					//	string iconPathPathOnly = Environment.ExpandEnvironmentVariables(iconPath.Substring(0, iconPath.LastIndexOf(','))); //Might throw if invalid iconPath
					//	int iconPathIndex = int.Parse(iconPath.Substring(iconPath.LastIndexOf(',') + 1)); //Might throw if invalid iconPath
					//	if (string.IsNullOrEmpty(iconPathPathOnly)) throw new Exception("Go to catch")
					//	Icon fullIcon = new IconExtractor(iconPathPathOnly).GetIcon(iconPathIndex); //Throws if invalid path or index
					//	Icon selectedIcon = IconUtil.Split(fullIcon).Last((icon) => icon.Size == new System.Drawing.Size(48, 48)); //Throws if no icons
					//	fileData.Icon = IconMethods.BitmapToBitmapSource(IconUtil.ToBitmap(selectedIcon)); //Might throw exceptions
					string iconPath; int iconIndex;
					MultiAssign(out iconPath, out iconIndex, SplitIconLocationString(iconLocationStr));
					//	Console.WriteLine("|" + iconPath + "|" + iconIndex + "|");
					try
					{
						//	Console.WriteLine("#1/1");

						//Get the icon from the EXE or DLL, if the path is not empty, using IconExtractor
						if (string.IsNullOrWhiteSpace(iconPath)) throw new Exception("Go to catch");
						return ExtractIconFromEXEDLL(iconPath, iconIndex, 48, 48);
					}
					catch
					{
						//	Console.WriteLine("#1/2");

						if (Path.GetExtension(shortcutTarget) == ".exe" || Path.GetExtension(shortcutTarget) == ".dll")
						{
							//	Console.WriteLine("#1/2/1");

							try {
								//	Console.WriteLine("#1/2/1/1");
										
								//Get the icon of the shortcut's target, using IconExtractor
								return ExtractIconFromEXEDLL(shortcutTarget, iconIndex, 48, 48);
							} catch {
								//	Console.WriteLine("#1/2/1/2");

								if (Path.GetExtension(shortcutTarget) == ".exe") {
									//	Console.WriteLine("#1/2/1/2/1");
									return defaultExeIcon;
								} else {
									//	Console.WriteLine("#1/2/1/2/2");
									throw new Exception("Go to catch");
								}
							}
						}
						else
						{
							//	Console.WriteLine("#1/2/2");

							//	//Get the icon of the shortcut (not it's target), using FileToIconConverter
							//	fileData.Icon = ftiConverter.GetImage(fileInfo.FullName, QuickZip.Tools.FileToIconConverter.IconSize.extraLarge);
									
							//Get the icon of the shortcut (not it's target), using ExtractAssociatedIcon
							return IconToBitmapSource(System.Drawing.Icon.ExtractAssociatedIcon(filePath));
						}
					}
				}
				catch (Exception)
				{
					//	Console.WriteLine("#2");

					//	Console.WriteLine(e);
					//	try
					//	{
					//		//Get the icon of the shortcut's target, using IconExtractor
					//		int iconPathIndex = int.Parse(iconLocationStr.Substring(iconLocationStr.LastIndexOf(',') + 1)); //Throws if invalid iconPath
					//		Console.WriteLine("|" + iconPathIndex + "|");
					//		if (Path.GetExtension(shortcutTarget) != ".exe" && Path.GetExtension(shortcutTarget) != ".dll") throw new Exception("Go to catch");
					//		Icon fullIcon = new IconExtractor(shortcutTarget).GetIcon(iconPathIndex); //Throws if invalid path or index
					//		Icon selectedIcon = IconUtil.Split(fullIcon).Last((icon) => icon.Size == new System.Drawing.Size(48, 48)); //Throws if no icons
					//		fileData.Icon = IconMethods.BitmapToBitmapSource(IconUtil.ToBitmap(selectedIcon)); //Might throw exceptions
					//	}
					//	catch (Exception e)
					//	{
					//		Console.WriteLine(e);
					//	}


					//	try
					//	{
					//		Console.WriteLine("#2/1");
					//	
					//		//Get the icon of the shortcut (not it's target), using FileToIconConverter
					//		fileData.Icon = ftiConverter.GetImage(fileInfo.FullName, QuickZip.Tools.FileToIconConverter.IconSize.extraLarge);
					//	
					//	}
					//	catch
					//	{
					//		Console.WriteLine("#2/2");
					//	
					//		//Get the icon of the shortcut (not it's target), using ExtractAssociatedIcon
					//		fileData.Icon = IconMethods.BasicIconToBitmapSource(System.Drawing.Icon.ExtractAssociatedIcon(fileInfo.FullName));
					//	}

					//Get the icon of the shortcut (not it's target), using ExtractAssociatedIcon
					return IconToBitmapSource(System.Drawing.Icon.ExtractAssociatedIcon(filePath));
				}

				//	switch (IndexOfFirstTruth(false, false, true))
				//	{
				//		case 1: FirstCondition:
				//			//Do stuff
				//			break;
				//		case 2: SecondCondition:
				//			//Do stuff
				//			if (true) goto Else;
				//			//Do stuff
				//			break;
				//		case 3:
				//			//Do stuff
				//			if ("Actually should have done FirstCondition" != "") goto FirstCondition;
				//			//Do stuff
				//			break;
				//		default: Else:
				//			//Do stuff
				//			break;
				//	}
						
			}
			else
			{
				return ftiConverter.GetImage(filePath, QuickZip.Tools.FileToIconConverter.IconSize.extraLarge); //Get the icon of the file
			}
		}
	}
}
