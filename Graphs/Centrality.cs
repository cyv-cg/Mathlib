using System;
using System.Collections.Generic;

using Mathlib.Sys;

namespace Mathlib.Graphs
{
	public static class Centrality
	{
		public const string HARMONIC = "harmonicCentrality";
		public const string CLOSENESS = "closenessCentrality";
		public const string RADIUS = "radiusOfCentrality";
		public const string BETWEENNESS = "betweennessCentrality";

		// Harmonic Centrality of a vertex v.
		// Eq. 3.2 on page 230 of "Axioms for Centrality" (Boldi & Vigna)
		public static double HarmonicCentrality(this Graph G, Vertex v)
		{
			double centrality = 0;

			if (!G.Directed)
			{
				// Perform a breadth-first search on the graph, starting at v.
				// This returns a new graph that contains all vertices of this graph that have a finite distance from v.
				// Also, the vertices in this new graph already have the length of the shortest path saved in the "distance" property.
				// These facts allow for the infinite distance logic check to be removed in the summation.
				// On top of all that, this method only needs to be called once to find the shortest path from v to every other vertex.
				// The downside of this is that it reverses the order in which it searches for edges.
				// The equation calls for the shortest path from u to v, but the BFS from v finds the shortest path from v to u.
				// Hence this method will only work properly for undirected graphs.
				Graph bfsTree = G.BreadthFirstSearch(v);

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
				foreach (Vertex u in G.Vertices)
				{
					// Disregard loops. Those always have a minimum distance of zero.
					if (u == v)
						continue;

					// Find the shortest path from u to v. The total length of the path is stored in the v's "distance" property.
					// In the case of a directed graph, it is simpler to find the path in every iteration.
					G.FindPath(u, v);
					// Ignore if the distance is explicitly stated as infinite.
					if (v.GetProp<bool>("isInfiniteDistance"))
						continue;

					// v's "distance" property is the length of the shortest path between u and v.
					// Add the reciprocal of the distance to the centrality measure.
					centrality += 1d / v.GetProp<double>("distance");
				}
			}

			v.SetProp(HARMONIC, centrality);
			return centrality;
		}

		// Closeness of a vertex v.
		// Eq. 3.1 on page 229 of "Axioms for Centrality" (Boldi & Vigna)
		public static double Closeness(this Graph G, Vertex v)
		{
			double centrality = 0;

			if (!G.Directed)
			{
				// Like in the harmonic centrality, the BFS method will only work properly in an undirected graph.
				Graph bfsTree = G.BreadthFirstSearch(v);

				foreach (Vertex u in bfsTree.Vertices)
				{
					if (u == v)
						continue;

					centrality += u.GetProp<double>("distance");
				}
			}
			else
			{
				foreach (Vertex u in G.Vertices)
				{
					if (u == v)
						continue;

					G.FindPath(u, v);
					// Disregard infinite distances.
					if (v.GetProp<bool>("isInfiniteDistance"))
						continue;

					centrality += v.GetProp<double>("distance");
				}
			}

			if (centrality > 0)
				v.SetProp(CLOSENESS, 1d / centrality);
			else
				v.SetProp(CLOSENESS, 0.0);
			return v.GetProp<double>(CLOSENESS);
		}

		public static int RadiusOfCentrality(this Graph G, Vertex v, string measure = HARMONIC)
		{
			// Start calculating centrality with radius 0.
			// Then v is trivially the most central vertex.
			int radius = 0;
			Graph G_0 = G.InducedSubgraph(v, radius);
			bool isMostCental;
			v.SetProp(RADIUS, radius);
			// Initialize the next subgraph in the sequence.
			// Initializing it to G_0 is important for when the loop is entered and G_0 is overwritten.
			Graph G_n = G_0;

			// Loop at least once to check.
			do
			{
				G_0 = G_n;
				radius++;

				G_n = G.InducedSubgraph(v, radius);

				// Compute the centrality of the subgraph based on the specified measure.
				switch (measure)
				{
					// HARMONIC and CLOSENESS run a similar task, so their cases are lumped together.
					case HARMONIC:
					case CLOSENESS:
						foreach (Vertex u in G_n.Vertices)
						{
							if (measure == HARMONIC)
								u.SetProp(HARMONIC, G_n.HarmonicCentrality(u));
							else if (measure == CLOSENESS)
								u.SetProp(CLOSENESS, G_n.Closeness(u));
						}
						break;
					// Case for betweenness centrality, which takes its own special calculations.
					case BETWEENNESS:
						Dictionary<Vertex, double> betweenness = G.Betweenness();
						foreach (Vertex w in G.Vertices)
							w.SetProp(BETWEENNESS, betweenness[w]);
						break;
					// Undefined cases.
					case RADIUS:
						throw new ArgumentException($"{RADIUS} cannot be computed using {RADIUS}");
					default:
						throw new ArgumentException($"Measure '{measure}' not defined.");
				}

				// Determine if v is still the most central vertex in G_n.
				isMostCental = PropertyHolder.ItemWithMaxProp<double, Vertex>(G_n.Vertices.ToArray(), measure).GetProp<double>(measure)
					<= v.GetProp<double>(measure);
				// If v is the most central, increment its radius.
				if (isMostCental)
					v.SetProp(RADIUS, radius);
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
				v.SetProp(RADIUS, radius);
			}

			return radius;
		}
		public static void RadiusOfCentrality(this Graph G, string measure = HARMONIC)
		{
			LoadingBar bar = new LoadingBar("Finding radii of centrality", G.Vertices.Count);
			foreach (Vertex v in G.Vertices)
			{
				G.RadiusOfCentrality(v, measure);
				bar.Progress++;
			}
		}

		// https://en.wikipedia.org/wiki/Floyd%E2%80%93Warshall_algorithm
		public static Dictionary<Vertex, Dictionary<Vertex, double>> Floyd_Warshall(this Graph G)
		{
			Dictionary<Vertex, Dictionary<Vertex, double>> dist = new Dictionary<Vertex, Dictionary<Vertex, double>>();
			foreach (Vertex u in G.Vertices)
			{
				dist.Add(u, new Dictionary<Vertex, double>());
				foreach (Vertex w in G.Vertices)
				{
					dist[u].Add(w, double.PositiveInfinity);
				}
			}

			foreach (Edge e in G.Edges)
			{
				dist[e.Initial][e.Terminal] = G.Weighted ? e.GetProp<double>(Edge.WEIGHT) : 1.0;
				if (!G.Directed)
					dist[e.Terminal][e.Initial] = G.Weighted ? e.GetProp<double>(Edge.WEIGHT) : 1.0;
			}
			foreach (Vertex u in G.Vertices)
				dist[u][u] = 0.0;

			foreach (Vertex k in G.Vertices)
			{
				foreach (Vertex i in G.Vertices)
				{
					foreach (Vertex j in G.Vertices)
					{
						if (dist[i][j] > dist[i][k] + dist[k][j])
						{
							dist[i][j] = dist[i][k] + dist[k][j];
							if (!G.Directed)
								dist[j][i] = dist[i][k] + dist[k][j];
						}
					}
				}
			}
			
			return dist;
		}

		// Algorithm 1 from "A Faster Algorithm for Betweenness Centrality" by Ulrik Brandes.
		public static Dictionary<Vertex, double> Betweenness(this Graph G)
		{
			Dictionary<Vertex, double> C_B = new Dictionary<Vertex, double>();
			foreach (Vertex v in G.Vertices)
				C_B.Add(v, 0);

			foreach (Vertex s in G.Vertices)
			{
				Stack<Vertex> S = new Stack<Vertex>();
				Dictionary<Vertex, List<Vertex>> P = new Dictionary<Vertex, List<Vertex>>();
				foreach (Vertex w in G.Vertices) P.Add(w, new List<Vertex>());

				Dictionary<Vertex, int> σ = new Dictionary<Vertex, int>();
				foreach (Vertex t in G.Vertices) σ.Add(t, 0);
				σ[s] = 1;

				Dictionary<Vertex, int> d = new Dictionary<Vertex, int>();
				foreach (Vertex t in G.Vertices) d.Add(t, -1);
				d[s] = 0;

				Queue<Vertex> Q = new Queue<Vertex>();
				Q.Enqueue(s);
				while (Q.Count > 0)
				{
					Vertex v = Q.Dequeue();
					S.Push(v);
					foreach (Vertex w in G.Neighbors(v))
					{
						// w found for the first time?
						if (d[w] < 0)
						{
							Q.Enqueue(w);
							d[w] = d[v] + 1;
						}
						// shortest path to w via v?
						if (d[w] == d[v] + 1)
						{
							σ[w] = σ[w] + σ[v];
							P[w].Add(v);
						}
					}
				}

				Dictionary<Vertex, int> δ = new Dictionary<Vertex, int>();
				foreach (Vertex v in G.Vertices) δ.Add(v, 0);
				// S returns vertices in order of non-increasing distance from s
				while (S.Count > 0)
				{
					Vertex w = S.Pop();
					foreach (Vertex v in P[w]) δ[v] = δ[v] + (σ[v] / σ[w] * (1 + δ[w]));
					if (w != s) C_B[w] = (C_B[w] + δ[w]) / 2d;
				}
			}

			foreach (Vertex v in C_B.Keys)
				v.SetProp<double>(BETWEENNESS, C_B[v]);

			return C_B;
		}
	}
}