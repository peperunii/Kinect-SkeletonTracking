﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KinectStreams
{
    public static class Extensions
    {
        #region Camera

        public static ImageSource ToBitmap(this ColorFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            byte[] pixels = new byte[width * height * ((format.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(pixels);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        public static ImageSource ToBitmap(this DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;

            ushort[] pixelData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(pixelData);

            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex)
            {
                ushort depth = pixelData[depthIndex];

                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                pixels[colorIndex++] = intensity; // Blue
                pixels[colorIndex++] = intensity; // Green
                pixels[colorIndex++] = intensity; // Red

                ++colorIndex;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        public static ImageSource ToBitmap(this InfraredFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort[] frameData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(frameData);

            int colorIndex = 0;
            for (int infraredIndex = 0; infraredIndex < frameData.Length; infraredIndex++)
            {
                ushort ir = frameData[infraredIndex];

                byte intensity = (byte)(ir >> 7);

                pixels[colorIndex++] = (byte)(intensity / 1); // Blue
                pixels[colorIndex++] = (byte)(intensity / 1); // Green   
                pixels[colorIndex++] = (byte)(intensity / 0.4); // Red

                colorIndex++;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        #endregion

        #region Body

        public static Joint ScaleTo(this Joint joint, double width, double height, float skeletonMaxX, float skeletonMaxY)
        {
            joint.Position = new CameraSpacePoint
            {
                X = Scale(width, skeletonMaxX, joint.Position.X),
                Y = Scale(height, skeletonMaxY, -joint.Position.Y),
                Z = joint.Position.Z
            };

            return joint;
        }

        public static Joint ScaleTo(this Joint joint, double width, double height)
        {
            return ScaleTo(joint, width, height, 1.0f, 1.0f);
        }

        private static float Scale(double maxPixel, double maxSkeleton, float position)
        {
            float value = (float)((((maxPixel / maxSkeleton) / 2) * position) + (maxPixel / 2));

            if (value > maxPixel)
            {
                return (float)maxPixel;
            }

            if (value < 0)
            {
                return 0;
            }

            return value;
        }

        #endregion

        #region Drawing

        public static void DrawPoint(this Canvas canvas, Body body, KinectSensor _sensor)
        {
            if (body == null) return;

            foreach (Joint joint in body.Joints.Values)
            {
                // 3D space point
                CameraSpacePoint jointPosition = joint.Position;

                // 2D space point
                Point point = new Point();

                ColorSpacePoint colorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(jointPosition);
                //DepthSpacePoint depthPoint = _sensor.CoordinateMapper.MapCameraPointToDepthSpace(jointPosition);

                 point.X = float.IsInfinity(colorPoint.X) ? 0 : colorPoint.X;
               // point.X = float.IsInfinity(depthPoint.X) ? 0 : depthPoint.X;

                 point.Y = float.IsInfinity(colorPoint.Y) ? 0 : colorPoint.Y;
                // point.Y = float.IsInfinity(depthPoint.Y) ? 0 : depthPoint.Y;

                // Draw
                Ellipse ellipse = new Ellipse
                {
                    Fill = Brushes.Red,
                    Width = 10,
                    Height = 10
                };

                Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

                canvas.Children.Add(ellipse);
            }
        }




        private static List<Tuple<JointType, JointType>> tupleLines = new List<Tuple<JointType, JointType>>();

       /* private static void InitTupleJoints()
        {
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HandTipLeft, JointType.ThumbLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HandTipRight, JointType.ThumbRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));
        } */

        public static void DrawSkeleton(this Canvas canvas, Body body, List<JointType> interestingPoints, KinectSensor _sensor)
        {
            if (body == null) return;
  

            tupleLines.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HandTipLeft, JointType.ThumbLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HandTipRight, JointType.ThumbRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));
            tupleLines.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            foreach (Joint joint in body.Joints.Values)
            {
                canvas.DrawPoint(body, _sensor);
            }

            foreach (var jointLine in tupleLines)
            {
                if (interestingPoints.Contains(jointLine.Item1) || interestingPoints.Contains(jointLine.Item2))
                {
                    canvas.DrawLine(body.Joints[jointLine.Item1], body.Joints[jointLine.Item2], _sensor);
                }
            }
        }


        public static void DrawSkeleton(this Canvas canvas, Body body, KinectSensor _sensor)
        {
            if (body == null) return;

            /* foreach (Joint joint in body.Joints.Values)
             {
                 CameraSpacePoint cameraPoint = joint.Position;
                 ColorSpacePoint colorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(cameraPoint);
                 canvas.DrawPoint(joint);
             } */

            foreach (Joint joint in body.Joints.Values)
            {
                canvas.DrawPoint(body, _sensor);
            }

            canvas.DrawLine(body.Joints[JointType.Head], body.Joints[JointType.Neck], _sensor);
            canvas.DrawLine(body.Joints[JointType.Neck], body.Joints[JointType.SpineShoulder], _sensor);
            canvas.DrawLine(body.Joints[JointType.SpineShoulder], body.Joints[JointType.ShoulderLeft], _sensor);
            canvas.DrawLine(body.Joints[JointType.SpineShoulder], body.Joints[JointType.ShoulderRight], _sensor);
            canvas.DrawLine(body.Joints[JointType.SpineShoulder], body.Joints[JointType.SpineMid], _sensor);
            canvas.DrawLine(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft], _sensor);
            canvas.DrawLine(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ElbowRight], _sensor);
            canvas.DrawLine(body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft], _sensor);
            canvas.DrawLine(body.Joints[JointType.ElbowRight], body.Joints[JointType.WristRight], _sensor);
            canvas.DrawLine(body.Joints[JointType.WristLeft], body.Joints[JointType.HandLeft], _sensor);
            canvas.DrawLine(body.Joints[JointType.WristRight], body.Joints[JointType.HandRight], _sensor);
            canvas.DrawLine(body.Joints[JointType.HandLeft], body.Joints[JointType.HandTipLeft], _sensor);
            canvas.DrawLine(body.Joints[JointType.HandRight], body.Joints[JointType.HandTipRight], _sensor);
            canvas.DrawLine(body.Joints[JointType.HandTipLeft], body.Joints[JointType.ThumbLeft], _sensor);
            canvas.DrawLine(body.Joints[JointType.HandTipRight], body.Joints[JointType.ThumbRight], _sensor);
            canvas.DrawLine(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase], _sensor);
            canvas.DrawLine(body.Joints[JointType.SpineBase], body.Joints[JointType.HipLeft], _sensor);
            canvas.DrawLine(body.Joints[JointType.SpineBase], body.Joints[JointType.HipRight], _sensor);
            canvas.DrawLine(body.Joints[JointType.HipLeft], body.Joints[JointType.KneeLeft], _sensor);
            canvas.DrawLine(body.Joints[JointType.HipRight], body.Joints[JointType.KneeRight], _sensor);
            canvas.DrawLine(body.Joints[JointType.KneeLeft], body.Joints[JointType.AnkleLeft], _sensor);
            canvas.DrawLine(body.Joints[JointType.KneeRight], body.Joints[JointType.AnkleRight], _sensor);
            canvas.DrawLine(body.Joints[JointType.AnkleLeft], body.Joints[JointType.FootLeft], _sensor);
            canvas.DrawLine(body.Joints[JointType.AnkleRight], body.Joints[JointType.FootRight], _sensor);
        }
 


        public static void DrawPoint(this Canvas canvas, Joint joint)
        {
            if (joint.TrackingState == TrackingState.NotTracked) return;



            joint = joint.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            Ellipse ellipse = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(Colors.Blue)
            };

            Canvas.SetLeft(ellipse, joint.Position.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, joint.Position.Y - ellipse.Height / 2);

            canvas.Children.Add(ellipse);
        }

        public static void DrawLine(this Canvas canvas, Joint first, Joint second, KinectSensor _sensor)
        {
            if (first.TrackingState == TrackingState.NotTracked || second.TrackingState == TrackingState.NotTracked) return;

            //first = first.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);
            //second = second.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

           
                // 3D space point
               // CameraSpacePoint jointPosition = joint.Position;

                CameraSpacePoint firstPosition = first.Position;
                CameraSpacePoint secondPosition = second.Position;

            // 2D space point
            //Point point = new Point();
            Point firstPoint = new Point();
            Point secondPoint = new Point();

            ColorSpacePoint firstColorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(firstPosition);
            ColorSpacePoint secondColorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(secondPosition);
            //DepthSpacePoint depthPoint = _sensor.CoordinateMapper.MapCameraPointToDepthSpace(jointPosition);

            // point.X = float.IsInfinity(colorPoint.X) ? 0 : colorPoint.X;

            firstPoint.X = float.IsInfinity(firstColorPoint.X) ? 0 : firstColorPoint.X;
            secondPoint.X = float.IsInfinity(secondColorPoint.X) ? 0 : secondColorPoint.X;
            // point.X = float.IsInfinity(depthPoint.X) ? 0 : depthPoint.X;

            //point.Y = float.IsInfinity(colorPoint.Y) ? 0 : colorPoint.Y;
            firstPoint.Y = float.IsInfinity(firstColorPoint.Y) ? 0 : firstColorPoint.Y;
            secondPoint.Y = float.IsInfinity(secondColorPoint.Y) ? 0 : secondColorPoint.Y;

            // point.Y = float.IsInfinity(depthPoint.Y) ? 0 : depthPoint.Y;

            // Draw
            Ellipse ellipse = new Ellipse
                {
                    Fill = Brushes.Red,
                    Width = 6,
                    Height = 6
                };

            /*       Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
                   Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

                   canvas.Children.Add(ellipse); */

            /* Line line = new Line
             {
                 X1 = first.Position.X,
                 Y1 = first.Position.Y,
                 X2 = second.Position.X,
                 Y2 = second.Position.Y,
                 StrokeThickness = 8,
                 Stroke = new SolidColorBrush(Colors.Blue)
             }; */

            Line line = new Line
            {
                X1 = firstPoint.X,
                Y1 = firstPoint.Y,
                X2 = secondPoint.X,
                Y2 = secondPoint.Y,
                StrokeThickness = 8,
                Stroke = new SolidColorBrush(Colors.Blue)
            };

            canvas.Children.Add(line);
        }

        #endregion
    }
}
