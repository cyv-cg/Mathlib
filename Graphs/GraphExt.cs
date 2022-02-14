using System;

using Mathlib.MathG;
using Mathlib.Sys;

namespace Mathlib.Graphs
{
	public static class GraphExt
	{
		public static Vertex[] Incidence(Edge e)
		{
			return new IncidenceData(e).endpoints;
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
		public static Graph RandomGraph(int vertices, double edgeDensityPercent = 0.5, double min = -5, double max = 5, int resolution = 1000)
		{
			// Choose a radius around each vertex where other vertices are prohibitied from being placed. (if possible).
			double radius = resolution / 150d;
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
						if ((v.Position - pos).Magnitude <= radius)
						{
							overlaps = true;
							break;
						}
					}
				}
				// Update the vertex's position properties accordingly.
				verts[i].SetProp(Vertex.POS_X, pos.X);
				verts[i].SetProp(Vertex.POS_Y, pos.Y);
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