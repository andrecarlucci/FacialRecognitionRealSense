using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.IO;

namespace App {
    public class FaceRepository {

        private List<FaceRegister> _faces = new List<FaceRegister>();

        public List<FaceRegister> List() {
            return _faces;
        }

        public void Save(string label, Image<Gray, byte> face) {
            Directory.CreateDirectory("Faces");
            var path = $"Faces\\{label}.jpg";
            face.Save(path);

            _faces.Add(new FaceRegister {
                Label = label,
                ImagePath = path
            });
        }
    }
}
