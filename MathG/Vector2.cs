namespace Mathlib.MathG
{
	public class Vector2 : Vector
	{
		public double X { get { return values[0]; } }
		public double Y { get { return values[1]; } }

		public Vector2() : base(0, 0) { }
		public Vector2(double x, double y) : base(x, y) { }
	}
}
