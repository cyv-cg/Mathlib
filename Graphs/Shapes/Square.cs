using System;
using System.Collections.Generic;
using System.Text;
using Mathlib.MathG;

namespace Mathlib.Graphs.Shapes
{
	public class Square : Quad
	{
		public readonly Vector2 center;
		public readonly double sideLength;

		public Square(Vector2 center, double sideLength) : base(new Vertex(0), new Vertex(1), new Vertex(2), new Vertex(3))
		{
			this.center = center;
			this.sideLength = sideLength;

			double halfLength = sideLength / 2;

			Vertices[0].SetProp(Vertex.POS_X, -halfLength + center.x);
			Vertices[0].SetProp(Vertex.POS_Y, halfLength + center.y);

			Vertices[1].SetProp(Vertex.POS_X, halfLength + center.x);
			Vertices[1].SetProp(Vertex.POS_Y, halfLength + center.y);

			Vertices[2].SetProp(Vertex.POS_X, -halfLength + center.x);
			Vertices[2].SetProp(Vertex.POS_Y, -halfLength + center.y);

			Vertices[3].SetProp(Vertex.POS_X, halfLength + center.x);
			Vertices[3].SetProp(Vertex.POS_Y, -halfLength + center.y);
		}

		public Square(Vector center, double sideLength) : this(center.ToVector2(), sideLength) { }
	}
}