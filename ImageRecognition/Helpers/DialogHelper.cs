using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;

namespace ImageRecognition.Helpers
{
	public static class DialogHelper
	{
		public static (Image<Bgr, byte> Image, string Path) GetImageFromUser()
		{
			var dialog = new OpenFileDialog
			{
				Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
			};

			return dialog.ShowDialog() == true
				? (Image: new Image<Bgr, byte>(new Bitmap(dialog.FileName)), Path: dialog.FileName)
				: (Image: null, Path: string.Empty);
		}
	}
}
