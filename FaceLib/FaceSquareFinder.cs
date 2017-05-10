using System.Diagnostics;

namespace FaceLib {
    public class FaceSquareFinder {
        public Face Face { get; } = new Face();
        private ushort[,] _image;
        private int _width, _height;
        private int _lowOffset;
        
        public void Parse(ushort[,] image) {
            _image = image;
            _width = _image.GetLength(0);
            _height = _image.GetLength(1);
            FindHigh();
            FindLow();
            _lowOffset = ((Face.Low.Y - Face.High.Y) /3)*2;
            FindLeft();
            FindRight();
        }

        private void FindHigh() {
            for (var y = 0; y < _height; y++) {
                for (var x = 0; x < _width; x++) {
                    if (_image[x, y] > 0 && 
                        (ImageHelper.PixelsBelowHaveValue(_image, x, y, 10))) {
                        Face.High.Set(x, y, _image[x, y]);
                        return;
                    }
                }
            }
        }

        private void FindLow() {
            for (var y = _height-1; y >= 0; y--) {
                if (_image[Face.High.X, y] == 0 &&
                    (ImageHelper.PixelsBelowHaveValue(_image, Face.High.X, y, 20))) {
                    Face.Low.Set(Face.High.X, y, _image[Face.High.X, y]);
                    return;
                }
            }
        }

        private void FindLeft() {
            Face.Left.SetX(0);
            for (var y = Face.High.Y; y < Face.Low.Y - _lowOffset; y++) {
                for (var x = 0; x < _width; x++) {
                    if (_image[x, y] > 0 &&
                        ImageHelper.PixelsOnRightHaveValue(_image, x, y, 20) 
                        //&& ImageHelper.PixelsBelowHaveNoValue(_image, x, y, 2)
                    ) {
                        if (x > Face.Left.X) {
                            Face.Left.Set(x, y, _image[x, y]);
                        }
                    }
                }
            }
        }

        private void FindRight() {
            Face.Right.SetX(_width-1);
            for (var y = Face.High.Y; y < Face.Low.Y - _lowOffset; y++) {
                for (var x = _width - 1; x >= 0; x--) {
                    if (_image[x, y] > 0 &&
                        (ImageHelper.PixelsOnLeftHaveValue(_image, x, y, 20)
                         //&& ImageHelper.PixelsBelowHaveNoValue(_image, x, y, 2)
                         )) {
                        if (x < Face.Right.X) {
                            //Debug.WriteLine($"FX: {Face.Right.X} x: {x}");
                            Face.Right.Set(x, y, _image[x, y]);
                        }
                    }
                }
            }
        }

        //private void FindClosest() {
        //    var max = int.MaxValue;
        //    var dx = _faceEdges.Left.X - _faceEdges.Right.X;
        //    var dy = _faceEdges.Low.Y - _faceEdges.High.Y;

        //    for (var y = _faceEdges.High.Y; y < _faceEdges.Low.Y; y++) {
        //        for (var x = _faceEdges.Right.X; x < _faceEdges.Left.X; x++) {
        //            if (_image[x, y] == 0) {
        //                continue;
        //            }
        //            var sum = _image[x, y - 1] + _image[x, y + 1] + _image[x, y] +
        //                      _image[x, y - 2] + _image[x, y + 2] + _image[x-1, y] +
        //                      _image[x, y - 3] + _image[x, y + 3] + _image[x+1, y];
        //            if (sum < max) {
        //                max = sum;
        //                _faceEdges.Closest.Set(x,y, _image[x,y]);
        //            }
        //        }
        //    }
        //}
    }
}