using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.IO;

namespace App {
    public class FaceRepository {
        private readonly string _directory;

        private List<FaceRegister> _faces = new List<FaceRegister>();

        public FaceRepository(string directory) {
            _directory = directory;
            Directory.CreateDirectory(_directory);
            var files = Directory.GetFiles(_directory);
            foreach (var file in files) {
                var label = Path.GetFileNameWithoutExtension(file);
                _faces.Add(new FaceRegister {
                    Label = label,
                    ImagePath = file
                });
            }
        }

        public List<FaceRegister> List() {
            return _faces;
        }

        public void Save(string label, Image<Gray, byte> face) {
            var path = $"{_directory}\\{label}.jpg";
            face.Save(path);

            _faces.Add(new FaceRegister {
                Label = label,
                ImagePath = path
            });
        }
    }
}