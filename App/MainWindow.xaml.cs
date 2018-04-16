using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Interop;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using SharpMediator;
using System.Runtime.InteropServices;
using Intel.RealSense;

namespace App {

    public partial class MainWindow : Window {

        private FaceRepository _faceRepository;
        private FaceDetector _faceDetector;
        private FaceRecognizer _faceRecognizer;
        private FacePipeline _facePipeline;

        private MirrorService _mirrorService;
        //private MotionDetector _motionDetector;
        private bool _streamEnabled;

        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            _faceRepository = new FaceRepository("Faces");
            //_faceDetector = new FaceDetector("Assets\\haarcascade_frontalface_default.xml");
            _faceDetector = new FaceDetector("Assets\\haarcascade_frontalface_alt_tree.xml");
            _faceRecognizer = new FaceRecognizer();

            _facePipeline = new FacePipeline(_faceDetector, _faceRecognizer, _faceRepository);
            _facePipeline.Prepare();

            //_motionDetector = new MotionDetector();
            //_motionDetector.OnMovement += MotionDetected;

            _mirrorService = new MirrorService("http://localhost:8080/");
            _mirrorService.Start();

            Mediator.Default.Subscribe<string>(this, username => {
                ChangeUI(() => Detected.Text = username);
            });
        }

        private void MotionDetected(object sender, EventArgs e) {

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(() => {
                try {
                    Start();
                }
                catch (Exception ex) {
                    Debug.WriteLine("Exception: " + ex);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Start() {

            var context = new Context();
            var devices = context.QueryDevices();
            var num = devices.Count();
            
            if (num > 0) {
                ChangeUI(() => Title = "Name: " + devices.First().Info);
            }
            else {
                ChangeUI(() => Title = "Camera not detected :(");
                devices.Dispose();
                context.Dispose();
                return;
            }

            var width = 640;
            var height = 480;
            //var config = new Config();
            //config.EnableStream(Intel.RealSense.Stream.Color, width, height, Format.Bgr8, 15);
            //config.EnableStream(Intel.RealSense.Stream.Depth, width, height, Format.Z16, 15);

            var pipe = new Pipeline();
            pipe.Start();
            

            var tick = false; 

            while (true) {
                using (var frames = pipe.WaitForFrames()) {
                    tick = !tick;
                    
                    //using (var depth = frames.First(x => x.Profile.Stream == Intel.RealSense.Stream.Depth) as DepthFrame) {
                    //    var msg = $"{depth.GetDistance(depth.Width / 2, depth.Height / 2)} meters away";
                    //    ChangeUI(() => Title = msg);

                    //}
                    using (var frame = frames.First(x => x.Profile.Stream == Intel.RealSense.Stream.Color) as VideoFrame) {
                        //var mat = new Mat();
                        //mat.Create(frame.Width, frame.Height, DepthType.Cv8U, 3);
                        //mat.
                        //var color = new Mat(new System.Drawing.Size(width, height), DepthType.Cv8U, 3, frame.Data, 1920);
                        //var currentFrame = new Mat();
                        //CvInvoke.Flip(color, currentFrame, FlipType.Horizontal);

                        var pointer = frame.Data;
                        var bitsPerPixel = frame.BitsPerPixel;
                        var length = bitsPerPixel / 8 * frame.Width * frame.Height;
                        var bytes = new byte[length];
                        Marshal.Copy(pointer, bytes, 0, length);

                        var pre = new Image<Bgr, byte>(frame.Width, frame.Height) {
                            Bytes = bytes
                        };
                        var toFlip = pre.Resize(width, height, Inter.Cubic);
                        
                        var currentFrame = toFlip.Flip(FlipType.Horizontal);
                        pre.Dispose();
                        frame.Dispose();
                        toFlip.Dispose();
                        
                        var result = _facePipeline.ProccessFrame(currentFrame);
                        
                        if (_streamEnabled) {
                            if (result.Status != FaceRecognizer.FaceRecognitionStatus.None) {
                                DrawFaceSquare(currentFrame, result.FacePosition);
                                DrawName(currentFrame, result.FacePosition, result.Label);
                            }

                            var bitmapSource = ConvertToBitmapSource(currentFrame.Bitmap);
                            bitmapSource.Freeze();
                            ChangeUI(() => Video.Source = bitmapSource);
                        }
                        
                        currentFrame.Dispose();
                    }
                }
            }

            //while (true) {
            //    device.WaitForFrames();
            //    var frame = device.GetFrameData(StreamType.Color);

            //    var currentFrame = new Image<Bgr, byte>(frame.Width, frame.Height) {
            //        Bytes = frame.Bytes
            //    };
            //    currentFrame = currentFrame.Flip(FLIP.HORIZONTAL);

            //    _motionDetector.ComputeFrame(currentFrame);

            //    string label = null;
            //    if (_recognizedCalled) {
            //        _recognizedCalled = false;
            //        label = _faceName;
            //    }
            //    var face = _faceRecognizer.DetectFirstFace(currentFrame, label);

            //    if (face != null) {
            //        if (_streamEnabled) {
            //            DrawFaceSquare(currentFrame, face);
            //            DrawName(currentFrame, face);
            //        }
            //        _mirrorService.SetNewLabel(face.Label);
            //    }
            //    else {
            //        _mirrorService.SetNewLabel("");
            //    }

            //    if (_streamEnabled) {
            //        var bitmapSource = ConvertToBitmapSource(currentFrame.Bitmap);
            //        bitmapSource.Freeze();
            //        ChangeUI(() => Video.Source = bitmapSource);
            //    }
            //    currentFrame.Dispose();
            //}
        }

        private void DrawName(Image<Bgr, byte> currentFrame, Rectangle facePosition, string label) {
            CvInvoke.PutText(currentFrame, 
                             label,
                             new System.Drawing.Point(facePosition.X - 2, facePosition.Y - 2),
                             FontFace.HersheyTriplex,
                             1,
                             new Bgr(0,255,0).MCvScalar);
        }

        private static void DrawFaceSquare(Image<Bgr, byte> currentFrame, Rectangle facePosition) {
            currentFrame.Draw(facePosition, new Bgr(System.Drawing.Color.Red), 2);
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ConvertToBitmapSource(Bitmap bmp) {
            var handle = bmp.GetHbitmap();
            try {
                return Imaging.CreateBitmapSourceFromHBitmap(
                    handle,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        private void ChangeUI(Action action) {
            Dispatcher.BeginInvoke(action);
        }

        private void Register_Click(object sender, RoutedEventArgs e) {
            if(String.IsNullOrEmpty(FaceName.Text)) {
                return;
            }
            _facePipeline.RegisterNextFace(FaceName.Text);
        }

        private void StreamEnabled_Click(object sender, RoutedEventArgs e) {
            _streamEnabled = !_streamEnabled;
            if (_streamEnabled) {
                StreamEnabledText.Text = "Disable Stream";
            }
            else {
                StreamEnabledText.Text = "Enable Stream";
            }
        }
    }
}
