To use these packages, add the following myget feed to your NuGet.Config file.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="tmds" value="https://www.myget.org/F/tmds/api/v3/index.json" />
  </packageSources>
</configuration>
```

# Tmds.Babeltrace

*Tmds.Babeltrace* provides .NET bindings for [libbabeltrace](https://www.efficios.com/babeltrace)
that allow reading [common trace format](http://diamon.org/ctf/) (CTF) files
generated using [Linux Trace Toolkit next generation](http://lttng.org/) (LTTng).

To use Tmds.Babeltrace you need *libbabeltrace* on your system.

Fedora: `dnf install libbabeltrace-devel`

The following example iterates over all events and prints out the event name, each field's name and its type.

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

*Tmds.DotnetTrace.Tool* allows to trace a dotnet application and get a summary report of the trace.

This is making use of LTTng tracing performed by .NET Core. You **must** set `export COMPlus_EnableEventLog=1` to enable
.NET Core tracing. You **may** want to set `export COMPlus_EventSourceFilter=q` to avoid overhead of enabling all
`EventSources` (`q` can be anything that isn't an event source name).

To use Tmds.DotnetTrace.Tool you need *libbabeltrace* and *lttng* on your system.

Fedora: `dnf install libbabeltrace lttng-tools`

Now add the tool using DotNetCliToolReference to a project (it doesn't matter which):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <DotNetCliToolReference Include="Tmds.DotnetTrace.Tool" Version="0.1.0-*" />
  </ItemGroup>
</Project>
```

Now run restore:
```
$ dotnet restore
```

You can start/stop and view the report using the following commands:

```
$ dotnet trace start
$ dotnet trace stop
$ dotnet trace report | less
```

PS: don't forget to set `COMPlus_EnableEventLog=1` (and `COMPlus_EventSourceFilter=q`) for the applications you wish to
trace!
