namespace Mathlib.Graphs
{
	public class Edge : PropertyHolder
	{
		public readonly Vertex initial;
		public readonly Vertex terminal;

		public Edge(Vertex initial, Vertex terminal)
		{
			this.initial = initial;
			this.terminal = terminal;
		}
	}
}