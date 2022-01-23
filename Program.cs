﻿using System;
using System.Text.Json;
using System.Collections.Generic;

using Mathlib.System;
using Mathlib.Graphs;

namespace Mathlib
{
	class Program
	{
		public static void Main()
		{
			//Commands.CmdOut($"cd {Commands.Path}", "cd ..", "cd ..", "cd ..", "DrawGraph.py");
			//Console.WriteLine();

			MakeGraph();
		}

		static void MakeGraph()
		{
			// This is just an example graph from online; properties are my own.
			// src: https://www.geeksforgeeks.org/graph-data-structure-and-algorithms/

			Vertex A = new Vertex(0);
			A.SetProp(Vertex.POS_X, 0);
			A.SetProp(Vertex.POS_Y, 0);
			Vertex B = new Vertex(1);
			B.SetProp(Vertex.POS_X, 1);
			B.SetProp(Vertex.POS_Y, 0);
			Vertex C = new Vertex(2);
			C.SetProp(Vertex.POS_X, 2);
			C.SetProp(Vertex.POS_Y, 0.5);
			Vertex D = new Vertex(3);
			D.SetProp(Vertex.POS_X, 1);
			D.SetProp(Vertex.POS_Y, 1);
			Vertex E = new Vertex(4);
			E.SetProp(Vertex.POS_X, 0);
			E.SetProp(Vertex.POS_Y, 1);

			Vertex F = new Vertex(5);
			F.SetProp(Vertex.POS_X, 0.5);
			F.SetProp(Vertex.POS_Y, 2);

			Vertex[] vertices = new Vertex[] { 
				A, 
				B, 
				C, 
				D, 
				E,
				F
			};

			Edge AB = new Edge(A, B);
			//AB.SetProp(Edge.WEIGHT, 4.0);
			Edge AE = new Edge(A, E);
			//AE.SetProp(Edge.WEIGHT, 3.0);
			Edge BC = new Edge(B, C);
			//BC.SetProp(Edge.WEIGHT, 6.0);
			Edge BD = new Edge(B, D);
			//BD.SetProp(Edge.WEIGHT, 4.0);
			Edge BE = new Edge(B, E);
			//BE.SetProp(Edge.WEIGHT, 8.0);
			Edge CD = new Edge(C, D);
			//CD.SetProp(Edge.WEIGHT, 2.0);
			Edge DE = new Edge(D, E);
			//DE.SetProp(Edge.WEIGHT, 2.0);

			// This is to test when multiple edges connect 2 vertices.
			Edge EB = new Edge(E, B);
			//EB.SetProp(Edge.WEIGHT, 4.0);

			Edge CF = new Edge(C, F);
			//CF.SetProp(Edge.WEIGHT, 2.0);
			Edge DF = new Edge(D, F);
			//DF.SetProp(Edge.WEIGHT, 7.0);
			Edge EF = new Edge(E, F);
			//EF.SetProp(Edge.WEIGHT, 343.0);

			Edge[] edges = new Edge[] {
				AB,
				AE,
				//BC,
				BD,
				BE,
				//CD,
				DE,
				//EB,
				//CF,
				//DF,
				//EF
			};

			Graph G = new Graph(vertices, edges, "New Graph", false);
			Console.WriteLine(G);
			G.Save(Commands.RootFolder + "/_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y }, new string[] { Edge.WEIGHT });

			//Graph wheelGraph = Graph.CreateWheelGraph(10);
			//wheelGraph.Save(Commands.RootFolder + "/_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y });

			// Display the Harmonic Centrality and Closeness of every vertex in G.
			//foreach (Vertex v in G.Vertices)
			//{
			//	Console.WriteLine();
			//	Console.WriteLine($"Harmonic Centrality about {v}: {G.HarmonicCentrality(v)}");
			//	Console.WriteLine($"Closeness of {v}: {G.Closeness(v)}");
			//}

			Console.WriteLine();
			Commands.CmdOut($"cd {Commands.RootFolder}", $"DrawGraph.py {2000} _outputs/{G.Name.Replace(' ', '_')}.json {true} {false} {false}");

			Graph BFS = G.BreadthFirstSearch(A);
			BFS.Save(Commands.RootFolder + "/_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y }, new string[] { Edge.WEIGHT });
			Commands.CmdOut($"cd {Commands.RootFolder}", $"DrawGraph.py {2000} _outputs/{BFS.Name.Replace(' ', '_')}.json {true} {false} {false}");
		}
	}
}
