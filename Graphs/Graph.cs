using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System;

using Mathlib.Arrays;
using Mathlib.Sys;
using System.Runtime.InteropServices;

namespace Mathlib.Graphs
{
	public class Graph
	{
		#region Properties
		public string Name { get; protected set; }

		public bool Directed { get; protected set; }
		public bool Weighted { get; protected set; }

		public List<Vertex> Vertices { get; private set; }
		public List<Edge> Edges { get; private set; }

		public Dictionary<Vertex, List<Vertex>> AdjList { get; protected set; }

		private string fileName;
		#endregion
		#region Construction
		public Graph() 
		{
			Name = "New Graph";
			Vertices = new List<Vertex>();
			Edges = new List<Edge>();
			AdjList = new Dictionary<Vertex, List<Vertex>>();
			Directed = false;
			Weighted = false;
		}

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

		public Vertex GetVertex(int id)
		{
			foreach (Vertex v in Vertices)
			{
				if (v.Id == id)
					return v;
			}
			return null;
		}
		public Edge GetEdge(Vertex initial, Vertex terminal)
		{
			foreach (Edge e in Edges)
			{
				if ((e.Initial == initial && e.Terminal == terminal) || (e.Initial == terminal && e.Terminal == initial && !Directed))
					return e;
			}
			return null;
		}
		#endregion
		#region Mutators
		public void AddVertex(Vertex v)
		{
			if (AdjList.ContainsKey(v))
			{
				throw new ArgumentException($"{Name} already contains vertex with ID '{v.Id}.'");
				//int maxIndex = -1;
				//foreach (Vertex u in Vertices)
				//{
				//	if (u.Id > maxIndex)
				//		maxIndex = u.Id;
				//}
				//v.ChangeId(maxIndex + 1);
			}
			if (v == null)
				throw new ArgumentNullException($"Vertex cannot be null.");

			AdjList.Add(v, new List<Vertex>());
			Vertices.Add(v);
		}
		public void AddVertices(Vertex[] verts)
		{
			foreach (Vertex v in verts)
				AddVertex(v);
		}
		public void AddEdge(Edge e)
		{
			if (GetEdge(e.Initial, e.Terminal) != null)
				return;

			if (e == null)
				throw new ArgumentNullException($"Edge cannot be null.");

			AdjList[e.Initial].Add(e.Terminal);
			if (!Directed)
				AdjList[e.Terminal].Add(e.Initial);
			Edges.Add(e);
		}
		public void AddEdges(Edge[] edges)
		{
			foreach (Edge e in edges)
				AddEdge(e);
		}

		public void RemoveVertex(Vertex v)
		{
			if (v == null)
				throw new ArgumentNullException($"Vertex cannot be null.");

			if (Vertices.Contains(v))
			{
				for (int i = Edges.Count - 1; i >= 0; i--)
				{
					if (Edges[i].Initial == v || Edges[i].Terminal == v)
						RemoveEdge(Edges[i]);
				}
				AdjList.Remove(v);
				Vertices.Remove(v);
			}
		}
		public void RemoveEdge(Edge e)
		{
			if (e == null)
				throw new ArgumentNullException($"Edge cannot be null.");

			if (Edges.Contains(e))
			{
				AdjList[e.Initial].Remove(e.Terminal);
				if (!Directed)
					AdjList[e.Terminal].Remove(e.Initial);
				Edges.Remove(e);
			}
		}

		public void Rename(string name)
		{
			//string path = fileName.Replace($"{Name}.json", "");

			//if (File.Exists(fileName))
			//	File.Move(fileName, $"{path}{name}.json");

			//if (File.Exists(fileName.Replace(".json", ".svg")))
			//	File.Move(fileName.Replace(".json", ".svg"), $"{path}{name}.json".Replace(".json", ".svg"));
			//if (File.Exists(fileName.Replace(".json", ".png")))
			//	File.Move(fileName.Replace(".json", ".png"), $"{path}{name}.json".Replace(".json", ".png"));
			//if (File.Exists(fileName.Replace(".json", ".pdf")))
			//	File.Move(fileName.Replace(".json", ".pdf"), $"{path}{name}.json".Replace(".json", ".pdf"));

			Name = name;
		}
		#endregion

		/// <summary>
		/// Find a subgraph with a given source vertex and radius.
		/// </summary>
		/// <param name="source">The first vertex added to the subgraph, from which the graph grows.</param>
		/// <param name="radius">Maximum unweighted distance from the source vertex.</param>
		/// <returns></returns>
		public Graph InducedSubgraph(Vertex source, int radius)
		{
			// Initialize relevant properties to the default value.
			foreach (Vertex v in Vertices)
			{
				v.SetProp("radius", int.MaxValue);
				v.SetProp("isInfiniteRadius", true);
			}

			List<Vertex> vertices = new List<Vertex>();
			List<Edge> edges = new List<Edge>();

			// Set the properties of the source vertex.
			source.SetProp("radius", 0);
			source.SetProp("isInfiniteRadius", false);
			// Create a queue which handles the processing order.
			Queue<Vertex> Q = new Queue<Vertex>();
			Q.Enqueue(source);

			// Repeat while there are still vertices in the queue.
			while (Q.Count > 0)
			{
				// Retrieve the next vertex.
				Vertex u = Q.Dequeue();
				// Get all the neighbors of u.
				Vertex[] neighbors = Neighbors(u);
				foreach (Vertex v in neighbors)
				{
					// Only update properties if a shorter path is found.
					// This avoids instances where cycles repeatedly increment their values.
					if (u.GetProp<int>("radius") + 1 < v.GetProp<int>("radius"))
					{
						v.SetProp("radius", u.GetProp<int>("radius") + 1);
						v.SetProp("isInfiniteRadius", u.GetProp<bool>("isInfiniteRadius"));
					}
					// Only consider neighbors that are within the specified radius.
					if (v.GetProp<int>("radius") <= radius)
					{
						// Get the edge connecting u to its neighbor.
						edges.Add(GetEdge(u, v));
						// Enqueue neighbors if they have not already been discovered.
						if (!vertices.Contains(v) && !Q.Contains(v))
							Q.Enqueue(v);
					}
				}
				// Add the fully processed vertex to the list.
				vertices.Add(u);
			}

			return new Graph(vertices.ToArray(), edges.ToArray(), $"Subgraph of {Name} at {source} with radius {radius}", Directed, Weighted);
		}

		#region Pathfinding
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
			fileName = path + "/" + Name.Replace(" ", "_") + ".graph";

			if (!Directory.Exists(path))
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Commands.Cmd($"mkdir {path}");
				}
				else
				{
					throw new NotSupportedException("Unsupported OS");
				}
			}

			if (!File.Exists(fileName))
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					using StreamWriter writer = File.CreateText(fileName);
					writer.WriteLine(JSON(vertexProps, edgeProps));
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Commands.Cmd($"touch {fileName}");
					File.WriteAllText(fileName, JSON(vertexProps, edgeProps));
				}
				else
				{
					throw new NotSupportedException("Unsupported OS");
				}
			}
			else
			{
				File.WriteAllText(fileName, JSON(vertexProps, edgeProps));
			}
			Console.WriteLine($"Saved to {fileName}");
		}

		public void SaveOut(string folder, int resolution, string[] vertProps = null, string[] edgeProps = null)
		{
			Save(Commands.RootFolder + folder, new string[] { Vertex.POS_X, Vertex.POS_Y }.Union(vertProps), edgeProps);
			Commands.CmdOut($"cd {Commands.RootFolder}", $"python3 DrawGraph.py {resolution} _outputs/{Name.Replace(' ', '_')}.graph {folder} true");
		}

		public static Graph Read(string fileName)
		{
			return Parse(new GraphSerializationData(fileName));
		}
		private static Graph Parse(GraphSerializationData data)
		{
			Graph G = new Graph(new Vertex[0], new Edge[0], data.Name, data.Directed, data.Weighted);

			Vertex[] verts = new Vertex[data.Vertices.Count];
			Edge[] edges = new Edge[data.Edges.Count];

			for (int i = 0; i < data.Vertices.Count; i++)
			{
				verts[i] = new Vertex(data.Vertices[i]);
				G.AddVertex(verts[i]);
			}
			for (int i = 0; i < data.Edges.Count; i++)
			{
				edges[i] = new Edge(data.Edges[i], G);
				G.AddEdge(edges[i]);
			}

			return G;
		}

		internal struct GraphSerializationData
		{
			public string Name { get; set; }

			public bool Directed { get; set; }
			public bool Weighted { get; set; }

			public List<Vertex.VertexSerializationData> Vertices { get; set; }
			public List<Edge.EdgeSerializationData> Edges { get; set; }

			public GraphSerializationData(Graph G, string[] vertexProps, string[] edgeProps)
			{
				Name = G.Name;
				Directed = G.Directed;
				Weighted = G.Weighted;

				Vertices = new List<Vertex.VertexSerializationData>();
				Edges = new List<Edge.EdgeSerializationData>();

				if (!Weighted && edgeProps != null)
					edgeProps = edgeProps.Difference(new string[] { Edge.WEIGHT });

				foreach (Vertex v in G.Vertices)
					Vertices.Add(new Vertex.VertexSerializationData(v, vertexProps));
				foreach (Edge e in G.Edges)
					Edges.Add(new Edge.EdgeSerializationData(e, edgeProps));
			}

			public GraphSerializationData(string fileName)
			{
				if (!File.Exists(fileName))
					throw new IOException($"File '{fileName}' not found.");

				string json = File.ReadAllText(fileName);

				GraphSerializationData data = JsonSerializer.Deserialize<GraphSerializationData>(json);

				Name = data.Name;
				Directed = data.Directed;
				Weighted = data.Weighted;

				Vertices = data.Vertices;
				Edges = data.Edges;
			}

			public override string ToString()
			{
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					WriteIndented = true
				};
				return JsonSerializer.Serialize(this, options);
			}
		}
		#endregion
	}
}