using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace DesktopFolders
{

	//Source: http://stackoverflow.com/a/35422860/4149474 (modified slightly)
	internal static class AlwaysAboveDesktop
	{
		private const uint WINEVENT_OUTOFCONTEXT = 0u;
		private const uint EVENT_SYSTEM_FOREGROUND = 3u;

		private const string WORKERW       = "WorkerW";
		private const string PROGMAN       = "Progman";
		private const string SHELL_TRAYWND = "Shell_TrayWnd";

		private static class NativeMethods
		{
			[DllImport("user32.dll")]
			internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, AlwaysAboveDesktop.WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

			[DllImport("user32.dll")]
			internal static extern bool UnhookWinEvent(IntPtr hWinEventHook);

			[DllImport("user32.dll")]
			internal static extern int GetClassName(IntPtr hwnd, StringBuilder name, int count);

			[DllImport("user32.dll", CharSet=CharSet.Auto)]
			public static extern int GetWindowTextLength(IntPtr hWnd);

			[DllImport("user32.dll", CharSet=CharSet.Auto)]
			public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
		}

		public static void AddHook(Process process, Window mainProcessWindow)
		{
			if (IsHooked)
			{
				return;
			}

			IsHooked = true;
			
			_delegate = new WinEventDelegate(WinEventHook);
			_hookIntPtr = NativeMethods.SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _delegate, 0, 0, WINEVENT_OUTOFCONTEXT);
			_window = mainProcessWindow;
			_windowClass = GetWindowClass(process.MainWindowHandle);
		}

		public static void RemoveHook()
		{
			if (!IsHooked)
			{
				return;
			}

			IsHooked = false;

			NativeMethods.UnhookWinEvent(_hookIntPtr.Value);

			_delegate = null;
			_hookIntPtr = null;
			_window = null;
		}

		private static string GetWindowClass(IntPtr hwnd)
		{
			StringBuilder _sb = new StringBuilder(32);
			NativeMethods.GetClassName(hwnd, _sb, _sb.Capacity);
			return _sb.ToString();
		}

		private static string GetWindowTitle(IntPtr hwnd)
		{
			int capacity = NativeMethods.GetWindowTextLength(hwnd) * 2;
            StringBuilder stringBuilder = new StringBuilder(capacity);
			NativeMethods.GetWindowText(hwnd, stringBuilder, stringBuilder.Capacity);
			return stringBuilder.ToString();
		}

		internal delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

		private static void WinEventHook(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			if (eventType == EVENT_SYSTEM_FOREGROUND)
			{
				string _class = GetWindowClass(hwnd);

				if (
					   string.Equals(_class, WORKERW      , StringComparison.Ordinal)
					|| string.Equals(_class, PROGMAN      , StringComparison.Ordinal)
					|| string.Equals(_class, SHELL_TRAYWND, StringComparison.Ordinal)
				) {
					_window.Topmost = true;
				}
				//else if (string.Equals(_class, _windowClass, StringComparison.OrdinalIgnoreCase))
				else if (string.Equals(GetWindowTitle(hwnd), _window.Title, StringComparison.Ordinal))
				{
					Console.WriteLine("###");
					//Don't disable topmost
				}
				else
				{
					Console.WriteLine("[[[" + _class + "|" + _windowClass + "]]]");
					_window.Topmost = false;
				}
			}
		}

		public static bool IsHooked { get; private set; } = false;

		private static IntPtr? _hookIntPtr { get; set; }

		private static WinEventDelegate _delegate { get; set; }

		private static Window _window { get; set; }

		private static string _windowClass { get; set; }
	}
}
