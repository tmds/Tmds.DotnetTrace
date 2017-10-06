# Tmds.Babeltrace

*Tmds.Babeltrace* provides .NET bindings for [libbabeltrace](https://www.efficios.com/babeltrace)
that allow reading [common trace format](http://diamon.org/ctf/) (CTF) files
generated using [Linux Trace Toolkit next generation](http://lttng.org/) (LTTng).

To use *Tmds.Babeltrace* you need libbabeltrace on your system.

Fedora: `dnf install libbabeltrace`

The following example iterates over all events and prints out the name, each field's name and its type.

```C#
using (var context = new BtContext())
{
    context.AddTrace("/home/tmds/lttng-traces/my-user-space-session-20170927-100141/ust/uid/1000/64-bit");
    int i = 0;
    foreach (var ev in context.GetEvents())
    {
        Console.WriteLine($"Event {i++}: {ev.Name}");
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
                System.Console.WriteLine($"* {field.Name}: {field.Declaration.Type}");
            }
        }
    }
}
```

# Tmds.DotnetTrace.Tool

*Tmds.DotnetTrace.Tool* allows to trace a dotnet application and get a summary of some important trace events.

To use *Tmds.DotnetTrace.Tool* you need libbabeltrace and lttng on your system.

Fedora: `dnf install libbabeltrace lttng-tools`

```
$ dotnet Tmds.DotnetTrace.Tool.dll start
$ dotnet Tmds.DotnetTrace.Tool.dll stop
$ dotnet Tmds.DotnetTrace.Tool.dll report
```