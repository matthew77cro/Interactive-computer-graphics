using System;
using System.Text;

namespace IRGLinearAlgebra
{
    public abstract class IVector
    {
        public abstract int Dimension { get; }
        public abstract double this[int i] { get; set; }

        public abstract IVector NewInstance(int dimension);
        public abstract IVector Copy();
        public abstract IVector CopyPart(int size);
        public abstract IVector Add(IVector other);
        public abstract IVector NAdd(IVector other);
        public abstract IVector Sub(IVector other);
        public abstract IVector NSub(IVector other);
        public abstract IVector ScalarMultiply(double value);
        public abstract IVector NScalarMultiply(double value);
        public abstract double Norm();
        public abstract IVector Normalize();
        public abstract IVector NNormalize();
        public abstract double ScalarProduct(IVector other);
        public abstract IVector NVectorProduct(IVector other);
        public abstract double Cosine(IVector other);
        public abstract IVector NFromHomogeneus();
        public abstract double[] ToArray();
        public abstract IMatrix ToRowMatrix(bool liveView);
        public abstract IMatrix ToColumnMatrix(bool liveView);

        public static IVector operator +(IVector v1, IVector v2)
        {
            return v1.NAdd(v2);
        }
        public static IVector operator -(IVector v1, IVector v2)
        {
            return v1.NSub(v2);
        }
        public static IVector operator *(IVector v, double other)
        {
            return v.NScalarMultiply(other);
        }
        public static IVector operator *(double other, IVector v)
        {
            return v * other;
        }
        public static IVector operator *(IVector v1, IVector v2)
        {
            return v1.NVectorProduct(v2);
        }
        public static IVector operator -(IVector v)
        {
            return -1 * v;
        }
    }

    public abstract class AbstractVector : IVector
    {
        public override IVector CopyPart(int size)
        {
            var v = NewInstance(size);

            int loopCount = Math.Min(Dimension, size);
            for (int i = 0; i < loopCount; i++)
            {
                v[i] = this[i];
            }
            for (int i = loopCount; i < size; i++)
            {
                v[i] = 0;
            }

            return v;
        }
        public override IVector Add(IVector other)
        {
            if (Dimension != other.Dimension)
                throw new InvalidOperationException();

            for (int i = 0; i < Dimension; i++)
            {
                this[i] = this[i] + other[i];
            }

            return this;
        }
        public override IVector NAdd(IVector other)
        {
            return Copy().Add(other);
        }
        public override IVector Sub(IVector other)
        {
            if (Dimension != other.Dimension)
                throw new InvalidOperationException();

            for (int i = 0; i < Dimension; i++)
            {
                this[i] = this[i] - other[i];
            }

            return this;
        }
        public override IVector NSub(IVector other)
        {
            return Copy().Sub(other);
        }
        public override IVector ScalarMultiply(double value)
        {
            for (int i = 0; i < Dimension; i++)
            {
                this[i] = this[i] * value;
            }

            return this;
        }
        public override IVector NScalarMultiply(double value)
        {
            return Copy().ScalarMultiply(value);
        }
        public override double Norm()
        {
            double sum = 0;
            for (int i = 0; i < Dimension; i++)
            {
                sum += this[i] * this[i];
            }
            return Math.Sqrt(sum);
        }
        public override IVector Normalize()
        {
            double norm = Norm();
            for (int i = 0; i < Dimension; i++)
            {
                this[i] = this[i] / norm;
            }

            return this;
        }
        public override IVector NNormalize()
        {
            return Copy().Normalize();
        }
        public override double ScalarProduct(IVector other)
        {
            if (Dimension != other.Dimension)
                throw new InvalidOperationException();

            double scalarProduct = 0;
            for (int i = 0; i < Dimension; i++)
            {
                scalarProduct += this[i] * other[i];
            }

            return scalarProduct;
        }
        public override IVector NVectorProduct(IVector other)
        {
            if (this.Dimension != 3 || other.Dimension != 3)
                throw new InvalidOperationException();

            double ax = this[0];
            double ay = this[1];
            double az = this[2];

            double bx = other[0];
            double by = other[1];
            double bz = other[2];

            IVector tmp = this.NewInstance(this.Dimension);
            tmp[0] = ay * bz - az * by;
            tmp[1] = az * bx - ax * bz;
            tmp[2] = ax * by - ay * bx;

            return tmp;
        }
        public override double Cosine(IVector other)
        {
            return this.ScalarProduct(other) / (Norm() * other.Norm());
        }
        public override IVector NFromHomogeneus()
        {
            var v = NewInstance(Dimension - 1);

            double homogeneus = this[Dimension - 1];
            for (int i = 0; i < v.Dimension; i++)
            {
                this[i] = this[i] / homogeneus;
            }

            return v;
        }
        public override double[] ToArray()
        {
            var array = new double[Dimension];
            for (int i = 0; i < Dimension; i++)
            {
                array[i] = this[i];
            }
            return array;
        }
        public override IMatrix ToRowMatrix(bool liveView)
        {
            if (liveView)
            {
                return new MatrixVectorView(this, true);
            }

            double[,] values = new double[1, Dimension];
            for (int i = 0; i < Dimension; i++)
                values[0, i] = this[i];
            return new Matrix(1, Dimension, values, true);
        }
        public override IMatrix ToColumnMatrix(bool liveView)
        {
            if (liveView)
            {
                return new MatrixVectorView(this, false);
            }

            double[,] values = new double[Dimension, 1];
            for (int i = 0; i < Dimension; i++)
                values[i, 0] = this[i];
            return new Matrix(Dimension, 1, values, true);
        }

        public string ToString(int precision)
        {
            if (precision < 0)
                throw new ArgumentException();

            StringBuilder formatSB = new StringBuilder();
            formatSB.Append("0.");
            for (int i = 0; i < precision; i++) formatSB.Append("#");
            string format = formatSB.ToString();

            StringBuilder sb = new StringBuilder();

            bool first = true;
            for (int i = 0; i < Dimension; i++)
            {
                if (first)
                {
                    sb.Append(this[i].ToString(format));
                    first = false;
                }
                else
                {
                    sb.Append(" " + this[i].ToString(format));
                }
            }

            return sb.ToString();
        }
        public override string ToString()
        {
            return ToString(3);
        }

    }

    public class Vector : AbstractVector
    {

        private readonly double[] values;
        private readonly bool locked;

        public override double this[int i] 
        { 
            get { return values[i]; } 
            set 
            { 
                if (i<0 || i >= Dimension) 
                    throw new IndexOutOfRangeException();
                if (locked)
                    throw new InvalidOperationException("Locked!");
                values[i] = value; 
            } 
        }
        public override int Dimension { get; }

        public Vector(bool locked, bool useGivenArray, double[] values)
        {
            if (values.Length == 0)
                throw new ArgumentException();

            this.locked = locked;
            Dimension = values.Length;

            if (useGivenArray)
            {
                this.values = values;
            } 
            else
            {
                this.values = new double[Dimension];
                Array.Copy(values, this.values, Dimension);
            }
            
        }
        public Vector(params double[] values)
            :this(false, true, values)
        {

        }
        public override IVector NewInstance(int dimension)
        {
            return new Vector(false, true, new double[dimension]);
        }
        public override IVector Copy()
        {
            return new Vector(locked, false, values);
        }

        public static IVector ParseSimple(string arg)
        {
            string[] split = System.Text.RegularExpressions.Regex.Split(arg.Trim(), @"\s+");
            double[] values = new double[split.Length];
            for(int i=0; i<split.Length; i++)
            {
                values[i] = double.Parse(split[i]);
            }
            return new Vector(false, true, values);
        }

    }

}
