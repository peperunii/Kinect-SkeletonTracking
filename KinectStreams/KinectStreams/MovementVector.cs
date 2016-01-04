using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitseps
{
    public class MovementVector
    {
        public Point3D _startPoint;
        public Point3D _endPoint;
        public double _distance;
        public double _angle;

        public MovementVector()
        {
            _startPoint = new Point3D();
            _endPoint = new Point3D();
            _angle = 0;
            _distance = 0;
        }

        public MovementVector(Point3D startPoint, Point3D endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;

            _distance = DistanceBetwenTwoPoints();
            _angle = AngleBetweenTwoPointsInDegrees();
        }

        public double DistanceBetwenTwoPoints()
        {
            return Math3DHelper.DistanceBetwenTwoPoints(_startPoint.X - _endPoint.X, _startPoint.Y - _endPoint.Y);
        }

        public double AngleBetweenTwoPointsInDegrees()
        {
            return Math3DHelper.AngleBetweenTwoPointsInDegrees(_endPoint.X - _startPoint.X, _endPoint.Y - _startPoint.Y);
        }
    }
}
