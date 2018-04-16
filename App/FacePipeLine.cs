using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using static App.FaceRecognizer;

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
            var rectangle = _faceDetector.DetectFirstFace(frame);
            if (rectangle.IsEmpty) {
                return new PipelineResult {
                    Status = FaceRecognitionStatus.Nobody
                };
            }
            
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

            var result = _faceRecognizer.Recognize(cropped);
            //var result = new FaceRecognitionResult {
            //    Status = FaceRecognitionStatus.Unknown
            //};
            return new PipelineResult {
                Status = result.Status,
                FacePosition = rectangle,
                Label = result.Label
            };
        }
    }
}