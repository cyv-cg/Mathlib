using System;
using System.Collections.Generic;
using Mathlib.MathG;
using Mathlib.Sys;

namespace Mathlib.Graphs
{
	public class FulcrumTree : Graph
	{
		// Seeds off vertex X.
		private static List<Vertex> seeds1 = new List<Vertex>();
		// Seeds on the right.
		private static List<Vertex> seeds2 = new List<Vertex>();
		private static Graph branch = null;

		byte k = 0;
		
		public FulcrumTree(byte k)
		{
			// This setup does not currently work for maxDegree <= 3.
			byte maxDegree = k;
			this.k = k;
			int n = maxDegree - 1;

			Vertex X = new Vertex("A", 0, 0);
			List<Edge> edges = new List<Edge>();

			// Create offshoots from X.
			Graph W = new Graph(new Vertex[] {X}, new Edge[0]);
			for (int i = 0; i < maxDegree - 1; i++)
			{
				// This line makes the branch.
				Graph w = GraphExt.Line(new Vector2(-1, 0), new Vector2(-n, 0), n);
				// This line creates the vertices sprouting from the end of the branch.
				Graph u = GraphExt.Wheel(new Vertex(0, w.Vertices[w.Vertices.Length - 1].Position.x - 1, w.Vertices[w.Vertices.Length - 1].Position.y), maxDegree - 1, Math.PI / 2, 3 * Math.PI / 2);
				// The 0th vertex of u is the one with the extra branches on it.
				seeds1.Add(u.Vertices[0]);

				edges.Add(new Edge(w.Vertices[w.Vertices.Length - 1], u.Vertices[0]));
				w = GraphExt.Union(w, u);

				// Rotate the branches so they look appealing :)
				double t = (double)i / (maxDegree - 2);
				double theta = -Math.PI / 2 + t * Math.PI;
				GraphExt.RotateGraph(w, theta);

				W = GraphExt.Union(W, w);
				edges.Add(new Edge(X, w.Vertices[0]));
			}

			// Create branch to the right of X.
			// The last vertex of Z has all the branches.
			Graph Z = GraphExt.Line(new Vector2(1, 0), new Vector2(n, 0), n);

			// Create and cache a copy of Z.
			branch = new Graph(Z.Vertices, Z.Edges, "Branch");

			// This makes the extra branches all the way on the right side of the tree.
			Z = GraphExt.Union(Z, GraphExt.Wheel(Z.Vertices[Z.Vertices.Length - 1], maxDegree - 1, 3 * Math.PI / 2, 5 * Math.PI / 2, 2));

			for (int i = 0; i < n - 1; i++)
			{
				Graph Y = GraphExt.Wheel(new Vertex(0, Z.Vertices[i].Position.x, Z.Vertices[i].Position.y - 1), maxDegree - 1, Math.PI, 2 * Math.PI, 0.5);

				if (i % 2 == 1)
				{
					GraphExt.RotateGraph(Y, Math.PI, Z.Vertices[i].Position);
				}
				
				edges.Add(new Edge(Z.Vertices[i], Y.Vertices[0]));
				Z = GraphExt.Union(Z, Y);
			}

			Graph G = GraphExt.Union(W, Z);

			foreach (Vertex v in G.Vertices)
				AddVertex(v);
			foreach (Edge e in G.Edges)
				AddEdge(e);
			Rename($"G{maxDegree}");

			// Save all but the left neighbor of the last vertex in the branch.
			for (int i = 1; i < G.Neighbors(branch.Vertices[branch.Vertices.Length - 1]).Length; i++)
			{
				seeds2.Add(G.Neighbors(branch.Vertices[branch.Vertices.Length - 1])[i]);
			}

			edges.Add(new Edge(X, Z.Vertices[0]));
			foreach (Edge e in edges)
				AddEdge(e);
			
			foreach (Vertex v in G.Vertices)
			{
				if (G.Degree(v) > maxDegree)
				Console.WriteLine($"MAX DEGREE VIOLATED: deg({v.IdToAlpha()}) = {G.Degree(v)}");
			}
		}
	
		public void Shift(int n, bool normalized = false, int maxIterations = 10)
		{
			// Remove the branches on the left seeds.
			foreach (Vertex v in seeds1)
			{
				Vertex[] neighbors = Neighbors(v);
				for (int i = 0; i < neighbors.Length - 1; i++)
					RemoveVertex(neighbors[i]);
			}

			// If n >= k, add additional length to the branch.
			// We do this by shifting everything in seeds2 to the right by a certain amount.
			// The number of vertices we need to add is n - branch.Vertices.Count.
			if (n >= k)
			{
				int diff = n - branch.Vertices.Length;
				foreach (Vertex v in seeds2)
				{
					// Move right by diff.
					v.SetProp(Vertex.POS_X, v.Position.x + diff);
				}

				Vertex A = branch.Vertices[branch.Vertices.Length - 1];
				Vertex B = branch.Vertices[branch.Vertices.Length - 2];

				// Also move the rightmost vertex in branch.
				A.SetProp(Vertex.POS_X, A.Position.x + diff);
			
				// Sever edge.
				RemoveEdge(GetEdge(A, B));

				// Add additional vertices.
				Graph l = GraphExt.Line(new Vector2(B.Position.x + 1, 0), new Vector2(A.Position.x - 1, 0), diff);

				foreach (Vertex v in l.Vertices)
				{
					AddVertex(v);
					branch.AddVertex(v);

					// Grow the tree by the same amount in the other direction.
					List<Vertex> newSeeds1 = new List<Vertex>();
					foreach (Vertex s in seeds1)
					{
						Vertex M = new Vertex(0, s.Position.x - 1, s.Position.y);
						AddVertex(M);
						AddEdge(new Edge(s, M));
						newSeeds1.Add(M);
					}
					seeds1 = newSeeds1;
				}
				foreach (Edge e in l.Edges)
				{
					AddEdge(e);
					branch.AddEdge(e);
				}

				AddEdge(new Edge(B, l.Vertices[0]));
				branch.AddEdge(new Edge(B, l.Vertices[0]));
				AddEdge(new Edge(l.Vertices[l.Vertices.Length - 1], A));
				branch.AddEdge(new Edge(l.Vertices[l.Vertices.Length - 1], A));
			}

			// Evaluate initial closeness.
			foreach (Vertex v in Vertices)
				v.SetProp(Centrality.CLOSENESS, this.Closeness(v, normalized));

			// Choose the vertex we want to be the center.
			Vertex targetCenter = null;
			foreach (Vertex v in branch.Vertices)
			{
				int d = FindPath(Vertices[0], v).Count - 1;
				if (d == n)
				{
					targetCenter = v;
					break;
				}
			}

			Vertex oldCenter = Vertices[0];
			branch.AddVertex(oldCenter);


			//G.Rename($"G{k}_{oldCenter}->{n}");


			LoadingBar bar = new LoadingBar($"Shifting center to {targetCenter}", n);
			bar.Progress = 0;

			// If true, only add one wheel at a time and check centrality after each addition.
			bool oneAtATime = false;

			// As long as the center is not the target vertex, grow the tree according to my notes.
			int j = 0;
			while (PropertyHolder.ItemWithMaxProp<double, Vertex>(branch.Vertices, Centrality.CLOSENESS) != targetCenter)
			{
				List<Vertex> newSeeds1 = new List<Vertex>();
				// Also grow from the other seeds.
				for (int i = 0; i < seeds1.Count; i++)
				{
					Vertex v = seeds1[i];
					// Add a single vertex.
					Vertex N = new Vertex(0, v.Position.x - 1, v.Position.y);
					AddVertex(N);
					AddEdge(new Edge(v, N));
					newSeeds1.Add(N);
				}
				// Replace seeds1 with the newly created vertices.
				seeds1 = newSeeds1;

				List<Vertex> newSeeds2 = new List<Vertex>();
				for (int i = 0; i < seeds2.Count; i++)
				{
					Vertex v = seeds2[i];
					// Create a new wheel of k-1 vertices off of this vertex.
					Graph wheel = GraphExt.Wheel(v, k - 1, 19 * Math.PI / 12, 29 * Math.PI / 12, Math.Pow(2, -j));

					for (int index = 1; index < wheel.Vertices.Length; index++)
					{
						AddVertex(wheel.Vertices[index]);
						newSeeds2.Add(wheel.Vertices[index]);
					}
					foreach (Edge e in wheel.Edges)
						AddEdge(e);

					// Ideally, after adding each wheel, we'd recalculate the closeness to make sure we don't overshoot.
					// This will be slow, but it might be the only way to make it work.
					// Unless I can accurately calculate how many vertices I'd need to add to get the desired shift.

					if (oneAtATime)
					{
						// Recalculate closeness since the graph was modified.
						foreach (Vertex vert in branch.Vertices)
							vert.SetProp(Centrality.CLOSENESS, this.Closeness(vert, normalized));
						Vertex mostCentralVertex = PropertyHolder.ItemWithMaxProp<double, Vertex>(branch.Vertices, Centrality.CLOSENESS);
						bar.Progress = FindPath(oldCenter, mostCentralVertex).Count - 1;
						if (mostCentralVertex == targetCenter)
						{
							break;
						}
					}
				}

				if (!oneAtATime)
				{
					// Here's what I *could* do:
					// Add all the branches for the iteration like I was doing at first.
					// Recalculate centrality.
					foreach (Vertex vert in branch.Vertices)
						vert.SetProp(Centrality.CLOSENESS, this.Closeness(vert, normalized));

					Vertex mostCentralVertex = PropertyHolder.ItemWithMaxProp<double, Vertex>(branch.Vertices, Centrality.CLOSENESS);
					// Then, check the distance from X to the current most central vertex.
					int dist = FindPath(oldCenter, mostCentralVertex).Count - 1;
					// If the distance is *less* than n, then iterate again.
					if (dist < n)
					{
						// Do nothing.
						bar.Progress = dist;
					}
					// If the distance is *equal* to n, stop.
					else if (dist == n)
					{
						bar.Progress = bar.MaxValue;
						break;
					}
					// If the distance is *more* than n, go back to the previous step and add one wheel at a time again.
					else
					{
						foreach (Vertex vert in newSeeds2)
							RemoveVertex(vert);
						oneAtATime = true;
						continue;
					}
					// That leads to the same result with far fewer calculations.
				}
				// Replace seeds2 with the newly created vertices.
				seeds2 = newSeeds2;

				j++;
				if (j >= maxIterations)
				{
					bar.Progress = bar.MaxValue;
					break;
				}
			}
			oneAtATime = false;

			bar = new LoadingBar($"Shifting center back to {oldCenter}", n);
			bar.Progress = 0;

			// Once the desired center is achieved, grow the tree back in the other direction so the center shifts back.
			j = 0;
			while (PropertyHolder.ItemWithMaxProp<double, Vertex>(branch.Vertices, Centrality.CLOSENESS) != oldCenter)
			{
				List<Vertex> newSeeds1 = new List<Vertex>();
				for (int i = 0; i < seeds1.Count; i++)
				{
					Vertex v = seeds1[i];
					// Add a wheel of k-1 vertices.
					Graph wheel = GraphExt.Wheel(v, k - 1, 7 * Math.PI / 12, 17 * Math.PI / 12, Math.Pow(2, -j));
					// Replace seeds1 with the newly created vertices.
					for (int index = 1; index < wheel.Vertices.Length; index++)
					{
						AddVertex(wheel.Vertices[index]);
						newSeeds1.Add(wheel.Vertices[index]);
					}
					foreach (Edge e in wheel.Edges)
						AddEdge(e);

					if (oneAtATime)
					{
						// Recalculate closeness since the graph was modified.
						foreach (Vertex vert in branch.Vertices)
						{
							vert.SetProp(Centrality.CLOSENESS, this.Closeness(vert, normalized));
						}
						Vertex mostCentralVertex = PropertyHolder.ItemWithMaxProp<double, Vertex>(branch.Vertices, Centrality.CLOSENESS);
						bar.Progress = n - (FindPath(oldCenter, mostCentralVertex).Count - 1);
						if (mostCentralVertex == oldCenter)
						{
							break;
						}
					}
				}

				if (!oneAtATime)
				{
					// Here's what I *could* do:
					// Add all the branches for the iteration like I was doing at first.
					// Recalculate centrality.
					foreach (Vertex vert in branch.Vertices)
						vert.SetProp(Centrality.CLOSENESS, this.Closeness(vert, normalized));

					Vertex mostCentralVertex = PropertyHolder.ItemWithMaxProp<double, Vertex>(branch.Vertices, Centrality.CLOSENESS);
					// Then, check the distance from X to the current most central vertex.
					// If the distance is *less* than n, then iterate again.
					if (mostCentralVertex != oldCenter && branch.GetVertex(mostCentralVertex.Id) != null)
					{
						// Do nothing.
						bar.Progress = n - (FindPath(oldCenter, mostCentralVertex).Count - 1);
					}
					// If the distance is *equal* to n, stop.
					else if (mostCentralVertex == oldCenter)
					{
						bar.Progress = bar.MaxValue;
						break;
					}
					// If the distance is *more* than n, go back to the previous step and add one wheel at a time again.
					else
					{
						foreach (Vertex vert in newSeeds1)
							RemoveVertex(vert);
						oneAtATime = true;
						continue;
					}
					// That leads to the same result with far fewer calculations.
				}
				seeds1 = newSeeds1;

				j++;
				if (j >= maxIterations)
				{
					bar.Progress = bar.MaxValue;
					break;
				}
			}

			foreach (Vertex v in Vertices)
				v.SetProp<bool>("hideName", v != targetCenter && v != oldCenter);

			GraphExt.ExportInducedSubgraphsWithClosenessColor(this, Vertices[0], "_outputs", normalized);
		}
	}
}