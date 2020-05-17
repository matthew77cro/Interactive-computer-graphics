using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRGLinearAlgebra
{
    public static class IRG
    {

        public static IMatrix Translate3D(float dx, float dy, float dz)
        {
            return new Matrix(4, 4, 
                new double[,] { { 1, 0, 0, 0 }, 
                                { 0, 1, 0, 0 }, 
                                { 0, 0, 1, 0 }, 
                                { dx, dy, dz, 1 } }, 
                true);
        }

        public static IMatrix Scale3D(float sx, float sy, float sz)
        {
            return new Matrix(4, 4, 
                new double[,] { { sx, 0, 0, 0 }, 
                                { 0, sy, 0, 0 }, 
                                { 0, 0, sz, 0 }, 
                                { 0, 0, 0, 1 } }, 
                true);
        }

        public static IMatrix LookAtMatrix(IVector eye, IVector center, IVector viewUp)
        {
            IVector forward = (center - eye).Normalize();
            IVector side = (forward*(viewUp.NNormalize())).Normalize();
            IVector up = (side*forward).NNormalize();

            return Translate3D(-(float)eye[0], -(float)eye[1], -(float)eye[2]) * new Matrix(4, 4,
                new double[,] { { side[0], side[1], side[2], 0},
                                { up[0], up[1], up[2], 0 },
                                { -forward[0], -forward[1], -forward[2], 0 },
                                { 0, 0, 0, 1 } },
                true).NTranspose(false);
        }

        public static IMatrix BuildFrustumMatrix(double l, double r, double b, double t, int n, int f)
        {

            return new Matrix(4, 4,
                new double[,] { { 2*n/(r-l), 0, 0, 0 },
                                { 0, 2*n/(t-b), 0, 0 },
                                { (r+l)/(r-l), (t+b)/(t-b), -(f+n)/(f-n), -1 },
                                { 0, 0, -2*f*n/(f-n), 0 } },
                true);

        }

        public static bool isAntiClockwise(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            IVector edge = new Vector(true, true, new double[] { y1 - y2, -(x1 - x2), x1 * y2 - y1 * x2 });
            IVector point = new Vector(true, true, new double[] { x3, y3, 1 });
            double sp = edge.ScalarProduct(point);

            return sp > 0;
        }

    }

}
