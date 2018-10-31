using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;

namespace ImageRecognition.Helpers
{
	public static class SurfHelper
	{
		public static VectorOfKeyPoint DescribeImage(this SURF detector, IInputArray image, out UMat observedDescriptors)
		{
			observedDescriptors = new UMat();
			var observedKeyPoints = new VectorOfKeyPoint(detector.Detect(image));
			detector.Compute(image, observedKeyPoints, observedDescriptors);
			return observedKeyPoints;
		}
	}
}
