using System.Text.Json;
using System.Collections.Generic;
using System;
using Mathlib.MathG;

namespace Mathlib.Graphs
{
	public class Edge : PropertyHolder
	{
		public const string WEIGHT = "weight";

		public Vertex Initial { get; private set; }
		public Vertex Terminal { get; private set; }

		public double Slope { get
			{
				Vector2 s = Initial.Position;
				Vector2 e = Terminal.Position;

				return (e.y - s.y) / (e.x - s.x);
			}
		}

		public double Length { get
			{
				return (Initial.Position - Terminal.Position).Magnitude();
			}
		}

		public Edge(Vertex initial, Vertex terminal)
		{
			Initial = initial;
			Terminal = terminal;
		}

		public string JSON()
		{
			return new EdgeSerializationData(this).ToString();
		}

		public override string ToString()
		{
			return $"{Initial} -> {Terminal}";
		}

		internal struct EdgeSerializationData
		{
			public int Initial { get; private set; }
			public int Terminal { get; private set; }

			public List<KeyValuePair<string,object>> Properties { get; private set; }

			public EdgeSerializationData(Edge e, params string[] properties)
			{
				Initial = e.Initial.Id;
				Terminal = e.Terminal.Id;

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