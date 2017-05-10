using System;

namespace FaceLib {
    public static class ImageHelper {

        public static void CopyToUshort(byte[] source, ushort[] target) {
            Buffer.BlockCopy(source, 0, target, 0, source.Length);
        }

        public static byte[] ToByte(ushort[] source) {
            var target = new byte[source.Length * 2];
            Buffer.BlockCopy(source, 0, target, 0, source.Length * 2);
            return target;
        }

        public static void CopyToMatrix(ushort[] source, int width, int heigth, ushort[,] target) {
            for (var i = 0; i < target.Length; i++) {
                var x = i % width;
                var y = i / width;
                target[x, y] = source[i];
            }
        }

        public static void CopyToArray(ushort[,] source, ushort[] target) {
            var width = source.GetLength(0);
            var height = source.GetLength(1);
            if (target.Length < width * height) {
                throw new ArgumentException("Array has to be greater than matrix width * heith", nameof(target));
            }
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    target[x + (y * width)] = source[x, y];
                }
            }
        }

        public static bool PixelsBelowHaveValue(ushort[,] matrix, int x, int y, int checkSize) {
            var max = matrix.GetLength(1);
            for (var i = 1; i < checkSize; i++) {
                if ((y + i >= max) || matrix[x, y + i] == 0) {
                    return false;
                }
            }
            return true;
        }

        public static bool PixelsBelowHaveNoValue(ushort[,] matrix, int x, int y, int checkSize) {
            var max = matrix.GetLength(1);
            for (var i = 1; i < checkSize; i++) {
                if ((y + i >= max) || matrix[x, y + i] > 0) {
                    return false;
                }
            }
            return true;
        }

        public static bool PixelsOnLeftHaveValue(ushort[,] matrix, int x, int y, int checkSize) {
            var width = matrix.GetLength(0);
            for (var i = 1; i < checkSize; i++) {
                if ((x + i >= width) || matrix[x + i, y] == 0) {
                    return false;
                }
            }
            return true;
        }

        public static bool PixelsOnRightHaveValue(ushort[,] matrix, int x, int y, int checkSize) {
            var width = matrix.GetLength(0);
            for (var i = 1; i < checkSize; i++) {
                if ((x - i <= 0) || matrix[x - i, y] == 0) {
                    return false;
                }
            }
            return true;
        }
    }
}

