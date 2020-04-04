using IRGLinearAlgebra;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinearnaAlgebraDemo
{
    class Z3
    {

        public static void Main(string[] args)
        {

            Console.WriteLine("Baricentricne koordinate");
            Console.WriteLine("Unesite x y z koordinate tocaka A,B,C i T:");

            // Ax=b
            IMatrix a = new Matrix(3, 3);
            IVector b = new Vector(false, true, new double[3]);

            for (int i = 0; i < 3; i++)
            {
input:          string[] input = System.Text.RegularExpressions.Regex.Split(Console.ReadLine().Trim(), @"\s+");
                if (input.Length != 3)
                {
                    Console.WriteLine("3 arguments per line expected");
                    goto input;
                }
                a[0, i] = double.Parse(input[0]);
                a[1, i] = double.Parse(input[1]);
                a[2, i] = double.Parse(input[2]);
            }

            {
                string[] input = System.Text.RegularExpressions.Regex.Split(Console.ReadLine().Trim(), @"\s+");
                b[0] = double.Parse(input[0]);
                b[1] = double.Parse(input[1]);
                b[2] = double.Parse(input[2]);
            }

            IVector x = (a.NInvert() * b.ToColumnMatrix(true)).ToVector(true);

            Console.WriteLine("Baricentricne koordinate tocke T:");
            Console.WriteLine(x);

            Console.ReadLine();
        }

    }
}
