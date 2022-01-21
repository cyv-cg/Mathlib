using System.Text.Json;
using System.Collections.Generic;

namespace Mathlib.Graphs
{
	public class Edge : PropertyHolder
	{
		public Vertex Initial { get; private set; }
		public Vertex Terminal { get; private set; }

		public Edge(Vertex initial, Vertex terminal)
		{
			Initial = initial;
			Terminal = terminal;
		}

		protected internal struct EdgeSerializationData
		{
			public Vertex Initial { get; private set; }
			public Vertex Terminal { get; private set; }

			public List<KeyValuePair<string,object>> Properties { get; private set; }

			public EdgeSerializationData(Edge e, params string[] properties)
			{
				Initial = e.Initial;
				Terminal = e.Terminal;

				if (properties != null && properties.Length > 0)
				{
					Properties = new List<KeyValuePair<string, object>>();
					foreach (string key in properties)
						if (e.HasProp(key))
							Properties.Add(new KeyValuePair<string, object>(key, e.GetProp<object>(key)));
				}
				else
					Properties = null;
			}

			public override string ToString()
			{
				return JsonSerializer.Serialize(this);
			}
		}
	}
}