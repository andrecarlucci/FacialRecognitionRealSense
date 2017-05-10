using OpenRealSense;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FaceLib;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Windows.Interop;
using System.IO;

namespace WpfApp {
    public partial class MainWindow : Window {

        private ImageProcessor _imageProcessor;

        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            Task.Run(async ()  =>  {
                try {
                    await Start();
                }
                catch(Exception ex) {
                    Debug.WriteLine("Exception: " + ex);
                }
            });
        }

        public async Task Start() {
            var context = Context.Create(11100);
            var num = context.GetDeviceCount();

            if (num == 0) {
                await ChangeUI(() => Title = "Camera not detected :(");
                return;
            }

            var device = context.GetDevice(0);
            await ChangeUI(() => Title = "Name: " + device.GetDeviceName());

            var width = 640;
            var height = 480;

            device.EnableStream(StreamType.color, width, height, FormatType.bgr8, 30);
            device.Start();

            //var cam = new Capture(1);
            
            while(true) {
                //var currentFrame = cam.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                device.WaitForFrames();
                var frame = device.GetFrameData(StreamType.color);

                var wb = PaintColorImage(frame.Bytes, width, height, PixelFormats.Bgr24);
                var bmp = BitmapFromWriteableBitmap(wb);


                var currentFrame = new Image<Bgr, Byte>(bmp);
                var gray = currentFrame.Convert<Gray, Byte>();
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


        //public async Task Start() {
        //    var context = Context.Create(11100);
        //    var num = context.GetDeviceCount();

        //    if (num == 0) {
        //        await ChangeUI(() => Title = "Camera not detected :(");
        //        return;
        //    }

        //    var device = context.GetDevice(0);
        //    await ChangeUI(() => Title = "Name: " + device.GetDeviceName());

        //    var width = 640;
        //    var height = 480;

        //    _imageProcessor = new ImageProcessor(device.GetDepthScale(), width, height);
        //    device.EnableStream(StreamType.depth, width, height, FormatType.z16, 30);
        //    device.Start();

        //    for (var i = 0; i < 30; i++) {
        //        device.WaitForFrames();
        //    }
        //    while (true) {
        //        for (var i = 0; i < 2; i++) {
        //            device.WaitForFrames();
        //        }
        //        var depthFrame = device.GetFrameData(StreamType.depth);
        //        await UpdateVideoAsync(depthFrame);
        //    }
        //}

        //private async Task UpdateVideoAsync(FrameData depthFrame) {
        //    var result = _imageProcessor.Process(depthFrame.Bytes);
        //    var wb = PaintDepthImage(result.Image, depthFrame.Width, depthFrame.Height, PixelFormats.Gray16);
        //    wb.Freeze();

        //    await Dispatcher.BeginInvoke(new Action(() => {
        //        Distance.Text = result.Text;
        //        Video.Source = wb;
        //    }));
        //}

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
    }
}
