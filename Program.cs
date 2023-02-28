using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

using Mathlib.Sys;
using Mathlib.Graphs;
using Mathlib.Arrays;
using Mathlib.MathG.Colors;
using Mathlib.MathG;

namespace Mathlib
{
	public class Program
	{
		public static void ExportInducedSubgraphsWithClosenessColor(Graph G, Vertex center, string folder, string minColor = "770077", string maxColor = "FF00FF")
		{
			byte radius = 0;
			Graph g;
			do
			{
				g = G.InducedSubgraph(center, radius);
				g.Rename($"{G.Name}_{center.IdToAlpha()}_{radius}");
				radius++;

				foreach (Vertex vert in g.Vertices)
					vert.SetProp(Centrality.CLOSENESS, g.Closeness(vert));
				g.ColorByPropDouble(Centrality.CLOSENESS, MathG.Colors.Gradient.Rainbow, true);

				double minVal = PropertyHolder.ItemWithMinProp<double, Vertex>(g.Vertices.ToArray(), Centrality.CLOSENESS).GetProp<double>(Centrality.CLOSENESS);
				double maxVal = PropertyHolder.ItemWithMaxProp<double, Vertex>(g.Vertices.ToArray(), Centrality.CLOSENESS).GetProp<double>(Centrality.CLOSENESS);

				foreach (Vertex vert in g.Vertices)
				{
					if (vert.GetProp<double>(Centrality.CLOSENESS) == minVal)
						vert.SetProp(Vertex.COLOR, minColor);
					if (vert.GetProp<double>(Centrality.CLOSENESS) == maxVal)
						vert.SetProp(Vertex.COLOR, maxColor);
				}

				g.SaveOut($"{folder}/{G.Name}", 1024, new string[] {Vertex.COLOR, Centrality.CLOSENESS});
			}
			while (g.Vertices.Count != G.Vertices.Count);
		}

		public static void Main(string[] args)
		{
			byte maxDegree = 10;
			int n = maxDegree - 1;

			Vertex X = new Vertex("X", 0, 0);
			List<Edge> edges = new List<Edge>();

			Graph W = new Graph();
			for (int i = 0; i < maxDegree - 1; i++)
			{
				Graph w = GraphExt.Line(new Vector2(-1, 0), new Vector2(-n, 0), n);
				Graph u = GraphExt.Wheel(new Vertex(0, w.Vertices[w.Vertices.Count - 1].Position.x - 1, w.Vertices[w.Vertices.Count - 1].Position.y), maxDegree - 1, Math.PI / 2, 3 * Math.PI / 2);
				edges.Add(new Edge(w.Vertices[w.Vertices.Count - 1], u.Vertices[0]));
				w = GraphExt.Union(w, u);

				double t = (double)i / (maxDegree - 2);
				double theta = -Math.PI / 2 + t * Math.PI;
				GraphExt.RotateGraph(w, theta);

				W = GraphExt.Union(W, w);
				edges.Add(new Edge(X, w.Vertices[0]));
			}

			Graph Z = GraphExt.Line(new Vector2(1, 0), new Vector2(n, 0), n);
			Z = GraphExt.Union(Z, GraphExt.Wheel(Z.Vertices[Z.Vertices.Count - 1], maxDegree, 3 * Math.PI / 2, 5 * Math.PI / 2, 0.5));

			for (int i = 0; i < n - 1; i++)
			{
				Graph Y = GraphExt.Wheel(new Vertex(0, Z.Vertices[i].Position.x, Z.Vertices[i].Position.y - 1), maxDegree, Math.PI, 2 * Math.PI, 0.5);

				if (i % 2 == 1)
				{
					GraphExt.RotateGraph(Y, Math.PI, Z.Vertices[i].Position);
				}
				
				edges.Add(new Edge(Z.Vertices[i], Y.Vertices[0]));
				Z = GraphExt.Union(Z, Y);
			}


			Graph G = GraphExt.Union(W, Z);
			G.Rename($"G{maxDegree}");

			edges.Add(new Edge(X, Z.Vertices[0]));
			G.AddVertex(X);
			foreach (Edge e in edges)
				G.AddEdge(e);
			
			

			ExportInducedSubgraphsWithClosenessColor(G, X, "_outputs");
		}
	}
}