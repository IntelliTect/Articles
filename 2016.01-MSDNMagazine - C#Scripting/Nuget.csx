using System.Diagnostics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

class Nuget
{
	public static IEnumerable<string> Install(
		string packageId, string outputDirectory=null)
	{
		if(outputDirectory == null)
		{
			outputDirectory = Environment.CurrentDirectory;
		}
		string arguments = 
			$"install { packageId } " 
				+ $"-OutputDirectory \"{ outputDirectory.Trim('"') }\" -NonInteractive";
	

		// Assume nugget.exe is in the path.
		ProcessStartInfo processInfo = 
			new ProcessStartInfo("nuget.exe", arguments);
		
		processInfo.UseShellExecute = false;
		processInfo.RedirectStandardError = true;
		processInfo.RedirectStandardOutput = true;
			
		Process nuget = 
			Process.Start(processInfo);
		nuget.WaitForExit();
		if(nuget.ExitCode != 0)
		{
			throw new Exception(nuget.StandardError.ReadToEnd());
		}
		
		return GetAssemblies(packageId, outputDirectory);
	}
	
	static public IEnumerable<string> GetAssemblies(string packageId, string directory=null)
	{
		directory = directory??Environment.CurrentDirectory;
		string nugetDirectory = Directory.GetDirectories(directory, $"{packageId}.*").First();
		return Directory.GetFiles(
			Path.Combine(nugetDirectory, "lib\\net45"), $"{packageId}.dll" );
	}
	static public void Uninstall(string packageId, string outputDirectory=null)
	{
		outputDirectory = outputDirectory??Environment.CurrentDirectory;
		string directory = Directory.GetDirectories(outputDirectory, $"{packageId}.*").First();
		Directory.Delete(directory, true);		
	}
	
	static public void AddReference(string packageId)
	{
		string assembly = Nuget.GetAssemblies("Newtonsoft.Json").First();
		if(!File.Exists(assembly))
		{
			throw new Exception($"{assembly} does not exists.");
		}
		
		Assembly.LoadFrom(assembly);
	}
}

