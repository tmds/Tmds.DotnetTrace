using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Tmds.DotnetTrace.Tool
{
    class Program
    {
        const string TraceFolderName = ".dotnet-trace";
        const string SessionName = "dotnet-tool-trace";

        static readonly string OutputArg = Directory.GetCurrentDirectory();
        static string TraceFolderPath => Path.Combine(Directory.GetCurrentDirectory(), TraceFolderName);
        static TraceFolder CurrentTraceFolder { get; } = new TraceFolder(TraceFolderPath);

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify a command argument: start, stop, report, clean.");
            }

            string command = args[0];
            switch (command)
            {
                case "start":
                    Start();
                    break;
                case "stop":
                    Stop();
                    break;
                case "report":
                    Report();
                    break;
                case "clean":
                    Clean();
                    break;
                default:
                    CurrentTraceFolder.TryAddIgnoreCurrentProcess();
                    Console.WriteLine($"Unknown command: {command}.");
                    break;
            }
        }

        static void Start()
        {
            Clean();
            CurrentTraceFolder.Create();
            CurrentTraceFolder.AddIgnoreCurrentProcess();

            // create session
            RunProcess("lttng", $"create {SessionName} --output {TraceFolderPath}");

            // add pid
            RunProcess($"lttng", $"add-context --userspace --session={SessionName} --type=vpid");

            // enable events
            var events = new List<string>() {
                DotNetEvents.RuntimeInformationStart,
                DotNetEvents.GCAllocationTick_V3,
                DotNetEvents.GCAllocationTick_V2, // https://github.com/dotnet/coreclr/pull/14338
                DotNetEvents.GCStart_V2,
                DotNetEvents.GCHeapStats_V1,
                DotNetEvents.ExceptionThrown_V1,
                DotNetEvents.MethodJitInliningFailed
            };
            RunProcess("lttng", $"enable-event --session={SessionName} --userspace {string.Join(",", events)}");

            // start
            RunProcess("lttng", $"start {SessionName}");

            Console.WriteLine($"Storing trace in: {CurrentTraceFolder.Path}");
            Console.WriteLine("Don't forget to 'export COMPlus_EnableEventLog=1' for traced applications.");
            Console.WriteLine("Set 'export COMPlus_EventSourceFilter=q' too to avoid overhead of EventSources.");
        }

        static void Stop()
        {
            CurrentTraceFolder.TryAddIgnoreCurrentProcess();

            RunProcess("lttng", $"stop {SessionName}", exitCode: null);
            RunProcess("lttng", $"destroy {SessionName}", exitCode: null);
        }

        static void Report()
        {
            var multiReportCollector = new MultiReportCollector();

            multiReportCollector.ReadEvents(CurrentTraceFolder);

            foreach (var collector in multiReportCollector.ReportCollectors)
            {
                collector.WriteReport(Console.Out);
            }
        }

        static void Clean()
        {
            Stop();
            CurrentTraceFolder.Clean();
        }

        static void RunProcess(string program, string arguments, int? exitCode = 0)
        {
            var psi = new ProcessStartInfo(program, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
                if (exitCode != null && exitCode != process.ExitCode)
                {
                    throw new InvalidOperationException($"Process '{program} {arguments}' failed with {process.ExitCode}");
                }
            }
        }
    }
}
