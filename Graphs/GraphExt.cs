using System;

namespace Mathlib.Graphs
{
	public static class GraphExt
	{
		public static Vertex[] Incidence(Edge e)
		{
			return new IncidenceData(e).endpoints;
		}

		public static Graph RandomGraph(int vertices, double min = -5, double max = 5)
		{
			Random rand = new Random();

			Vertex[] verts = new Vertex[vertices];

			for (int i = 0; i < vertices; i++)
			{
				verts[i] = new Vertex(i);
				verts[i].SetProp(Vertex.POS_X, rand.NextDouble() * (max - min) + min);
				verts[i].SetProp(Vertex.POS_Y, rand.NextDouble() * (max - min) + min);
			}

			return new Graph(verts, new Edge[0], "Random Graph", false, false);
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