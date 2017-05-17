using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace App {
    public class FaceRepository {
        private readonly string _directory;

        private List<FaceRegister> _faces = new List<FaceRegister>();

        public FaceRepository(string directory) {
            _directory = directory;
            Directory.CreateDirectory(_directory);
            var files = Directory.GetFiles(_directory);
            foreach (var file in files) {
                var filename = Path.GetFileNameWithoutExtension(file);
                var label = filename.Split('_')[0];
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
            var files = Directory.GetFiles(_directory);

            var num = 0;
            foreach(var file in files) {
                var filename = Path.GetFileNameWithoutExtension(file);
                if(filename.Split('_')[0] == label) {
                    num++;
                }
            }
            
            var path = $"{_directory}\\{label}_{num}.jpg";
            face.Save(path);

            _faces.Add(new FaceRegister {
                Label = label,
                ImagePath = path
            });
        }
    }
}