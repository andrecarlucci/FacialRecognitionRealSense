using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace App.MediatorMessages {
    public class SomeOneInFrontOfMirror {
        public int NumberOfFaces { get; set; }
        public Image<Bgr, Byte> Image { get; set; }
    }
}
