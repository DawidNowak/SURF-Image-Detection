using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace ImageRecognition.Helpers
{
	public static class ImageHelper
	{
		[DllImport("gdi32")]
		private static extern int DeleteObject(IntPtr o);

		public static BitmapSource ToBitmapSource(this IImage image)
		{
			using (Bitmap source = image.Bitmap)
			{
				IntPtr ptr = source.GetHbitmap();

				BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					ptr,
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());

				DeleteObject(ptr);
				return bs;
			}
		}

		public static Bitmap ToBitmap(this byte[] bytes)
		{
			using (var ms = new MemoryStream(bytes))
			{
				return new Bitmap(ms);
			}
		}
	}
}
