using System;
using System.Text.Json;
using System.Collections.Generic;

using Mathlib.Sys;
using Mathlib.Graphs;
using Mathlib.Graphs.Shapes;
using Mathlib.MathG;
using Mathlib.Arrays;

namespace Mathlib
{
	class Program
	{
		public static void Main()
		{
			//Commands.CmdOut($"cd {Commands.Path}", "cd ..", "cd ..", "cd ..", "DrawGraph.py");
			//Console.WriteLine();

			//MakeGraph();

			//Shapes();

			//Rand();

			Rand2();
		}

		static void Rand2()
		{
			Graph G = GraphExt.RandomGraph(256, 0.5, -10, 10, 500);

			foreach (Vertex v in G.Vertices)
				G.HarmonicCentrality(v);

			G.SaveOut("_outputs", 500, new string[] { Vertex.POS_X, Vertex.POS_Y, "harmonicCentrality" });


			//Vertex mostCentralVertex = PropertyHolder.ItemWithMaxProp<double, Vertex>(G.Vertices.ToArray(), "harmonicCentrality");
			//Vertex leastCentralVertex = PropertyHolder.ItemWithMinProp<double, Vertex>(G.Vertices.ToArray(), "harmonicCentrality");

			//Console.WriteLine($"Most central vertex: {mostCentralVertex}.");
			//Console.WriteLine($"Least central vertex: {leastCentralVertex}.");
		}

		static void Rand()
		{
			Graph G = GraphExt.RandomGraph(4, -1, 1);
			G.Save(Commands.RootFolder + "_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y });

			// Initialize extreme values.
			double x_max = int.MinValue;
			double x_min = int.MaxValue;
			double y_max = int.MinValue;
			double y_min = int.MaxValue;
			// Find the extreme points of the bounds.
			foreach (Vertex v in G.Vertices)
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

			Circle circumcircle = Circle.Circumscribe(superTriangle);
			circumcircle.Rename("Circumcircle of Super Triangle");


			//////////////////////////////////////////////////////////////////////////////////////////////
			//Vertex test = new Vertex(26*26*26);
			//test.SetProp<double>(Vertex.POS_X, 0);
			//test.SetProp<double>(Vertex.POS_Y, 0);
			//circumcircle.AddVertex(test);

			//Vertex test2 = new Vertex(26 * 26 + 1);
			//test2.SetProp(Vertex.POS_X, circumcircle.center.X);
			//test2.SetProp(Vertex.POS_Y, circumcircle.center.Y);
			//circumcircle.AddVertex(test2);

			Graph bounds = Graph.Union(G, superTriangle, circumcircle);
			bounds = Graph.Union(Graph.Grid(bounds), bounds);
			bounds.Rename("Bounds");
			bounds.Save(Commands.RootFolder + "_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y });
			//////////////////////////////////////////////////////////////////////////////////////////////


			Commands.CmdOut($"cd {Commands.RootFolder}",
				$"DrawGraph.py {1000} _outputs/{G.Name.Replace(' ', '_')}.json _outputs true false",
				$"DrawGraph.py {1000} _outputs/{bounds.Name.Replace(' ', '_')}.json _outputs true false");

			// The next step here is to use Delauney Triangulation to create non-intersecting edges between the random points.
		}

		static void Shapes()
		{
			Circle c = new Circle(16, 1.5, new Vector2());
			c.Save(Commands.RootFolder + "_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y });

			Quad q = new Quad(new Vector2(-1, 3), new Vector2(2, 4), new Vector2(-2, -3), new Vector2(1, -1));
			q.Save(Commands.RootFolder + "_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y });

			Triangle t = new Triangle(new Vector2(0, 3), new Vector2(-1, -1), new Vector2(1, 2));
			t.Save(Commands.RootFolder + "_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y });

			Commands.CmdOut($"cd {Commands.RootFolder}", 
				$"DrawGraph.py {2000} _outputs/{c.Name.Replace(' ', '_')}.json _outputs {true} {false} {false}",
				$"DrawGraph.py {2000} _outputs/{q.Name.Replace(' ', '_')}.json _outputs {true} {false} {false}",
				$"DrawGraph.py {2000} _outputs/{t.Name.Replace(' ', '_')}.json _outputs {true} {false} {false}");
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
			AB.SetProp(Edge.WEIGHT, 4.0);
			Edge AE = new Edge(A, E);
			AE.SetProp(Edge.WEIGHT, 3.0);
			Edge BC = new Edge(B, C);
			BC.SetProp(Edge.WEIGHT, 6.0);
			Edge BD = new Edge(B, D);
			BD.SetProp(Edge.WEIGHT, 4.0);
			Edge BE = new Edge(B, E);
			BE.SetProp(Edge.WEIGHT, 8.0);
			Edge CD = new Edge(C, D);
			CD.SetProp(Edge.WEIGHT, 2.0);
			Edge DE = new Edge(D, E);
			DE.SetProp(Edge.WEIGHT, 2.0);

			// This is to test when multiple edges connect 2 vertices.
			Edge EB = new Edge(E, B);
			EB.SetProp(Edge.WEIGHT, 4.0);

			Edge CF = new Edge(C, F);
			CF.SetProp(Edge.WEIGHT, 2.0);
			Edge DF = new Edge(D, F);
			DF.SetProp(Edge.WEIGHT, 7.0);
			Edge EF = new Edge(E, F);
			EF.SetProp(Edge.WEIGHT, 343.0);

			Edge[] edges = new Edge[] {
				AB,
				AE,
				//BC,
				BD,
				BE,
				CD,
				DE,
				//EB,
				//CF,
				//DF,
				//EF
			};

			Graph G = new Graph(vertices, edges, "G", false, false);
			Console.WriteLine(G);
			G.Save(Commands.RootFolder + "_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y }, new string[] { Edge.WEIGHT });

			//Graph wheelGraph = Graph.CreateWheelGraph(10);
			//wheelGraph.Save(Commands.RootFolder + "/_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y });

			// Display the Harmonic Centrality and Closeness of every vertex in G.
			foreach (Vertex v in G.Vertices)
			{
				Console.WriteLine();
				Console.WriteLine($"Harmonic Centrality about {v}: {G.HarmonicCentrality(v)}");
				Console.WriteLine($"Closeness of {v}: {G.Closeness(v)}");
			}

			Console.WriteLine();
			Commands.CmdOut($"cd {Commands.RootFolder}", $"DrawGraph.py {2000} _outputs/{G.Name.Replace(' ', '_')}.json _outputs {true} {false} {false}");

			Graph H = G.Subgraph(D, 1);
			H.Save(Commands.RootFolder + "_outputs", new string[] { Vertex.POS_X, Vertex.POS_Y }, new string[] { Edge.WEIGHT });
			Commands.CmdOut($"cd {Commands.RootFolder}", $"DrawGraph.py {2000} _outputs/{H.Name.Replace(' ', '_')}.json _outputs {true} {false} {false}");
		}
	}
}
