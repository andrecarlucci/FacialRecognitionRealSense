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

        public Rectangle DetectFirstFace(Image<Bgr, Byte> frame) {
            using (var grayFrame = frame.Convert<Gray, Byte>()) {
                var facesDetected = _cascadeClassifier.DetectMultiScale(grayFrame, 1.1, 10, Size.Empty);

                if (!facesDetected.Any()) {
                    return Rectangle.Empty;
                }

                return facesDetected[0];
            }
        }

            //using (var faceImage = frame.Copy(face)
            //                            .Convert<Gray, byte>()
            //                            .Resize(100, 100, Inter.Cubic)) {

            //    faceImage._EqualizeHist();

            //    if (!String.IsNullOrEmpty(newLabel)) {
            //        _recognizerProvider.AddNewLabel(newLabel, faceImage.Mat);
            //        _rep.Save(newLabel, faceImage);
            //        result.Label = newLabel;
            //        return result;
            //    }

            //    if (_recognizerProvider.HasConfiguredFaces()) {
            //        var recognizer = _recognizerProvider.GetRecognizer();
            //        var predictionResult = recognizer.Predict(faceImage);
            //        result.Label = predictionResult.Label.ToString();
            //    }
            //    else {
            //        result.Label = UnknownLabel;
            //    }
            //}
            //return result;
        //}
    }
}