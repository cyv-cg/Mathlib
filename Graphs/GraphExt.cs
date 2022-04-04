using System;
using System.Collections.Generic;

using Mathlib.MathG;
using Mathlib.Sys;
using Mathlib.Graphs.Shapes;
using Mathlib.Arrays;
using Mathlib.MathG.Colors;

namespace Mathlib.Graphs
{
	public static class GraphExt
	{
		public static Vertex[] Incidence(Edge e)
		{
			return new IncidenceData(e).endpoints;
		}

		public static Matrix AdjMat(this Graph G)
		{
			int V = G.Vertices.Count;
			Matrix adj = new Matrix(V, V);

			foreach (Edge e in G.Edges)
			{
				adj.Set(e.Initial.Id, e.Terminal.Id, 1);
				if (!G.Directed)
					adj.Set(e.Terminal.Id, e.Initial.Id, 1);
			}

			return adj;
		}

		public static void ColorByPropDouble(this Graph G, string doubleProp, Gradient gradient = null, bool scale = false)
		{
			if (gradient == null)
				gradient = Gradient.Rainbow;

			double minValue = PropertyHolder.ItemWithMinProp<double, Vertex>(G.Vertices.ToArray(), doubleProp).GetProp<double>(doubleProp);
			double maxValue = PropertyHolder.ItemWithMaxProp<double, Vertex>(G.Vertices.ToArray(), doubleProp).GetProp<double>(doubleProp);
			foreach (Vertex v in G.Vertices)
			{
				double percent = scale switch
				{
					false => Math.Abs(v.GetProp<double>(doubleProp) / maxValue),
					true => (Math.Abs(v.GetProp<double>(doubleProp)) - minValue) / (maxValue - minValue)
				};
				v.SetProp(Vertex.COLOR, gradient.Evaluate(percent).Hex());
			}
		}
		public static void ColorByPropInt(this Graph G, string intProp, Gradient gradient = null)
		{
			if (gradient == null)
				gradient = Gradient.Rainbow;

			int maxValue = PropertyHolder.ItemWithMaxProp<int, Vertex>(G.Vertices.ToArray(), intProp).GetProp<int>(intProp);
			foreach (Vertex v in G.Vertices)
			{
				double percent = Math.Abs(v.GetProp<int>(intProp) / (double)maxValue);
				v.SetProp(Vertex.COLOR, gradient.Evaluate(percent).Hex());
			}
		}

		/// <summary>
		/// Create a Delaunay Triangulation of the graph using the Bowyer-Watson algorithm.
		/// https://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm
		/// </summary>
		/// <returns></returns>
		public static Graph Triangulate(this Graph G)
		{
			List<Triangle> tris = new List<Triangle>();

			double x_max = int.MinValue;
			double x_min = int.MaxValue;
			double y_max = int.MinValue;
			double y_min = int.MaxValue;
			// Find the extreme points of the bounds.
			foreach (Vertex v in G.Vertices)
			{
				if (v.Position.x < x_min)
					x_min = v.Position.x;
				if (v.Position.x > x_max)
					x_max = v.Position.x;

				if (v.Position.y < y_min)
					y_min = v.Position.y;
				if (v.Position.y > y_max)
					y_max = v.Position.y;
			}

			// Add some buffer to the extreme coordinates so they are slightly separated from the points.
			x_max += 1;
			x_min -= 1;
			y_max += 1;
			y_min -= 1;

			// Square bounds covering all points.
			Square squareBounds = new Square(0.5 * new Vector2(x_min + x_max, y_min + y_max), Math.Max(Math.Abs(x_max - x_min), Math.Abs(y_max - y_min)));
			// Use that square to create a triangle that also covers all points.
			Triangle superTriangle = Triangle.Circumscribe(squareBounds);
			superTriangle.Rename("Super Triangle");
			// Add the super triangle to the list of triangles.
			tris.Add(superTriangle);

			// Create a new graph that will store the triangulation.
			Graph triangulation = GraphExt.Union(G, superTriangle);
			triangulation.Rename("Triangulation");

			LoadingBar bar = new LoadingBar("Triangulating", G.Vertices.Count);

			// Add all points one at a time to the triangulation.
			for (int i = 0; i < G.Vertices.Count; i++)
			{
				bar.Progress = i + 1;

				List<Triangle> badTriangles = new List<Triangle>();
				// First find all the triangles that are no longer valid due to the insertion.
				foreach (Triangle t in tris)
				{
					Circle circumcircle = Circle.Circumscribe(t);
					if (circumcircle.ContainsPoint(G.Vertices[i]))
						badTriangles.Add(t);
				}
				List<Edge> polygon = new List<Edge>();
				// Find the boundary of the polygonal hole.
				foreach (Triangle t in badTriangles)
				{
					foreach (Edge e in t.Edges)
					{
						bool isShared = false;
						foreach (Triangle s in badTriangles)
						{
							if (s == t)
								continue;
							// Find all edges that are shared between 2 or more bad triangles.
							foreach (Edge f in s.Edges)
							{
								if ((f.Initial == e.Initial && f.Terminal == e.Terminal) || (f.Terminal == e.Initial && f.Initial == e.Terminal))
								{
									isShared = true;
									break;
								}
							}
						}
						if (!isShared)
							polygon.Add(e);
					}
				}
				// Remove them from the graph.
				foreach (Triangle t in badTriangles)
					tris.Remove(t);
				// The polygon stores edges that are sill valid to use, so put them back.
				foreach (Edge e in polygon)
				{
					Triangle newTri = new Triangle(e.Initial, e.Terminal, G.Vertices[i]);
					tris.Add(newTri);
				}
			}
			// Add every edge from the valid triangles to the triangulation.
			foreach (Triangle t in tris)
			{
				foreach (Edge e in t.Edges)
					triangulation.AddEdge(e);
			}
			// Done inserting, now clean up.
			foreach (Vertex v in superTriangle.Vertices)
				triangulation.RemoveVertex(v);

			return triangulation;
		}

		public static Graph Union(params Graph[] graphs)
		{
			Vertex[] verts = new Vertex[0];
			Edge[] edges = new Edge[0];
			int numVerts = 0;

			foreach (Graph H in graphs)
			{
				foreach (Vertex v in H.Vertices)
					v.ChangeId(v.Id + numVerts);
				numVerts += H.Vertices.Count;
				verts = verts.Union(H.Vertices.ToArray());
				edges = edges.Union(H.Edges.ToArray());
			}
			return new Graph(verts, edges);
		}

		/// <summary>
		/// Create a graph with one vertex at the center, connected to a specified number of other vertices.
		/// </summary>
		/// <param name="spokes">Number of surrounding vertices.</param>
		/// <returns></returns>
		public static Graph CreateWheelGraph(int spokes, bool directed = false, string name = "New Wheel Graph")
		{
			// Initialize the vertex array with extra length to account for the center vertex.
			Vertex[] verts = new Vertex[spokes + 1];
			Edge[] edges = new Edge[spokes];

			// Create the center vertex.
			verts[0] = new Vertex(0);
			// This vertex is at the geometric origin, (0, 0).
			verts[0].SetProp(Vertex.POS_X, 0.0);
			verts[0].SetProp(Vertex.POS_Y, 0.0);

			// Start the loop with i=1 since the 0th vertex has already been created.
			for (int i = 1; i < verts.Length; i++)
			{
				// Create a new vertex.
				verts[i] = new Vertex(i);
				// The new vertex is placed along a circle around the center vertex.
				// As new vertices are created, they are placed at an angle starting at 0 up to just under 2*pi.
				double angle = 2 * Math.PI * ((double)(i - 1) / spokes);
				verts[i].SetProp(Vertex.POS_X, Math.Cos(angle));
				verts[i].SetProp(Vertex.POS_Y, Math.Sin(angle));
				// Create an edge from the center vertex to the new vertex.
				// The subscript offset is because the vertex array is 1 ahead of the edge array.
				edges[i - 1] = new Edge(verts[0], verts[i]);
			}
			// Once these properties are set, it is as simple as creating any other graph.
			return new Graph(verts, edges, name, directed);
		}

		/// <summary>
		/// Generate a random Graph.
		/// </summary>
		/// <param name="vertices">Number of vertices in the graph.</param>
		/// <param name="edgeDensityPercent">[0,1] Percent of vertex pairs that have an edge between them.</param>
		/// <param name="min">Minimum x/y coordinate where vertices are placed.</param>
		/// <param name="max">Maximum x/y coordinate where vertices are places.</param>
		/// <param name="resolution">Intended size of the output image.</param>
		/// <returns></returns>
		public static Graph RandomGraph(int vertices, double edgeDensityPercent = 0.5, double min = -5, double max = 5)
		{
			// Choose a radius around each vertex where other vertices are prohibitied from being placed. (if possible).
			double radius = 0.35d;
			// Create a new random object to handle the random number generation.
			Random rand = new Random();

			// Initialize an array of vertices to store each generated vertex.
			Vertex[] verts = new Vertex[vertices];
			// Create a loading bar to display the progress of the vertex placement.
			LoadingBar bar = new LoadingBar("Placing Vertices", vertices);

			for (int i = 0; i < vertices; i++)
			{
				// Set the loading bar progress to the representative value.
				bar.Progress = i + 1;

				// Create a new vertex.
				verts[i] = new Vertex(i);
				// Initialize a vector to store the vertex position.
				Vector2 pos = new Vector2();

				// Keep track of if this vertex overlaps with any other.
				bool overlaps = true;
				// Try at most 1000 times to place this vertex.
				// If at any point the vertex no longer overlaps any other, or it
				// exceeds 1000 attempts, the loop ends and the vertex is left overlapping another.
				for (int j = 0; j < 1000 && overlaps; j++)
				{
					// Choose random coordinates within the min and max for the position vector.
					pos = new Vector2(rand.NextDouble() * (max - min) + min, rand.NextDouble() * (max - min) + min);
					// Assume this no longer (if it ever did) overlaps another vertex.
					overlaps = false;
					// Check every other vertex to determine if they overlap.
					foreach (Vertex v in verts)
					{
						// Don't compare this vertex to itself.
						if (v == verts[i])
							continue;
						// If v is null, that means there are no other vertices to compare to.
						// This is because the 'verts' array is initialized with null values,
						// so if a null value is dicovered, then there is nothing after it either.
						if (v == null)
							break;
						// If this position is within a radius of another vertex, then they overlap.
						if ((v.Position - pos).Magnitude() <= radius)
						{
							overlaps = true;
							break;
						}
					}
				}
				// Update the vertex's position properties accordingly.
				verts[i].SetProp(Vertex.POS_X, pos.x);
				verts[i].SetProp(Vertex.POS_Y, pos.y);
			}

			// Create a new Graph object to store these vertices.
			Graph G = new Graph(verts, new Edge[0], "Random Graph", false, false);
			// Create a Delaunay triangulation of these vertices, so that none of the edges overlap.
			G = G.Triangulate();
			G.Rename("Random Graph");
			// Randomly go through the graph and remove some of the edges.
			// Only do this if the edge density is less than one, meaning not every edge will be kept.
			if (edgeDensityPercent < 1)
			{
				Random r = new Random();
				for (int i = G.Edges.Count - 1; i >= 0; i--)
				{
					double next = r.NextDouble();
					if (next < 1d - edgeDensityPercent)
						G.RemoveEdge(G.Edges[i]);
				}
			}
			// Now we have a visually appealing random graph!
			return G;
		}

		public static Graph Grid(Graph G)
		{
			Vertex leftmost = PropertyHolder.ItemWithMinProp<double, Vertex>(G.Vertices.ToArray(), Vertex.POS_X);
			Vertex rightmost = PropertyHolder.ItemWithMaxProp<double, Vertex>(G.Vertices.ToArray(), Vertex.POS_X);
			Vertex downmost = PropertyHolder.ItemWithMinProp<double, Vertex>(G.Vertices.ToArray(), Vertex.POS_Y);
			Vertex upmost = PropertyHolder.ItemWithMaxProp<double, Vertex>(G.Vertices.ToArray(), Vertex.POS_Y);

			Vertex A = new Vertex(0);
			Vertex B = new Vertex(1);
			Vertex C = new Vertex(2);
			Vertex D = new Vertex(3);

			A.SetProp(Vertex.POS_X, Math.Min(leftmost.Position.x, 0));
			A.SetProp<double>(Vertex.POS_Y, 0);
			B.SetProp(Vertex.POS_X, Math.Max(rightmost.Position.x, 0));
			B.SetProp<double>(Vertex.POS_Y, 0);

			C.SetProp<double>(Vertex.POS_X, 0);
			C.SetProp(Vertex.POS_Y, Math.Min(downmost.Position.y, 0));
			D.SetProp<double>(Vertex.POS_X, 0);
			D.SetProp(Vertex.POS_Y, Math.Max(upmost.Position.y, 0));

			return new Graph(new Vertex[] { A, B, C, D }, new Edge[] { new Edge(A, B), new Edge(C, D) }, "Grid", false, false);
		}
	}

	public class IncidenceData
	{
		public readonly Vertex[] endpoints;
		public readonly EdgeType type;

		public IncidenceData(Edge e)
		{
			endpoints = new Vertex[2] { e.Initial, e.Terminal };

			if (e.Initial == e.Terminal)
				type = EdgeType.Loop;
			else
				type = EdgeType.Link;
		}
	}

	public enum EdgeType
	{
		Loop,
		Link
	}
}