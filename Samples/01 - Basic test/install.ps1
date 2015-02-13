<#
	Installs and configure TFS Aggregator on target TFS
#>

# make sure these path are correct for target environment
$TfsFolder = "$env:ProgramFiles\Microsoft Team Foundation Server 12.0"
$VsFolder = "${env:ProgramFiles(x86)}\Microsoft Visual Studio 12.0"
$CollectionUrl = "http://localhost:8080/tfs/2013u4Collection"
$ProjectName = "Aggregator"

$PluginsFolder = "$TfsFolder\Application Tier\Web Services\bin\Plugins"

# create EventLog source for TFS Aggregator
New-EventLog -LogName "Application" -Source "TFSAggregator" -ErrorAction SilentlyContinue

# install the plug-in
Copy-Item .\TFSAggregator.dll $PluginsFolder
Copy-Item .\TFSAggregator.pdb $PluginsFolder
Copy-Item .\AggregatorItems.xml $PluginsFolder
