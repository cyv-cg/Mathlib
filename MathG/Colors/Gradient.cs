using System;
using System.Collections.Generic;

namespace Mathlib.MathG.Colors
{
	public class Gradient
	{
		private readonly List<double> times;
		private readonly List<Vector3> colors;

		public Gradient()
		{
			times = new List<double>();
			colors = new List<Vector3>();
		}

		public Gradient(params KeyValuePair<double, Vector3>[] timeline) : this()
		{
			foreach (KeyValuePair<double, Vector3> p in timeline)
			{
				AddColor(p.Key, p.Value);
			}
		}

		public static Gradient Rainbow { get {
				return new Gradient(
					new KeyValuePair<double, Vector3>(0.00, Colors.red),
					new KeyValuePair<double, Vector3>(0.33, Colors.yellow),
					new KeyValuePair<double, Vector3>(0.50, Colors.green),
					new KeyValuePair<double, Vector3>(0.67, Colors.cyan),
					new KeyValuePair<double, Vector3>(1.00, Colors.blue)
				);
			} 
		}
		public static Gradient Grayscale { get {
				return new Gradient(
					new KeyValuePair<double, Vector3>(0.00, Colors.black),
					new KeyValuePair<double, Vector3>(1.00, Colors.white)
				);
			} 
		}

		public void AddColor(double time, Vector3 color)
		{
			times.Add(time);
			colors.Add(color);
			Sort();
		}

		// Sort the lists based on time via bubble sort.
		private void Sort()
		{
			for (int i = 0; i < times.Count - 1; i++)
			{
				for (int j = 0; j < times.Count - i - 1; j++)
				{
					if (times[j] > times[j + 1])
					{
						double temp_time = times[j];
						Vector3 temp_color = colors[j];

						times[j] = times[j + 1];
						colors[j] = colors[j + 1];

						times[j + 1] = temp_time;
						colors[j + 1] = temp_color;
					}
				}
			}
		}

		public Vector3 Evaluate(double time)
		{
			if (time > times[^1] || time < times[0])
				throw new ArgumentOutOfRangeException();

			if (colors.Count == 1)
			{
				return colors[0];
			}

			int index = -1;
			for (int i = 0; i < times.Count - 1; i++)
			{
				if (times[i] > time)
					break;
				index++;
			}

			double percent = (time - times[index]) / (times[index + 1] - times[index]);
			return ((1 - percent) * colors[index]) + (percent * colors[index + 1]);
		}
	}
}