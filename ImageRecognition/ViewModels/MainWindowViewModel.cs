using System.Windows.Interop;
using System.Windows.Media;
using Emgu.CV;
using Emgu.CV.Structure;
using ImageRecognition.Helpers;
using Prism.Commands;
using Prism.Mvvm;

namespace ImageRecognition.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		#region fields
		private readonly VideoCapture _camCap;
		#endregion fields

		#region construction
		public MainWindowViewModel()
		{
			_camCap = new VideoCapture(0);
			SetSceneCommand = new DelegateCommand(SetScene);
			SetPatternCommand = new DelegateCommand(SetPattern);
			CamRbtnCheckedCommand = new DelegateCommand(CamRbtnChecked);
			ImgRbtnCheckedCommand = new DelegateCommand(ImgRbtnChecked);
		}
		#endregion construction

		#region commands
		public DelegateCommand SetSceneCommand { get; }
		public DelegateCommand SetPatternCommand { get; }
		public DelegateCommand CamRbtnCheckedCommand { get; }
		public DelegateCommand ImgRbtnCheckedCommand { get; }
		#endregion commands

		#region properties
		private string _title;

		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		private Image<Bgr, byte> _imgSource;

		public Image<Bgr, byte> ImgSource
		{
			get => _imgSource;
			set => SetProperty(ref _imgSource, value);
		}

		private string _imgSourcePath = "...";

		public string ImgSourcePath
		{
			get => _imgSourcePath;
			set => SetProperty(ref _imgSourcePath, value);
		}

		private Image<Bgr, byte> _imgPattern;

		public Image<Bgr, byte> ImgPattern
		{
			get => _imgPattern;
			set => SetProperty(ref _imgPattern, value);
		}

		private string _imgPatternPath = "...";

		public string ImgPatternPath
		{
			get => _imgPatternPath;
			set => SetProperty(ref _imgPatternPath, value);
		}

		private bool _isCamRbtnChecked;
		public bool IsCamRbtnChecked
		{
			get => _isCamRbtnChecked;
			set => SetProperty(ref _isCamRbtnChecked, value);
		}

		private ImageSource _imageScene;

		public ImageSource ImageScene
		{
			get => _imageScene;
			set => SetProperty(ref _imageScene, value);
		}
		#endregion properties

		#region methods
		private void SetScene()
		{
			var scene = DialogHelper.GetImageFromUser();
			if (scene.Image == null) return;
			ImgSource = scene.Image;
			ImgSourcePath = scene.Path;
			ImageScene = ImgSource.ToBitmapSource();
		}

		private void SetPattern()
		{
			var pattern = DialogHelper.GetImageFromUser();
			if (pattern.Image == null) return;
			ImgPattern = pattern.Image;
			ImgPatternPath = pattern.Path;
		}

		private void CamRbtnChecked()
		{
			ComponentDispatcher.ThreadIdle += ComponentDispatcher_ThreadIdle;
		}

		private void ImgRbtnChecked()
		{
			ComponentDispatcher.ThreadIdle -= ComponentDispatcher_ThreadIdle;
			ImageScene = null;
		}

		private void ComponentDispatcher_ThreadIdle(object sender, System.EventArgs e)
		{
			ImgSource = _camCap.QueryFrame().ToImage<Bgr, byte>();
			ImageScene = ImgSource.ToBitmapSource();
		}
		#endregion methods
	}
}
