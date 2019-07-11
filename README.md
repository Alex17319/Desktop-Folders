# Desktop Folders
My year 10 student inquiry project - simple on-desktop folders for Windows, similar to how phones have folders of apps in addition to proper filesystem folders

To build, use Visual Studio 2017

To use, create a shortcut on the desktop. The destination of this shortcut should be the path to the executable, followed by the path to a folder containing the files and folders that will be shown in the desktop-folder. Both of these paths should be surrounded in double quotes. For example:  
"C:\Users\.......\DesktopFolders\bin\Release\DesktopFolders.exe" "C:\Users\.........\My Desktop Folder"  
You can then optionally select an icon for the desktop-folder, through the shortcut's properties (Right Click > Properties > Shortcut > Change Icon). The executable should contain a number of useful icons, if you browse to it's path.