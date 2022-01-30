using Mathlib.MathG;
using System;
using System.Collections.Generic;

namespace Mathlib.Graphs.Shapes
{
	public class Circle : Graph
	{
		public double Radius { get; private set; }

		public double Circumference { get { return Math.PI * Radius * 2; } }
		public double Area { get { return Math.PI * Radius * Radius; } }

		public readonly Vector2 center;

		public Circle(int vertices, double radius, double center_x = 0, double center_y = 0)
		{
			center = new Vector2(center_x, center_y);

			// Initialize the vertex array.
			Vertex[] verts = new Vertex[vertices];
			Edge[] edges = new Edge[vertices];

			// Start the loop with i=1 since the 0th vertex has already been created.
			for (int i = 0; i < verts.Length; i++)
			{
				// Create a new vertex.
				verts[i] = new Vertex(i);
				// The new vertex is placed along a circle around the center vertex.
				// As new vertices are created, they are placed at an angle starting at 0 up to just under 2*pi.
				double angle = 2 * Math.PI * ((double)(i - 1) / vertices);
				verts[i].SetProp(Vertex.POS_X, center_x + radius * Math.Cos(angle));
				verts[i].SetProp(Vertex.POS_Y, center_y - radius * Math.Sin(angle));

				if (i > 0)
				{
					// Create an edge from the previous vertex to the new vertex.
					edges[i - 1] = new Edge(verts[i - 1], verts[i]);
				}
			}
			// Add an edge from the last vertex to the first.
			edges[^1] = new Edge(verts[^1], verts[0]);

			Name = "Circle";
			Directed = false;
			Weighted = false;

			Radius = radius;

			foreach (Vertex v in verts)
				AddVertex(v);
			foreach (Edge e in edges)
				AddEdge(e);
		}
	}
}
