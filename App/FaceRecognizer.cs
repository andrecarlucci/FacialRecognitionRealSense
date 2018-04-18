using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace App {
    public partial class FaceRecognizer {

        private readonly List<Image<Gray, Byte>> _images = new List<Image<Gray, Byte>>();
        private readonly List<string> _labels = new List<string>();

        private LBPHFaceRecognizer _faceRecognizer;

        private bool _shouldTrain = true;
        private object _sync = new object();

        public void AddNewLabel(string newLabel, Image<Gray, Byte> faceImage) {
            lock (_sync) {
                _images.Add(faceImage);
                _labels.Add(newLabel);
                _shouldTrain = true;
            }
        }

        public bool Train() {
            lock (_sync) {
                if(_images.Count <= 1) {
                    return false;
                }
                if (_faceRecognizer != null) {
                    _faceRecognizer.Dispose();
                }

                _faceRecognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 100);
                _faceRecognizer.Train(_images.ToArray(), _images.Select((c, i) => i).ToArray());
                _shouldTrain = false;
                return true;
            }
        }

        public FaceRecognitionResult Recognize(Image<Gray, byte> faceImage) {
            lock (_sync) {
                if (_shouldTrain) {
                    if(!Train()) {
                        return new FaceRecognitionResult {
                            Status = FaceRecognitionStatus.Someone
                        };
                    }
                }
                if (_images.Any()) {
                    var result = _faceRecognizer.Predict(faceImage);
                    if (result.Label > 0) {
                        return new FaceRecognitionResult {
                            Status = FaceRecognitionStatus.IdentifiedUser,
                            Label = _labels[result.Label]
                        };
                    }
                }
                return new FaceRecognitionResult {
                    Status = FaceRecognitionStatus.Someone
                };
            }
        }
    }
}