# Intro
Simplest sample: concatenates strings in the same task work item

# Security
Local Service won't work: use a local or domain account. The service account must be, at least, Project Admin

# Troubleshooting
Ensure `loggingVerbosity` is set to `Diagnostic` in the `AggregatorItems.xml` file.

```
<?xml version="1.0" encoding="utf-8"?>
<AggregatorItems
    tfsServerUrl="http://localhost:8080/tfs/2013u4Collection"
    loggingVerbosity="Diagnostic">
  <!--...-->
</AggregatorItems>
```

Launch [dbgView](https://technet.microsoft.com/en-us/sysinternals/bb896647.aspx)

Ensure `Capture Win32` and `Capture Global Win32` options are checked under `Capture` menu.

Enable tracing `http://`<tfs_server>`:8080/tfs/tftrace.ashx?traceWriter=true&All=Verbose`

Disable when done `http://`<tfs_server>`:8080/tfs/tftrace.ashx?traceWriter=false&All=Off`