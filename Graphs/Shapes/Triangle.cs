using System;
using Mathlib.MathG;

namespace Mathlib.Graphs.Shapes
{
	public class Triangle : Graph
	{
		public double Perimeter { get 
			{
				double len = 0;
				foreach (Edge e in Edges)
					len += e.Length;
				return len;
			} 
		}

		public double Area { get 
			{
				double s = 0.5 * Perimeter;
				return Math.Sqrt(s * (s - Edges[0].Length) * (s - Edges[1].Length) * (s - Edges[2].Length));
			} 
		}

		public Triangle(Vector2 a, Vector2 b, Vector2 c)
		{
			Vertex A = new Vertex(0);
			Vertex B = new Vertex(1);
			Vertex C = new Vertex(2);

			A.SetProp(Vertex.POS_X, a.x);
			A.SetProp(Vertex.POS_Y, a.y);
			B.SetProp(Vertex.POS_X, b.x);
			B.SetProp(Vertex.POS_Y, b.y);
			C.SetProp(Vertex.POS_X, c.x);
			C.SetProp(Vertex.POS_Y, c.y);

			Edge AB = new Edge(A, B);
			Edge AC = new Edge(A, C);
			Edge BC = new Edge(B, C);

			AddVertex(A);
			AddVertex(B);
			AddVertex(C);
			AddEdge(AB);
			AddEdge(AC);
			AddEdge(BC);

			Name = "Triangle";
			Directed = false;
			Weighted = false;
		}
		public Triangle(Vertex A, Vertex B, Vertex C)
		{
			Edge AB = new Edge(A, B);
			Edge AC = new Edge(A, C);
			Edge BC = new Edge(B, C);

			AddVertex(A);
			AddVertex(B);
			AddVertex(C);
			AddEdge(AB);
			AddEdge(AC);
			AddEdge(BC);

			Name = "Triangle";
			Directed = false;
			Weighted = false;
		}

		public Vector2 Center()
		{
			Vertex E = Vertices[0];
			Vertex F = Vertices[1];
			Vertex G = Vertices[2];

			// The following 3 variables form linear equations from one point to another.
			// For example, the line from E to F is given by: y1 = m1(x-c1)+d1.
			// Find the x value where the difference of the two lines is 0, which is where the lines cross.

			// Get the slope of each line.
			double m1 = GetEdge(E, F).Slope;
			double m2 = GetEdge(E, G).Slope;

			//Console.WriteLine($"m1: {m1}\nm2: {m2}");

			// Get the slope perpendicular to each line.
			// Also make sure there is no division by zero.
			m1 = Math.Abs(m1) > 0.0001 ? -1 / m1 : double.MinValue;
			m2 = Math.Abs(m2) > 0.0001 ? -1 / m2 : double.MinValue;
			// 'c' is the starting x value of the line, which is the average x value between the 2 endpoints.
			double c1 = 0.5 * (E.GetProp<double>(Vertex.POS_X) + F.GetProp<double>(Vertex.POS_X));
			double c2 = 0.5 * (E.GetProp<double>(Vertex.POS_X) + G.GetProp<double>(Vertex.POS_X));
			// Similar to 'c', 'd' is the average y value.
			double d1 = 0.5 * (E.GetProp<double>(Vertex.POS_Y) + F.GetProp<double>(Vertex.POS_Y));
			double d2 = 0.5 * (E.GetProp<double>(Vertex.POS_Y) + G.GetProp<double>(Vertex.POS_Y));

			// Use this formula derived from Newton's method to find the x-coordinate where the two lines meet.
			double x = -(d1 - d2 + m2*c2 - m1*c1) / (m1 - m2);
			// Put that x value in one of the lines to find the y position.
			double y = m2 * (x - c2) + d2;

			return new Vector2(x, y);
		}

		public static Triangle Circumscribe(Square s)
		{
			// Return a triangle perfectly circumscribing the given square with the minimum area.
			// The method and reasoning are here: https://www.desmos.com/calculator/wzof9qq9hg.
			// Be warned: that graph is entirely undocumented. 
			// At one time, the only beings that knew what anything meant were me and God.
			// Now it is only God.
			// Just know that it reveals many assumptions that make this construction much simpler.

			// These are the vertices of the triangle.
			Vertex E = new Vertex(0);
			Vertex F = new Vertex(1);
			Vertex G = new Vertex(2);
			// These are the (required) vertices of the square.
			Vertex A = s.Vertices[0];
			Vertex C = s.Vertices[2];
			Vertex D = s.Vertices[3];

			// Align vertex E in the middle of the square horizontally and exactly the same height as the square above it.
			E.SetProp(Vertex.POS_X, s.center.x);
			E.SetProp(Vertex.POS_Y, A.GetProp<double>(Vertex.POS_Y) + s.sideLength);
			// Align the other 2 vertices of the triangle with the bottom 2 vertices of the square.
			// Also put them half a side-length away horizontally
			F.SetProp(Vertex.POS_X, C.GetProp<double>(Vertex.POS_X) - s.sideLength / 2);
			F.SetProp(Vertex.POS_Y, C.GetProp<double>(Vertex.POS_Y));
			G.SetProp(Vertex.POS_X, D.GetProp<double>(Vertex.POS_X) + s.sideLength / 2);
			G.SetProp(Vertex.POS_Y, D.GetProp<double>(Vertex.POS_Y));

			return new Triangle(E, F, G);
		}

		public override string ToString()
		{
			return $"{Vertices[0]}{Vertices[1]}{Vertices[2]}";
		}
	}
}
