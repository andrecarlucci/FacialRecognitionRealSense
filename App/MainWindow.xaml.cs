using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenRealSense;
using System.Drawing;
using System.IO;
using System.Windows.Interop;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using SharpMediator;
using System.Runtime.InteropServices;

namespace App {
   
    public partial class MainWindow : Window {

        private MCvFont _font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        private FaceRecognizer _faceRecognizer;
        private MirrorService _mirrorService;
        private bool _recognizedCalled;
        private string _faceName;
        private bool _streamEnabled;

        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            var faceRep = new FaceRepository("Faces");
            _faceRecognizer = new FaceRecognizer("Assets\\haarcascade_frontalface_default.xml", faceRep);

            _mirrorService = new MirrorService("http://localhost:8080/");
            _mirrorService.Start();

            Mediator.Default.Subscribe<string>(this, username => {
                ChangeUI(() => Detected.Text = username);
            });
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
            var context = Context.Create();
            var num = context.GetDeviceCount();

            if (num == 0) {
                ChangeUI(() => Title = "Camera not detected :(");
                return;
            }

            var device = context.GetDevice(0);
            ChangeUI(() => Title = "Name: " + device.GetDeviceName());

            var width = 640;
            var height = 480;

            device.EnableStream(StreamType.Color, width, height, FormatType.bgr8, 30);
            device.Start();
            
            while (true) {
                device.WaitForFrames();
                var frame = device.GetFrameData(StreamType.Color);

                var currentFrame = new Image<Bgr, byte>(frame.Width, frame.Height) {
                    Bytes = frame.Bytes
                };
                currentFrame = currentFrame.Flip(FLIP.HORIZONTAL);

                string label = null;
                if(_recognizedCalled) {
                    _recognizedCalled = false;
                    label = _faceName;
                }
                var face = _faceRecognizer.DetectFirstFace(currentFrame, label);

                if (face != null) {
                    if(_streamEnabled) {
                        DrawFaceSquare(currentFrame, face);
                        DrawName(currentFrame, face);
                    }
                    _mirrorService.SetNewLabel(face.Label);
                }
                else {
                    _mirrorService.SetNewLabel("");
                }

                if (_streamEnabled) {
                    var bitmapSource = ConvertToBitmapSource(currentFrame.Bitmap);
                    bitmapSource.Freeze();
                    ChangeUI(() => Video.Source = bitmapSource);
                }
                currentFrame.Dispose();
            }
        }

        private void DrawName(Image<Bgr, byte> currentFrame, DetectedFace face) {
            currentFrame.Draw(face.Label,
                ref _font,
                new System.Drawing.Point(face.FaceInfo.rect.X - 2, face.FaceInfo.rect.Y - 2),
                new Bgr(System.Drawing.Color.LightGreen));
        }

        private static void DrawFaceSquare(Image<Bgr, byte> currentFrame, DetectedFace face) {
            currentFrame.Draw(face.FaceInfo.rect, new Bgr(System.Drawing.Color.Red), 2);
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
            _recognizedCalled = true;
            _faceName = FaceName.Text;
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
