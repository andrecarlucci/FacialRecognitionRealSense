using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace App {
    public class FaceRecognizer {

        public const string UnknownLabel = "unknown";

        private HaarCascade _face;
        private FaceRepository _rep;
        private List<Image<Gray, byte>> _images = new List<Image<Gray, byte>>();
        private List<string> _labels = new List<string>();

        public FaceRecognizer(string haarcascade, FaceRepository faceLabelsRepository) {
            _face = new HaarCascade(haarcascade);
            _rep = faceLabelsRepository;
            var regs = faceLabelsRepository.List();
            foreach (var reg in regs) {
                _images.Add(new Image<Gray, byte>(reg.ImagePath));
                _labels.Add(reg.Label);
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
            
            if(!facesDetected[0].Any()) {
                return null;
            }

            MCvAvgComp faceInfo = facesDetected[0].First();
            result.FaceInfo = faceInfo;

            var faceImage = frame.Copy(faceInfo.rect)
                                .Convert<Gray, byte>()
                                .Resize(100, 100,
                                INTER.CV_INTER_CUBIC);

            if (!String.IsNullOrEmpty(newLabel)) {
                _rep.Save(newLabel, faceImage);
                _images.Add(faceImage);
                _labels.Add(newLabel);
                result.Label = newLabel;
                return result;
            }

            if(_images.Count > 0) {
                //TermCriteria for face recognition with numbers of trained images like maxIteration
                var termCrit = new MCvTermCriteria(_images.Count, 0.001);
                //Eigen face recognizer
                var recognizer = new CustomEigenObjectRecognizer(
                        _images.ToArray(),
                        _labels.ToArray(),
                        3000,
                        ref termCrit);
                result.Label = recognizer.Recognize(faceImage);
            }
            else {
                result.Label = UnknownLabel;
            }
            return result;
        }
    }
}
