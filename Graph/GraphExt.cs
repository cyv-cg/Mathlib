using System;
using System.Collections.Generic;
using System.Text;

namespace Mathlib.Graphs
{
	public static class GraphExt
	{
		public static Vertex[] Incidence(Edge e)
		{
			return new IncidenceData(e).endpoints;
		}

		public static Vertex VertWithMinProp<T>(Vertex[] vertices, string property) where T : IComparable
		{
			try
			{
				Vertex vertWithMinVal = vertices[0];
				for (int i = 1; i < vertices.Length; i++)
				{
					if (vertices[i].GetProp<T>(property).CompareTo(vertWithMinVal.GetProp<T>(property)) < 0)
						vertWithMinVal = vertices[i];
				}
				return vertWithMinVal;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
		}
	}

	public class IncidenceData
	{
		public readonly Vertex[] endpoints;
		public readonly EdgeType type;

		public IncidenceData(Edge e)
		{
			endpoints = new Vertex[2] { e.initial, e.terminal };

			if (e.initial == e.terminal)
				type = EdgeType.Loop;
			else
				type = EdgeType.Link;
		}
	}

	public enum EdgeType
	{
		Loop,
		Link
	}
}