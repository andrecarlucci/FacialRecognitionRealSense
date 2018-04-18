using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;

namespace App {
    public partial class FacePipeline {
        private readonly FaceDetector _faceDetector;
        private readonly FaceRecognizer _faceRecognizer;
        private readonly FaceRepository _faceLabelsRepository;

        private object _sync = new object();
        private string _nextFaceLabel;

        public FacePipeline(FaceDetector faceDetector, 
                            FaceRecognizer faceRecognizer, 
                            FaceRepository faceLabelsRepository) {
            _faceDetector = faceDetector;
            _faceRecognizer = faceRecognizer;
            _faceLabelsRepository = faceLabelsRepository;
        }

        public void Prepare() {
            var regs = _faceLabelsRepository.List();
            foreach (var reg in regs) {
                var img = new Image<Gray, byte>(reg.ImagePath);
                _faceRecognizer.AddNewLabel(reg.Label, img);
            }
        }

        public void RegisterNextFace(string label) {
            lock(_sync) {
                _nextFaceLabel = label;
            }
        }

        public PipelineResult ProccessFrame(Image<Bgr, byte> frame) {
            var rectangles = _faceDetector.DetectFaces(frame);
            if (rectangles.Length == 0) {
                return new PipelineResult {
                    Status = FaceRecognitionStatus.Nobody
                };
            }

            var result = ReconizeFace(frame, rectangles[0]);

            return new PipelineResult {
                Status = result.Status,
                FacePositions = rectangles,
                FirstFaceLabel = result.Label
            };
        }

        private FaceRecognitionResult ReconizeFace(Image<Bgr, byte> frame, Rectangle rectangle) {
            var cropped = frame.Copy(rectangle)
                               .Convert<Gray, byte>()
                               .Resize(100, 100, Inter.Cubic);
            cropped._EqualizeHist();

            lock (_sync) {
                if (_nextFaceLabel != null) {
                    _faceRecognizer.AddNewLabel(_nextFaceLabel, cropped);
                    _faceLabelsRepository.Save(_nextFaceLabel, cropped);
                    _nextFaceLabel = null;
                }
            }

            return _faceRecognizer.Recognize(cropped);
        }
    }
}