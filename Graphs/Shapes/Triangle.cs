using System;
using System.Collections.Generic;
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

			A.SetProp(Vertex.POS_X, a.X);
			A.SetProp(Vertex.POS_Y, a.Y);
			B.SetProp(Vertex.POS_X, b.X);
			B.SetProp(Vertex.POS_Y, b.Y);
			C.SetProp(Vertex.POS_X, c.X);
			C.SetProp(Vertex.POS_Y, c.Y);

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
			E.SetProp(Vertex.POS_X, s.center.X);
			E.SetProp(Vertex.POS_Y, A.GetProp<double>(Vertex.POS_Y) + s.sideLength);
			// Align the other 2 vertices of the triangle with the bottom 2 vertices of the square.
			// Also put them half a side-length away horizontally
			F.SetProp(Vertex.POS_X, C.GetProp<double>(Vertex.POS_X) - s.sideLength / 2);
			F.SetProp(Vertex.POS_Y, C.GetProp<double>(Vertex.POS_Y));
			G.SetProp(Vertex.POS_X, D.GetProp<double>(Vertex.POS_X) + s.sideLength / 2);
			G.SetProp(Vertex.POS_Y, D.GetProp<double>(Vertex.POS_Y));

			return new Triangle(E, F, G);
		}
	}
}
