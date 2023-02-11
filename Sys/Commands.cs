using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace Mathlib.Sys
{
	public static class Commands
	{
		public static string Path { get { return AppDomain.CurrentDomain.BaseDirectory; } }
		public static string RootFolder { get
			{
				string[] directories = Path.Split('/');
				string root = "";
				foreach (string s in directories)
				{
					if (s.Contains("bin"))
						break;	
					root += s + "/";
				}
				return root;
			} 
		}

		/// <summary> 
		/// Run one or more commands in the command prompt.
		/// </summary>
		/// <param name="commands">Commands to be executed in the command prompt.</param>
		/// <returns>Full command line text after execution of the given commands.</returns>
		public static StreamReader Cmd(params string[] commands)
		{
			try
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					// Create a batch file in the exe directory, given a unique file name.
					// This file will contains all given commands to be executed in order.
					string batFileName = Path + @"\" + Guid.NewGuid() + ".bat";
					// Write each given command to the batch.
					using (StreamWriter batFile = new StreamWriter(batFileName))
					{
						foreach (string s in commands)
							batFile.WriteLine(s);
					}

					// Create the start info for the command line process, referencing the batch file instead of a single command.
					ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c" + batFileName)
					{
						CreateNoWindow = true,
						UseShellExecute = false,
						RedirectStandardOutput = true
					};
					// Start the process, executing each command in the batch file.
					Process process = Process.Start(processInfo);

					process.WaitForExit();
					// Once the process has finished, delete the batch file.
					File.Delete(batFileName);

					// Return the cmd output of the commands.
					return process.StandardOutput;
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					// Create a shell file in the application directory, given a unique file name.
					// This file will contains all given commands to be executed in order.
					string bashFileName = Path + Guid.NewGuid() + ".sh";
					// Write each given command to the batch.
					using (StreamWriter bashFile = new StreamWriter(bashFileName))
					{
						foreach (string s in commands)
							bashFile.WriteLine(s);
					}


					// Create the start info for the command line process, referencing the bash file instead of a single command.
					ProcessStartInfo startInfo = new ProcessStartInfo() 
					{ 
						FileName = "/bin/bash", 
						Arguments = $"{bashFileName}",
        				RedirectStandardOutput = true
					}; 
					// Start the process, executing each command in the batch file.
					Process process = Process.Start(startInfo);

					StreamReader output = process.StandardOutput;

					process.WaitForExit();
					// Once the process has finished, delete the bash file.
					File.Delete(bashFileName);

					// Return the cmd output of the commands.
					return process.StandardOutput;
				}
				else
				{
					throw new NotSupportedException("Unsupported OS");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
		}

		/// <summary>
		/// Suppliment of the Cmd method.
		/// Implicitly reads the command line output. 
		/// </summary>
		/// <param name="commands">Commands to be executed in the command prompt.</param>
		public static void CmdOut(params string[] commands)
		{
			StreamReader output = Cmd(commands);
			Read(output);
		}

		/// <summary>
		/// Print each line of a StreamReader.
		/// Designed to read non-command output from the command line.
		/// </summary>
		public static void Read(StreamReader reader)
		{
			// Loop until the end of the stream.
			while (!reader.EndOfStream)
			{
				// Get the next line.
				string line = reader.ReadLine();
				// First check that the line does not begin with the specified string.
				// Since this is specialized for command line text, this culls out the actual input command itself.
				// Also, don't print a blank line. That's silly.
				if (!line.StartsWith("C:\\Users") && line.Length > 0)
					Console.WriteLine(line);
			}
			reader.Close();
		}

		internal static void ClearLastLine(int index = -1)
		{
			// Get the line index of the last line.
			int currentLineCursor = index == -1 ? Console.CursorTop - 1 : index;
			// Move the cursor to the start of that line.
			Console.SetCursorPosition(0, currentLineCursor);
			// Write a bunch of whitespace to overwrite anything on that line.
			Console.Write(new string(' ', Console.WindowWidth));
			// Move the cursor back to the start of the line.
			Console.SetCursorPosition(0, currentLineCursor);
		}
	}
}