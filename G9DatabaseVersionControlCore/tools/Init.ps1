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
}

Write-Host "Check exist folder: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\'" -ForegroundColor Yellow
if (-not (Test-Path "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\")){
    Write-Host "Create folder: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\'" -ForegroundColor Green
    New-Item "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\" -ItemType Directory -Force
}

$currentDateTime = Get-Date -format "yyyyMMdd";
Write-Host "Check exist folder: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime'" -ForegroundColor Yellow
if (-not (Test-Path "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime")){
    Write-Host "Create folder: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime'" -ForegroundColor Green
    New-Item "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime" -ItemType Directory -Force
}

# Files
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
-- <Version>9</Version>"

Write-Host "Create/Ovewrite file: '$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime\002- Sample.sql'" -ForegroundColor Green
New-Item "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime\002- Sample.sql" -Force
Set-Content -Path "$targetProjectDirectory\DatabaseUpdateFiles\$projectName\1.0.0.0-$currentDateTime\002- Sample.sql" -Value "-- Note: Do not use this file in any way, this file will be replaced with new content in updates.
-- <Author>Iman Kari</Author>
-- <Description>Updated view for fixing problems in task number 226</Description>
-- <UpdateDateTime>1990-09-01 12:45:00</UpdateDateTime>
-- <Version>9</Version>
Select 1 --What you want done!"

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