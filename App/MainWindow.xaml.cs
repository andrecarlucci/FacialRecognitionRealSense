using App.MediatorMessages;
using App.OCR;
using App.Selfie;
using App.Twitter;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Intel.RealSense;
using Serilog;
using SharpMediator;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace App {

    public partial class MainWindow : Window {

        private static int _width = 640;
        private static int _height = 480;
        public static bool UseRealSense = true;
        public static bool UseOcr = true;
        public static bool UseMotionDetection = true;
        public static int CameraIndex = 0;
        public static string WordForPicture = "SELFIE";

        private FaceRepository _faceRepository;
        private FaceDetector _faceDetector;
        private FaceRecognizer _faceRecognizer;
        private FacePipeline _facePipeline;
        private VideoCapture _capture;
        
        private MirrorStateMachine _mirrorUserStateMachine;
        private FrameAggregator _frameAggregator;

        private bool _streamEnabled;
        private int _forceFaces = -1;

        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            _faceRepository = new FaceRepository("Faces");
            _faceDetector = new FaceDetector("Assets\\haarcascade_frontalface_alt_tree.xml");
            _faceRecognizer = new FaceRecognizer();

            _facePipeline = new FacePipeline(_faceDetector, _faceRecognizer, _faceRepository);
            _facePipeline.Prepare();

            _frameAggregator = new FrameAggregator();

            var mirrorClient = new MirrorClient();
            _mirrorUserStateMachine = new MirrorStateMachine(mirrorClient);

            Mediator.Default.Subscribe<MirrorUserChanged>(this, msg => {
                ChangeUI(() => Detected.Text = msg.Username);
            });
            
            Task.Factory.StartNew(async () => {
                try {
                    if (UseRealSense) {
                        await StartRealSense();
                    }
                    else {
                        await StartCamera();
                    }
                }
                catch (Exception ex) {
                    Log.Logger.Error(ex, "On StartCamera");
                    if (ex.InnerException != null) {
                        Log.Logger.Error(ex.InnerException, "Inner exception");
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public async Task StartCamera() {
            _capture = new VideoCapture(CameraIndex);
            while (true) {
                var frame = _capture.QueryFrame();
                await ProcessFrame(frame.ToImage<Bgr, Byte>());
            }
        }

        public async Task StartRealSense() {
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
            
            var pipe = new Pipeline();
            pipe.Start();

            var tick = false;
            for (int i = 0; i < 30; i++) {
                var frame = pipe.WaitForFrames();
                frame.Dispose();
            }

            while (true) {
                using (var frames = pipe.WaitForFrames()) {
                    tick = !tick;

                    //using (var depth = frames.First(x => x.Profile.Stream == Intel.RealSense.Stream.Depth) as DepthFrame) {
                    //    var msg = $"{depth.GetDistance(depth.Width / 2, depth.Height / 2)} meters away";
                    //    ChangeUI(() => Title = msg);

                    //}
                    using (var frame = frames.First(x => x.Profile.Stream == Intel.RealSense.Stream.Color) as VideoFrame) {
                        var pointer = frame.Data;
                        var bitsPerPixel = frame.BitsPerPixel;
                        var length = bitsPerPixel / 8 * frame.Width * frame.Height;
                        var bytes = new byte[length];
                        Marshal.Copy(pointer, bytes, 0, length);

                        var pre = new Image<Bgr, byte>(frame.Width, frame.Height) {
                            Bytes = bytes
                        };
                        await ProcessFrame(pre);
                    }
                }
            }
        }

        private async Task ProcessFrame(Image<Bgr, byte> frame) {
            Image<Bgr, byte> resized = null;
            Image<Bgr, byte> currentFrame = null;
            try {
                resized = frame.Resize(_width, _height, Inter.Cubic);
                currentFrame = resized.Flip(FlipType.Horizontal);
                
                var result = _facePipeline.ProccessFrame(currentFrame);
                
                if (_streamEnabled) {
                    UpdateScreen(currentFrame, result);
                }

                var aggregated = _frameAggregator.ProcessEvent(result);
                if (aggregated == null) {
                    return;
                }
                await _mirrorUserStateMachine.ProcessEvent(aggregated);
            }
            catch (Exception ex) {
                Log.Logger.Error(ex, "On Proccess Frame");
            }
            finally {
                frame.Dispose();
                currentFrame?.Dispose();
                resized?.Dispose();
            }
        }

        private void UpdateScreen(Image<Bgr, byte> currentFrame, PipelineResult result) {
            foreach(var rec in result.FacePositions) {
                DrawFaceSquare(currentFrame, rec);
            }
            if(result.Status == FaceRecognitionStatus.IdentifiedUser) {
                DrawName(currentFrame, result.FacePositions[0], result.FirstFaceLabel);
            }
            var bitmapSource = ConvertToBitmapSource(currentFrame.Bitmap);
            bitmapSource.Freeze();
            ChangeUI(() => Video.Source = bitmapSource);
        }

        private void DrawName(Image<Bgr, byte> currentFrame, Rectangle facePosition, string label) {
            CvInvoke.PutText(currentFrame,
                             label,
                             new System.Drawing.Point(facePosition.X - 2, facePosition.Y - 2),
                             FontFace.HersheyTriplex,
                             1,
                             new Bgr(0, 255, 0).MCvScalar);
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
            if (String.IsNullOrEmpty(FaceName.Text)) {
                return;
            }
            _facePipeline.RegisterNextFace(FaceName.Text);
        }

        private void ForceFacesBtn_Click(object sender, RoutedEventArgs e) {
            if(Int32.TryParse(ForceFaces.Text, out int forceFaces)) {
                _forceFaces = forceFaces;
            }
            else {
                _forceFaces = -1;
            }
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
