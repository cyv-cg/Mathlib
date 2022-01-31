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

		public static Graph RandomGraph(int vertices, double edgeDensityPercent = 0.5, double min = -5, double max = 5, int resolution = 1000)
		{
			double radius = 2 * ((double)resolution / 32) + 1;
			Random rand = new Random();

			Vertex[] verts = new Vertex[vertices];

			for (int i = 0; i < vertices; i++)
			{
				verts[i] = new Vertex(i);
				Vector2 pos = new Vector2();

				bool overlaps = true;
				for (int j = 0; j < 1000 && overlaps; j++)
				{
					pos = new Vector2(rand.NextDouble() * (max - min) + min, rand.NextDouble() * (max - min) + min);
					overlaps = false;
					foreach (Vertex v in verts)
					{
						if (v == verts[i])
							continue;
						if (v == null)
							break;
						if ((v.Position - pos).Magnitude <= radius)
						{
							overlaps = true;
							break;
						}
					}
				}
				verts[i].SetProp(Vertex.POS_X, pos.X);
				verts[i].SetProp(Vertex.POS_Y, pos.Y);
			}

			Graph G = new Graph(verts, new Edge[0], "Random Graph", false, false);

			G = G.Triangulate();
			G.Rename("Random Graph");

			Random r = new Random();
			for (int i = G.Edges.Count - 1; i >= 0; i--)
			{
				double next = r.NextDouble();
				if (next < 1d - edgeDensityPercent)
					G.RemoveEdge(G.Edges[i]);
			}

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