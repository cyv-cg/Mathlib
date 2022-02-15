using System;

namespace Mathlib.MathG
{
	public class Vector2
	{
		public double x;
		public double y;

		public Vector2()
		{
			x = 0;
			y = 0;
		}

		public Vector2(double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		public double Dot(Vector2 b)
		{
			return x * b.x + y * b.y;
		}

		public double Magnitude()
		{
			return Math.Sqrt((x * x) + (y * y));
		}

		public override string ToString()
		{
			return $"<{x}, {y}>";
		}

		#region Operators
		public static Vector2 operator -(Vector2 a)
		{
			return new Vector2(-a.x, -a.y);
		}
		public static Vector2 operator +(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x + b.x, a.y + b.y);
		}
		public static Vector2 operator -(Vector2 a, Vector2 b)
		{
			return a + (-b);
		}

		public static Vector2 operator *(int b, Vector2 a)
		{
			return (double)b * a;
		}
		public static Vector2 operator *(double b, Vector2 a)
		{
			return new Vector2(b * a.x, b * a.y);
		}

		public static Vector2 operator *(Vector2 a, int b)
		{
			return b * a;
		}
		public static Vector2 operator /(Vector2 a, int b)
		{
			return a / (double)b;
		}
		public static Vector2 operator *(Vector2 a, double b)
		{
			return b * a;
		}
		public static Vector2 operator /(Vector2 a, double b)
		{
			if (b == 0)
				throw new DivideByZeroException();

			return new Vector2(a.x / b, a.y / b);
		}
		#endregion
	}
}
