using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System;

using Mathlib.Arrays;
using Mathlib.Graphs.Shapes;
using Mathlib.MathG;
using Mathlib.Sys;

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

		public static Graph Union(params Graph[] graphs)
		{
			Vertex[] verts = new Vertex[0];
			Edge[] edges = new Edge[0];
			int numVerts = 0;

			foreach (Graph G in graphs)
			{
				foreach (Vertex v in G.Vertices)
					v.ChangeId(v.Id + numVerts);
				numVerts += G.Vertices.Count;
				verts = verts.Union(G.Vertices.ToArray());
				edges = edges.Union(G.Edges.ToArray());
			}
			return new Graph(verts, edges);
		}

		public static Graph Grid(Graph G)
		{
			Vertex leftmost = PropertyHolder.ItemWithMinProp<double, Vertex>(G.Vertices.ToArray(), Vertex.POS_X);
			Vertex rightmost = PropertyHolder.ItemWithMaxProp<double, Vertex>(G.Vertices.ToArray(), Vertex.POS_X);
			Vertex downmost = PropertyHolder.ItemWithMinProp<double, Vertex>(G.Vertices.ToArray(), Vertex.POS_Y);
			Vertex upmost = PropertyHolder.ItemWithMaxProp<double, Vertex>(G.Vertices.ToArray(), Vertex.POS_Y);

			Vertex A = new Vertex(0);
			Vertex B = new Vertex(1);
			Vertex C = new Vertex(2);
			Vertex D = new Vertex(3);

			A.SetProp(Vertex.POS_X, Math.Min(leftmost.Position.X, 0));
			A.SetProp<double>(Vertex.POS_Y, 0);
			B.SetProp(Vertex.POS_X, Math.Max(rightmost.Position.X, 0));
			B.SetProp<double>(Vertex.POS_Y, 0);

			C.SetProp<double>(Vertex.POS_X, 0);
			C.SetProp(Vertex.POS_Y, Math.Min(downmost.Position.Y, 0));
			D.SetProp<double>(Vertex.POS_X, 0);
			D.SetProp(Vertex.POS_Y, Math.Max(upmost.Position.Y, 0));

			return new Graph(new Vertex[] {A,B,C,D}, new Edge[] {new Edge(A, B), new Edge(C,D)}, "Grid", false, false);
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

		protected Edge GetEdge(Vertex initial, Vertex terminal)
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
		public Graph Subgraph(Vertex source, int radius)
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

		// Create a Delaunay Triangulation of the graph using the Bowyer-Watson algorithm.
		// https://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm
		public Graph Triangulate()
		{
			List<Triangle> tris = new List<Triangle>();

			double x_max = int.MinValue;
			double x_min = int.MaxValue;
			double y_max = int.MinValue;
			double y_min = int.MaxValue;
			// Find the extreme points of the bounds.
			foreach (Vertex v in Vertices)
			{
				if (v.Position.X < x_min)
					x_min = v.Position.X;
				if (v.Position.X > x_max)
					x_max = v.Position.X;

				if (v.Position.Y < y_min)
					y_min = v.Position.Y;
				if (v.Position.Y > y_max)
					y_max = v.Position.Y;
			}

			// Add some buffer to the extreme coordinates so they are slightly separated from the points.
			x_max += 1;
			x_min -= 1;
			y_max += 1;
			y_min -= 1;

			// Square bounds covering all points.
			Square squareBounds = new Square(0.5 * new Vector2(x_min + x_max, y_min + y_max), Math.Max(Math.Abs(x_max - x_min), Math.Abs(y_max - y_min)));
			// Use that square to create a triangle that also covers all points.
			Triangle superTriangle = Triangle.Circumscribe(squareBounds);
			superTriangle.Rename("Super Triangle");
			// Add the super triangle to the list of triangles.
			tris.Add(superTriangle);

			// Create a new graph that will store the triangulation.
			Graph triangulation = Union(this, superTriangle);
			triangulation.Rename("Triangulation");

			LoadingBar bar = new LoadingBar("Triangulating", Vertices.Count);

			// Add all points one at a time to the triangulation.
			for (int i = 0; i < Vertices.Count; i++)
			{
				if (Vertices.Count > 64)
				{
					bar.Progress = i + 1;
					//Console.WriteLine($"Triangulating... {i + 1}/{Vertices.Count}");
				}
				
				List<Triangle> badTriangles = new List<Triangle>();
				// First find all the triangles that are no longer valid due to the insertion.
				foreach (Triangle t in tris)
				{
					Circle circumcircle = Circle.Circumscribe(t);
					if (circumcircle.ContainsPoint(Vertices[i]))
						badTriangles.Add(t);
				}
				List<Edge> polygon = new List<Edge>();
				// Find the boundary of the polygonal hole.
				foreach (Triangle t in badTriangles)
				{
					foreach (Edge e in t.Edges)
					{
						bool isShared = false;
						foreach (Triangle s in badTriangles)
						{
							if (s == t)
								continue;
							// Find all edges that are shared between 2 or more bad triangles.
							foreach (Edge f in s.Edges)
							{
								if ((f.Initial == e.Initial && f.Terminal == e.Terminal) || (f.Terminal == e.Initial && f.Initial == e.Terminal))
								{
									isShared = true;
									break;
								}
							}
						}
						if (!isShared)
							polygon.Add(e);
					}
				}
				// Remove them from the graph.
				foreach (Triangle t in badTriangles)
					tris.Remove(t);
				// The polygon stores edges that are sill valid to use, so put them back.
				foreach (Edge e in polygon)
				{
					Triangle newTri = new Triangle(e.Initial, e.Terminal, Vertices[i]);
					tris.Add(newTri);
				}
			}
			// Add every edge from the valid triangles to the triangulation.
			foreach (Triangle t in tris)
			{
				foreach (Edge e in t.Edges)
					triangulation.AddEdge(e);
			}
			// Done inserting, now clean up.
			foreach (Vertex v in superTriangle.Vertices)
				triangulation.RemoveVertex(v);

			return triangulation;
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

		#region Centrality measures
		// Harmonic Centrality of a vertex v.
		// Eq. 3.2 on page 230 of "Axioms for Centrality" (Boldi & Vigna)
		public double HarmonicCentrality(Vertex v, bool setProp = false)
		{
			double centrality = 0;

			if (!Directed)
			{
				// Perform a breadth-first search on the graph, starting at v.
				// This returns a new graph that contains all vertices of this graph that have a finite distance from v.
				// Also, the vertices in this new graph already have the length of the shortest path saved in the "distance" property.
				// These facts allow for the infinite distance logic check to be removed in the summation.
				// On top of all that, this method only needs to be called once to find the shortest path from v to every other vertex.
				// The downside of this is that it reverses the order in which it searches for edges.
				// The equation calls for the shortest path from u to v, but the BFS from v finds the shortest path from v to u.
				// Hence this method will only work properly for undirected graphs.
				Graph bfsTree = BreadthFirstSearch(v);

				foreach (Vertex u in bfsTree.Vertices)
				{
					// Disregard loops. Those always have a minimum distance of zero.
					if (u == v)
						continue;

					// v's "distance" property is the length of the shortest path between u and v.
					// Add the reciprocal of the distance to the centrality measure.
					centrality += 1d / u.GetProp<double>("distance");
				}
			}
			else
			{
				foreach (Vertex u in Vertices)
				{
					// Disregard loops. Those always have a minimum distance of zero.
					if (u == v)
						continue;

					// Find the shortest path from u to v. The total length of the path is stored in the v's "distance" property.
					// In the case of a directed graph, it is simpler to find the path in every iteration.
					FindPath(u, v);
					// Ignore if the distance is explicitly stated as infinite.
					if (v.GetProp<bool>("isInfiniteDistance"))
						continue;

					// v's "distance" property is the length of the shortest path between u and v.
					// Add the reciprocal of the distance to the centrality measure.
					centrality += 1d / v.GetProp<double>("distance");
				}
			}

			if (setProp)
				v.SetProp("harmonicCentrality", centrality);
			return centrality;
		}

		// Closeness of a vertex v.
		// Eq. 3.1 on page 229 of "Axioms for Centrality" (Boldi & Vigna)
		public double Closeness(Vertex v)
		{
			double centrality = 0;

			if (!Directed)
			{
				// Like in the harmonic centrality, the BFS method will only work properly in an undirected graph.
				Graph bfsTree = BreadthFirstSearch(v);

				foreach (Vertex u in bfsTree.Vertices)
				{
					if (u == v)
						continue;

					centrality += u.GetProp<double>("distance");
				}
			}
			else
			{
				foreach (Vertex u in Vertices)
				{
					if (u == v)
						continue;

					FindPath(u, v);
					// Disregard infinite distances.
					if (v.GetProp<bool>("isInfiniteDistance"))
						continue;

					centrality += v.GetProp<double>("distance");
				}
			}

			return 1d / centrality;
		}

		public int RadiusOfCentrality(Vertex v)
		{
			// Start calculating centrality with radius 0.
			// Then v is trivially the most central vertex.
			int radius = 0;
			Graph G_0 = Subgraph(v, radius);
			bool isMostCental;
			v.SetProp("radiusOfCentrality", radius);
			// Initialize the next subgraph in the sequence.
			// Initializing it to G_0 is important for when the loop is entered and G_0 is overwritten.
			Graph G_n = G_0;

			// Loop at least once to check.
			do
			{
				G_0 = G_n;
				radius++;

				G_n = Subgraph(v, radius);
				// Calculate the harmonic centrality of every vertex in the subgraph.
				foreach (Vertex u in G_n.Vertices)
					u.SetProp("harmonicCentrality", G_n.HarmonicCentrality(u));

				// Determine if v is still the most central vertex in G_n.
				isMostCental = PropertyHolder.ItemWithMaxProp<double, Vertex>(G_n.Vertices.ToArray(), "harmonicCentrality")
					.GetProp<double>("harmonicCentrality") <= v.GetProp<double>("harmonicCentrality");
				// If v is the most central, increment its radius.
				if (isMostCental)
					v.SetProp("radiusOfCentrality", radius);
			}
			// Continue while v is the most central vertex in its graph,
			// and also while the subgraph with radius n has fewer vertices than the subgraph with radius n+1.
			// This condition implies that the graph has not reached its maximum radius, and there is still more to check.
			while (isMostCental && G_0.Vertices.Count < G_n.Vertices.Count);

			// At this point, if "isMostCentral" is true, that means v is the most central vertex in the whole graph
			// (or rather, it's connected component.)
			// So what happens is the loop reaches everything it can, then checks (and increments the radius) one more time
			// in which it decides "yep, this vertex is still the most central," before realizing that the radius it is looking in
			// is farther than actually makes sense.
			//
			// In short, the most central vertex gets its radius set 1 higher than it should, so that is adjusted here.
			if (isMostCental)
			{
				radius--;
				v.SetProp("radiusOfCentrality", radius);
			}

			return radius;
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
			fileName = path + "/" + Name.Replace(" ", "_") + ".json";

			if (!File.Exists(fileName))
			{
				using StreamWriter writer = File.CreateText(fileName);
				writer.WriteLine(JSON(vertexProps, edgeProps));
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
			Commands.CmdOut($"cd {Commands.RootFolder}", $"DrawGraph.py {resolution} _outputs/{Name.Replace(' ', '_')}.json {folder} true");
		}

		internal struct GraphSerializationData
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

				if (!Weighted && edgeProps != null)
					edgeProps = edgeProps.Difference(new string[] { Edge.WEIGHT });

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