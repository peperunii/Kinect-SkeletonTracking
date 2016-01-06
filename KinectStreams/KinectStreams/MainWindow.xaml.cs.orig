using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KinectStreams;
using Bitseps;
using Fitnematic.Core.BenchPressTest;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
//using System.Speech.Recognition;
//using System.Speech.AudioFormat;
using System.Globalization;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;

namespace KinectCoordinateMapping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        CameraMode _mode = CameraMode.Color;
        //CameraMode _mode = CameraMode.Depth;
        
          
        DepthFrame depthFrame;
        ColorFrame colorFrame;

        List<JointType> jointsOfInterest = new List<JointType>();
        Dictionary<JointType, List<MovementVector>> movementVector = null;

        int pointsToAnalyze = 9;
        RecordMovement record;// = new RecordMovement(pointsOfInterest, pointsToAnalyze);
        WorkoutTracking benchPress;
        static bool PressSpaced = false;
        static bool escape = false;

        static bool jointsChange = false;
        static bool finishedCountdown = false;

        [DllImport("user32")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        private const byte VK_MENU = 0x12;
        private const byte VK_TAB = 0x09;
        private const int KEYEVENTF_EXTENDEDKEY = 0x01;
        private const int KEYEVENTF_KEYUP = 0x02;

        //Speech
        private KinectAudioStream convertStream = null;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
        private SpeechRecognitionEngine speechEngine = null;

        private List<Span> recognitionSpans;

        private const string MediumGreyBrushKey = "MediumGreyBrush";



        public MainWindow()
        {
            InitializeComponent();

            jointsOfInterest.Clear();
            jointsOfInterest.Add(JointType.HandLeft);
            jointsOfInterest.Add(JointType.HandRight);
            record = new RecordMovement(jointsOfInterest, pointsToAnalyze);
            benchPress = new WorkoutTracking(jointsOfInterest, 2);
        }

        void SelectMode (object sender, RoutedEventArgs e)
        {
            if (depthMode.IsChecked == true)
            {
                _mode = CameraMode.Depth;
            }
            else
            {
                _mode = CameraMode.Color;
            }
        }
        void NumberOfJointPoints (object sender, RoutedEventArgs e)
        { 
            if(sixJoints.IsChecked == true)
            {
                jointsOfInterest.Clear();
                jointsOfInterest.Add(JointType.HandLeft);
                jointsOfInterest.Add(JointType.HandRight);
                /* jointsOfInterest.Add(JointType.ElbowLeft);
                 jointsOfInterest.Add(JointType.ElbowRight);
                 jointsOfInterest.Add(JointType.AnkleLeft);
                 jointsOfInterest.Add(JointType.AnkleRight); */
                jointsOfInterest.Add(JointType.Head);
                jointsOfInterest.Add(JointType.KneeLeft);
                jointsOfInterest.Add(JointType.KneeRight);
                jointsOfInterest.Add(JointType.SpineMid);

                jointsChange = true;
                PressSpaced = false;
                escape = false;

                record = new RecordMovement(jointsOfInterest, pointsToAnalyze);
                benchPress = new WorkoutTracking(jointsOfInterest, 2);
            }
            else
            {
                jointsOfInterest.Clear();
                jointsOfInterest.Add(JointType.HandLeft);
                jointsOfInterest.Add(JointType.HandRight);

                jointsChange = true;
                PressSpaced = false;
                escape = false;

                record = new RecordMovement(jointsOfInterest, pointsToAnalyze);
                benchPress = new WorkoutTracking(jointsOfInterest, 2);

                
            }
        }

       /// <summary>
        /// Gets the metadata for the speech recognizer (acoustic model) most suitable to
        /// process audio from Kinect device.
        /// </summary>
        /// <returns>
        /// RecognizerInfo if found, <code>null</code> otherwise.
        /// </returns>
        private static RecognizerInfo TryGetKinectRecognizer()
        {
            IEnumerable<RecognizerInfo> recognizers;

            // This is required to catch the case when an expected recognizer is not installed.
            // By default - the x86 Speech Runtime is always expected. 
            try
            {
                recognizers = SpeechRecognitionEngine.InstalledRecognizers();
            }
            catch (COMException)
            {
                return null;
            }

            foreach (RecognizerInfo recognizer in recognizers)
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }
        
          

        private void ButtonRecordMovement_Click(object sender, RoutedEventArgs e)
        {

            camera.Visibility = Visibility.Hidden;
            canvas.Visibility = Visibility.Hidden;
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerAsync();
          
        }

        private void UpdateUI(string i, Brush brush, int fontWeight)
        {
            this.testWindow.FontSize = fontWeight;
            this.testWindow.Text = i;
            this.testWindow.Foreground = brush;
        }

        private delegate void Updater(string UI, Brush brush, int fontWeight);

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            buttonALT_TAB();

                Work1();
            PressSpaced = true;
            worker.CancelAsync();
        }

        private void UpdateTest()
        {
            Work1();
        }

        private void Work1()
        {
            Updater uiUpdater = new Updater(UpdateUI);
            //testWindow.Visibility = Visibility.Visible;

            for (int i = 3; i >= 1; i--)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, uiUpdater, i.ToString(), Brushes.Black, 120);
                Thread.Sleep(1000);
            }

            Dispatcher.Invoke(() =>
            {
                /* if(_mode == CameraMode.Color)
                 { */
                finishedCountdown = true;
<<<<<<< HEAD
               // canvas.Visibility = Visibility.Visible;
               // camera.Visibility = Visibility.Visible;
=======
                canvas.Visibility = Visibility.Visible;
                camera.Visibility = Visibility.Visible;
>>>>>>> 43e279247e4d78e50cc6ca8c2d1e3abd86e1130a
               /* }
                else if(_mode == CameraMode.Depth)
                {*/
                    //canvasDepth.Visibility = Visibility.Visible;                
                   // cameraDepth.Visibility = Visibility.Visible;
                //  }
                
                    testWindow.Visibility = Visibility.Hidden;
                
               
                //button.Visibility = Visibility.Hidden;
            });
        }

        private void buttonALT_TAB()
        {
            System.Threading.Thread.Sleep(10);
            keybd_event(VK_TAB, 0x8f, 0, 0); // Tab Press 
                                             //
            keybd_event(VK_TAB, 0x8f, KEYEVENTF_KEYUP, 0); // Tab Release 
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                //Depth:_reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);//FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared |
                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Depth | FrameSourceTypes.Color);//FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared |
               _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;


                // grab the audio stream
                IReadOnlyList<AudioBeam> audioBeamList = this._sensor.AudioSource.AudioBeams;
                System.IO.Stream audioStream = audioBeamList[0].OpenInputStream();

                // create the convert stream
                this.convertStream = new KinectAudioStream(audioStream);
            }

            RecognizerInfo ri = TryGetKinectRecognizer();

            if (null != ri)
            {
                this.recognitionSpans = new List<Span> { start, stop };

                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(KinectStreams.Properties.Resources.SpeechGrammar)))
                {
                    var g = new Grammar(memoryStream);
                    this.speechEngine.LoadGrammar(g);
                }

                this.speechEngine.SpeechRecognized += this.SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected += this.SpeechRejected;

                // let the convertStream know speech is going active
                this.convertStream.SpeechActive = true;

                // For long recognition sessions (a few hours or more), it may be beneficial to turn off adaptation of the acoustic model. 
                // This will prevent recognition accuracy from degrading over time.
                ////speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

                this.speechEngine.SetInputToAudioStream(
                    this.convertStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                this.speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        

        private void Window_Closed(object sender, EventArgs e)
        {
            if (null != this.convertStream)
            {
                this.convertStream.SpeechActive = false;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= this.SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= this.SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }

            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }


        // Speech
        private void ClearRecognitionHighlights()
        {
            foreach (Span span in this.recognitionSpans)
            {
                span.Foreground = (Brush)this.Resources[MediumGreyBrushKey];
                span.FontWeight = FontWeights.Normal;
            }
        }

        /// <summary>
        /// Handler for recognized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.3;

            this.ClearRecognitionHighlights();

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "START":
                        testWindow.Visibility = Visibility.Visible;
                        this.ButtonRecordMovement_Click(null, null);

                        break;

                    case "STOP":
                        escape = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Handler for rejected speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            this.ClearRecognitionHighlights();
        }


        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            colorFrame = reference.ColorFrameReference.AcquireFrame();
            {
                if (colorFrame != null)
                {
                    
                    if (_mode == CameraMode.Color)
                    {
                        camera.Source = colorFrame.ToBitmap();
                    }
                }
            } 

            
            // Depth
            depthFrame = reference.DepthFrameReference.AcquireFrame();
            {
                if (depthFrame != null)
                {
                    if (_mode == CameraMode.Depth)
                    {
                        cameraDepth.Source = depthFrame.ToBitmap();
                    }
                }
            }

           

            Ellipse ellipse1 = new Ellipse
            {
                Fill = Brushes.Green,
                Width = 20,
                Height = 20
            };
            Ellipse ellipse2 = new Ellipse
            {
                Fill = Brushes.Pink,
                Width = 20,
                Height = 20
            };

            Ellipse ellipse3 = new Ellipse
            {
                Fill = Brushes.Green,
                Width = 20,
                Height = 20
            };
            Ellipse ellipse4 = new Ellipse
            {
                Fill = Brushes.Pink,
                Width = 20,
                Height = 20
            };

            Ellipse ellipse5 = new Ellipse
            {
                Fill = Brushes.Green,
                Width = 20,
                Height = 20
            };
            Ellipse ellipse6 = new Ellipse
            {
                Fill = Brushes.Pink,
                Width = 20,
                Height = 20
            };

            Ellipse ellipse7 = new Ellipse
            {
                Fill = Brushes.Green,
                Width = 20,
                Height = 20
            };
            Ellipse ellipse8 = new Ellipse
            {
                Fill = Brushes.Pink,
                Width = 20,
                Height = 20
            };

            Ellipse ellipse9 = new Ellipse
            {
                Fill = Brushes.Green,
                Width = 20,
                Height = 20
            };
            Ellipse ellipse10 = new Ellipse
            {
                Fill = Brushes.Pink,
                Width = 20,
                Height = 20
            };

            Ellipse ellipse11 = new Ellipse
            {
                Fill = Brushes.Green,
                Width = 20,
                Height = 20
            };
            Ellipse ellipse12 = new Ellipse
            {
                Fill = Brushes.Pink,
                Width = 20,
                Height = 20
            };

           //if(finishedCountdown)
            //{
                if (_mode == CameraMode.Color)
                {
                    camera.Visibility = Visibility.Visible;
                    canvas.Visibility = Visibility.Visible;
                    cameraDepth.Visibility = Visibility.Hidden;
                    canvasDepth.Visibility = Visibility.Hidden;
                    finishedCountdown = false;
                }

                if (_mode == CameraMode.Depth)
                {
                    camera.Visibility = Visibility.Hidden;
                    canvas.Visibility = Visibility.Hidden;
                    cameraDepth.Visibility = Visibility.Visible;
                    canvasDepth.Visibility = Visibility.Visible;
                    finishedCountdown = false;
                }
            //}
            

            
            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    
                    this.canvas.Children.Clear();
                    this.canvasDepth.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    //int bodyIndex = 0;
                    foreach (var body in _bodies)
                    {
                        //if (bodyIndex != 0) continue;
                        //bodyIndex++;
                        var cameraSpacePoint = new CameraSpacePoint();

                        DepthSpacePoint depthPointWristLeft = new DepthSpacePoint();
                        DepthSpacePoint depthPointWristRight = new DepthSpacePoint();
                        DepthSpacePoint depthPointHead = new DepthSpacePoint();
                        DepthSpacePoint depthPointSpineMid = new DepthSpacePoint();
                        DepthSpacePoint depthPointKneeLeft = new DepthSpacePoint();
                        DepthSpacePoint depthPointKneeRight = new DepthSpacePoint();

                        ColorSpacePoint colorPointWristLeft = new ColorSpacePoint();
                        ColorSpacePoint colorPointWristRight = new ColorSpacePoint();
                        ColorSpacePoint colorPointHead = new ColorSpacePoint();
                        ColorSpacePoint colorPointSpineMid = new ColorSpacePoint();
                        ColorSpacePoint colorPointKneeLeft = new ColorSpacePoint();
                        ColorSpacePoint colorPointKneeRight = new ColorSpacePoint();
                            
     

                        if (body.IsTracked)
                        {
                            BodyStructure currentFrameBody = new BodyStructure();


                            var lines = new List<string>();

                            foreach (var joint in body.Joints.Values)
                            {
                                cameraSpacePoint.X = joint.Position.X;//(float)currentFrameBody.currentFrameBody[joint.JointType].X;
                                cameraSpacePoint.Y = joint.Position.Y; //(float)currentFrameBody.currentFrameBody[joint.JointType].Y;
                                cameraSpacePoint.Z = joint.Position.Z; //(float)currentFrameBody.currentFrameBody[joint.JointType].Z;

                                var colorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(cameraSpacePoint);
                                var depthPoint = _sensor.CoordinateMapper.MapCameraPointToDepthSpace(cameraSpacePoint);

                                if(_mode == CameraMode.Depth)
                                {
                                    Ellipse ellipseBlue = new Ellipse
                                    {
                                        Fill = Brushes.Blue,
                                        Width = 10,
                                        Height = 10
                                    };
                                    var xPos = depthPoint.X - ellipseBlue.Width / 2;
                                    var yPos = depthPoint.Y - ellipseBlue.Height / 2;

                                    if (xPos >= 0 && xPos < this.canvasDepth.ActualWidth &&
                                        yPos >= 0 && yPos < this.canvasDepth.ActualHeight)
                                    {
                                        Canvas.SetLeft(ellipseBlue, xPos);
                                        Canvas.SetTop(ellipseBlue, yPos);

                                        canvasDepth.Children.Add(ellipseBlue);
                                    }
                                    //currentFrameBody.currentFrameBody.Add(joint.JointType, new Point3D());
                                    if (jointsOfInterest.Contains(joint.JointType))
                                    {
                                        if (joint.JointType == JointType.HandLeft)
                                            depthPointWristLeft = depthPoint;

                                        else if (joint.JointType == JointType.HandRight)
                                            depthPointWristRight = depthPoint;

                                        else if (joint.JointType == JointType.Head)
                                            depthPointHead = depthPoint;

                                        else if (joint.JointType == JointType.SpineMid)
                                            depthPointSpineMid = depthPoint;

                                        else if (joint.JointType == JointType.KneeRight)
                                            depthPointKneeRight = depthPoint;

                                        else if (joint.JointType == JointType.KneeLeft)
                                            depthPointKneeLeft = depthPoint;


                                        currentFrameBody.currentFrameBody.Add(joint.JointType, new Point3D(depthPoint.X, depthPoint.Y, 0));
                                    }
                                }
                                else
                                { 
                                    Ellipse ellipseBlue = new Ellipse
                                    {
                                        Fill = Brushes.Blue,
                                        Width = 10,
                                        Height = 10
                                    };
                                    var xPos = colorPoint.X - ellipseBlue.Width / 2;
                                    var yPos = colorPoint.Y - ellipseBlue.Height / 2;

                                    if (xPos >= 0 && xPos < this.canvas.ActualWidth &&
                                        yPos >= 0 && yPos < this.canvas.ActualHeight)
                                    {
                                        Canvas.SetLeft(ellipseBlue, xPos);
                                        Canvas.SetTop(ellipseBlue, yPos);

                                        canvas.Children.Add(ellipseBlue);
                                    }
                                    //currentFrameBody.currentFrameBody.Add(joint.JointType, new Point3D());
                                    if (jointsOfInterest.Contains(joint.JointType))
                                    {
                                        if (joint.JointType == JointType.HandLeft)
                                            colorPointWristLeft = colorPoint;

                                        else if (joint.JointType == JointType.HandRight)
                                            colorPointWristRight = colorPoint;

                                        else if (joint.JointType == JointType.Head)
                                            colorPointHead = colorPoint;

                                        else if (joint.JointType == JointType.SpineMid)
                                            colorPointSpineMid = colorPoint;

                                        else if (joint.JointType == JointType.KneeRight)
                                            colorPointKneeRight = colorPoint;

                                        else if (joint.JointType == JointType.KneeLeft)
                                            colorPointKneeLeft = colorPoint;
                                        

                                        currentFrameBody.currentFrameBody.Add(joint.JointType, new Point3D(colorPoint.X, colorPoint.Y, 0));
                                    }
                                }
                            }


                            /*TODO - null or empty Dictionary  (.Count == 0)*/
                            //  if (movementVector == null)
                            if (PressSpaced && !escape)
                            {
                                if (movementVector != null && jointsChange)
                                {
                                    movementVector.Clear();
                                }

                                jointsChange = false;
                                movementVector = record.GetMovementVectors(currentFrameBody);

                            }
                            //else if(movementVector != null)
                            else if (escape && movementVector != null)
                            {
                                var curFrameResult = benchPress.ValidateCurrentSkeletonFrame(movementVector, currentFrameBody);
                                var resultWristLeft = curFrameResult.BoneStates[JointType.HandLeft];
                                var resultWristRight = curFrameResult.BoneStates[JointType.HandRight];

                                if(sixJoints.IsChecked == true) 
                                {
                                    var resultHead = curFrameResult.BoneStates[JointType.Head];
                                    var resultSpineMid = curFrameResult.BoneStates[JointType.SpineMid];
                                    var resultKneeLeft = curFrameResult.BoneStates[JointType.KneeLeft];
                                    var resultKneeRight = curFrameResult.BoneStates[JointType.KneeRight];

                                    if (_mode == CameraMode.Depth)
                                    {
                                        if (resultSpineMid == SkeletonBoneState.Matched)
                                        {
                                            var xPosMid = depthPointSpineMid.X - ellipse7.Width / 2;
                                            var yPosMid = depthPointSpineMid.Y - ellipse7.Height / 2;

                                            if (xPosMid >= 0 && xPosMid < this.canvasDepth.ActualWidth &&
                                                yPosMid >= 0 && yPosMid < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse7, xPosMid);
                                                Canvas.SetTop(ellipse7, yPosMid);

                                                canvasDepth.Children.Add(ellipse7);
                                            }
                                        }
                                        else
                                        {
                                            var xPosMid = depthPointSpineMid.X - ellipse8.Width / 2;
                                            var yPosMid = depthPointSpineMid.Y - ellipse8.Height / 2;

                                            if (xPosMid >= 0 && xPosMid < this.canvasDepth.ActualWidth &&
                                                yPosMid >= 0 && yPosMid < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse8, xPosMid);
                                                Canvas.SetTop(ellipse8, yPosMid);

                                                canvasDepth.Children.Add(ellipse8);
                                            }
                                        }

                                        if (resultKneeLeft == SkeletonBoneState.Matched)
                                        {
                                            var xPosKneeLeft = depthPointKneeLeft.X - ellipse9.Width / 2;
                                            var yPosKneeLeft = depthPointKneeLeft.Y - ellipse9.Height / 2;

                                            if (xPosKneeLeft >= 0 && xPosKneeLeft < this.canvasDepth.ActualWidth &&
                                                yPosKneeLeft >= 0 && yPosKneeLeft < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse9, xPosKneeLeft);
                                                Canvas.SetTop(ellipse9, yPosKneeLeft);

                                                canvasDepth.Children.Add(ellipse9);
                                            }
                                        }
                                        else
                                        {
                                            var xPosKneeLeft = depthPointKneeLeft.X - ellipse10.Width / 2;
                                            var yPosKneeLeft = depthPointKneeLeft.Y - ellipse10.Height / 2;

                                            if (xPosKneeLeft >= 0 && xPosKneeLeft < this.canvasDepth.ActualWidth &&
                                                yPosKneeLeft >= 0 && yPosKneeLeft < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse10, xPosKneeLeft);
                                                Canvas.SetTop(ellipse10, yPosKneeLeft);

                                                canvasDepth.Children.Add(ellipse10);
                                            }
                                        }

                                        if (resultKneeRight == SkeletonBoneState.Matched)
                                        {
                                            var xPosKneeRight = depthPointKneeRight.X - ellipse11.Width / 2;
                                            var yPosKneeRight = depthPointKneeRight.Y - ellipse11.Height / 2;

                                            if (xPosKneeRight >= 0 && xPosKneeRight < this.canvasDepth.ActualWidth &&
                                                yPosKneeRight >= 0 && yPosKneeRight < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse11, xPosKneeRight);
                                                Canvas.SetTop(ellipse11, yPosKneeRight);

                                                canvasDepth.Children.Add(ellipse11);
                                            }
                                        }
                                        else
                                        {
                                            var xPosKneeRight = depthPointKneeRight.X - ellipse12.Width / 2;
                                            var yPosKneeRight = depthPointKneeRight.Y - ellipse12.Height / 2;

                                            if (xPosKneeRight >= 0 && xPosKneeRight < this.canvasDepth.ActualWidth &&
                                                yPosKneeRight >= 0 && yPosKneeRight < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse12, xPosKneeRight);
                                                Canvas.SetTop(ellipse12, yPosKneeRight);

                                                canvasDepth.Children.Add(ellipse12);
                                            }
                                        }

                                        if (resultHead == SkeletonBoneState.Matched)
                                        {
                                            var xPosHead = depthPointHead.X - ellipse5.Width / 2;
                                            var yPosHead = depthPointHead.Y - ellipse5.Height / 2;

                                            if (xPosHead >= 0 && xPosHead < this.canvasDepth.ActualWidth &&
                                                yPosHead >= 0 && yPosHead < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse5, xPosHead);
                                                Canvas.SetTop(ellipse5, yPosHead);

                                                canvasDepth.Children.Add(ellipse5);
                                            }

                                        }
                                        else
                                        {
                                            var xPosHead = depthPointHead.X - ellipse6.Width / 2;
                                            var yPosHead = depthPointHead.Y - ellipse6.Height / 2;

                                            if (xPosHead >= 0 && xPosHead < this.canvasDepth.ActualWidth &&
                                                yPosHead >= 0 && yPosHead < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse6, xPosHead);
                                                Canvas.SetTop(ellipse6, yPosHead);

                                                canvasDepth.Children.Add(ellipse6);
                                            }
                                        }

                                        if (resultWristLeft == SkeletonBoneState.Matched)
                                        {
                                            var xPosLeft = depthPointWristLeft.X - ellipse1.Width / 2;
                                            var yPosLeft = depthPointWristLeft.Y - ellipse1.Height / 2;

                                            if (xPosLeft >= 0 && xPosLeft < this.canvasDepth.ActualWidth &&
                                                yPosLeft >= 0 && yPosLeft < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse1, xPosLeft);
                                                Canvas.SetTop(ellipse1, yPosLeft);

                                                canvasDepth.Children.Add(ellipse1);
                                            }

                                        }
                                        else
                                        {
                                            var xPosLeft = depthPointWristLeft.X - ellipse2.Width / 2;
                                            var yPosLeft = depthPointWristLeft.Y - ellipse2.Height / 2;

                                            if (xPosLeft >= 0 && xPosLeft < this.canvasDepth.ActualWidth &&
                                                yPosLeft >= 0 && yPosLeft < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse2, xPosLeft);
                                                Canvas.SetTop(ellipse2, yPosLeft);

                                                canvasDepth.Children.Add(ellipse2);
                                            }
                                        }

                                        if (resultWristRight == SkeletonBoneState.Matched)
                                        {
                                            var xPosRight = depthPointWristRight.X - ellipse3.Width / 2;
                                            var yPosRight = depthPointWristRight.Y - ellipse3.Height / 2;

                                            if (xPosRight >= 0 && xPosRight < this.canvasDepth.ActualWidth &&
                                                yPosRight >= 0 && yPosRight < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse3, xPosRight);
                                                Canvas.SetTop(ellipse3, yPosRight);

                                                canvasDepth.Children.Add(ellipse3);
                                            }

                                        }
                                        else
                                        {
                                            var xPosRight = depthPointWristRight.X - ellipse4.Width / 2;
                                            var yPosRight = depthPointWristRight.Y - ellipse4.Height / 2;

                                            if (xPosRight >= 0 && xPosRight < this.canvasDepth.ActualWidth &&
                                                yPosRight >= 0 && yPosRight < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse4, xPosRight);
                                                Canvas.SetTop(ellipse4, yPosRight);

                                                canvasDepth.Children.Add(ellipse4);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (resultSpineMid == SkeletonBoneState.Matched)
                                        {
                                            var xPosMid = colorPointSpineMid.X - ellipse7.Width / 2;
                                            var yPosMid = colorPointSpineMid.Y - ellipse7.Height / 2;

                                            if (xPosMid >= 0 && xPosMid < this.canvas.ActualWidth &&
                                                yPosMid >= 0 && yPosMid < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse7, xPosMid);
                                                Canvas.SetTop(ellipse7, yPosMid);

                                                canvas.Children.Add(ellipse7);
                                            }
                                        }
                                        else
                                        {
                                            var xPosMid = colorPointSpineMid.X - ellipse8.Width / 2;
                                            var yPosMid = colorPointSpineMid.Y - ellipse8.Height / 2;

                                            if (xPosMid >= 0 && xPosMid < this.canvas.ActualWidth &&
                                                yPosMid >= 0 && yPosMid < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse8, xPosMid);
                                                Canvas.SetTop(ellipse8, yPosMid);

                                                canvas.Children.Add(ellipse8);
                                            }
                                        }

                                        if (resultKneeLeft == SkeletonBoneState.Matched)
                                        {
                                            var xPosKneeLeft = colorPointKneeLeft.X - ellipse9.Width / 2;
                                            var yPosKneeLeft = colorPointKneeLeft.Y - ellipse9.Height / 2;

                                            if (xPosKneeLeft >= 0 && xPosKneeLeft < this.canvas.ActualWidth &&
                                                yPosKneeLeft >= 0 && yPosKneeLeft < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse9, xPosKneeLeft);
                                                Canvas.SetTop(ellipse9, yPosKneeLeft);

                                                canvas.Children.Add(ellipse9);
                                            }
                                        }
                                        else
                                        {
                                            var xPosKneeLeft = colorPointKneeLeft.X - ellipse10.Width / 2;
                                            var yPosKneeLeft = colorPointKneeLeft.Y - ellipse10.Height / 2;

                                            if (xPosKneeLeft >= 0 && xPosKneeLeft < this.canvas.ActualWidth &&
                                                yPosKneeLeft >= 0 && yPosKneeLeft < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse10, xPosKneeLeft);
                                                Canvas.SetTop(ellipse10, yPosKneeLeft);

                                                canvas.Children.Add(ellipse10);
                                            }
                                        }

                                        if (resultKneeRight == SkeletonBoneState.Matched)
                                        {
                                            var xPosKneeRight = colorPointKneeRight.X - ellipse11.Width / 2;
                                            var yPosKneeRight = colorPointKneeRight.Y - ellipse11.Height / 2;

                                            if (xPosKneeRight >= 0 && xPosKneeRight < this.canvas.ActualWidth &&
                                                yPosKneeRight >= 0 && yPosKneeRight < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse11, xPosKneeRight);
                                                Canvas.SetTop(ellipse11, yPosKneeRight);

                                                canvas.Children.Add(ellipse11);
                                            }
                                        }
                                        else
                                        {
                                            var xPosKneeRight = colorPointKneeRight.X - ellipse12.Width / 2;
                                            var yPosKneeRight = colorPointKneeRight.Y - ellipse12.Height / 2;

                                            if (xPosKneeRight >= 0 && xPosKneeRight < this.canvas.ActualWidth &&
                                                yPosKneeRight >= 0 && yPosKneeRight < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse12, xPosKneeRight);
                                                Canvas.SetTop(ellipse12, yPosKneeRight);

                                                canvas.Children.Add(ellipse12);
                                            }
                                        }

                                        if (resultHead == SkeletonBoneState.Matched)
                                        {
                                            var xPosHead = colorPointHead.X - ellipse5.Width / 2;
                                            var yPosHead = colorPointHead.Y - ellipse5.Height / 2;

                                            if (xPosHead >= 0 && xPosHead < this.canvas.ActualWidth &&
                                                yPosHead >= 0 && yPosHead < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse5, xPosHead);
                                                Canvas.SetTop(ellipse5, yPosHead);

                                                canvas.Children.Add(ellipse5);
                                            }

                                        }
                                        else
                                        {
                                            var xPosHead = colorPointHead.X - ellipse6.Width / 2;
                                            var yPosHead = colorPointHead.Y - ellipse6.Height / 2;

                                            if (xPosHead >= 0 && xPosHead < this.canvas.ActualWidth &&
                                                yPosHead >= 0 && yPosHead < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse6, xPosHead);
                                                Canvas.SetTop(ellipse6, yPosHead);

                                                canvas.Children.Add(ellipse6);
                                            }
                                        }

                                        if (resultWristLeft == SkeletonBoneState.Matched)
                                        {
                                            var xPosLeft = colorPointWristLeft.X - ellipse1.Width / 2;
                                            var yPosLeft = colorPointWristLeft.Y - ellipse1.Height / 2;

                                            if (xPosLeft >= 0 && xPosLeft < this.canvas.ActualWidth &&
                                                yPosLeft >= 0 && yPosLeft < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse1, xPosLeft);
                                                Canvas.SetTop(ellipse1, yPosLeft);

                                                canvas.Children.Add(ellipse1);
                                            }

                                        }
                                        else
                                        {
                                            var xPosLeft = colorPointWristLeft.X - ellipse2.Width / 2;
                                            var yPosLeft = colorPointWristLeft.Y - ellipse2.Height / 2;

                                            if (xPosLeft >= 0 && xPosLeft < this.canvas.ActualWidth &&
                                                yPosLeft >= 0 && yPosLeft < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse2, xPosLeft);
                                                Canvas.SetTop(ellipse2, yPosLeft);

                                                canvas.Children.Add(ellipse2);
                                            }
                                        }

                                        if (resultWristRight == SkeletonBoneState.Matched)
                                        {
                                            var xPosRight = colorPointWristRight.X - ellipse3.Width / 2;
                                            var yPosRight = colorPointWristRight.Y - ellipse3.Height / 2;

                                            if (xPosRight >= 0 && xPosRight < this.canvas.ActualWidth &&
                                                yPosRight >= 0 && yPosRight < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse3, xPosRight);
                                                Canvas.SetTop(ellipse3, yPosRight);

                                                canvas.Children.Add(ellipse3);
                                            }

                                        }
                                        else
                                        {
                                            var xPosRight = colorPointWristRight.X - ellipse4.Width / 2;
                                            var yPosRight = colorPointWristRight.Y - ellipse4.Height / 2;

                                            if (xPosRight >= 0 && xPosRight < this.canvas.ActualWidth &&
                                                yPosRight >= 0 && yPosRight < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse4, xPosRight);
                                                Canvas.SetTop(ellipse4, yPosRight);

                                                canvas.Children.Add(ellipse4);
                                            }
                                        }
                                    }
                                }
                               
                                else
                                {
                                    if (_mode == CameraMode.Depth)
                                    {
                                        if (resultWristLeft == SkeletonBoneState.Matched)
                                        {
                                            var xPosLeft = depthPointWristLeft.X - ellipse1.Width / 2;
                                            var yPosLeft = depthPointWristLeft.Y - ellipse1.Height / 2;

                                            if (xPosLeft >= 0 && xPosLeft < this.canvasDepth.ActualWidth &&
                                                yPosLeft >= 0 && yPosLeft < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse1, xPosLeft);
                                                Canvas.SetTop(ellipse1, yPosLeft);

                                                canvasDepth.Children.Add(ellipse1);
                                            }

                                        }
                                        else
                                        {
                                            var xPosLeft = depthPointWristLeft.X - ellipse2.Width / 2;
                                            var yPosLeft = depthPointWristLeft.Y - ellipse2.Height / 2;

                                            if (xPosLeft >= 0 && xPosLeft < this.canvasDepth.ActualWidth &&
                                                yPosLeft >= 0 && yPosLeft < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse2, xPosLeft);
                                                Canvas.SetTop(ellipse2, yPosLeft);

                                                canvasDepth.Children.Add(ellipse2);
                                            }
                                        }

                                        if (resultWristRight == SkeletonBoneState.Matched)
                                        {
                                            var xPosRight = depthPointWristRight.X - ellipse3.Width / 2;
                                            var yPosRight = depthPointWristRight.Y - ellipse3.Height / 2;

                                            if (xPosRight >= 0 && xPosRight < this.canvasDepth.ActualWidth &&
                                                yPosRight >= 0 && yPosRight < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse3, xPosRight);
                                                Canvas.SetTop(ellipse3, yPosRight);

                                                canvasDepth.Children.Add(ellipse3);
                                            }

                                        }
                                        else
                                        {
                                            var xPosRight = depthPointWristRight.X - ellipse4.Width / 2;
                                            var yPosRight = depthPointWristRight.Y - ellipse4.Height / 2;

                                            if (xPosRight >= 0 && xPosRight < this.canvasDepth.ActualWidth &&
                                                yPosRight >= 0 && yPosRight < this.canvasDepth.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse4, xPosRight);
                                                Canvas.SetTop(ellipse4, yPosRight);

                                                canvasDepth.Children.Add(ellipse4);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (resultWristLeft == SkeletonBoneState.Matched)
                                        {
                                            var xPosLeft = colorPointWristLeft.X - ellipse1.Width / 2;
                                            var yPosLeft = colorPointWristLeft.Y - ellipse1.Height / 2;

                                            if (xPosLeft >= 0 && xPosLeft < this.canvas.ActualWidth &&
                                                yPosLeft >= 0 && yPosLeft < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse1, xPosLeft);
                                                Canvas.SetTop(ellipse1, yPosLeft);

                                                canvas.Children.Add(ellipse1);
                                            }

                                        }
                                        else
                                        {
                                            var xPosLeft = colorPointWristLeft.X - ellipse2.Width / 2;
                                            var yPosLeft = colorPointWristLeft.Y - ellipse2.Height / 2;

                                            if (xPosLeft >= 0 && xPosLeft < this.canvas.ActualWidth &&
                                                yPosLeft >= 0 && yPosLeft < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse2, xPosLeft);
                                                Canvas.SetTop(ellipse2, yPosLeft);

                                                canvas.Children.Add(ellipse2);
                                            }
                                        }

                                        if (resultWristRight == SkeletonBoneState.Matched)
                                        {
                                            var xPosRight = colorPointWristRight.X - ellipse3.Width / 2;
                                            var yPosRight = colorPointWristRight.Y - ellipse3.Height / 2;

                                            if (xPosRight >= 0 && xPosRight < this.canvas.ActualWidth &&
                                                yPosRight >= 0 && yPosRight < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse3, xPosRight);
                                                Canvas.SetTop(ellipse3, yPosRight);

                                                canvas.Children.Add(ellipse3);
                                            }

                                        }
                                        else
                                        {
                                            var xPosRight = colorPointWristRight.X - ellipse4.Width / 2;
                                            var yPosRight = colorPointWristRight.Y - ellipse4.Height / 2;

                                            if (xPosRight >= 0 && xPosRight < this.canvas.ActualWidth &&
                                                yPosRight >= 0 && yPosRight < this.canvas.ActualHeight)
                                            {
                                                Canvas.SetLeft(ellipse4, xPosRight);
                                                Canvas.SetTop(ellipse4, yPosRight);

                                                canvas.Children.Add(ellipse4);
                                            }
                                        }
                                    }
                                }
                               

                                

                            }
                            else
                            {
                                escape = false;
                            }
                        }
                    }
                }
            }

            if (depthFrame != null)
            depthFrame.Dispose();
            depthFrame = null;
            if (colorFrame != null)
            colorFrame.Dispose();
            colorFrame = null;
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                this.ButtonRecordMovement_Click(null, null);
            }

            else if(e.Key == Key.Escape)
            {
                escape = true;
            }
        }
    }

    enum CameraMode
    {
        Color,
        Depth,
        Infrared
    }
}
