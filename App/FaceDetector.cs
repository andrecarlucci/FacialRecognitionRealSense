using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Linq;

namespace App {
    public class FaceDetector {
        
        private readonly CascadeClassifier _cascadeClassifier;

        public FaceDetector(string haarcascade) {
            _cascadeClassifier = new CascadeClassifier(haarcascade);
        }

        public Rectangle[] DetectFaces(Image<Bgr, Byte> frame) {
            using (var grayFrame = frame.Convert<Gray, Byte>()) {
                var facesDetected = _cascadeClassifier.DetectMultiScale(grayFrame, 1.1, 10, Size.Empty);

                if (!facesDetected.Any()) {
                    return new Rectangle[0];
                }

                return facesDetected;
            }
        }
    }
}