namespace Mathlib.Graphs
{
	public class Vertex : PropertyHolder
	{
		public readonly int id;

		public Vertex(int id)
		{
			this.id = id;
		}

		public override string ToString()
		{
			return id.ToString();
		}
	}
}