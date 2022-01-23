using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System;

namespace Mathlib.Graphs
{
	public class Graph
	{
		#region Properties
		public string Name { get; private set; }

		public bool Directed { get; private set; }
		public bool Weighted { get; private set; }

		public List<Vertex> Vertices { get; private set; }
		public List<Edge> Edges { get; private set; }

		public Dictionary<Vertex, List<Vertex>> AdjList { get; private set; }
		#endregion
		#region Construction
		public Graph(Vertex[] vertices, Edge[] edges, string name = "New Graph", bool directed = false, bool weighted = false)
		{
			Name = name;
			Directed = directed;
			Weighted = weighted;

			Vertices = new List<Vertex>();
			Edges = new List<Edge>();
			AdjList = new Dictionary<Vertex, List<Vertex>>();

			foreach (Vertex v in vertices)
				AddVertex(v);
			foreach (Edge e in edges)
				AddEdge(e);
		}

		/// <summary>
		/// Create a graph with one vertex at the center, connected to a specified number of other vertices.
		/// </summary>
		/// <param name="spokes">Number of surrounding vertices.</param>
		/// <returns></returns>
		public static Graph CreateWheelGraph(int spokes, bool directed = false, string name = "New Wheel Graph")
		{
			// Initialize the vertex array with extra length to account for the center vertex.
			Vertex[] verts = new Vertex[spokes + 1];
			Edge[] edges = new Edge[spokes];

			// Create the center vertex.
			verts[0] = new Vertex(0);
			// This vertex is at the geometric origin, (0, 0).
			verts[0].SetProp(Vertex.POS_X, 0.0);
			verts[0].SetProp(Vertex.POS_Y, 0.0);

			// Start the loop with i=1 since the 0th vertex has already been created.
			for (int i = 1; i < verts.Length; i++)
			{
				// Create a new vertex.
				verts[i] = new Vertex(i);
				// The new vertex is placed along a circle around the center vertex.
				// As new vertices are created, they are placed at an angle starting at 0 up to just under 2*pi.
				double angle = 2 * Math.PI * ((double)(i - 1) / spokes);
				verts[i].SetProp(Vertex.POS_X, Math.Cos(angle));
				verts[i].SetProp(Vertex.POS_Y, -Math.Sin(angle));
				// Create an edge from the center vertex to the new vertex.
				// The subscript offset is because the vertex array is 1 ahead of the edge array.
				edges[i - 1] = new Edge(verts[0], verts[i]);
			}
			// Once these properties are set, it is as simple as creating any other graph.
			return new Graph(verts, edges, name, directed);
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
		#endregion

		#region Accessors
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
		#endregion

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
				// For principle, set the distance as high as it can get.
				v.SetProp("distance", double.MaxValue);
				// Explicitly mark the distance as infinite.
				v.SetProp("isInfiniteDistance", true);
				v.SetProp<Vertex>("predecessor", null);
				// Add every vertex to the list under consideration.
				Q.Add(v);
			}
			// Set the distance from the source to the source (that's zero).
			source.SetProp("distance", 0.0);
			// 0 is a finite number.
			source.SetProp("isInfiniteDistance", false);

			// Loop as long as there are still vertices to visit.
			while (Q.Count > 0)
			{
				// Find which remaining vertex has the shortest distance from the source.
				// These will probably be vertices indicent to one already visited.
				Vertex u = PropertyHolder.ItemWithMinProp<double, Vertex>(Q.ToArray(), "distance");
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
						double weight = 1;
						// Use edge weight ONLY IF this is a weighted graph.
						// Otherwise, treat the weight as 1.
						if (Weighted && GetEdge(u, v).HasProp(Edge.WEIGHT))
							weight = GetEdge(u, v).GetProp<double>(Edge.WEIGHT);
						double alt = u.GetProp<double>("distance") + weight;
						// If the newly computed distance is less than the existing distance, then a shorter path there has been discovered.
						// Thus we update its properties accordingly.
						if (alt < v.GetProp<double>("distance") || v.GetProp<bool>("isInfiniteDistance"))
						{
							// Update its distance and predecessor.
							v.SetProp("distance", alt);
							// If the predecessor is infinitely far away, then this one is as well.
							// Otherwise, this node is also a finite distance.
							v.SetProp("isInfiniteDistance", u.GetProp<bool>("isInfiniteDistance"));
							v.SetProp("predecessor", u);
						}
					}
				}
			}
			// If we get here, then the target was never found.
			return null;
		}

		public Graph BreadthFirstSearch(Vertex source)
		{
			// Create a list to store the every vertex that gets fully processed.
			List<Vertex> tree = new List<Vertex>();

			// Initialize a new queue of vertices.
			Queue<Vertex> Q = new Queue<Vertex>();
			// Initialize properties for each vertex in the graph.
			foreach (Vertex v in Vertices)
			{
				// Default every vertex to be undiscovered.
				v.SetProp("state", "undiscovered");
				// For principle, set the distance as high as it can get.
				v.SetProp("distance", double.MaxValue);
				// Explicitly mark the distance as infinite.
				v.SetProp("isInfiniteDistance", true);
				v.SetProp<Vertex>("predecessor", null);
			}
			// Since we start at the source, it is discovered.
			source.SetProp("state", "discovered");
			// Set the distance from the source to the source (that's zero).
			source.SetProp("distance", 0.0);
			// 0 is a finite number.
			source.SetProp("isInfiniteDistance", false);
			// Add only the source vertex to the queue.
			Q.Enqueue(source);

			while (Q.Count > 0)
			{
				Vertex u = Q.Dequeue();
				foreach (Vertex v in Neighbors(u))
				{
					// Only check vertices that have not been discovered yet.
					if (v.GetProp<string>("state") == "undiscovered")
					{
						// Mark this vertex as discovered.
						v.SetProp("state", "discovered");

						// Compute the distance to this vertex.
						// This is done by getting the distance to its predecessor and adding the weight of the edge connecting them.
						double weight = 1;
						// Use edge weight ONLY IF this is a weighted graph.
						// Otherwise, treat the weight as 1.
						Edge e = GetEdge(u, v);
						if (Weighted && e.HasProp(Edge.WEIGHT))
							weight = e.GetProp<double>(Edge.WEIGHT);
						double alt = u.GetProp<double>("distance") + weight;
						// Set the distance of this vertex from the source.
						v.SetProp("distance", alt);

						// Keep track of this vertex's predecessor.
						v.SetProp("predecessor", u);
						// Add this vertex to the queue so its neighbors can be checked later.
						Q.Enqueue(v);
					}
				}
				// Once all of u's neighbors have been discovered, it has been fully processed.
				u.SetProp("state", "processed");
				// Add the processed vertex to the list.
				tree.Add(u);
			}

			// Create a new list of edges.
			// This will store every edge that is part of the BFS tree.
			List<Edge> edges = new List<Edge>();

			foreach (Vertex v in tree)
			{
				// Ignore any vertex that doesn't have a predecessor. (the source vertex)
				// The predecessor of the source is null, so this check is necessary.
				if (v.GetProp<Vertex>("predecessor") == null)
					continue;
				
				// Find the edge that connects v and its predecessor.
				// This *should* be unique, since a vertex can't have more than one predecessor.
				Edge e = GetEdge(v.GetProp<Vertex>("predecessor"), v);
				edges.Add(e);
			}

			// Return a new graph with the discovered vertices and edges.
			// This is the BFS tree staring at the source vertex.
			return new Graph(tree.ToArray(), edges.ToArray(), $"BFS Tree of {Name} from {source}", Directed, Weighted);
		}

		#region Centrality measures
		// Harmonic Centrality of a vertex v.
		// Eq. 3.2 on page 230 of "Axioms for Centrality" (Boldi & Vigna)
		public double HarmonicCentrality(Vertex v)
		{
			double centrality = 0;

			foreach (Vertex u in Vertices)
			{
				// Disregard loops. Those always have a minimum distance of zero.
				if (u == v)
					continue;

				// Find the shortest path from u to v. The total length of the path is stored in the v's "distance" property.
				FindPath(u, v);
				// Ignore if the distance is explicitly stated as infinite.
				if (v.GetProp<bool>("isInfiniteDistance"))
					continue;

				// v's "distance" property is the length of the shortest path between u and v.
				// Add the reciprocal of the distance to the centrality measure.
				centrality += 1d / v.GetProp<double>("distance");
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
				if (patched && v.GetProp<bool>("isInfiniteDistance"))
					continue;

				centrality += v.GetProp<double>("distance");
			}

			return 1d / centrality;
		}
		#endregion

		#region String manipulation
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
					if (Weighted && RC.HasProp(Edge.WEIGHT))
						graph += $"({RC.GetProp<double>(Edge.WEIGHT)})";
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

		public string JSON(string[] vertexProps, string[] edgeProps, bool writeIndented = true)
		{
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = writeIndented
			};
			return JsonSerializer.Serialize(new GraphSerializationData(this, vertexProps, edgeProps), options);
		}

		public void Save(string path, string[] vertexProps = null, string[] edgeProps = null)
		{
			string fileName = path + "/" + Name.Replace(" ", "_") + ".json";

			if (!File.Exists(fileName))
			{
				using StreamWriter writer = File.CreateText(fileName);
				writer.WriteLine(JSON(vertexProps, edgeProps));
			}
			else
			{
				File.WriteAllText(fileName, JSON(vertexProps, edgeProps));
			}
		}

		private struct GraphSerializationData
		{
			public string Name { get; private set; }

			public bool Directed { get; private set; }
			public bool Weighted { get; private set; }

			public List<Vertex.VertexSerializationData> Vertices { get; private set; }
			public List<Edge.EdgeSerializationData> Edges { get; private set; }

			public GraphSerializationData(Graph G, string[] vertexProps, string[] edgeProps)
			{
				Name = G.Name;
				Directed = G.Directed;
				Weighted = G.Weighted;

				Vertices = new List<Vertex.VertexSerializationData>();
				Edges = new List<Edge.EdgeSerializationData>();

				foreach (Vertex v in G.Vertices)
					Vertices.Add(new Vertex.VertexSerializationData(v, vertexProps));
				foreach (Edge e in G.Edges)
					Edges.Add(new Edge.EdgeSerializationData(e, edgeProps));
			}

			public override string ToString()
			{
				return JsonSerializer.Serialize(this);
			}
		}
		#endregion
	}
}