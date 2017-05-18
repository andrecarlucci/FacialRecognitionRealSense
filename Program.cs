using FaceLib;
using OpenRealSense;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RealSenseMirror {
    class Program {

        private static ImageProcessor _imageProcessor;
        static void Main(string[] args) {
            Start();
        }
        public static void Start() {
            var context = Context.Create(11100);
            var num = context.GetDeviceCount();

            if (num == 0) {
                Console.WriteLine("Camera not detected :(");
                return;
            }

            var device = context.GetDevice(0);
            Console.WriteLine("Name: " + device.GetDeviceName());

            var width = 640;
            var height = 480;

            _imageProcessor = new ImageProcessor(device.GetDepthScale(), width, height);
            device.EnableStream(StreamType.depth, width, height, FormatType.z16, 30);
            device.Start();

            for (var i = 0; i < 30; i++) {
                device.WaitForFrames();
            }
            while (true) {
                for (var i = 0; i < 10; i++) {
                    device.WaitForFrames();
                }
                var depthFrame = device.GetFrameData(StreamType.depth);
                UpdateVideo(depthFrame);
            }
        }

        private static string _last;

        private static void UpdateVideo(FrameData depthFrame) {
            var result = _imageProcessor.Process(depthFrame.Bytes);
            if(_last != result.Face.Person.Name) {
                Console.WriteLine($"{result.Face.Person.Name} ({result.Face.K1})");
                _last = result.Face.Person.Name;
            }
        }

        public void Post() {
            HttpClient client;
        }
    }
}