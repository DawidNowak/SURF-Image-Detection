using System;
using System.Windows.Interop;
using System.Windows.Media;
using Emgu.CV;
using Emgu.CV.Structure;
using ImageRecognition.Detection;
using ImageRecognition.Helpers;
using Prism.Commands;
using Prism.Mvvm;

namespace ImageRecognition.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		#region fields
		private readonly VideoCapture _camCap;
		private readonly PatternDetector _detector;
		#endregion fields

		#region construction
		public MainWindowViewModel()
		{
			_camCap = new VideoCapture(0);
			_detector = new PatternDetector();
			SetSceneCommand = new DelegateCommand(SetScene);
			SetPatternCommand = new DelegateCommand(SetPattern);
			CamRbtnCheckedCommand = new DelegateCommand(CamRbtnChecked);
			ImgRbtnCheckedCommand = new DelegateCommand(ImgRbtnChecked);
			PerformSurfDetectionCommand = new DelegateCommand(PerformSurfDetection, () => IsCamRbtnChecked || IsImgRBtnChecked && ImgSource != null && ImgPattern != null);
		}
		#endregion construction

		#region commands
		public DelegateCommand SetSceneCommand { get; }
		public DelegateCommand SetPatternCommand { get; }
		public DelegateCommand CamRbtnCheckedCommand { get; }
		public DelegateCommand ImgRbtnCheckedCommand { get; }
		public DelegateCommand PerformSurfDetectionCommand { get; }
		#endregion commands

		#region properties
		private string _title;

		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		private string _surfButtonText = "Perform SURF detection";

		public string SurfButtonText
		{
			get => _surfButtonText;
			set => SetProperty(ref _surfButtonText, value);
		}

		private Image<Bgr, byte> _imgSource;

		public Image<Bgr, byte> ImgSource
		{
			get => _imgSource;
			set
			{
				SetProperty(ref _imgSource, value);
				PerformSurfDetectionCommand.RaiseCanExecuteChanged();
			}
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
			set
			{
				SetProperty(ref _imgPattern, value);
				PerformSurfDetectionCommand.RaiseCanExecuteChanged();
			}
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
			set
			{
				SetProperty(ref _isCamRbtnChecked, value);
				if (value) SurfButtonText = "Capture pattern to detect.";
			}
		}

		private bool _isImgRbtnChecked;

		public bool IsImgRBtnChecked
		{
			get => _isImgRbtnChecked;
			set
			{
				SetProperty(ref _isImgRbtnChecked, value);
				if (value) SurfButtonText = "Perform SURF detection.";
			}
		}

		private ImageSource _imageScene;

		public ImageSource ImageScene
		{
			get => _imageScene;
			set => SetProperty(ref _imageScene, value);
		}

		private double _hessianThresh = 450d;

		public double HessianThresh
		{
			get => _hessianThresh;
			set => SetProperty(ref _hessianThresh, value);
		}

		private double _uniqueness = 95d;

		public double Uniqueness
		{
			get => _uniqueness;
			set => SetProperty(ref _uniqueness, value);
		}

		private bool _drawKeyPoints = true;

		public bool DrawKeyPoints
		{
			get => _drawKeyPoints;
			set => SetProperty(ref _drawKeyPoints, value);
		}

		private bool _drawMatchLines = true;

		public bool DrawMatchLines
		{
			get => _drawMatchLines;
			set => SetProperty(ref _drawMatchLines, value);
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
			ImgSource = null;
			ImgPattern = null;
			ComponentDispatcher.ThreadIdle += ComponentDispatcher_ThreadIdle;
		}

		private void ImgRbtnChecked()
		{
			ComponentDispatcher.ThreadIdle -= ComponentDispatcher_ThreadIdle;
			ImageScene = null;
			ImgPattern = null;
			ImgSource = null;
		}

		private void ComponentDispatcher_ThreadIdle(object sender, System.EventArgs e)
		{
			try
			{
				ImgSource = _camCap.QueryFrame().ToImage<Bgr, byte>();
				Detect();
			}
			catch (Exception) { }
		}

		private void PerformSurfDetection()
		{
			if (IsCamRbtnChecked)
			{
				ImgPattern = ImgSource;
			}
			else //ImgRbtnChecked
			{
				Detect();
			}
		}

		private void Detect()
		{
			Mat result = null;
			if (ImgPattern != null)
			{
				result = _detector.Recognize(ImgPattern, ImgSource, DrawKeyPoints, DrawMatchLines, HessianThresh, Uniqueness / 100);
			}
			ImageScene = result != null ? result.ToBitmapSource() : ImgSource.ToBitmapSource();
		}

		#endregion methods
	}
}
