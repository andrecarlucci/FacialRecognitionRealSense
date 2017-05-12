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

namespace App {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private MCvFont _font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        private FaceRecognizer _faceRecognizer;
        private bool _recognizedCalled;
        private string _faceName;

        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            var faceRep = new FaceRepository();
            _faceRecognizer = new FaceRecognizer("Assets\\haarcascade_frontalface_default.xml", faceRep);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            Task.Run(async () => {
                try {
                    await Start();
                }
                catch (Exception ex) {
                    Debug.WriteLine("Exception: " + ex);
                }
            });
        }

        public async Task Start() {
            var context = Context.Create();
            var num = context.GetDeviceCount();

            if (num == 0) {
                await ChangeUI(() => Title = "Camera not detected :(");
                return;
            }

            var device = context.GetDevice(0);
            await ChangeUI(() => Title = "Name: " + device.GetDeviceName());

            var width = 640;
            var height = 480;

            device.EnableStream(StreamType.Color, width, height, FormatType.bgr8, 30);
            device.Start();
            
            while (true) {
                device.WaitForFrames();
                var frame = device.GetFrameData(StreamType.Color);

                var wb = PaintColorImage(frame.Bytes, width, height, PixelFormats.Bgr24);
                var bmp = BitmapFromWriteableBitmap(wb);

                var currentFrame = new Image<Bgr, Byte>(bmp);
                string label = null;
                if(_recognizedCalled) {
                    _recognizedCalled = false;
                    label = _faceName;
                }
                var face = _faceRecognizer.DetectFirstFace(currentFrame, label);

                if(face != null) {
                    //draw the face detected in the 0th (gray) channel with blue color
                    currentFrame.Draw(face.FaceInfo.rect, new Bgr(System.Drawing.Color.Red), 2);
                    currentFrame.Draw(face.Label,
                                        ref _font,
                                        new System.Drawing.Point(face.FaceInfo.rect.X - 2, face.FaceInfo.rect.Y - 2),
                                        new Bgr(System.Drawing.Color.LightGreen));
                }
                
                var bitmapSource = ConvertToBitmapSource(currentFrame.Bitmap);
                bitmapSource.Freeze();
                await ChangeUI(() => Video.Source = bitmapSource);
            }

        }

        public BitmapSource ConvertToBitmapSource(Bitmap bitmap) {
            return Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );
        }

        private Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp) {
            using (var outStream = new MemoryStream()) {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(writeBmp));
                enc.Save(outStream);
                return new Bitmap(outStream);
            }
        }

        private WriteableBitmap PaintColorImage(byte[] bytes, int width, int height, PixelFormat format) {
            var stride = width * format.BitsPerPixel / 8;
            var wb = new WriteableBitmap(width, height, 96, 96, format, null);
            wb.WritePixels(new Int32Rect(0, 0, width, height), bytes, stride, 0);
            return wb;
        }

        private WriteableBitmap PaintDepthImage(byte[] bytes, int width, int height, PixelFormat format) {
            var depthStride = width * format.BitsPerPixel / 8;
            var wb = new WriteableBitmap(width, height, 96, 96, format, null);
            wb.WritePixels(new Int32Rect(0, 0, width, height), bytes, depthStride, 0);
            return wb;
        }

        private async Task ChangeUI(Action action) {
            await Dispatcher.InvokeAsync(action);
        }

        private void Register_Click(object sender, RoutedEventArgs e) {
            _recognizedCalled = true;
            _faceName = FaceName.Text;
        }
    }
}
