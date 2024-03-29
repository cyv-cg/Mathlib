﻿using System.Text.Json;
using System.Collections.Generic;
using System;
using Mathlib.MathG;

namespace Mathlib.Graphs
{
	public class Vertex : PropertyHolder
	{
		public const string POS_X = "xPos";
		public const string POS_Y = "yPos";

		public const string COLOR = "color";

		public int Id { get; private set; }

		public Vector2 Position { get 
			{
				if (!HasProp(POS_X))
					throw new Exception($"Vetrex '{Id}' has no 'x' position.");
				if (!HasProp(POS_Y))
					throw new Exception($"Vertex '{Id}' has no 'y' position.");

				return new Vector2(GetProp<double>(POS_X), GetProp<double>(POS_Y)); 
			} 
		}

		public Vertex(int id)
		{
			Id = id;
		}
		public Vertex(string id) : this(StrToID(id)) { }

		public Vertex(int id, double xPos, double yPos) : this(id)
		{
			SetProp<double>(POS_X, xPos);
			SetProp<double>(POS_Y, yPos);
		}
		public Vertex(string id, double xPos, double yPos) : this(StrToID(id), xPos, yPos) { }

		internal Vertex(VertexSerializationData data)
		{
			Id = data.Id;

			if (data.Properties != null)
			{
				foreach (KeyValuePair<string, object> pair in data.Properties)
				{
					if (double.TryParse(pair.Value.ToString(), out double v1))
					{
						if (pair.Key == POS_Y)
						{
							SetProp<double>(pair.Key, -v1);
						}
						else
						{
							SetProp<double>(pair.Key, v1);
						}

						continue;
					}

					if (int.TryParse(pair.Value.ToString(), out int v2))
					{
						SetProp<int>(pair.Key, v2);
						continue;
					}

					SetProp<string>(pair.Key, pair.Value.ToString());
				}
			}
		}

		public void ChangeId(int newId)
		{
			Id = newId;
		}

		// Convert the numerical ID to alpha characters.
		// For example, 0 = A, 1 = B, ..., 25 = Z, 26 = AA, 27 = AB, ...
		public string IdToAlpha()
		{
			string name = "";

			int index = Id;

			while (index > 25)
			{
				int digit = index / 26;
				name += (char)(digit + 64);
				index -= 26 * digit;
			}
			name += (char)(index + 65);

			return name;
		}
		public static int StrToID(string id)
		{
			id = id.ToUpper();
			int value = 0;
			for (int i = id.Length - 2; i >= 0; i--)
				value += (id[i] - 64) * (int)Math.Pow(26, id.Length - 1 - i);
			value += id[id.Length - 1] - 65;
			return value;
		}

		public override string ToString()
		{
			return IdToAlpha();
		}

		public string JSON()
		{
			return new VertexSerializationData(this).ToString();
		}

		internal struct VertexSerializationData
		{
			public int Id { get; set; }
			public string Name {get; set; }

			public List<KeyValuePair<string, object>> Properties { get; set; }

			public VertexSerializationData(Vertex v, params string[] properties)
			{
				Id = v.Id;
				Name = v.IdToAlpha();

				if (properties != null && properties.Length > 0)
				{
					Properties = new List<KeyValuePair<string, object>>();
					foreach (string key in properties)
					{
						if (v.HasProp(key))
						{
							if (key == POS_Y)
							{
								Properties.Add(new KeyValuePair<string, object>(key, -v.GetProp<double>(POS_Y)));
							}
							else
							{
								Properties.Add(new KeyValuePair<string, object>(key, v.GetProp<object>(key)));
							}
							
						}
					}
				}
				else
				{
					Properties = null;
				}
			}

			public override string ToString()
			{
				return JsonSerializer.Serialize(this);
			}
		}
	}
}