using System;
using System.Text.RegularExpressions;
using Cake.Common;
using Cake.Common.IO;
using Cake.Core;
using Cake.Frosting;
using Cake.FileHelpers;
using Cake.Core.IO;
using System.Collections.Generic;
using Cake.Common.Tools.DotNet;
using Cake.SevenZip;
using Cake.SevenZip.Commands;
using Cake.SevenZip.Switches;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public string DotNetBuildConfiguration { get; set; }
    public string Framework { get; set; }
    public string Runtime { get; set; }
    public string GitVersion { get; set; }
    public string AssemblyVersion { get; set; }
    public bool DisablePublish { get; set; }
    public bool CleanPublish { get; set; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        DotNetBuildConfiguration = context.Argument("configuration", "Release");
        Framework = context.Argument("framework", "net8.0");
        Runtime = context.Argument("runtime", "win-x64");
        DisablePublish = context.Argument("disablepublish", false);
        CleanPublish = context.Argument("cleanpublish", false);
    }
}

[TaskName("CleanRelease")]
public sealed class CleanReleaseTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        Console.WriteLine(Environment.NewLine + "Cleaning release related folders...");
        context.CleanDirectory($"../HeadlessMonitor/bin/{context.DotNetBuildConfiguration}");
        context.CleanDirectory($"../Release");
    }
}

[TaskName("AssemblyVersion")]
public sealed class AssemblyVersionTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (FileAliases.FileExists(context, "../HeadlessMonitor/HeadlessMonitor.csproj"))
        {
            Console.WriteLine(Environment.NewLine + "Reading assembly version from HeadlessMonitor.csproj");
            string _AssemblyVersion = FileHelperAliases.FindRegexMatchInFile(context, "../HeadlessMonitor/HeadlessMonitor.csproj", "<AssemblyVersion>(.*?)</AssemblyVersion>", RegexOptions.None);
            context.AssemblyVersion = _AssemblyVersion.Replace("<AssemblyVersion>", "").Replace("</AssemblyVersion>", "").Replace("0.0.", "");
            Console.WriteLine(Environment.NewLine + $"AssemblyVersion version: {context.AssemblyVersion}");
        }
    }
}

[TaskName("GitVersion")]
public sealed class GitVersionTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        IEnumerable<string> _GitVersion;
        var exitCodeWithArgument = ProcessAliases.StartProcess(
            context,
            "git",
            new ProcessSettings
            {
                Arguments = "rev-parse --short HEAD",
                RedirectStandardOutput = true
            }
            ,
            out _GitVersion
        );
        context.GitVersion = string.Join("", _GitVersion).Trim();
        if (context.GitVersion == "") context.GitVersion = "0";
        context.GitVersion = "git-" + context.GitVersion;
        Console.WriteLine(Environment.NewLine + $"Git version: {context.GitVersion}");
    }
}

[TaskName("PatchGitVersion")]
[IsDependentOn(typeof(GitVersionTask))]
public sealed class PatchGitVersionTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (FileAliases.FileExists(context, "../HeadlessMonitor/Program.cs"))
        {
            Console.WriteLine(Environment.NewLine + $"Patching Git version: {context.GitVersion}");
            FileHelperAliases.ReplaceRegexInFiles(context, "../HeadlessMonitor/Program.cs", "readonly string GitVersion = \"(.*?)\"", $"readonly string GitVersion = \"{context.GitVersion}\"");
        }
        else
            Console.WriteLine("Could not patch Git version! (Program.cs not found)");
    }
}

[TaskName("Build")]
[IsDependentOn(typeof(CleanReleaseTask))]
[IsDependentOn(typeof(PatchGitVersionTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        Console.WriteLine(Environment.NewLine + $"Starting {context.Runtime} release building... (ignore possible warnings)");
        if (context.Runtime.ToLower().Contains("linux"))
        {
            throw new Exception("This programm can not be build for Linux OS!");
        }
        else
        {
            context.DotNetPublish("../HeadlessMonitor.sln", new Cake.Common.Tools.DotNet.Publish.DotNetPublishSettings
            {
                Configuration = context.DotNetBuildConfiguration,
                Runtime = context.Runtime,
                SelfContained = true,
                PublishTrimmed = true,
                PublishSingleFile = false,
                PublishReadyToRun = false,
                PublishReadyToRunShowWarnings = false,
                EnableCompressionInSingleFile = true,
                IncludeAllContentForSelfExtract = true,
                IncludeNativeLibrariesForSelfExtract = true,
                Verbosity = DotNetVerbosity.Quiet,
                DiagnosticOutput = false,
                ArgumentCustomization = args => args.Append("/p:DebugType=none")
                                                    .Append("/p:DebugSymbols=false")
                                                    .Append("/p:PublishAoT=true")
                                                    .Append("/p:TrimMode=full")
                                                    .Append("/p:TrimmerRemoveSymbols=true")
                                                    .Append("/p:TrimmerSingleWarn=true")
                                                    .Append("/p:SuppressTrimAnalysisWarnings=true")
                                                    .Append("/p:EnableTrimAnalyzer=false")
                                                    .Append("/p:DebuggerSupport=false")
                                                    .Append("/p:EnableUnsafeBinaryFormatterSerialization=false")
                                                    .Append("/p:PublishDir=\"../Release\"")
            });
        }
        //Delete for later publishing unwanted debug files
        FileAliases.DeleteFiles(context, "../Release/*.pdb");
    }
}

[TaskName("Publish")]
[IsDependentOn(typeof(AssemblyVersionTask))]
[IsDependentOn(typeof(BuildTask))]
public sealed class PublishTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (context.DisablePublish)
        {
            Console.WriteLine(Environment.NewLine + $"Skipping Zip publishing...");
            return;
        }

        if (context.CleanPublish)
        {
            Console.WriteLine(Environment.NewLine + "Cleaning publish related folders...");
            context.CleanDirectory($"../Publish");
        }

        Console.WriteLine(Environment.NewLine + $"Starting {context.Runtime} Zip publishing...");
        SevenZipAliases.SevenZip(context, new SevenZipSettings
        {
            Command = new AddCommand
            {
                Files = new FilePathCollection(new[]
                {
                    new FilePath("../Release/."),
                    new FilePath("../LICENSE"),
                    new FilePath("../README.txt")
                }),
                Archive = new FilePath($"../Publish/HeadlessMonitor_V{context.AssemblyVersion}_{context.Runtime}.zip")
            }
        });
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(PublishTask))]
public class DefaultTask : FrostingTask
{
}