using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Linq;

namespace App {
    public class RecognizerProvider {
        private List<Image<Gray, byte>> _images = new List<Image<Gray, byte>>();
        private List<string> _labels = new List<string>();
        private CustomEigenObjectRecognizer _recognizer;
        private bool _shouldCreateNew = true;


        public void AddNewLabel(string newLabel, Image<Gray, byte> faceImage) {
            _images.Add(faceImage);
            _labels.Add(newLabel);
            _shouldCreateNew = true;
        }

        public bool HasConfiguredFaces() {
            return _images.Any();
        }

        public CustomEigenObjectRecognizer GetRecognizer() {
            if (_shouldCreateNew) {
                var termCrit = new MCvTermCriteria(_images.Count, 0.001);
                _recognizer = new CustomEigenObjectRecognizer(
                       _images.ToArray(),
                       _labels.ToArray(),
                       3000,
                       ref termCrit);
            }
            return _recognizer;
        }
    }
}
