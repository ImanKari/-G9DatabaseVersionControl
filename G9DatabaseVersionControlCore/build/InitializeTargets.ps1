﻿Param([string] $ProjectDir, [string] $ProjectCprojPath)

Write-Host "### Start Generate 'G9DatabaseVersionControlCore.targets' ###"

# Random Delay to ignore DDOS access request for multi-target project :))
$randomDelay = Get-Random -Minimum 100 -Maximum 2000
sleep -m $randomDelay

# Lock thread
$SyncHash = [hashtable]::Synchronized(@{Test='Test'})
[System.Threading.Monitor]::Enter($SyncHash)

# Get assembly version
$xml = [Xml] (Get-Content $projectCprojPath)
[String] $version = [String]::Empty
if ($xml.Project.PropertyGroup.Version -is [Array]){
    $version = $xml.Project.PropertyGroup.Version[0].ToString()
}else{
    $version = $xml.Project.PropertyGroup.Version.ToString()
}
$targetFileName = "$($projectDir)\build\G9DatabaseVersionControlCore.targets"

# Check validation
if ([String]::IsNullOrEmpty($version)){
    Write-Host $projectDir'build\InitializeTargets.ps1(20)' : error -9: 'Version not found!'
    exit 1
}

# Create file if not exist
if (-not(Test-Path $targetFileName)){
    New-Item $targetFileName -force
}

# Set content for file
Set-Content $targetFileName "<?xml version=""1.0"" encoding=""utf-8""?>
<!-- NOTICE: This file is created automatically by Script 'InitializeTargets.ps1'. -->
<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Target Name=""BeforeBeforeBuild"" BeforeTargets=""BeforeBuild"">
    <Exec
      Command=""powershell.exe -executionpolicy Unrestricted -File &quot;`$(NuGetPackageRoot)/g9databaseversioncontrolcore/$version/tools/G9DBVCValidation.ps1&quot; -projectDir &quot;`$(ProjectDir)\&quot; ""
      Condition="" EXISTS('`$(ProjectDir)DatabaseVersionControlValidation.ps1') AND '`$(NuGetPackageRoot)' != '' AND '`$(OS)' == 'Windows_NT' "" />
    <Exec
      Command=""command -v pwsh >/dev/null 2>&amp;1 &amp;&amp; pwsh `$(NuGetPackageRoot)/g9databaseversioncontrolcore/$version/tools/G9DBVCValidation.ps1 `$(ProjectDir) || echo 'PowerShell' : warning -9: '[Warning]: PowerShell software must be installed for verification and validation of the Database version control file (Only required for development environment on Linux and Mac to access &quot;pwsh&quot; command). https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux?view=powershell-7.1'""
      Condition="" EXISTS('`$(ProjectDir)DatabaseVersionControlValidation.ps1') AND '`$(NuGetPackageRoot)' != '' AND '`$(OS)' != 'Windows_NT' "" />
  </Target>
</Project>" -force

# Unlock
if ($LockTaken)
{
    [System.Threading.Monitor]::Exit($SyncHash)
}

Write-Host "### Finish Generate 'G9DatabaseVersionControlCore.targets' ###"