using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;

namespace App {
    public static class FaceCropper {
        public static Image<Gray, byte> CropGray(Image<Bgr, Byte> image, Rectangle rectangle) {
            var cropped = image.Copy(rectangle)
                               .Convert<Gray, byte>()
                               .Resize(100, 100, Inter.Cubic);
            cropped._EqualizeHist();
            return cropped;
        }
    }
}