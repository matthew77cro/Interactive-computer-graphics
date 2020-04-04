using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;

namespace IRGLinearAlgebra
{
    public abstract class IMatrix
    {

        public abstract int RowsCount { get; }
        public abstract int ColsCount { get; }
        public abstract double this[int row, int col] { get; set; }

        public abstract IMatrix NewInstance(int row, int col);
        public abstract IMatrix Copy();
        public abstract IMatrix NTranspose(bool liveView);
        public abstract IMatrix Add(IMatrix other);
        public abstract IMatrix NAdd(IMatrix other);
        public abstract IMatrix Sub(IMatrix other);
        public abstract IMatrix NSub(IMatrix other);
        public abstract IMatrix NMultiply(IMatrix other);
        public abstract double Determinant();
        public abstract IMatrix SubMatrix(int row, int col, bool liveView);
        public abstract IMatrix NInvert();
        public abstract double[,] ToArray();
        public abstract IVector ToVector(bool liveView);

        public static IMatrix operator +(IMatrix m1, IMatrix m2)
        {
            return m1.NAdd(m2);
        }
        public static IMatrix operator -(IMatrix m1, IMatrix m2)
        {
            return m1.NSub(m2);
        }
        public static IMatrix operator *(IMatrix m1, IMatrix m2)
        {
            return m1.NMultiply(m2);
        }
        public static IMatrix operator *(double k, IMatrix m)
        {
            var matrix = m.Copy();

            for (int i = 0; i < matrix.RowsCount; i++)
            {
                for (int j = 0; j < matrix.ColsCount; j++)
                {
                    matrix[i, j] = m[i, j] * k;
                }
            }

            return matrix;
        }

    }

    public abstract class AbstractMatrix : IMatrix
    {
        public override IMatrix NTranspose(bool liveView)
        {
            if (liveView)
            {
                return new MatrixTransposeView(this);
            }

            var m = NewInstance(ColsCount, RowsCount);

            for (int i = 0; i < ColsCount; i++)
            {
                for (int j = 0; j < RowsCount; j++)
                {
                    m[i, j] = this[j, i];
                }
            }

            return m;
        }
        public override IMatrix Add(IMatrix other)
        {
            if (RowsCount != other.RowsCount || ColsCount != other.ColsCount)
                throw new ArgumentException();

            var m = NewInstance(RowsCount, ColsCount);
            for (int i = 0; i < m.RowsCount; i++)
            {
                for (int j = 0; j < m.ColsCount; j++)
                {
                    m[i, j] = this[i, j] + other[i, j];
                }
            }

            return m;
        }
        public override IMatrix NAdd(IMatrix other)
        {
            return Copy().Add(other);
        }
        public override IMatrix Sub(IMatrix other)
        {
            if (RowsCount != other.RowsCount || ColsCount != other.ColsCount)
                throw new ArgumentException();

            var m = NewInstance(RowsCount, ColsCount);
            for (int i = 0; i < m.RowsCount; i++)
            {
                for (int j = 0; j < m.ColsCount; j++)
                {
                    m[i, j] = this[i, j] - other[i, j];
                }
            }

            return m;
        }
        public override IMatrix NSub(IMatrix other)
        {
            return Copy().Sub(other);
        }
        public override IMatrix NMultiply(IMatrix other)
        {
            if (this.ColsCount != other.RowsCount)
                throw new ArgumentException();

            var m = this.NewInstance(this.RowsCount, other.ColsCount);
            for (int i = 0; i < this.RowsCount; i++)
            {
                for (int j = 0; j < other.ColsCount; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < this.ColsCount; k++)
                    {
                        sum += this[i, k] * other[k, j];
                    }
                    m[i, j] = sum;
                }
            }

            return m;
        }
        public override double Determinant()
        {
            return Deter(this);
        }
        public override IMatrix SubMatrix(int row, int col, bool liveView)
        {
            if (liveView)
            {
                return new MatrixSubMatrixView(this, row, col);
            }

            var m = NewInstance(RowsCount - 1, ColsCount - 1);

            int origRow = 0, origCol = 0, newRow = 0, newCol = 0;
            while (newRow < m.RowsCount)
            {
                while (newCol < m.ColsCount)
                {
                    m[newRow, newCol] = this[origRow, origCol];
                    newCol++;
                    origCol++;
                    if (origCol == col) origCol++;
                }
                newRow++;
                origRow++;
                if (origRow == row) origRow++;
            }

            return m;
        }
        public override IMatrix NInvert()
        {
            if (RowsCount != ColsCount)
                throw new InvalidOperationException();

            if(RowsCount == 1)
            {
                return new Matrix(1, 1, new double[,] { { 1 / this[0, 0] } }, true);
            }

            var cof = NewInstance(RowsCount, ColsCount);
            double det = Determinant();

            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                {
                    cof[i, j] = Math.Pow(-1, i + j) * this.SubMatrix(i, j, true).Determinant();
                }
            }

            return (1 / det) * cof.NTranspose(true);
        }
        public override double[,] ToArray()
        {
            double[,] array = new double[RowsCount, ColsCount];

            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                {
                    array[i, j] = this[i, j];
                }
            }

            return array;
        }
        public override IVector ToVector(bool liveView)
        {
            if (liveView)
            {
                return new VectorMatrixView(this);
            }

            double[] values = null;
            if (RowsCount == 1)
            {
                values = new double[ColsCount];
                for (int i = 0; i < ColsCount; i++)
                {
                    values[i] = this[0, i];
                }
            }
            else if (ColsCount == 1)
            {
                values = new double[RowsCount];
                for (int i = 0; i < RowsCount; i++)
                {
                    values[i] = this[i, 0];
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            return new Vector(false, false, values);
        }

        private static double Deter(IMatrix matrix)
        {
            if (matrix.RowsCount != matrix.ColsCount)
                throw new InvalidOperationException();

            double det = 0;

            if (matrix.RowsCount > 2)
            {
                for (int i = 0; i < matrix.RowsCount; i++)
                {
                    det += Math.Pow(-1, i) * matrix[0, i] * Deter(matrix.SubMatrix(0, i, true));
                }
            }
            else if (matrix.RowsCount == 2)
            {
                det = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
            }
            else
            {
                det = matrix[0, 0];
            }

            return det;

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

            for (int i = 0; i < RowsCount; i++)
            {
                sb.Append("| ");
                for (int j = 0; j < ColsCount; j++)
                {
                    sb.Append(this[i, j].ToString(format) + " ");
                }
                sb.Append("|");
                sb.AppendLine();
            }

            return sb.ToString();
        }
        public override string ToString()
        {
            return ToString(3);
        }

    }

    public class Matrix : AbstractMatrix
    {

        private readonly double[,] values;

        public override int RowsCount { get; }
        public override int ColsCount { get; }

        public override double this[int row, int col] 
        { 
            get { return values[row, col]; }
            set
            {
                if (row < 0 || row >= RowsCount || col < 0 || col >= ColsCount)
                    throw new IndexOutOfRangeException();
                values[row, col] = value;
            }
        }

        public Matrix(int rowsCount, int colsCount, double[,] values, bool useGivenArray)
        {
            if (rowsCount <= 0 || colsCount <= 0)
                throw new ArgumentException();

            RowsCount = rowsCount;
            ColsCount = colsCount;
            if(useGivenArray)
            {
                this.values = values;
            }
            else
            {
                this.values = new double[RowsCount, ColsCount];
                Array.Copy(values, this.values, this.values.Length);
            }
        }

        public Matrix(int rowsCount, int colsCount)
            :this(rowsCount, colsCount, new double[rowsCount, colsCount], true)
        {

        }

        public override IMatrix NewInstance(int row, int col)
        {
            return new Matrix(row, col);
        }
        public override IMatrix Copy()
        {
            return new Matrix(RowsCount, ColsCount, values, false);
        }

        public static IMatrix ParseSimple(string arg)
        {
            string[] splitRows = System.Text.RegularExpressions.Regex.Split(arg.Trim(), "\\|");
            int numOfRows = splitRows.Length;
            List<double> values = new List<double>();

            foreach (string s in splitRows)
            {
                string[] split = System.Text.RegularExpressions.Regex.Split(s.Trim(), @"\s+");
                foreach (string value in split)
                {
                    values.Add(double.Parse(value));
                }
            }

            if (values.Count % numOfRows != 0)
                throw new ArgumentException();

            int numOfCols = values.Count / numOfRows;
            double[,] valuesArray = new double[numOfRows, numOfCols];
            IEnumerator<double> enumerator = values.GetEnumerator();
            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < numOfCols; j++)
                {
                    enumerator.MoveNext();
                    valuesArray[i, j] = enumerator.Current;
                }
            }
            return new Matrix(numOfRows, numOfCols, valuesArray, true);
        }

    }

    public class MatrixTransposeView : AbstractMatrix
    {

        private readonly IMatrix matrix;

        public override int RowsCount { get { return matrix.ColsCount; } }
        public override int ColsCount { get { return matrix.RowsCount; } }

        public override double this[int row, int col]
        {
            get { return matrix[col, row]; }
            set { matrix[col, row] = value; }
        }

        public MatrixTransposeView(IMatrix matrix)
        {
            this.matrix = matrix ?? throw new NullReferenceException();
        }

        public override IMatrix NewInstance(int row, int col)
        {
            return new Matrix(row, col);
        }

        public override IMatrix Copy()
        {
            return new MatrixTransposeView(matrix);
        }

    }

    public class MatrixSubMatrixView : AbstractMatrix
    {
        private readonly IMatrix matrix;
        private readonly int[] rowIndexes; // Sorted array
        private readonly int[] colIndexes; // Sorted array

        public override int RowsCount { get { return rowIndexes.Length; } }
        public override int ColsCount { get { return colIndexes.Length; } }

        public override double this[int row, int col]
        {
            get
            {
                return matrix[rowIndexes[row], colIndexes[col]];
            }
            set
            {
                matrix[rowIndexes[row], colIndexes[col]] = value;
            }
        }

        public MatrixSubMatrixView(IMatrix matrix, int deleteRow, int deleteCol)
        {
            this.matrix = matrix ?? throw new NullReferenceException();
            rowIndexes = new int[matrix.RowsCount - 1];
            colIndexes = new int[matrix.ColsCount - 1];

            int number = -1;
            for (int i = 0; i < rowIndexes.Length; i++)
            {
                number++;
                if (number == deleteRow) number++;
                rowIndexes[i] = number;
            }

            number = -1;
            for (int i = 0; i < colIndexes.Length; i++)
            {
                number++;
                if (number == deleteCol) number++;
                colIndexes[i] = number;
            }
        }
        private MatrixSubMatrixView(IMatrix matrix, int[] rows, int[] cols)
        {
            this.matrix = matrix ?? throw new NullReferenceException();
            rowIndexes = new int[rows.Length];
            colIndexes = new int[cols.Length];
            Array.Copy(rows, rowIndexes, rowIndexes.Length);
            Array.Copy(cols, colIndexes, colIndexes.Length);
        }

        public override IMatrix Copy()
        {
            return new MatrixSubMatrixView(matrix, rowIndexes, colIndexes);
        }
        public override IMatrix NewInstance(int row, int col)
        {
            return new Matrix(row, col);
        }

    }

}
