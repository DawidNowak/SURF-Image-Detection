using System;
using System.Collections.Generic;
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
		private readonly MCvScalar _matchColor;
		private readonly Bgr _indicatorsColor;

		public PatternDetector()
		{
			_matchColor = new Bgr(Color.Blue).MCvScalar;
			_indicatorsColor = new Bgr(Color.White);
		}

		public Mat Recognize(Image<Bgr, byte> pattern, Image<Bgr, byte> frame, bool drawKeyPoints = true, bool drawMatchLines = true, double hessianThresh = 450d, double uniqueness = 0.8)
		{
			if (frame == null) return null;

			Mat result;
			using (var detector = new SURF(hessianThresh))
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
				Features2DToolbox.VoteForUniqueness(matches, uniqueness, mask);

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
				if (drawKeyPoints && drawMatchLines)
				{
					Features2DToolbox.DrawMatches(pattern, modelKeyPoints, frame, observedKeyPoints,
						matches, result, _indicatorsColor.MCvScalar, _indicatorsColor.MCvScalar, mask);
				}
				else if (drawKeyPoints)
				{
					var temp = new Mat();
					Features2DToolbox.DrawKeypoints(frame, observedKeyPoints, result, _indicatorsColor);
					Features2DToolbox.DrawKeypoints(pattern, modelKeyPoints, temp, _indicatorsColor);
					var merged = result.ToImage<Bgr, byte>().ConcateHorizontal(temp.ToImage<Bgr, byte>());
					result = merged.Mat;
				}
				else if (!drawMatchLines)
				{
					result = frame.ConcateHorizontal(pattern).Mat;
				}
				else
				{
					//should not get here
				}

				if (homography == null) return null;
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
				var angles = points.Select((p, i) => GetAngle(points[i % 4], points[(i + 1) % 4], points[(i + 2) % 4]))
					.ToArray();
				var isMatch = IsMatch(angles);
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

		private static double GetAngle(Point a, Point b, Point c)
		{
			var ab = GetDist(a, b);
			var bc = GetDist(b, c);
			var ac = GetDist(a, c);
			var cosB = Math.Pow(ac, 2) - Math.Pow(ab, 2) - Math.Pow(bc, 2);
			cosB = cosB / (2 * ab * bc);
			return 180 - Math.Acos(cosB) * 180 / Math.PI;
		}

		private static double GetDist(Point a, Point b) => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

		private static bool IsMatch(IReadOnlyCollection<double> angles)
		{
			const int anglesEpsilon = 20;
			const int epsilon = 40;
			if (angles.Count != 4) return false;

			var evenSum = angles.Where((a, i) => i % 2 == 0).Sum();
			var oddSum = angles.Where((a, i) => i % 2 != 0).Sum();

			var isMatch = Math.Abs(evenSum - 180) <= anglesEpsilon && Math.Abs(oddSum - 180) <= anglesEpsilon;

			if (isMatch && angles.Any(a => Math.Abs(a - 90) > epsilon))
				return false;

			return isMatch;
		}
	}
}
