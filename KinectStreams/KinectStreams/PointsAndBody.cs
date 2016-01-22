using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitseps
{
    public class Point3D
    {
        public double X;
        public double Y;
        public double Z;

        public Point3D()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class BodyStructure
    {
        public Dictionary<JointType, Point3D> currentFrameBody;

        public BodyStructure()
        {
            currentFrameBody = new Dictionary<JointType, Point3D>();
        }
    }

    public static class Math3DHelper
    {
        #region distanceAndAngleCalculation
        public static double DistanceBetwenTwoPoints(Point3D startPoint, Point3D endPoint)
        {
            return DistanceBetwenTwoPoints(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y);
        }

        public static double DistanceBetwenTwoPoints(double deltaX, double deltaY)
        {
            return System.Math.Sqrt(System.Math.Pow(deltaX, 2) + System.Math.Pow(deltaY, 2));
        }

        public static double AngleBetweenTwoPointsInDegrees(Point3D startPoint, Point3D endPoint)
        {
            return AngleBetweenTwoPointsInDegrees(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y);
        }

        public static double AngleBetweenTwoPointsInDegrees(double deltaX, double deltaY)
        {
            double angle = 0;// System.Math.Atan(Math.Abs(deltaY / deltaX)) * 180 / System.Math.PI;

            if (deltaX > 0 && deltaY > 0)
            {
                angle = 180 + System.Math.Atan(Math.Abs(deltaY / deltaX)) * 180 / System.Math.PI;
            }

            else if (deltaX < 0 && deltaY < 0)
            {
                angle = System.Math.Atan(Math.Abs(deltaY / deltaX)) * 180 / System.Math.PI;
            }

            else if (deltaX < 0 && deltaY > 0)
            {
                angle = 270 + System.Math.Atan(Math.Abs(deltaY / deltaX)) * 180 / System.Math.PI;
            }
            else if (deltaY < 0 && deltaX > 0)
            {
                angle = 90 + System.Math.Atan(Math.Abs(deltaY / deltaX)) * 180 / System.Math.PI;
            }

            else if (deltaX < 0 && deltaY == 0)
            {
                angle = 0;
            }

            else if (deltaX > 0 && deltaY == 0)
            {
                angle = 180;
            }

            else if (deltaY < 0 && deltaX == 0)
            {
                angle = 90;
            }

            else if (deltaY > 0 && deltaX == 0)
            {
                angle = 270;
            }

            else if (deltaX == 0 && deltaY == 0)
            {
                angle = 0;
            }

            return angle;
        }
        #endregion
    }
}
