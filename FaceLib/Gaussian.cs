using System;
public class Gaussian {

    private double[,] _kernel;
    private int _kernelHalf;
    private ushort[,] _res1, _res2;

    public Gaussian(double deviation, int matrixWidth, int matrixHeight) {
        _kernel = CalculateNormalized1DSampleKernel(deviation);
        _kernelHalf = _kernel.GetLength(0) / 2;
        _res1 = new ushort[matrixWidth, matrixHeight];
        _res2 = new ushort[matrixWidth, matrixHeight];
    }
    
    private double[,] CalculateNormalized1DSampleKernel(double deviation) {
        return NormalizeMatrix(Calculate1DSampleKernel(deviation));
    }

    private double[,] Calculate1DSampleKernel(double deviation) {
        int size = (int)Math.Ceiling(deviation * 3) * 2 + 1;
        return Calculate1DSampleKernel(deviation, size);
    }

    private double[,] Calculate1DSampleKernel(double deviation, int size) {
        double[,] ret = new double[size, 1];
        double sum = 0;
        int half = size / 2;
        for (int i = 0; i < size; i++) {
            ret[i, 0] = 1 / (Math.Sqrt(2 * Math.PI) * deviation) * Math.Exp(-(i - half) * (i - half) / (2 * deviation * deviation));
            sum += ret[i, 0];
        }
        return ret;
    }

    private double[,] NormalizeMatrix(double[,] matrix) {
        double[,] ret = new double[matrix.GetLength(0), matrix.GetLength(1)];
        double sum = 0;
        for (int i = 0; i < ret.GetLength(0); i++) {
            for (int j = 0; j < ret.GetLength(1); j++) {
                sum += matrix[i, j];
            }
        }
        if (sum != 0) {
            for (int i = 0; i < ret.GetLength(0); i++) {
                for (int j = 0; j < ret.GetLength(1); j++) {
                    ret[i, j] = matrix[i, j] / sum;
                }
            }
        }
        return ret;
    }

    public ushort[,] GaussianConvolution(ushort[,] matrix) {
        //x-direction
        for (int i = 0; i < matrix.GetLength(0); i++) {
            for (int j = 0; j < matrix.GetLength(1); j++) {
                _res1[i, j] = ProcessPoint(matrix, i, j, 0);
            }
        }
        //y-direction
        for (int i = 0; i < matrix.GetLength(0); i++) {
            for (int j = 0; j < matrix.GetLength(1); j++) {
                _res2[i, j] = ProcessPoint(_res1, i, j, 1);
            }
        }
        return _res2;
    }
    
    private ushort ProcessPoint(ushort[,] matrix, int x, int y, int direction) {
        ushort res = 0;
        for (int i = 0; i < _kernel.GetLength(0); i++) {
            int cox = direction == 0 ? x + i - _kernelHalf : x;
            int coy = direction == 1 ? y + i - _kernelHalf : y;
            if (cox >= 0 && cox < matrix.GetLength(0) && coy >= 0 && coy < matrix.GetLength(1)) {
                res += Convert.ToUInt16(matrix[cox, coy] * _kernel[i, 0]);
            }
        }
        return res;
    }
}
