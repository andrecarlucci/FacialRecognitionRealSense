using System.Collections.Generic;

namespace FaceLib {
    public class ImageProcessor {

        private float _deviceDepthScale;
        private int _width, _height;
        private ushort[,] _matrix;
        private ushort[] _imageInUshort;

        private FaceSquareFinder _faceFinder;
        private Gaussian _gaussian;

        private List<Person> _people = new List<Person>();

        public ImageProcessor(float deviceDepthScale, int frameWidth, int frameHeight) {
            _deviceDepthScale = deviceDepthScale;
            _faceFinder = new FaceSquareFinder();
            _gaussian = new Gaussian(0.5, frameWidth, frameHeight);
            _matrix = new ushort[frameWidth, frameHeight];
            _imageInUshort = new ushort[frameWidth * frameHeight];
            _width = frameWidth;
            _height = frameHeight;

            _people.Add(new Person("andre", 1.75));
            _people.Add(new Person("roberta", 1.56));
        }




        public Result Process(byte[] imageInBytes) {
            var oneMeter = 1 / _deviceDepthScale;
            ImageHelper.CopyToUshort(imageInBytes, _imageInUshort);

            //remove points farther than one meter
            for (var i = 0; i < _imageInUshort.Length; i++) {
                if (_imageInUshort[i] > oneMeter) {
                    _imageInUshort[i] = 0;
                }
            }

            ImageHelper.CopyToMatrix(_imageInUshort, _width, _height, _matrix);
            //_matrix = _gaussian.GaussianConvolution(_matrix);

            for (var y = 0; y < _height; y++) {
                for (var x = 0; x < _width; x++) {
                    if (_matrix[x, y] > oneMeter) {
                        _matrix[x, y] = 0;
                    }
                }
            }

            _faceFinder.Parse(_matrix);
            var face = _faceFinder.Face;

            //Paint
            for (int x = 0; x < _width; x++) {
                _matrix[x, face.High.Y] = 20;
                _matrix[x, face.Low.Y] = 20;
            }

            for (int y = 0; y < _height; y++) {
                _matrix[face.Left.X, y] = 20;
                _matrix[face.Right.X, y] = 20;
            }

            for (int x = 0; x < _width; x++) {
                _matrix[x, face.Left.Y] = 20;
                //matrix[x, face.Right.Y] = 20;
            }

            //remove non detected depth (zeros)
            ImageHelper.CopyToArray(_matrix, _imageInUshort);
            for (var i = 0; i < _imageInUshort.Length; i++) {
                if (_imageInUshort[i] == 0) {
                    _imageInUshort[i] = ushort.MaxValue;
                }
            }

            imageInBytes = ImageHelper.ToByte(_imageInUshort);


            var dx = face.Left.X - face.Right.X;
            var dy = face.Low.Y - face.High.Y;
            var k1 = dy / (float)dx;

            face.K1 = (int)(k1 * 100);

            Person person = new Person("stranger", 0);
            foreach (var p in _people) {
                if (k1 > p.KeyMin && k1 < p.KeyMax) {
                    person = p;
                    break;
                }
            }
            var text = $"Person {person.Name} Min: {person.KeyMin} Max: {person.KeyMax}";
            face.Person = person;

            string t = $"Ratio: X: {dx} Y:{dy} K: {k1:#.##} {text}";

            return new Result {
                Image = imageInBytes,
                Text = t,
                Face = face
            };
        }

        //private void Force(ushort[,] matrix) {
        //    for (int y = 1; y < matrix.GetLength(0)-1; y++) {
        //        for (int x = 1; x < matrix.GetLength(1)-1; x++) {
        //            if(matrix[x,y] )
        //        }
        //    }
    }
}
