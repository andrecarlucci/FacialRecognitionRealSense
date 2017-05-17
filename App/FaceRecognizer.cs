using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Linq;

namespace App {
    public class FaceRecognizer {

        public const string UnknownLabel = "unknown";

        private HaarCascade _face;
        private FaceRepository _rep;
        private RecognizerProvider _recognizerProvider;

        public FaceRecognizer(string haarcascade, FaceRepository faceLabelsRepository) {
            _face = new HaarCascade(haarcascade);
            _recognizerProvider = new RecognizerProvider();
            _rep = faceLabelsRepository;
            var regs = faceLabelsRepository.List();
            foreach (var reg in regs) {
                _recognizerProvider.AddNewLabel(reg.Label, new Image<Gray, byte>(reg.ImagePath));
            }
        }

        public DetectedFace DetectFirstFace(Image<Bgr, Byte> frame, string newLabel = null) {
            var result = new DetectedFace();
            var gray = frame.Convert<Gray, Byte>();
            var facesDetected = gray.DetectHaarCascade(
                    _face,
                    1.2,
                    10,
                    HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                    new Size(20, 20));
            gray.Dispose();

            if (!facesDetected[0].Any()) {
                return null;
            }

            MCvAvgComp faceInfo = facesDetected[0].First();
            result.FaceInfo = faceInfo;

            var faceImage = frame.Copy(faceInfo.rect)
                                .Convert<Gray, byte>()
                                .Resize(100, 100,
                                INTER.CV_INTER_CUBIC);

            if (!String.IsNullOrEmpty(newLabel)) {
                _recognizerProvider.AddNewLabel(newLabel, faceImage);
                _rep.Save(newLabel, faceImage);
                result.Label = newLabel;
                return result;
            }

            if (_recognizerProvider.HasConfiguredFaces()) {
                result.Label = _recognizerProvider.GetRecognizer()
                                                  .Recognize(faceImage);
            }
            else {
                result.Label = UnknownLabel;
            }
            faceImage.Dispose();
            return result;
        }
    }
}
