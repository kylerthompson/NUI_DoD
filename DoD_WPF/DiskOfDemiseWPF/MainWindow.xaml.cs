using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SS = System.Speech;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using DiskOfDemiseWPF.Gesture;
using DiskOfDemiseWPF.Gesture.Parts;
using DiskOfDemiseWPF.Gesture.Parts.SwipeLeft;
using DiskOfDemiseWPF.Gesture.Parts.SwipeRight;
using DiskOfDemiseWPF.EventArguments;
using System.Threading;
using MS = Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using System.Windows.Threading;
using System.Collections;

namespace DiskOfDemiseWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// global variables
        /// </summary>
        private Storyboard myStoryboard;
        private DiskOfDemiseGame d1;
        private double angle = 0;
        private Boolean inSpeech = false;
        private Window1 window1;

        /// <summary>
        /// kinect global variables
        /// </summary>
        KinectSensor sensor = null;
        GestureController gestureController = null;
        String mostRecentGesture;
        
        /// <summary>
        /// color stream global variables
        /// </summary>
        private byte[] colorPixels;          // image source for image control
        private WriteableBitmap bitmap;    // bitmap for color stream image

        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        private Pen inferredPen = new Pen(Brushes.Red, 4);
        private Pen trackedPen = new Pen(Brushes.Green, 4);
        
        ///<summary>
        /// voice recognition global variables
        /// </summary>
        private SS.Recognition.RecognizerInfo priRI;
        private KinectAudioSource audioSource;
        private SpeechRecognitionEngine sre;
        //private Thread audioThread;
        
        private int armConfirm = 0;

        /// <summary>
        /// method for actions taken when a gesture is recognized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WhenGestureRecognized(object sender, GestureEventArgs e)
        {
            //audioThread = new Thread(startAudioListening);
            //audioThread.Start();
            //initializeSpeech();
            //startAudioListening();

            /// disable gesture service
            // GestureServiceOff();
            /// output gesture type to console
            mostRecentGesture = e.gestureType;
            System.Console.Write(e.gestureType + "\n");
            /// MessageBox.Show(e.gestureType);
            /// 
            if (!inSpeech)
            {
                /// spin wheel
                if (e.gestureType == "swipe_left" || e.gestureType == "swipe_right" ||
                    e.gestureType == "kick_left" || e.gestureType == "kick_right")
                {
                    double randomDouble; ;
                    Random random = new Random();
                    randomDouble = 180.00 + random.NextDouble() * 720;
                    this.findBodyPart(randomDouble);
                    if (e.gestureType == "swipe_left" || e.gestureType == "kick_left")
                    {
                        randomDouble *= -1;
                    }
                    this.spinWheel(randomDouble);
                    initializeSpeech();
                    startAudioListening();
                }
            }
            else
            {
                if (e.gestureType == "raise_hand_right")
                {
                    armConfirm = 1;
                }
            }

        }

        private void findBodyPart(Double number)
        {
            while (number > 360)
            {
                number -= 360;
            }
            if (number <= 90)
            {
                d1.setBodyPart("RightArm");
            }
            else if (number <= 180)
            {
                d1.setBodyPart("RightLeg");
            }
            else if (number <= 270)
            {
                d1.setBodyPart("LeftArm");
            }
            else if (number <= 360)
            {
                d1.setBodyPart("LeftLeg");
            }
        }
        
        /// <summary>
        /// actions taken when a new skeleton frame is ready
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skeleton in skeletons)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            gestureController.UpdateGestures(skeleton);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// draws rgb stream and skeleton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            KinectSensor sensor = sender as KinectSensor;
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    colorFrame.CopyPixelDataTo(this.colorPixels);
                    this.bitmap.WritePixels(
                        new Int32Rect(0, 0, this.bitmap.PixelWidth, this.bitmap.PixelHeight),
                        this.colorPixels,
                        this.bitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                dc.DrawImage(this.bitmap, new Rect(0.0, 0.0, this.bitmap.PixelWidth, this.bitmap.PixelHeight));
                Skeleton[] skeletons = new Skeleton[0];
                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame != null)
                    {
                        skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                        skeletonFrame.CopySkeletonDataTo(skeletons);
                    }
                }
                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skeleton in skeletons)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawSkeleton(skeleton, dc);
                        }
                        else if (skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(Brushes.Blue, null, this.SkeletonPointToScreen(skeleton.Position), 10, 10);
                        }
                    }
                }
            }
        }

        private Point SkeletonPointToScreen(SkeletonPoint skeletonPoint)
        {
            ColorImagePoint colorPoint = this.sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);
            return new Point(colorPoint.X, colorPoint.Y);
        }

        private void DrawSkeleton(Skeleton skeleton, DrawingContext dc)
        {
            //Torso
            this.DrawBone(skeleton, dc, JointType.Head, JointType.ShoulderCenter, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.ShoulderCenter, JointType.ShoulderLeft, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.ShoulderCenter, JointType.ShoulderRight, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.ShoulderCenter, JointType.Spine, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.Spine, JointType.HipCenter, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.HipCenter, JointType.HipLeft, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.HipCenter, JointType.HipRight, this.trackedPen);
            //Left Arm
            this.DrawBone(skeleton, dc, JointType.ShoulderLeft, JointType.ElbowLeft, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.ElbowLeft, JointType.WristLeft, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.WristLeft, JointType.HandLeft, this.trackedPen);
            //Right Arm
            this.DrawBone(skeleton, dc, JointType.ShoulderRight, JointType.ElbowRight, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.ElbowRight, JointType.WristRight, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.WristRight, JointType.HandRight, this.trackedPen);
            //Left Leg
            this.DrawBone(skeleton, dc, JointType.HipLeft, JointType.KneeLeft, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.KneeLeft, JointType.AnkleLeft, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.AnkleLeft, JointType.FootLeft, this.trackedPen);
            //Right Leg
            this.DrawBone(skeleton, dc, JointType.HipRight, JointType.KneeRight, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.KneeRight, JointType.AnkleRight, this.trackedPen);
            this.DrawBone(skeleton, dc, JointType.AnkleRight, JointType.FootRight, this.trackedPen);
            //Joints

            foreach (Joint joint in skeleton.Joints)
            {
                Brush brush = null;
                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    brush = Brushes.Green;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    brush = Brushes.Yellow;
                }

                if (brush != null)
                {
                    dc.DrawEllipse(brush, null, this.SkeletonPointToScreen(joint.Position), 10, 10);
                }
            }

        }

        private void DrawBone(Skeleton skeleton, DrawingContext dc, JointType jointType1, JointType jointType2, Pen drawPen)
        {
            Joint startJoint = skeleton.Joints[jointType1];
            Joint endJoint = skeleton.Joints[jointType2];

            // return if either joint isn't being tracked
            if (startJoint.TrackingState == JointTrackingState.NotTracked || endJoint.TrackingState == JointTrackingState.NotTracked)
                return;

            // return if both joints are being inferred
            if (startJoint.TrackingState == JointTrackingState.Inferred && endJoint.TrackingState == JointTrackingState.Inferred)
                return;

            if (!(startJoint.TrackingState == JointTrackingState.Tracked && endJoint.TrackingState == JointTrackingState.Tracked))
                drawPen = this.inferredPen;

            dc.DrawLine(drawPen, this.SkeletonPointToScreen(startJoint.Position), this.SkeletonPointToScreen(endJoint.Position));
        }

        /// <summary>
        /// initialize the kinect sensor
        /// </summary>
        private void initKinect()
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }
            if (this.sensor != null)
            {
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.bitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                this.sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensorAllFramesReady);
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.sensor.SkeletonStream.Enable();
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                this.sensor.Start();
                ///
                System.Console.Write("kinect initialized\n");
                ///
            }
        }

        private void initializeKSpeech()
        {

            KinectAudioSource source = sensor.AudioSource;

            source.EchoCancellationMode = EchoCancellationMode.None;
            source.AutomaticGainControlEnabled = false;
            SS.Recognition.RecognizerInfo ri = SS.Recognition.SpeechRecognitionEngine.InstalledRecognizers().FirstOrDefault();
        }

        private void initializeSpeech()
        {
            inSpeech = true;
            System.Console.Write("Initialize speech");
            SS.Recognition.RecognizerInfo ri = SS.Recognition.SpeechRecognitionEngine.InstalledRecognizers().FirstOrDefault();
            sre = new SpeechRecognitionEngine(ri.Id);

            Choices letters = new Choices(new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" });

            GrammarBuilder gb = new GrammarBuilder("Guess");
            gb.Append(letters);

            Grammar grammar = new Grammar(gb);
            grammar.Name = "DisK of Demise";        

            sre.LoadGrammarCompleted += new EventHandler<LoadGrammarCompletedEventArgs>(LoadGrammarCompleted);
            sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);
            sre.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(SpeechRejected);

            //sre.SetInputToDefaultAudioDevice();
            sre.LoadGrammarAsync(grammar);
        }


        public void StopKSpeech()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                sre.RecognizeAsyncCancel();
                sre.RecognizeAsyncStop();

               // audioSource.SoundSourceAngleChanged -= this.SoundSourceChanged;
               // sre.SpeechRecognized -= sre.SpeechRecognized;
               // sre.SpeechRecognitionRejected -= this.SreSpeechRecognitionRejected;
            }
        }


        private void startAudioListening()
        {
            audioSource = sensor.AudioSource;

            Stream aStream = audioSource.Start();
            sre.SetInputToDefaultAudioDevice();
           // sre.LoadGrammarAsync(grammar);
            sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void initGestureService()
        {
            /// initialize gesture controller
            gestureController = new GestureController();
            gestureController.GestureRecognized += this.WhenGestureRecognized;
            /// initialize and add swipe right to controller
            GestureSegment[] swipeRight = new GestureSegment[3];
            swipeRight[0] = new SwipeRightSegment1();
            swipeRight[1] = new SwipeRightSegment2();
            swipeRight[2] = new SwipeRightSegment3();
            gestureController.AddGesture("swipe_right", swipeRight);
            /// initialize and add swipe left to controller
            GestureSegment[] swipeLeft = new GestureSegment[3];
            swipeLeft[0] = new SwipeLeftSegment1();
            swipeLeft[1] = new SwipeLeftSegment2();
            swipeLeft[2] = new SwipeLeftSegment3();
            gestureController.AddGesture("swipe_left", swipeLeft);
            /// initialize and add kick right gesture to controller
            GestureSegment[] kickRight = new GestureSegment[3];
            kickRight[0] = new KickRightSegment();
            kickRight[1] = new KickRightSegment();
            kickRight[2] = new KickRightSegment();
            gestureController.AddGesture("kick_right", kickRight);
            /// initialize and add kick left gesture to controller
            GestureSegment[] kickLeft = new GestureSegment[3];
            kickLeft[0] = new KickLeftSegment();
            kickLeft[1] = new KickLeftSegment();
            kickLeft[2] = new KickLeftSegment();
            gestureController.AddGesture("kick_left", kickLeft);
            /// initialize and add kick left gesture to controller
            GestureSegment[] raiseHandRight = new GestureSegment[3];
            raiseHandRight[0] = new RaiseHandRightSegment();
            raiseHandRight[1] = new RaiseHandRightSegment();
            raiseHandRight[2] = new RaiseHandRightSegment();
            gestureController.AddGesture("raise_hand_right",raiseHandRight);
            ///
            System.Console.Write("gesture service initialized\n");
            ///
        }

        private void GestureServiceOff()
        {
            if (gestureController != null)
            {
                gestureController.GestureRecognized -= this.WhenGestureRecognized;
            }
        }

        private void GestureServiceOn()
        {
            if (gestureController != null)
            {
                gestureController.GestureRecognized += this.WhenGestureRecognized;
            }
        }

        static void LoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        {
            Console.WriteLine(e.Grammar.Name + " successfully loaded");
        }

        String guessedLetters = "Guessed letters: ";

        void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("Speech recognized: " + e.Result.Text);

            char letterUserGuessed = e.Result.Text[6];
            confirmLetter.Text = "Did you say: " + letterUserGuessed + "?";
            Console.WriteLine("Did you say: " + letterUserGuessed + "?");
            window1.showInstruction("Did you say: "+letterUserGuessed+"?");
            //GestureServiceOn();
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                if (armConfirm == 1)
                {
                    d1.checkLetterInPhrase(letterUserGuessed);
                    Console.WriteLine("You guessed the letter: " + letterUserGuessed);
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        try
                        {
                            confirmLetter.Text = "You guessed the letter: " + letterUserGuessed;
                        }
                        catch
                        {
                            confirmLetter.Text = "You guessed the letter: " + letterUserGuessed;
                        }
                    }));
                    StopKSpeech();
                    //audioThread.Abort();
                    reset();
                    guessedLetters += " " + letterUserGuessed;
                }
                else
                {
                     Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        try
                        {
                            confirmLetter.Text = "Please try again.";
                        }
                        catch
                        {

                        }
                    }));
                    Console.WriteLine("Please try again.");
                }
                armConfirm = 0;
                //GestureServiceOff();
                //confirmLetter.Text = "";
            });
        }

        static void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("Speech input failed. Please Repeat.");
        }

        public MainWindow()
        {
            InitializeComponent();
            window1 = new Window1();
            window1.Show();
            /// new (single player) game
            d1 = new DiskOfDemiseGame();
            /// kinect initializations
            initKinect();
            initializeKSpeech();
            initGestureService();
            /// color stream things
            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(this.drawingGroup);
            kinectColorImage.Source = this.imageSource;

            /// ???
            InitializeComponent();
            reset();
        }

        public void reset()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                try
                {
                    phraseLabel.Text = d1.displayPhrase();
                    nameLabel.Text = " Player " + d1.displayName();
                    guessedLetter.Text = guessedLetters;
                    clearBodyParts();
                    colorBodyParts(d1.displayName());
                    displayBodyParts();
                }
                catch
                {
                    phraseLabel.Text = d1.displayPhrase();
                    nameLabel.Text = " Player " + d1.displayName();
                    guessedLetter.Text = guessedLetters;
                }
            }));
            ///clearBodyParts();
            ///colorBodyParts(d1.displayName());
            ///displayBodyParts();
            //GestureServiceOn();
            inSpeech = false;
        }

        public void spinWheel(double addedAngle)
        {
            wheelPicture.RenderTransform = new RotateTransform(angle);
            double currentAngle = ((RotateTransform)wheelPicture.RenderTransform).Angle;
            int duration = Math.Abs((int)addedAngle / 100);

            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = currentAngle;
            myDoubleAnimation.To = currentAngle + addedAngle;
            myDoubleAnimation.DecelerationRatio = 0.5;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));

            angle = currentAngle + addedAngle;

            myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);

            ((RotateTransform)wheelPicture.RenderTransform).BeginAnimation(RotateTransform.AngleProperty, myDoubleAnimation);
        }

        public SS.Recognition.RecognizerInfo getRI()
        {
            return priRI;
        }

        private void colorBodyParts(String color)
        {
            SolidColorBrush bodyColor = Brushes.Black;
            if (color.Equals("Red"))
            {
                bodyColor = Brushes.OrangeRed;
            }
            else if (color.Equals("Yellow"))
            {
                bodyColor = Brushes.LightGoldenrodYellow;
            }
            else if (color.Equals("Green"))
            {
                bodyColor = Brushes.LightGreen;
            }
            else if (color.Equals("Blue"))
            {
                bodyColor = Brushes.CornflowerBlue;
            }
            headShape.Fill = bodyColor;
            rightArmShape.Fill = bodyColor;
            leftArmShape.Fill = bodyColor;
            rightLegShape.Fill = bodyColor;
            leftLegShape.Fill = bodyColor;
            bodyShape.Fill = bodyColor;
        }

        public void clearBodyParts()
        {
            headShape.Opacity = 0;
            rightArmShape.Opacity = 0;
            leftArmShape.Opacity = 0;
            rightLegShape.Opacity = 0;
            leftLegShape.Opacity = 0;
        }

        private void displayBodyParts()
        {
            ArrayList temp = d1.returnBodyParts();
            for (int i = 0; i < temp.Count; i++)
            {
                if (temp[i].Equals("Head"))
                {
                    headShape.Opacity = 100;
                }
                if (temp[i].Equals("RightArm"))
                {
                    rightArmShape.Opacity = 100;
                }
                if (temp[i].Equals("LeftArm"))
                {
                    leftArmShape.Opacity = 100;
                }
                if (temp[i].Equals("RightLeg"))
                {
                    rightLegShape.Opacity = 100;
                }
                if (temp[i].Equals("LeftLeg"))
                {
                    leftLegShape.Opacity = 100;
                }
            }
        }
    }
}
