using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;
using Screen;
using Windows.Win32;

//Disable unimportant warnings
#pragma warning disable CS8604
#pragma warning disable S1075

namespace HeadlessMonitor
{
    public static class Program
    {
        //Constants declarations
        private static readonly string AppVersion = $"V{Assembly.GetEntryAssembly()?.GetName().Version?.Major.ToString()}.{Assembly.GetEntryAssembly()?.GetName().Version?.MinorRevision.ToString()}";
        private static readonly string AppName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "HeadlessMonitor";
#if RELEASE
        private static readonly string AppConfig = "(Release)";
#else
        private static readonly string AppConfig = "(Debug)";
#endif
        private static readonly string AppCopyright = $"© Alexander Feuster 2024";
        private static readonly string AppURL = "https://github.com/feuster/HeadlessMonitor";
        //GitVersion will only be actualized/overwritten when using Cake build!
        private static readonly string GitVersion = "git-d30ead0";
        private static readonly bool   OSPlatformWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        //Program start
        static void Main(string[] args)
        {
            #region Initialize program
            //Local declarations
            bool ResultSetPrimaryResolution;
            bool ResultSetPrimaryDPI;

            //Init console
            PrimaryScreen.DpiAwareness.SetDPIPerMonitorAware();
            PInvoke.AllocConsole();
            ConsoleWidgets.ConsoleEncodingUnicode();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.WriteLine(ConsoleWidgets.Figlet);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            ConsoleWidgets.PrintLine("About", ConsoleColor.Green, Console.BackgroundColor);
            Console.WriteLine();
            if(DateTime.Now.Year > 2024)
                Console.WriteLine($"{AppName} {AppVersion} {AppConfig} {GitVersion}   {AppCopyright}-{DateTime.Now.Year}   {AppURL}");
            else
                Console.WriteLine($"{AppName} {AppVersion} {AppConfig} {GitVersion}   {AppCopyright}   {AppURL}");

            //In case someone builds a non Windows version throw an error message and quit
            if(!OSPlatformWindows)
            {
                Console.WriteLine();
                ConsoleWidgets.PrintLine("Unsupported OS Platform", ConsoleColor.Red, Console.BackgroundColor);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.WriteLine("This tool is only intended for Windows OS!");
                Console.WriteLine();
                ConsoleWidgets.PressAnyKey(true);
            }
            #endregion

            #region Define Commandline Arguments
            //Map different args formats to a single value
            var switchMappings = new Dictionary<string, string>()
            {
                { "-w",          "Width" },
                { "--width",     "Width" },
                { "-h",          "Height" },
                { "--height",    "Height" },
                { "-d",          "DPI" },
                { "--dpi",       "DPI" }
            };

            //default values
            var InMemoryCollection = new Dictionary<string, string?>()
            {
                ["Width"] =  "",
                ["Height"] = "",
                ["DPI"] =    ""
            };

            //Build a configuration object from command line
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(InMemoryCollection).AddCommandLine(args, switchMappings).Build();
            #endregion

            #region Validate Commandline Arguments
            //Close application on error or finish without calling PressAnyKey
            if (string.Join(' ', args).Contains("-c", StringComparison.OrdinalIgnoreCase))
                ConsoleWidgets.Close = true;

            //call help if argument is called
            if (string.Join(' ', args).Contains("--help", StringComparison.OrdinalIgnoreCase))
                Help();

            //copy arguments to value variables
            string? Width = string.IsNullOrEmpty(config["Width"]) ? "" : config["Width"];
            string? Height = string.IsNullOrEmpty(config["Height"]) ? "" : config["Height"];
            string? DPI = string.IsNullOrEmpty(config["DPI"]) ? "" : config["DPI"];

            //validate values
            Console.WriteLine();
            ConsoleWidgets.PrintLine("Arguments", ConsoleColor.Green, Console.BackgroundColor);
            Console.WriteLine();

            //no values available
            if (string.IsNullOrEmpty(Width) && string.IsNullOrEmpty(Height) && string.IsNullOrEmpty(DPI))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No arguments given! See \"{AppName}.exe --help\"");
                Console.ForegroundColor = ConsoleColor.Blue;
                Help();
            }

            //Width missing
            if (string.IsNullOrEmpty(Width) && !string.IsNullOrEmpty(Height))
            {
                Console.WriteLine($"Width argument missing! See \"{AppName}.exe --help\"");
                Console.WriteLine();
                ConsoleWidgets.PressAnyKey(true);
            }

            //Height missing
            if (!string.IsNullOrEmpty(Width) && string.IsNullOrEmpty(Height))
            {
                Console.WriteLine($"Height argument missing! See \"{AppName}.exe --help\"");
                Console.WriteLine();
                ConsoleWidgets.PressAnyKey(true);
            }

            //DPI missing
            if (string.IsNullOrEmpty(DPI))
            {
                Console.WriteLine($"DPI argument missing! See \"{AppName}.exe --help\"");
                Console.WriteLine();
                ConsoleWidgets.PressAnyKey(true);
            }

            //list values
            Console.WriteLine("Forcing new headless primary screen resolution with following settings:");
            Console.WriteLine($"  Width:  {Width} pixels");
            Console.WriteLine($"  Height: {Height} pixels");
            Console.WriteLine($"  DPI:    {DPI}%");
            #endregion

            #region Actions
            //check if desired screen settings are not already active
            if (uint.Parse(Width ?? "0") == PrimaryScreen.PrimaryScreenStats.GetPrimaryScreenWidth() &&
                uint.Parse(Height ?? "0") == PrimaryScreen.PrimaryScreenStats.GetPrimaryScreenHeight() &&
                uint.Parse(DPI ?? "0") == PrimaryScreen.PrimaryScreenStats.GetPrimaryScreenDPI())
            {
                Console.WriteLine();
                ConsoleWidgets.PrintLine("Finished", ConsoleColor.Green, Console.BackgroundColor);
                Console.WriteLine();
                Console.WriteLine($"Actual screen values are already set to the desired values!");
                Console.WriteLine();
                ConsoleWidgets.PressAnyKey(true);
            }

            //Actions
            Console.WriteLine();
            ConsoleWidgets.PrintLine("Actions", ConsoleColor.Green, Console.BackgroundColor);
            Console.WriteLine();

            //set new screen resolution
            ResultSetPrimaryResolution = PrimaryScreen.Actions.SetPrimaryResolution(UInt32.Parse(Width), UInt32.Parse(Height));
            if (!ResultSetPrimaryResolution)
                Console.WriteLine("SetPrimaryResolution: failed!");
            else
                Console.WriteLine("SetPrimaryResolution: succeeded!");

            //set new screen DPI
            ResultSetPrimaryDPI = PrimaryScreen.Actions.SetPrimaryDPI(UInt32.Parse(DPI));
            if (!ResultSetPrimaryDPI)
                Console.WriteLine("SetPrimaryDPI:        failed!");
            else
                Console.WriteLine("SetPrimaryDPI:        succeeded!");
            #endregion

            #region Validate if actions were successful
            //finished
            Console.WriteLine();
            ConsoleWidgets.PrintLine("Finished", ConsoleColor.Green, Console.BackgroundColor);
            Console.WriteLine();
            Console.WriteLine("Actual primary screen resolution:");
            Console.WriteLine($"  Width:  {PrimaryScreen.PrimaryScreenStats.GetPrimaryScreenWidth()} pixels");
            Console.WriteLine($"  Height: {PrimaryScreen.PrimaryScreenStats.GetPrimaryScreenHeight()} pixels");
            Console.WriteLine($"  DPI:    {PrimaryScreen.PrimaryScreenStats.GetPrimaryScreenDPI()}%");
            Console.WriteLine();

            //check if no difference
            if (uint.Parse(Width ?? "0") == PrimaryScreen.PrimaryScreenStats.GetPrimaryScreenWidth() &&
                uint.Parse(Height ?? "0") == PrimaryScreen.PrimaryScreenStats.GetPrimaryScreenHeight() &&
                uint.Parse(DPI ?? "0") == PrimaryScreen.PrimaryScreenStats.GetPrimaryScreenDPI())
            {
                Console.WriteLine($"Actual screen values have been set to the desired values.");
            }
            else
                Console.WriteLine($"Actual screen values have not been set to the desired values!");
            #endregion

            #region Exit program with matching ExitCode
            //end
            Console.WriteLine();
            ConsoleWidgets.PressAnyKey(true);
            if (!ResultSetPrimaryResolution || !ResultSetPrimaryDPI)
                Environment.Exit(-1);
            else
                Environment.Exit(0);
            #endregion
        }

        #region Help
        /// <summary>
        /// Show help
        /// </summary>
        public static void Help()
        {
            Console.WriteLine();
            ConsoleWidgets.PrintLine("Help", ConsoleColor.Green, Console.BackgroundColor);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine($"{AppName} can set a new resolution in a Windows system which is used headless without physical monitor");
            Console.WriteLine();
            Console.WriteLine("  -w, --width     the new headless resolution pixel width");
            Console.WriteLine("  -h, --height    the new headless resolution pixel height");
            Console.WriteLine("  -d, --dpi       the new headless resolution DPI in percent");
            Console.WriteLine($"  -c, --close     close {AppName} on errors and after finishing");
            Console.WriteLine("      --help      opens this help page");
            Console.WriteLine();
            Console.WriteLine($"  Example:  \"{AppName}.exe --width 1920 --height 1080 --dpi 125\"");
            Console.WriteLine();
            ConsoleWidgets.PressAnyKey(true);
        }
        #endregion
    }
}
