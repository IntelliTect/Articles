using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace EssentialDotNetConfiguration
{
public class Program
{
    static public string DefaultConnectionString { get; } =
        @"Server=(localdb)\\mssqllocaldb;Database=SampleData-0B3B0919-C8B3-481C-9833-36C21776A565;Trusted_Connection=True;MultipleActiveResultSets=true";

    static IReadOnlyDictionary<string, string> DefaultConfigurationStrings{ get; } =
        new Dictionary<string, string>()
        {
            ["Profile:UserName"] = Environment.UserName,
            [$"AppConfiguration:ConnectionString"] = DefaultConnectionString,
            [$"AppConfiguration:MainWindow:Height"] = "40",
            [$"AppConfiguration:MainWindow:Width"] = "60",
            [$"AppConfiguration:MainWindow:Top"] = "0",
            [$"AppConfiguration:MainWindow:Left"] = "0",
        };

    static public Dictionary<string,string> GetSwitchMappings(
        IReadOnlyDictionary<string, string> configurationStrings)
    {
        return configurationStrings.Select(item =>
            new KeyValuePair<string, string>(
                "-" + item.Key.Substring(item.Key.LastIndexOf(':')+1),
                item.Key))
                .ToDictionary(
                    item => item.Key, item=>item.Value);
    }

    static public IConfiguration Configuration { get; set; }

    public static void Main(string[] args = null)
    {
        ConfigurationBuilder configurationBuilder = 
                new ConfigurationBuilder();
            

        if (args == null)
        {
            // Add defaultConfigurationStrings
            configurationBuilder.AddInMemoryCollection(
                DefaultConfigurationStrings);
        }
        else
        {
            configurationBuilder
                .AddInMemoryCollection(DefaultConfigurationStrings)
                .AddJsonFile("Config.json", 
                    true) // bool indicates file is optional
                // "EssentialDotNetConfiguration" is an optional prefix for all 
                // environment configuration keys  
                .AddEnvironmentVariables("EssentialDotNetConfiguration")
                .AddCommandLine(
                    args, GetSwitchMappings(DefaultConfigurationStrings));
        }
        Configuration = configurationBuilder.Build();

        Console.WriteLine($"Hello {Configuration["Profile:UserName"]}");

        ConsoleWindow consoleWindow = 
                Configuration.Get<ConsoleWindow>("AppConfiguration:MainWindow");
        ConsoleWindow.SetConsoleWindow(consoleWindow);
    }
}
}
