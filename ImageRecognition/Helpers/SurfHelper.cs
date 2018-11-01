using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

		public static double GetAngle(Point a, Point b, Point c)
		{
			var ab = GetDist(a, b);
			var bc = GetDist(b, c);
			var ac = GetDist(a, c);
			var cosB = Math.Pow(ac, 2) - Math.Pow(ab, 2) - Math.Pow(bc, 2);
			cosB = cosB / (2 * ab * bc);
			return 180 - Math.Acos(cosB) * 180 / Math.PI;
		}

		public static double GetDist(Point a, Point b) => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

		public static bool IsMatch(IReadOnlyCollection<double> angles)
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
