using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageRecognition.Controls
{
	/// <summary>
	/// Interaction logic for ImageSourceControl.xaml
	/// </summary>
	public partial class ImageSourceControl : UserControl
	{
		public ImageSourceControl()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty TextProperty
			= DependencyProperty.Register(nameof(Text), typeof(string), typeof(ImageSourceControl), new PropertyMetadata("Description"));

		public static readonly DependencyProperty PathProperty
			= DependencyProperty.Register(nameof(Path), typeof(string), typeof(ImageSourceControl), new PropertyMetadata("..."));

		public static readonly DependencyProperty CommandProperty
			= DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ImageSourceControl));

		public string Text
		{
			get => (string) GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public string Path
		{
			get => (string)GetValue(PathProperty);
			set => SetValue(PathProperty, value);
		}

		public ICommand Command
		{
			get => (ICommand) GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}
	}
}
