using IRGLinearAlgebra;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinearnaAlgebraDemo
{
    class Z2
    {

        public static void Main(string[] args)
        {

            Console.WriteLine("3 jednadzbe s 3 nepoznanice");
            Console.WriteLine("Unesite koeficijente jednadzbi:");

            // Ax=b
            IMatrix a = new Matrix(3, 3);
            IVector b = new Vector(false, true, new double[3]);

            for (int i = 0; i < 3; i++)
            {
input:          string[] input = System.Text.RegularExpressions.Regex.Split(Console.ReadLine().Trim(), @"\s+");
                if (input.Length != 4)
                {
                    Console.WriteLine("4 arguments per line expected");
                    goto input;
                }
                a[i, 0] = double.Parse(input[0]);
                a[i, 1] = double.Parse(input[1]);
                a[i, 2] = double.Parse(input[2]);
                b[i] = double.Parse(input[3]);
            }

            IVector x = (a.NInvert() * b.ToColumnMatrix(true)).ToVector(true);

            Console.WriteLine("Solutions:");
            Console.WriteLine(x);

            Console.ReadLine();
        }

    }
}
