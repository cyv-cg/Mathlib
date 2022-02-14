namespace Mathlib.Graphs
{
	public static class Centrality
	{
		// Harmonic Centrality of a vertex v.
		// Eq. 3.2 on page 230 of "Axioms for Centrality" (Boldi & Vigna)
		public static double HarmonicCentrality(this Graph G, Vertex v, bool setProp = false)
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

			if (setProp)
				v.SetProp("harmonicCentrality", centrality);
			return centrality;
		}

		// Closeness of a vertex v.
		// Eq. 3.1 on page 229 of "Axioms for Centrality" (Boldi & Vigna)
		public static double Closeness(this Graph G, Vertex v, bool setProp = false)
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

			if (setProp)
				v.SetProp("closeness", 1d / centrality);
			return 1d / centrality;
		}

		public static int RadiusOfCentrality(this Graph G, Vertex v)
		{
			// Start calculating harmonic centrality with radius 0.
			// Then v is trivially the most central vertex.
			int radius = 0;
			Graph G_0 = G.InducedSubgraph(v, radius);
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

				G_n = G.InducedSubgraph(v, radius);
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
	}
}