using System;

namespace Mathlib.Sys
{
	public class LoadingBar
	{
		// These chars are the building blocks for the displayed loading bar.
		const char LEFT_BOUND = '[';
		const char RIGHT_BOUND = ']';
		const char FILLED_CHAR = '#';
		const char UNFILLED_CHAR = '-';

		// Title of the loading bar, should be descripting of what action is happening behind the scenes.
		public string Title { get; private set; }

		// Progress, represents how close the task is to completion.
		private int _progress;
		public int Progress { get { 
				return _progress; 
			} 
			set { 
				// Automatically update the loading bar when progress is changed.
				_progress = value; 
				Update(); 
			} 
		}

		// Max value of the bar. 
		// Determines percent completion and length of the loading bar.
		private int _maxValue;
		public int MaxValue { get { 
				return _maxValue; 
			} 
			set {
				_maxValue = value;
				Length = Math.Clamp((byte)(value / double.MaxValue), (byte)8, byte.MaxValue);
			} 
		}
		// Length of the loading bar in characters.
		public byte Length { get; private set; }

		// Whether or not the bar has been displayed yet.
		// True if it hasn't, false if it has.
		private bool firstPass = true;

		public LoadingBar(string title, int maxValue)
		{
			Title = title;
			MaxValue = maxValue;
		}

		// Redraw the loading bar in the console.
		private void Update()
		{
			// If the bar has been drawn before, clear the line it's on so it can be redrawn in the same place.
			if (!firstPass)
				Commands.ClearLastLine();
			// Note that the bar has been drawn before.
			else
				firstPass = false;
			// Redraw the bar.
			DrawBar();
		}

		// Draw the loading bar with the constants specified above.
		private void DrawBar()
		{
			// Initialize the bar with the title, then the opening char.
			string bar = $"{Title}... {LEFT_BOUND}";
			// Calculate the percent completion of the bar.
			double percent = (double)Progress / MaxValue;
			for (int i = 0; i < Length; i++)
			{
				// Draw the filled mark for a proportionate number of slots...
				if ((double)i / Length <= percent)
					bar += $"{FILLED_CHAR}";
				// and the unfilled mark for the rest of the bar.
				else
					bar += $"{UNFILLED_CHAR}";
			}
			// If the task is complete, end with "Done", otherwise the completion percent.
			string message = Progress == MaxValue ? "Done" : $"{(int)(100 * percent)}%";
			// Close the bar and end the line.
			bar += $"{RIGHT_BOUND} {message}\n";
			// Draw the bar.
			Console.Write(bar);
		}
	}
}
