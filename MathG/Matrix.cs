using System;

namespace Mathlib.MathG
{
	public class Matrix
	{
		public int M { get; }
		public int N { get; }

		private readonly double[,] mat;

		public Matrix(int m, int n)
		{
			M = m;
			N = n;
			mat = new double[M, N];
		}

		public double Get(int i, int j)
		{
			return mat[i, j];
		}
		public void Set(int i, int j, double v)
		{
			mat[i, j] = v;
		}

		public static Matrix Pow(Matrix A, int power)
		{
			Matrix result = A;
			for (int i = 0; i < power; i++)
			{
				result *= A;
			}
			return result;
		}

		public static Matrix operator *(Matrix A, Matrix B)
		{
			if (A.N != B.M)
				throw new ArgumentException();

			Matrix result = new Matrix(A.M, B.N);

			for (int i = 0; i < A.M; i++)
			{
				Vector row = new Vector(A.N);
				for (int k = 0; k < A.N; k++)
					row.Set(k, A.Get(i, k));

				for (int j = 0; j < B.N; j++)
				{
					Vector col = new Vector(B.M);
					for (int k = 0; k < B.M; k++)
						col.Set(k, B.Get(k, j));

					result.Set(i, j, row.Dot(col));
				}
			}

			return result;
		}
	}
}