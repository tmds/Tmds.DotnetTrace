using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tmds.Babeltrace;

namespace Tmds.DotnetTrace.Tool
{
    class MultiReportCollector
    {
        private Dictionary<int, ReportCollector> _pidReportCollectors = new Dictionary<int, ReportCollector>();

        public ReportCollector[] ReportCollectors => _pidReportCollectors.Values.ToArray();

        public void ReadEvents(TraceFolder traceFolder)
        {
            using (var context = new BtContext())
            {
                var traces = Directory.GetFiles(traceFolder.Path, "metadata", SearchOption.AllDirectories);
                foreach (var trace in traces)
                {
                    context.AddTrace(Path.GetDirectoryName(trace));
                }

                ReadEvents(context, traceFolder.IgnorePids);
            } 
        }

        private void ReadEvents(BtContext context, int[] ignorePids)
        {
            ReportCollector previousCollector = null;

            foreach (var ev in context.GetEvents())
            {
                int vpid = ev.Scope(CtfScope.StreamEventContext).Field("vpid").GetInt32();

                // Check if ignoring this pid
                for (int i = 0; i < ignorePids.Length; i++)
                {
                    if (vpid == ignorePids[i])
                    {
                        continue;
                    }
                }

                // Find collector
                ReportCollector collector;
                if (previousCollector != null && previousCollector.Pid == vpid)
                {
                    // Same collector as previous event
                    collector = previousCollector;
                }
                else
                {
                    if (!_pidReportCollectors.TryGetValue(vpid, out collector))
                    {
                        collector = new ReportCollector(vpid);
                        _pidReportCollectors.Add(vpid, collector);
                    }
                    previousCollector = collector;
                }

                switch (ev.Name)
                {
                    case DotNetEvents.RuntimeInformationStart:
                        collector.NewProcess = true;
                        break;
                    case DotNetEvents.GCAllocationTick_V3:
                    {
                        string typeName;
                        EventFieldReader.ReadGCAllocationTickV3(ev, out typeName);
                        collector.AddAllocationSample(typeName);
                    }
                        break;
                    case DotNetEvents.GCStart_V2:
                    {
                        int depth;
                        EventFieldReader.ReadGCStart_V2(ev, out depth);
                        collector.AddGC(depth);
                    }
                        break;
                    case DotNetEvents.GCHeapStats_V1:
                    {
                        ulong gen0Size;
                        ulong gen1Size;
                        ulong gen2Size;
                        EventFieldReader.ReadGCHeapStats_V1(ev, out gen0Size, out gen1Size, out gen2Size);
                        // convert to MB
                        gen0Size = gen0Size >> 16;
                        gen1Size = gen1Size >> 16;
                        gen2Size = gen2Size >> 16;
                        collector.AddHeapStats(gen0Size, gen1Size, gen2Size);
                    }
                        break;
                    case DotNetEvents.ExceptionThrown_V1:
                    {
                        string typeName;
                        string message;
                        EventFieldReader.ReadExceptionThrown_V1(ev, out typeName, out message);
                        collector.AddExceptionThrown(typeName, message);
                    }
                        break;
                }
            }
        }

        // For development
        static void PrintFields(Event ev)
        {
            Console.WriteLine($"Item: {ev.Name} - {ev.TimestampNs} - {ev.TimestampCycles}");
            foreach (var scope in new[] {
                CtfScope.PacketHeader,
                CtfScope.PacketContext,
                CtfScope.StreamEventHeader,
                CtfScope.StreamEventContext,
                CtfScope.EventContext,
                CtfScope.EventFields
            })
            {
                Console.WriteLine($"Scope: {scope}");
                foreach (var field in ev.Scope(scope).Fields)
                {
                    Console.WriteLine($"* {field.Name}: {field.Declaration.Type}");
                }
            }
        }
    }
}