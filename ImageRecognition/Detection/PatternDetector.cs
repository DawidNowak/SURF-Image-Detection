using System;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using ImageRecognition.Helpers;

namespace ImageRecognition.Detection
{
	public class PatternDetector
	{
		#region field
		private readonly MCvScalar _matchColor;
		private readonly Bgr _indicatorsColor;
		#endregion field

		#region construction
		public PatternDetector()
		{
			_matchColor = new Bgr(Color.Blue).MCvScalar;
			_indicatorsColor = new Bgr(Color.White);
		}
		#endregion construction

		#region properties

		public double HessianThresh { get; set; } = 450d;
		public double Uniqueness { get; set; } = 0.95;
		public bool DrawKeyPoints { get; set; } = true;
		public bool DrawMatchLines { get; set; } = true;
		#endregion properties

		#region Methods
		public Mat Recognize(Image<Bgr, byte> pattern, Image<Bgr, byte> frame)
		{
			if (frame == null) return null;

			Mat result;
			using (var detector = new SURF(HessianThresh))
			using (var matches = new VectorOfVectorOfDMatch())
			{
				var observedKeyPoints = detector.DescribeImage(frame, out var observedDescriptors);
				var k = 2;
				Mat homography = null;
				var modelKeyPoints = new VectorOfKeyPoint();

				var modelDescriptors = new UMat();
				modelKeyPoints = new VectorOfKeyPoint(detector.Detect(pattern));
				detector.Compute(pattern, modelKeyPoints, modelDescriptors);

				var matcher = new BFMatcher(DistanceType.L2);
				matcher.Add(modelDescriptors);

				matcher.KnnMatch(observedDescriptors, matches, k, null);
				var mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
				mask.SetTo(new MCvScalar(255));
				Features2DToolbox.VoteForUniqueness(matches, Uniqueness, mask);

				var nonZeroCount = CvInvoke.CountNonZero(mask);
				if (nonZeroCount >= 4)
				{
					nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
						matches, mask, 1.5, 20);
					if (nonZeroCount >= 4)
						homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
							observedKeyPoints, matches, mask, 2);
				}

				result = new Mat();
				if (DrawKeyPoints && DrawMatchLines)
				{
					Features2DToolbox.DrawMatches(pattern, modelKeyPoints, frame, observedKeyPoints,
						matches, result, _indicatorsColor.MCvScalar, _indicatorsColor.MCvScalar, mask);
				}
				else if (DrawKeyPoints)
				{
					var temp = new Mat();
					Features2DToolbox.DrawKeypoints(frame, observedKeyPoints, result, _indicatorsColor);
					Features2DToolbox.DrawKeypoints(pattern, modelKeyPoints, temp, _indicatorsColor);
					var merged = result.ToImage<Bgr, byte>().ConcateHorizontal(temp.ToImage<Bgr, byte>());
					result = merged.Mat;
				}
				else if (!DrawMatchLines || homography == null)
				{
					result = frame.ConcateHorizontal(pattern).Mat;
				}

				if (homography == null)
					return result;
				//draw a rectangle along the projected model
				var rect = new Rectangle(Point.Empty, pattern.Size);
				PointF[] pts =
				{
							new PointF(rect.Left, rect.Bottom),
							new PointF(rect.Right, rect.Bottom),
							new PointF(rect.Right, rect.Top),
							new PointF(rect.Left, rect.Top)
						};
				pts = CvInvoke.PerspectiveTransform(pts, homography);

				var points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
				var angles = points.Select((p, i) => SurfHelper.GetAngle(points[i % 4], points[(i + 1) % 4], points[(i + 2) % 4]))
					.ToArray();
				var isMatch = SurfHelper.IsMatch(angles);
				if (isMatch)
				{
					using (var vp = new VectorOfPoint(points))
					{
						CvInvoke.Polylines(result, vp, true, _matchColor, 5);
					}
				}
			}

			return result;
		}
		#endregion Methods
	}
}
