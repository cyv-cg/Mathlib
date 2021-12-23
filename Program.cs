using System;
using System.Collections.Generic;

using Mathlib.System;
using Mathlib.Graphs;

namespace Mathlib
{
	class Program
	{
		public static void Main()
		{
			Commands.CmdOut($"cd {Commands.Path}", "cd ..", "cd ..", "cd ..", "DrawGraph.py");
			Console.WriteLine();

			MakeGraph();
		}

		static void MakeGraph()
		{
			Vertex A = new Vertex(0);
			A.SetProp("xPos", 0);
			A.SetProp("yPos", 0);
			Vertex B = new Vertex(1);
			B.SetProp("xPos", 2);
			B.SetProp("yPos", 0);
			Vertex C = new Vertex(2);
			C.SetProp("xPos", 3);
			C.SetProp("yPos", 1);
			Vertex D = new Vertex(3);
			D.SetProp("xPos", 2);
			D.SetProp("yPos", 2);
			Vertex E = new Vertex(4);
			E.SetProp("xPos", 0);
			E.SetProp("yPos", 2);
			Vertex[] vertices = new Vertex[] { A, B, C, D, E };

			Edge AB = new Edge(A, B);
			AB.SetProp("weight", 4);
			Edge AE = new Edge(A, E);
			AE.SetProp("weight", 3);
			Edge BC = new Edge(B, C);
			BC.SetProp("weight", 6);
			Edge BD = new Edge(B, D);
			BD.SetProp("weight", 4);
			Edge BE = new Edge(B, E);
			BE.SetProp("weight", 8);
			Edge CD = new Edge(C, D);
			CD.SetProp("weight", 2);
			Edge DE = new Edge(D, E);
			DE.SetProp("weight", 2);
			Edge[] edges = new Edge[] { AB, AE, BC, BD, BE, CD, DE };

			Graph G = new Graph(vertices, edges, false, "Graph 1");
			Console.WriteLine(G);

			Console.WriteLine();

			//Stack<Vertex> path = G.FindPath(A, D);
			//int pathWeight = 0;
			//while (path.Count > 0)
			//{
			//	Vertex v = path.Pop();
			//	Console.WriteLine($"{v}");
			//	pathWeight = v.GetProp<int>("distance");
			//}
			//Console.WriteLine($"Distance: {pathWeight}");
			//Console.WriteLine();
			
			foreach (Vertex v in G.Vertices)
			{
				//Vertex target = C;
				Console.WriteLine($"Harmonic Centrality about {v}: {G.HarmonicCentrality(v)}");
			}
		}
	}
}
