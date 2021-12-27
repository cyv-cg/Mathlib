using System;
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
			A.SetProp("xPos", 0);
			A.SetProp("yPos", 0);
			Vertex B = new Vertex(1);
			B.SetProp("xPos", 1);
			B.SetProp("yPos", 0);
			Vertex C = new Vertex(2);
			C.SetProp("xPos", 2);
			C.SetProp("yPos", 0.5);
			Vertex D = new Vertex(3);
			D.SetProp("xPos", 1);
			D.SetProp("yPos", 1);
			Vertex E = new Vertex(4);
			E.SetProp("xPos", 0);
			E.SetProp("yPos", 1);

			Vertex F = new Vertex(5);
			F.SetProp("xPos", 0.5);
			F.SetProp("yPos", 2);

			Vertex[] vertices = new Vertex[] { 
				A, 
				B, 
				C, 
				D, 
				E,
				//F
			};

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

			// This is to test when multiple edges connect 2 vertices.
			Edge EB = new Edge(E, B);
			EB.SetProp("weight", 4);

			Edge CF = new Edge(C, F);
			CF.SetProp("weight", 2);
			Edge DF = new Edge(D, F);
			DF.SetProp("weight", 7);
			Edge EF = new Edge(E, F);
			EF.SetProp("weight", 343);

			Edge[] edges = new Edge[] { 
				AB, 
				AE,
				BC,
				BD, 
				BE,
				CD,
				DE, 
				//EB,
				//CF,
				//DF,
				//EF
			};

			Graph G = new Graph(vertices, edges, false, "Sample Graph");
			Console.WriteLine(G);
			G.Save(Commands.RootFolder);

			// Display the Harmonic Centrality and Closeness of every vertex in G.
			foreach (Vertex v in G.Vertices)
			{
				Console.WriteLine();
				Console.WriteLine($"Harmonic Centrality about {v}: {G.HarmonicCentrality(v)}");
				Console.WriteLine($"Closeness of {v}: {G.Closeness(v)}");
			}

			Console.WriteLine();
			Commands.CmdOut($"cd {Commands.RootFolder}", $"DrawGraph.py {2000} {G.Name.Replace(' ', '_')}.graph");
		}
	}
}
