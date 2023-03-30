using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mathlib.Sys
{
	public static class Writer
	{
		public static void Write(string directory, string fileName, string text, bool logOutput = true)
		{
			string file = directory + "/" + fileName;

			if (!Directory.Exists(directory))
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Commands.Cmd($"mkdir {directory}");
				}
				else
				{
					throw new NotSupportedException("Unsupported OS");
				}
			}

			if (!File.Exists(file))
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					using StreamWriter writer = File.CreateText(file);
					writer.WriteLine(text);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Commands.Cmd($"touch {file}");
					File.WriteAllText(file, text);
				}
				else
				{
					throw new NotSupportedException("Unsupported OS");
				}
			}
			else
			{
				File.WriteAllText(file, text);
			}

			if (logOutput)
				Console.WriteLine($"Saved to {file}");
		}
	}
}