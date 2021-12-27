using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System;

namespace Mathlib.Graphs
{
	public class Graph
	{
		public string Name { get; private set; }

		public bool Directed { get; private set; }

		public List<Vertex> Vertices { get; private set; }
		public List<Edge> Edges { get; private set; }

		public Dictionary<Vertex, List<Vertex>> AdjList { get; private set; }
		
		public Graph(Vertex[] vertices, Edge[] edges, bool directed = false, string name = "New Graph")
		{
			this.Name = name;
			this.Directed = directed;

			Vertices = new List<Vertex>();
			Edges = new List<Edge>();
			AdjList = new Dictionary<Vertex, List<Vertex>>();

			foreach (Vertex v in vertices)
				AddVertex(v);
			foreach (Edge e in edges)
				AddEdge(e);
		}

		public void AddVertex(Vertex v)
		{
			AdjList.Add(v, new List<Vertex>());
			Vertices.Add(v);
		}
		public void AddEdge(Edge e)
		{
			AdjList[e.Initial].Add(e.Terminal);
			if (!Directed)
				AdjList[e.Terminal].Add(e.Initial);
			Edges.Add(e);
		}

		public Vertex[] Neighbors(Vertex v)
		{
			return AdjList[v].ToArray();
		}
		public int Degree(Vertex v)
		{
			return Neighbors(v).Length;
		}

		private Edge GetEdge(Vertex initial, Vertex terminal)
		{
			foreach (Edge e in Edges)
			{
				if ((e.Initial == initial && e.Terminal == terminal) || (e.Initial == terminal && e.Terminal == initial && !Directed))
					return e;
			}
			return null;
		}

		// Single-source shortest path finding via Dijkstra's Algorithm.
		// src: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
		public Stack<Vertex> FindPath(Vertex source, Vertex target)
		{
			// Create a stack to store the path as once it is discovered.
			// The stack is used for the LIFO structure, meaning the source vertex is the last thing added.
			Stack<Vertex> path = new Stack<Vertex>();

			// Initialize a new list of vertices.
			List<Vertex> Q = new List<Vertex>();
			// Initialize properties for each vertex in the graph.
			foreach (Vertex v in Vertices)
			{
				// Technically, the distance should be initialized to infinity.
				v.SetProp("distance", int.MaxValue);
				v.SetProp<Vertex>("predecessor", null);
				// Add every vertex to the list under consideration.
				Q.Add(v);
			}
			// Set the distance from the source to the source (that's zero).
			source.SetProp("distance", 0);

			// Loop as long as there are still vertices to visit.
			while (Q.Count > 0)
			{
				// Find which remaining vertex has the shortest distance from the source.
				// These will probably be vertices indicent to one already visited.
				Vertex u = PropertyHolder.ItemWithMinProp<int, Vertex>(Q.ToArray(), "distance");
				// Remove that vertex from consideration.
				Q.Remove(u);

				// Target acquired. PEW PEW PEW!!!
				// Once the target is reached, add all vertices on the path there to the stack by reverse iteration.
				if (u == target)
				{
					// Initialize t as the target vertex.
					Vertex t = u;
					// Until the predecessor is null (i.e. we have reached the source).
					while (t != null)
					{
						// Add vertex to the top of the stack.
						path.Push(t);
						// Set t to its own predecessor.
						t = t.GetProp<Vertex>("predecessor");
					}
					// Return the discovered path.
					// Since the target was reached, we can simply stop here. No need to look any further.
					return path;
				}

				// Check each neighbor of the current vertex.
				foreach (Vertex v in Neighbors(u))
				{
					// Only operate on vertices still within the domain of consideration.
					if (Q.Contains(v))
					{
						// Compute the distance to this vertex.
						// This is done by getting the distance to its predecessor and adding the weight of the edge connecting them.
						int alt = u.GetProp<int>("distance") + GetEdge(u, v).GetProp<int>("weight");
						// If the newly computed distance is less than the existing distance, then a shorter path there has been discovered.
						// Thus we update its properties accordingly.
						if (alt < v.GetProp<int>("distance"))
						{
							// Update its distance and predecessor.
							v.SetProp("distance", alt);
							v.SetProp("predecessor", u);
						}
					}
				}
			}
			// If we get here, then the target was never found.
			return null;
		}

		// Harmonic Centrality of a vertex v.
		// Eq. 3.2 on page 230 of "Axioms for Centrality" (Boldi & Vigna)
		public double HarmonicCentrality(Vertex v)
		{
			double centrality = 0;

			foreach (Vertex u in Vertices)
			{
				// Disregard loops. Those always have a distance of zero.
				if (u == v)
					continue;

				// Find the shortest path from u to v. The total length of the path is stored in the v's "distance" property.
				FindPath(u, v);
				// If the distance from start (u) to finish (v) is less than zero, that means integer overflow has occured.
				// This is because vertices that are disconnected from the rest of the graph have a distance of int.MaxValue.
				// Once the distance from the rest of the path is added to that, it overflows into the negatives.
				// Hence, if the calculated path distance is negative, the actual distance is infinity, so we do not consider it.
				if (v.GetProp<int>("distance") == int.MaxValue || v.GetProp<int>("distance") < 0)
					continue;

				// v's "distance" property is the length of the shortest path between u and v.
				// Add the reciprocal of the distance to the centrality measure.
				centrality += 1d / v.GetProp<int>("distance");
			}

			return centrality;
		}

		// Closeness of a vertex v.
		// Eq. 3.1 on page 229 of "Axioms for Centrality" (Boldi & Vigna)
		public double Closeness(Vertex v, bool patched = true)
		{
			double centrality = 0;
			
			foreach (Vertex u in Vertices)
			{
				if (u == v)
					continue;

				FindPath(u, v);
				// Only disregard infinite distances if we are using the "patched" version of the Closeness equation.
				if (patched && (v.GetProp<int>("distance") == int.MaxValue || v.GetProp<int>("distance") < 0))
					continue;

				centrality += v.GetProp<int>("distance");
			}

			return 1d / centrality;
		}

		public override string ToString()
		{
			// Initialize the string as just the name of the graph.
			string graph = Name + "\n";
			// Loop through each vertex in the graph.
			for (int r = 0; r < AdjList.Count; r++)
			{
				// Start each row with the corresponding vertex pointing to its neighbors.
				graph += $"{Vertices[r]} -> ";
				// Loop through all the vertices adjacent to this vertex.
				Vertex[] adjacency = Neighbors(Vertices[r]);
				for (int c = 0; c < adjacency.Length; c++)
				{
					// Add it to the string.
					graph += $"{adjacency[c]}";
					// Find the edge connecting these vertices.
					Edge RC = GetEdge(Vertices[r], adjacency[c]);
					// If the edge has a weight, print it as well.
					if (RC.HasProp("weight"))
						graph += $"({RC.GetProp<int>("weight")})";
					// Separate with a comma if this is not the last value.
					if (c < adjacency.Length - 1)
						graph += ", ";
				}
				// End this line.
				if (r < AdjList.Count - 1)
					graph += "\n";
			}
			return graph;
		}

		public string JSON(bool writeIndented = true)
		{
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = writeIndented
			};
			return JsonSerializer.Serialize(new GraphSerializationData(this), options);
		}

		public void Save(string path)
		{
			string fileName = path + "/" + Name.Replace(" ", "_") + ".graph";

			if (!File.Exists(fileName))
				File.Create(fileName);
			File.WriteAllText(fileName, JSON());
		}

		private struct GraphSerializationData
		{
			public string Name { get; private set; }

			public bool Directed { get; private set; }

			public List<Vertex.VertexSerializationData> Vertices { get; private set; }
			public List<Edge.EdgeSerializationData> Edges { get; private set; }

			public GraphSerializationData(Graph G)
			{
				Name = G.Name;
				Directed = G.Directed;

				Vertices = new List<Vertex.VertexSerializationData>();
				Edges = new List<Edge.EdgeSerializationData>();

				foreach (Vertex v in G.Vertices)
					Vertices.Add(new Vertex.VertexSerializationData(v, "xPos", "yPos"));
				foreach (Edge e in G.Edges)
					Edges.Add(new Edge.EdgeSerializationData(e, "weight"));
			}

			public override string ToString()
			{
				return JsonSerializer.Serialize(this);
			}
		}
	}
}