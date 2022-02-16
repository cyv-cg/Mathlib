using System;
using System.Collections.Generic;

using Mathlib.Sys;
using Mathlib.Graphs;
using Mathlib.MathG;
using Mathlib.MathG.Colors;
using Mathlib.Arrays;

namespace Mathlib
{
	class Program
	{
		public static void Main()
		{
			Test();

			Graph G = Graph.Read("C:/Users/Christopher/Documents/GitHub/mathlib/mathlib/_outputs/test.json");
			G.Rename("input_test");
			G.SaveOut("_outputs", 2000, new string[] { Vertex.POS_X, Vertex.POS_Y, Vertex.COLOR, Centrality.RADIUS });
		}

		static void Test()
		{
			{
				Vertex A = new Vertex(0);
				A.SetProp<double>(Vertex.POS_X, 1);
				A.SetProp<double>(Vertex.POS_Y, 1);
				Vertex F = new Vertex(5);
				F.SetProp<double>(Vertex.POS_X, -1);
				F.SetProp<double>(Vertex.POS_Y, 1);

				Vertex B = new Vertex(1);
				B.SetProp<double>(Vertex.POS_X, 0);
				B.SetProp<double>(Vertex.POS_Y, 1);

				Vertex C = new Vertex(2);
				C.SetProp<double>(Vertex.POS_X, -1);
				C.SetProp<double>(Vertex.POS_Y, 0);

				Vertex D = new Vertex(3);
				D.SetProp<double>(Vertex.POS_X, 1);
				D.SetProp<double>(Vertex.POS_Y, 0);

				Vertex E = new Vertex(4);
				E.SetProp<double>(Vertex.POS_X, 0);
				E.SetProp<double>(Vertex.POS_Y, -1);


				Edge CB = new Edge(C, B);
				CB.SetProp<double>(Edge.WEIGHT, 4);

				Edge CD = new Edge(C, D);
				CD.SetProp<double>(Edge.WEIGHT, 3);

				Edge BD = new Edge(B, D);
				BD.SetProp<double>(Edge.WEIGHT, -2);

				Edge DE = new Edge(D, E);
				DE.SetProp<double>(Edge.WEIGHT, 2);

				Edge EC = new Edge(E, C);
				EC.SetProp<double>(Edge.WEIGHT, -1);

				//Graph G = new Graph(new Vertex[] { B, C, D, E }, new Edge[] { CB, CD, BD, DE, EC }, "test", false, false);
			}

			Graph G = GraphExt.RandomGraph(100, 0.6, -10, 10); G.Rename("test");

			//Graph G = GraphExt.CreateWheelGraph(4, false, "test");
			//G.AddEdge(new Edge(G.GetVertex(1), G.GetVertex(2)));
			//G.RemoveEdge(G.GetEdge(G.GetVertex(0), G.GetVertex(2)));


			G.RadiusOfCentrality(Centrality.HARMONIC);

			Vertex vWMaxBtwns = PropertyHolder.ItemWithMaxProp<int, Vertex>(G.Vertices.ToArray(), Centrality.RADIUS);
			Console.WriteLine($"{vWMaxBtwns}: {vWMaxBtwns.GetProp<int>(Centrality.RADIUS)}");
			
			G.ColorByPropInt(Centrality.RADIUS);
			G.SaveOut("_outputs", 2000, new string[] { Vertex.POS_X, Vertex.POS_Y, Vertex.COLOR, Centrality.RADIUS });
		}
	}
}
