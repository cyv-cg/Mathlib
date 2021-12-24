namespace Mathlib.Graphs
{
	public static class GraphExt
	{
		public static Vertex[] Incidence(Edge e)
		{
			return new IncidenceData(e).endpoints;
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