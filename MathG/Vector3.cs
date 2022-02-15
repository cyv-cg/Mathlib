using System;

namespace Mathlib.MathG
{
	public class Vector3
	{
		public double x;
		public double y;
		public double z;

		public Vector3() 
		{ 
			x = 0; 
			y = 0; 
			z = 0; 
		}

		public Vector3(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public double Dot(Vector3 b)
		{
			return x * b.x + y * b.y + z * b.z;
		}

		public double Magnitude()
		{
			return Math.Sqrt((x * x) + (y * y) + (z * z));
		}

		public override string ToString()
		{
			return $"<{x}, {y}, {z}>";
		}

		public string Hex()
		{
			string x_hex = ((int)x).ToString("X");
			string y_hex = ((int)y).ToString("X");
			string z_hex = ((int)z).ToString("X");
			int len = Math.Max(Math.Max(x_hex.Length, y_hex.Length), z_hex.Length);

			x_hex = ((int)x).ToString($"X{len}");
			y_hex = ((int)y).ToString($"X{len}");
			z_hex = ((int)z).ToString($"X{len}");

			return $"{x_hex}{y_hex}{z_hex}";
		}

		#region Operators
		public static Vector3 operator -(Vector3 a)
		{
			return new Vector3(-a.x, -a.y, -a.z);
		}
		public static Vector3 operator +(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
		}
		public static Vector3 operator -(Vector3 a, Vector3 b)
		{
			return a + (-b);
		}

		public static Vector3 operator *(int b, Vector3 a)
		{
			return (double)b * a;
		}
		public static Vector3 operator *(double b, Vector3 a)
		{
			return new Vector3(b * a.x, b * a.y, b * a.z);
		}

		public static Vector3 operator *(Vector3 a, int b)
		{
			return b * a;
		}
		public static Vector3 operator /(Vector3 a, int b)
		{
			return a / (double)b;
		}
		public static Vector3 operator *(Vector3 a, double b)
		{
			return b * a;
		}
		public static Vector3 operator /(Vector3 a, double b)
		{
			if (b == 0)
				throw new DivideByZeroException();

			return new Vector3(a.x / b, a.y / b, a.z / b);
		}
		#endregion
	}
}
