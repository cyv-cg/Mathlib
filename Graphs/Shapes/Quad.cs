using System.Collections.Generic;
using Mathlib.MathG;

namespace Mathlib.Graphs.Shapes
{
	public class Quad : Graph
	{
		public double Perimeter
		{
			get
			{
				double len = 0;
				foreach (Edge e in Edges)
					len += e.Length;
				return len;
			}
		}

		public double Area
		{
			get
			{
				Triangle abc = new Triangle(Vertices[0].Position, Vertices[1].Position, Vertices[2].Position);
				Triangle bcd = new Triangle(Vertices[3].Position, Vertices[1].Position, Vertices[2].Position);
				return abc.Area + bcd.Area;
			}
		}

		public Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			AdjList = new Dictionary<Vertex, List<Vertex>>();

			Vertex A = new Vertex(0);
			Vertex B = new Vertex(1);
			Vertex C = new Vertex(2);
			Vertex D = new Vertex(3);

			A.SetProp(Vertex.POS_X, a.X);
			A.SetProp(Vertex.POS_Y, a.Y);
			B.SetProp(Vertex.POS_X, b.X);
			B.SetProp(Vertex.POS_Y, b.Y);
			C.SetProp(Vertex.POS_X, c.X);
			C.SetProp(Vertex.POS_Y, c.Y);
			D.SetProp(Vertex.POS_X, d.X);
			D.SetProp(Vertex.POS_Y, d.Y);

			Edge AB = new Edge(A, B);
			Edge AC = new Edge(A, C);
			Edge BD = new Edge(B, D);
			Edge CD = new Edge(C, D);

			AddVertex(A);
			AddVertex(B);
			AddVertex(C);
			AddVertex(D);
			AddEdge(AB);
			AddEdge(AC);
			AddEdge(BD);
			AddEdge(CD);

			Name = "Quad";
			Directed = false;
			Weighted = false;
		}

		public Quad(Vertex A, Vertex B, Vertex C, Vertex D)
		{
			AdjList = new Dictionary<Vertex, List<Vertex>>();

			Edge AB = new Edge(A, B);
			Edge AC = new Edge(A, C);
			Edge BD = new Edge(B, D);
			Edge CD = new Edge(C, D);

			AddVertex(A);
			AddVertex(B);
			AddVertex(C);
			AddVertex(D);
			AddEdge(AB);
			AddEdge(AC);
			AddEdge(BD);
			AddEdge(CD);

			Name = "Square";
			Directed = false;
			Weighted = false;

		}
	}
}
