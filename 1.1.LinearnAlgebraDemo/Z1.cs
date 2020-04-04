using IRGLinearAlgebra;
using System;

namespace LinearnaAlgebraDemo
{
    class Z1
    {
        public static void Main(string[] args)
        {
            IVector v1 = Vector.ParseSimple("2 3 -4") + Vector.ParseSimple("-1 4 -3");
            double s = v1.ScalarProduct(Vector.ParseSimple("-1 4 -3"));
            IVector v2 = v1 * Vector.ParseSimple("2 2 4");
            IVector v3 = v2.NNormalize();
            IVector v4 = -v2;

            IMatrix m1 = Matrix.ParseSimple("1 2 3 | 2 1 3 | 4 5 1") + Matrix.ParseSimple("-1 2 -3 | 5 -2 7 | -4 -1 3");
            IMatrix m2 = Matrix.ParseSimple("1 2 3 | 2 1 3 | 4 5 1") * Matrix.ParseSimple("-1 2 -3 | 5 -2 7 | -4 -1 3").NTranspose(false);
            IMatrix m3 = Matrix.ParseSimple("-24 18 5 | 20 -15 -4 | -5 4 1").NInvert() * Matrix.ParseSimple("1 2 3 | 0 1 4 | 5 6 0").NInvert();

            Console.WriteLine("v1:");
            Console.WriteLine(v1);
            Console.WriteLine("s:");
            Console.WriteLine(s);
            Console.WriteLine("v2:");
            Console.WriteLine(v2);
            Console.WriteLine("v3:");
            Console.WriteLine(v3);
            Console.WriteLine("v4:");
            Console.WriteLine(v4);
            Console.WriteLine("m1:");
            Console.WriteLine(m1);
            Console.WriteLine("m2:");
            Console.WriteLine(m2);
            Console.WriteLine("m3:");
            Console.WriteLine(m3);

            Console.ReadLine();
        }
    }
}
