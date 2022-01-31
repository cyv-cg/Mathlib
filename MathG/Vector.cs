using System;

namespace Mathlib.MathG
{
	public class Vector
	{
		protected double[] values;

		public int Length { get { return values.Length; } }

		public double Magnitude { get 
			{
				double mag = 0;
				for (int i = 0; i < Length; i++)
					mag += values[i] * values[i];
				return Math.Sqrt(mag);
			} 
		}

		public Vector(int length)
		{
			values = new double[length];
		}
		public Vector(params double[] values)
		{
			this.values = values;
		}

		public double Get(int i)
		{
			return values[i];
		}
		public void Set(int i, double val)
		{
			values[i] = val;
		}

		public double Dot(Vector b)
		{
			if (Length != b.Length)
				throw new Exception("Vectors must me of the same length.");

			double product = 0;
			for (int i = 0; i < Length; i++)
			{
				product += values[i] * b.Get(i);
			}

			return product;
		}

		public override string ToString()
		{
			string str = "<";
			for (int i = 0; i < values.Length; i++)
			{
				str += values[i];
				if (i < values.Length - 1)
					str += ", ";
			}
			str += ">";
			return str;
		}

		public Vector2 ToVector2()
		{
			return new Vector2(values[0], values[1]);
		}

		#region Operators
		public static Vector operator -(Vector a)
		{
			Vector v = new Vector(a.Length);

			for (int i = 0; i < a.Length; i++)
				v.Set(i, -a.Get(i));

			return v;
		}
		public static Vector operator +(Vector a, Vector b)
		{
			if (a.Length != b.Length)
				throw new Exception("Vectors must me of the same length.");

			Vector v = new Vector(a.Length);

			for (int i = 0; i < a.Length; i++)
				v.Set(i, a.Get(i) + b.Get(i));

			return v;
		}
		public static Vector operator -(Vector a, Vector b)
		{
			if (a.Length != b.Length)
				throw new Exception("Vectors must me of the same length.");

			return a + (-b);
		}

		public static Vector operator *(int b, Vector a)
		{
			Vector v = new Vector(a.Length);

			for (int i = 0; i < a.Length; i++)
				v.Set(i, a.Get(i) * b);

			return v;
		}
		public static Vector operator *(double b, Vector a)
		{
			Vector v = new Vector(a.Length);

			for (int i = 0; i < a.Length; i++)
				v.Set(i, a.Get(i) * b);

			return v;
		}

		public static Vector operator *(Vector a, int b)
		{
			Vector v = new Vector(a.Length);

			for (int i = 0; i < a.Length; i++)
				v.Set(i, a.Get(i) * b);

			return v;
		}
		public static Vector operator /(Vector a, int b)
		{
			if (b == 0)
				throw new DivideByZeroException();

			Vector v = new Vector(a.Length);

			for (int i = 0; i < a.Length; i++)
				v.Set(i, a.Get(i) / b);

			return v;
		}
		public static Vector operator *(Vector a, double b)
		{
			Vector v = new Vector(a.Length);

			for (int i = 0; i < a.Length; i++)
				v.Set(i, a.Get(i) * b);

			return v;
		}
		public static Vector operator /(Vector a, double b)
		{
			if (b == 0)
				throw new DivideByZeroException();

			Vector v = new Vector(a.Length);

			for (int i = 0; i < a.Length; i++)
				v.Set(i, a.Get(i) / b);

			return v;
		}
		#endregion
	}
}