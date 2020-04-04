using System;
using System.Collections.Generic;
using System.Text;

namespace IRGLinearAlgebra
{
    public class MatrixVectorView : AbstractMatrix
    {
        private readonly IVector vector;
        private readonly bool asRowMatrix;

        public override int RowsCount { get => asRowMatrix ? 1 : vector.Dimension; }
        public override int ColsCount { get => asRowMatrix ? vector.Dimension : 1; }

        public override double this[int row, int col]
        {
            get
            {
                if (asRowMatrix && (row != 0 || col < 0 || col >= vector.Dimension)
                    || !asRowMatrix && (col != 0 || row < 0 || row >= vector.Dimension))
                        throw new IndexOutOfRangeException();
                if (asRowMatrix)
                    return vector[col];
                else
                    return vector[row];
            }
            set
            {
                if (asRowMatrix && (row != 0 || col < 0 || col >= vector.Dimension)
                    || !asRowMatrix && (col != 0 || row < 0 || row >= vector.Dimension))
                    throw new IndexOutOfRangeException();
                if (asRowMatrix)
                    vector[col] = value;
                else
                    vector[row] = value;
            }
        }

        public MatrixVectorView(IVector vector, bool asRowMatrix)
        {
            this.vector = vector ?? throw new NullReferenceException();
            this.asRowMatrix = asRowMatrix;
        }
        public override IMatrix NewInstance(int row, int col)
        {
            return new Matrix(row, col);
        }
        public override IMatrix Copy()
        {
            return new MatrixVectorView(vector, asRowMatrix);
        }
    }

    public class VectorMatrixView : AbstractVector
    {
        private readonly IMatrix matrix;
        private readonly bool rowMatrix;

        public override int Dimension => rowMatrix ? matrix.ColsCount : matrix.RowsCount;
        public override double this[int i] 
        { 
            get => rowMatrix ? matrix[0, i] : matrix[i, 0]; 
            set 
            {
                if (rowMatrix) 
                    matrix[0, i] = value; 
                else 
                    matrix[0, 1] = value; 
            } 
        }
        
        public VectorMatrixView (IMatrix matrix)
        {
            if (matrix.RowsCount != 1 && matrix.ColsCount != 1)
                throw new ArgumentException();

            this.matrix = matrix;
            this.rowMatrix = matrix.RowsCount == 1;
        }

        public override IVector Copy()
        {
            return new VectorMatrixView(matrix);
        }

        public override IVector NewInstance(int dimension)
        {
            return new Vector(false, true, new double[dimension]);
        }
    }
}
