param($installPath, $toolsPath, $package, $project)
# Start log
Write-Host "################ Start Install ################" -ForegroundColor Green

# Get target path
$projectFullName = $project.FullName
$fileInfo = new-object -typename System.IO.FileInfo -ArgumentList $projectFullName
$targetProjectDirectory = $fileInfo.DirectoryName
$projectName = $project.Name;

# Add requirement - File And Folders
Write-Host ""
Write-Host "Initialize requirement:" -ForegroundColor Yellow
Write-Host "Target project path: $targetProjectDirectory" -ForegroundColor Yellow

# Folders
Write-Host "Check exist folder: '$targetProjectDirectory\DatabaseUpdateFiles\'" -ForegroundColor Yellow
if (-not (Test-Path "$targetProjectDirectory\DatabaseUpdateFiles\")){
    Write-Host "Create folder: '$targetProjectDirectory\DatabaseUpdateFiles\'" -ForegroundColor Green
    New-Item "$targetProjectDirectory\DatabaseUpdateFiles\" -ItemType Directory -Force

    # --- Create other folders and files just with first initialize when base folder create ---

    Write-Host "Check exist folder: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\'" -ForegroundColor Yellow
    if (-not (Test-Path "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\")){
        Write-Host "Create folder: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\'" -ForegroundColor Green
        New-Item "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\" -ItemType Directory -Force
    }

    # Create default sample folder
    $currentDateTime = Get-Date -format "yyyyMMdd";
    Write-Host "Check exist folder: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime'" -ForegroundColor Yellow
    if (-not (Test-Path "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime")){
        Write-Host "Create folder: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime'" -ForegroundColor Green
        New-Item "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime" -ItemType Directory -Force
    }

    # Create default samole files
    Write-Host "Create/Ovewrite file: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime\001- Help.sql'" -ForegroundColor Green
    New-Item "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime\001- Help.sql" -Force
    Set-Content -Path "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime\001- Help.sql" -Value "-- Note: Do not use this file in any way, this file will be replaced with new content in updates.
    -- To maintain database update scripts, you must create folders like this in the 'DatabaseUpdateFiles\ProjectName\' path in your project and keep the scripts inside.
    -- The folder naming format consists of two parts, version, and date, for example:
    -- 1.0.0.0-20210601
    -- This naming format is mandatory for folders.
    -- You can then save the scripts in any format inside any folder. The suggested format for script names is to start with a 3-digit number, a hyphen, and then a brief description of the script. like the:
    -- 001- Update View Vw_Customers.sql
    -- Tip 1: Folder formatting is mandatory, database version, and database update date are set based on folder names.
    -- Tip 2: The priority of executing scripts is based on the name, so you can set their execution priority by using numbers in first the name of the script.
    -- Tip 3: In each script, you can use the following example to write explanations to update and change this script, this description is kept as a log.
    -- <Author>Iman Kari</Author>
    -- <Description>Updated view for fixing problems in task number 226</Description>
    -- <UpdateDateTime>1990-09-01 12:45:00</UpdateDateTime>
    -- <Version>1.0.0.0</Version>"

    Write-Host "Create/Ovewrite file: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime\002- Sample.sql'" -ForegroundColor Green
    New-Item "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime\002- Sample.sql" -Force
    Set-Content -Path "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime\002- Sample.sql" -Value "-- Note: Do not use this file in any way, this file will be replaced with new content in updates.
    -- <Author>Iman Kari</Author>
    -- <Description>Updated view for fixing problems in task number 226</Description>
    -- <UpdateDateTime>1990-09-01 09:09:09</UpdateDateTime>
    -- <Version>1.0.0.0</Version>
    Select 1 --What you want done!"
}

# Check exist validation file
Write-Host "Check exist validation file 'DatabaseVersionControlValidation.ps1'" -ForegroundColor Yellow
if (-not (Test-Path "$targetProjectDirectory\DatabaseVersionControlValidation.ps1")){
    # If not exist => Create it
    Write-Host "Create file: '$targetProjectDirectory\DatabaseVersionControlValidation.ps1'" -ForegroundColor Green
    New-Item "$targetProjectDirectory\DatabaseVersionControlValidation.ps1" -Force
    Set-Content -Path "$targetProjectDirectory\DatabaseVersionControlValidation.ps1" -Value '<#
Database Validation Description:
Sample: return ((param1, Param2, param3, param4, param5), (param1, Param2, param3, param4, param5), etc...)
[Boolean] Parameter 1:
It specifies whether "Author" is required in update scripts or not. it causes a build time error if the "Author" value is mandatory and it is not specified in the update scripts.
[Boolean] Parameter 2:
It specifies whether "Description" is required in update scripts or not. it causes a build time error if the "Description" value is mandatory and it is not specified in the update scripts.
[Boolean] Parameter 3:
It specifies whether "UpdateDateTime" is required in update scripts or not. it causes a build time error if the "UpdateDateTime" value is mandatory and it is not specified in the update scripts.
[Boolean] Parameter 4:
It specifies whether "Version" is required in update scripts or not. it causes a build time error if the "Version" value is mandatory and it is not specified in the update scripts.
[String] Parameter 5:
This parameter is optional. Using this parameter, the path of a project can be specified for review and validation.
Note: it is used for projects that have more than one database management project and it needs to be validated differently for each project.
Important Note: The database validation settings in multi-project (Bidimensional Array) mode, cant''t be null or empty for project path param ([String] Parameter 5: "SpecifiesCustomProject")
[Warning]: PowerShell software must be installed for verification and validation of the Database version control file (Only required for development environment on Linux and Mac to access "pwsh" command).
Linux Install: https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux?view=powershell-7.1
Mac Install: https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-macos?view=powershell-7.1
#>
return @(($false, $false, $false, $false, $null))'
}

# Add pre build event for check validations
#Write-Host "Add pre build event"
#$project.Properties.Item("PreBuildEvent").Value = 
#"
#New-Item ""$targetProjectDirectory\Abababa.sql"" -Force
#Set-Content -Path ""$targetProjectDirectory\Abababa.sql"" -Value ""-- Note: Do not use this file in any way, this file will be replaced with new content in updates.""
#exit 0
#"

# "IF $(ConfigurationName) == Debug IF $(PlatformName) == ARM goto DebugARM
# IF $(ConfigurationName) == Debug IF $(PlatformName) == x86 goto Debugx86
# IF $(ConfigurationName) == Release IF $(PlatformName) == ARM goto ReleaseARM
# IF $(ConfigurationName) == Release IF $(PlatformName) == x86 goto Releasex86

#$validationScriptPath = "$installPath\tools\G9DBVCValidation.ps1"
#Write-Host $validationScriptPath
#if (Test-Path $validationScriptPath){
#    Write-Host powershell "$installPath\tools\G9DBVCValidation.ps1" $targetProjectDirectory
#    powershell "$installPath\tools\G9DBVCValidation.ps1" $targetProjectDirectory
#}

# Finish log
$currentYear = get-date -Format yyyy
Write-Host "
######## G9DatabaseVersionControlCore #########
   __________ ________  ___ __________  __  ___
  / ____/ __ /_  __/  |/  // ____/ __ \/  |/  /
 / / __/ /_/ // / / /|_/ // /   / / / / /|_/ / 
/ /_/ /\__, // / / /  / _/ /___/ /_/ / /  / /  
\____//____//_/ /_/  /_(_\____/\____/_/  /_/   
 					   G9Studio - ©2010-$currentYear
############ Successful completion ############" -ForegroundColor Magenta