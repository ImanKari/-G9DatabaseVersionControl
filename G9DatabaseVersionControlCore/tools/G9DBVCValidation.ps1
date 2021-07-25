# --- Param to get project path ----
param([string]$projectDir=$null) 

# --- Check validation for project path ---
if ([String]::IsNullOrEmpty($projectDir)){
    Write-Host 'DatabaseVersionControlValidation.ps1' : warning -9: 'The "$projectDir" parameter is empty. Ignore the message if you do not need it.'
    exit 0
}

# --- Check validation for exist validation settings file ---
[string] $ValidationConfigFullPath = [System.IO.Path]::Combine($projectDir, "DatabaseVersionControlValidation.ps1");
if ([String]::IsNullOrEmpty($ValidationConfigFullPath) -or -not[System.IO.File]::Exists($ValidationConfigFullPath)){
    Write-Host 'DatabaseVersionControlValidation.ps1' : warning -9: 'The database control version validation settings file (DatabaseVersionControlValidation.ps1) does not exist in the project root. Ignore the message if you do not need it. Path: ' + $ValidationConfigFullPath
    exit 0
}

# --- Get validations settings ---
$result = Invoke-Expression $ValidationConfigFullPath
if (-not ($result -is [array])){
    Write-Host $ValidationConfigFullPath : error -9: 'Return ERROR. The database validation settings file must be returned an array. Sample: "return @(($true, $false, $false, $false, $false, $null), etc...)". Path: ' + $ValidationConfigFullPath
    exit 1
}

try{

    # --- Set const ---
    [string] $OpenAuthorTag = '<Author>'
    [string] $ClosetAuthorTag = '</Author>'
    [string] $OpenDescriptionTag = '<Description>'
    [string] $ClosetDescriptionTag = '</Description>'
    [string] $OpenUpdateDateTimeTag = '<UpdateDateTime>'
    [string] $ClosetUpdateDateTimeTag = '</UpdateDateTime>'
    [string] $OpenVersionTag = '<Version>'
    [string] $ClosetVersionTag = '</Version>'

    # --- Set length ---
    [int] $LenghtOfOpenAuthorTag = $OpenAuthorTag.length
    [int] $LenghtOfOpenDescriptionTag = $OpenDescriptionTag.length
    [int] $LenghtOfOpenUpdateDateTimeTag = $OpenUpdateDateTimeTag.length
    [int] $LenghtOfOpenVersionTag = $OpenVersionTag.length

    # --- Method to get projects form path ---
    function GetProjects{
        Param(
            [String] $basePath
        )
        Write-Host $basePath
        return ([System.IO.Directory]::GetDirectories($basePath))
    }

    # --- Method to get update directories from project path ---
    function GetProjectsUpdateDirectories(){
        Param(
            [String] $projectPath
        )
        return ([System.IO.Directory]::GetDirectories([System.IO.Path]::Combine($projectPath)))
    }

    # --- Method to get update scripts path from update directory ---
    function GetProjectsUpdateDirectoryFile(){
        Param(
            [String] $updateFolderPath
        )
        return ([System.IO.Directory]::GetFiles($updateFolderPath))
    }

    # --- Method to check validation for update script file ---
    function CheckValidation
    {
        Param(
            [String] $filePath,
            [bool] $checkAuthor,
            [bool] $checkDescription,
            [bool] $checkUpdateDateTime,
            [bool] $checkVersion
        )

        [int] $firstPosAuthor = -1
        [int] $lastPosAuthor = -1

        [int] $firstPosDescription = -1
        [int] $lastPosDescription = -1

        [int] $firstPosUpdateDateTime = -1
        [int] $lastPosUpdateDateTime = -1

        [int] $firstPosVersion = -1
        [int] $lastPosVersion = -1

        # --- Read file data ---
        [string] $fileData = Get-Content -LiteralPath $filePath -Encoding UTF8 -Raw

        # --- Temp variables for save final result ---
        [bool] $finalResult = $true 

        # --- Check Author ---
        if ($checkAuthor){
            $firstPosAuthor = $fileData.IndexOf($OpenAuthorTag);
            $lastPosAuthor = $fileData.IndexOf($ClosetAuthorTag);
            [int] $lengthInfoOfAuthor = ($lastPosAuthor - ($firstPosAuthor + $LenghtOfOpenAuthorTag))
            if ($lengthInfoOfAuthor -gt 0){
                [string] $resultOfAuthor = $fileData.SubString(($firstPosAuthor + $LenghtOfOpenAuthorTag), $lengthInfoOfAuthor).Trim()
                if ([String]::IsNullOrEmpty($resultOfAuthor)){
                    $finalResult = $false
                    $indexOfItem = [Array]::FindIndex(($fileData -split "\r\n"), [Predicate[string]] { $args[0].Contains("<Author>")}) + 1
                    Write-Host $script'('$indexOfItem')' : error -9: 'The update script file does not specify "Author". Specifying "Author" is mandatory. Sample: "-- <Author>Full Name</Author>"'
                }
            }else{
                $finalResult = $false
                $indexOfItem = [Array]::FindIndex(($fileData -split "\r\n"), [Predicate[string]] { $args[0].Contains("<Author>")}) + 1
                Write-Host $script'('$indexOfItem')' : error -9: 'The update script file does not specify "Author". Specifying "Author" is mandatory. Sample: "-- <Author>Full Name</Author>"'
            }
        }
        # --- Check Description ---
        if ($checkDescription){
            $firstPosDescription = $fileData.IndexOf($OpenDescriptionTag);
            $lastPosDescription = $fileData.IndexOf($ClosetDescriptionTag);
            [int] $lengthInfoOfDescription = ($lastPosDescription - ($firstPosDescription + $LenghtOfOpenDescriptionTag))
            if ($lengthInfoOfDescription -gt 0){
                [string] $resultOfDescription = $fileData.SubString(($firstPosDescription + $LenghtOfOpenDescriptionTag), $lengthInfoOfDescription).Trim()
                if ([String]::IsNullOrEmpty($resultOfDescription)){
                    $finalResult = $false
                    $indexOfItem = [Array]::FindIndex(($fileData -split "\r\n"), [Predicate[string]] { $args[0].Contains("<Description>")}) + 1
                    Write-Host $script'('$indexOfItem')' : error -9: 'The update script file does not specify "Description". Specifying "Description" is mandatory. Sample: "-- <Description>Description Info</Description>"'
                }
            }else{
                $finalResult = $false
                $indexOfItem = [Array]::FindIndex(($fileData -split "\r\n"), [Predicate[string]] { $args[0].Contains("<Description>")}) + 1
                 Write-Host $script'('$indexOfItem')' : error -9: 'The update script file does not specify "Description". Specifying "Description" is mandatory. Sample: "-- <Description>Description Info</Description>"'
            }
        }
        # --- Check UpdateDateTime  ---
        if ($checkUpdateDateTime){
            $firstPosUpdateDateTime  = $fileData.IndexOf($OpenUpdateDateTimeTag);
            $lastPosUpdateDateTime  = $fileData.IndexOf($ClosetUpdateDateTimeTag);
            [int] $lengthInfoOfUpdateDateTime = ($lastPosUpdateDateTime - ($firstPosUpdateDateTime + $LenghtOfOpenUpdateDateTimeTag))
            if ($lengthInfoOfUpdateDateTime -gt 0){
                [string] $resultOfUpdateDateTime = $fileData.SubString(($firstPosUpdateDateTime + $LenghtOfOpenUpdateDateTimeTag), $lengthInfoOfUpdateDateTime).Trim()
                # Check try parse for date time
                [DateTime]$tempParseDateTime = New-Object DateTime
                if ([String]::IsNullOrEmpty($resultOfUpdateDateTime)){
                    $finalResult = $false
                    $indexOfItem = [Array]::FindIndex(($fileData -split "\r\n"), [Predicate[string]] { $args[0].Contains("<UpdateDateTime>")}) + 1
                    Write-Host $script'('$indexOfItem')' : error -9: 'The update script file does not specify "UpdateDateTime". Specifying "UpdateDateTime" is mandatory. Sample: "-- <UpdateDateTime>1990-09-01 12:45:00</UpdateDateTime>"'
                }
                elseif (-not[DateTime]::TryParse($resultOfUpdateDateTime, [ref]$tempParseDateTime)){
                    $finalResult = $false
                    $indexOfItem = [Array]::FindIndex(($fileData -split "\r\n"), [Predicate[string]] { $args[0].Contains("<UpdateDateTime>")}) + 1
                    Write-Host $script'('$indexOfItem')' : error -9: 'The value specified for "UpdateDateTime" is incorrect. Sample correct format: -- <UpdateDateTime>1990-09-01 12:45:00</UpdateDateTime>"'
                }
            }else{
                $finalResult = $false
                $indexOfItem = [Array]::FindIndex(($fileData -split "\r\n"), [Predicate[string]] { $args[0].Contains("<UpdateDateTime>")}) + 1
                Write-Host $script'('$indexOfItem')' : error -9: 'The update script file does not specify "UpdateDateTime". Specifying "UpdateDateTime" is mandatory. Sample: "-- <UpdateDateTime>1990-09-01 12:45:00</UpdateDateTime>"'
            }
        }
        # --- Check Version ---
        if ($checkVersion){
            $firstPosVersion = $fileData.IndexOf($OpenVersionTag);
            $lastPosVersion = $fileData.IndexOf($ClosetVersionTag);
            [int] $lengthInfoOfVersion = ($lastPosVersion - ($firstPosVersion + $LenghtOfOpenVersionTag))
            if ($lengthInfoOfVersion -gt 0){
                [string] $resultOfVersion = $fileData.SubString(($firstPosVersion + $LenghtOfOpenVersionTag), $lengthInfoOfVersion).Trim()
                # Check try parse for version
                [int]$tempParseInt = New-Object int
                if ([String]::IsNullOrEmpty($resultOfVersion)){
                    $finalResult = $false
                    $indexOfItem = [Array]::FindIndex(($fileData -split "\r\n"), [Predicate[string]] { $args[0].Contains("<Version>")}) + 1
                    Write-Host $script'('$indexOfItem')' : error -9: 'The update script file does not specify "Version". Specifying "Version" is mandatory. Sample: "-- <Version>1.0.0.0</Version>"'
                }elseif (-not[int]::TryParse($resultOfVersion.Replace(".", [String]::Empty), [ref]$tempParseInt)){
                    $finalResult = $false
                    $indexOfItem = [Array]::FindIndex(($fileData -split "\r\n"), [Predicate[string]] { $args[0].Contains("<Version>")}) + 1
                    Write-Host $indexOfItem
                    Write-Host $script'('$indexOfItem')' : error -9: 'The value specified for "Version" is incorrect. Sample correct format (Numbers and dots): "-- <Version>1.0.0.0</Version>"'
                }
            }else{
                $finalResult = $false
                $indexOfItem = [Array]::FindIndex(($fileData -split "\r\n"), [Predicate[string]] { $args[0].Contains("<Version>")}) + 1
                Write-Host $script'('$indexOfItem')' : error -9: 'The update script file does not specify "Version". Specifying "Version" is mandatory. Sample: "-- <Version>1.0.0.0</Version>"'
            }
        }

        # Write log about update script
        if ($finalResult){
            Write-Host "[Correct Validation]: $script" -ForegroundColor Green
        }else{
            Write-Host "[Incorrect Validation]: $script" -ForegroundColor Red
        }
    }

    # --- Method to check folder name validation (Version-Date <=> 1.0.0.0-20210723) ---
    function CheckFolderNameValidation(){
        param([String] $updateFolderPath)
        $updateProjectName = RemovePreFixPath $updateFolderPath
        $versionAndDateArray = $updateProjectName.split("-");
        # Check validations:
        if ($versionAndDateArray.length -ne 2){
            Write-Host $updateFolderPath'(0)' : error -9: 'Incorrect folder name, The update folder name must be in two sections, "Version" and "Date", such as "1.0.0.0-20210612". Incorrect folder path: '$updateFolderPath
            return $false
        }
        # Check folder version name
        [int]$tempFolderVersion = New-Object int
        if (-not ([Int]::TryParse($versionAndDateArray[0].Replace(".", [String]::Empty), [ref]$tempFolderVersion))){
            Write-Host $updateFolderPath'(0)' : error -9: 'Incorrect folder name (Incorrect set "'$versionAndDateArray[0]'" for section version), The update folder name must be in two sections, "Version" and "Date", such as "1.0.0.0-20210612". Incorrect folder path: '$updateFolderPath
            return $false
        }
        # Check folder Date
        if ($versionAndDateArray[1].length -ne 8){
            Write-Host $updateFolderPath'(0)' : error -9: 'Incorrect folder name (Incorrect set "'$versionAndDateArray[1]'" for section Date), The update folder name must be in two sections, "Version" and "Date", such as "1.0.0.0-20210612". Incorrect folder path: '$updateFolderPath
            return $false
        }
        [DateTime]$tempFolderDate = New-Object DateTime
        if (-not ([DateTime]::TryParse("$($versionAndDateArray[1].Substring(0, 4))/$($versionAndDateArray[1].Substring(4, 2))/$($versionAndDateArray[1].Substring(6, 2))", [ref]$tempFolderDate))){
            Write-Host $updateFolderPath'(0)' : error -9: 'Incorrect folder name (Incorrect set "'$versionAndDateArray[1]'" for section Date), The update folder name must be in two sections, "Version" and "Date", such as "1.0.0.0-20210612". Incorrect folder path: '$updateFolderPath
            return $false
        }
        return $true
    }

    # --- Method to remove pre fix from path ---
    function RemovePreFixPath{
        Param(
            [String] $path
        )
        return $path.Remove(0, $path.LastIndexOf("\") + 1).Remove(0, $path.LastIndexOf("/") + 1);
    }

    # --- Method to handle and management validation ---
    function HandlerForCheckValidation(){
        param([String] $projectPath, $validationConfig)

        if (-not $validationConfig[0] -and -not $validationConfig[1] -and -not $validationConfig[2] -and -not $validationConfig[3]){
            Write-Host 'Skip => All validations Are set to "$false"' -ForegroundColor Yellow
            return
        }

        $updateDirectories = GetProjectsUpdateDirectories $projectPath
        foreach($directory in $updateDirectories){
            # Check update folder name validation
            if (-not (CheckFolderNameValidation $directory)){
                # Ignore
                continue
            }
            $updateScriptFiles = GetProjectsUpdateDirectoryFile $directory
            foreach($script in $updateScriptFiles){
                CheckValidation $script $validationConfig[0] $validationConfig[1] $validationConfig[2] $validationConfig[3]
            }
        }
    }

    # --- Method to check config/setting of validation ---
    function CheckConfigValidate(){
        param($validationConfig)
        if (-not ($validationConfig[0] -is [array]) -and ($validationConfig.length -eq 5 -and $validationConfig[0] -is [bool] -and $validationConfig[1] -is [bool] -and $validationConfig[2] -is [bool] -and $validationConfig[3] -is [bool] -and ($validationConfig[4] -eq $null -or $validationConfig[4] -is [string]))){
            if ($validationConfig[4] -eq $null -or [String]::IsNullOrEmpty($validationConfig[4])){
                $fullPathGenerate = [System.IO.Path]::Combine($projectDir, "DatabaseUpdateFiles")
                $projects = GetProjects $fullPathGenerate
                foreach($project in $projects){
                    Write-Host "###### Check validations for project name '$(RemovePreFixPath $project)' ######" -ForegroundColor Blue
                    Write-Host "Validation: IsRequiredToSetAuthor: $($validationConfig[0]) | IsRequiredToSetDescription: $($validationConfig[1]) | IsRequiredToSetUpdateDateTime: $($validationConfig[2]) | IsRequiredToSetVersion: $($validationConfig[3]) | SpecifiesCustomProject: $($validationConfig[4])" -ForegroundColor Magenta
                    HandlerForCheckValidation $project $validationConfig
                    Write-Host
                }
            }else{
                Write-Host "###### Check validations for project name '$(RemovePreFixPath $validationConfig[4])' ######" -ForegroundColor Blue
                Write-Host "Validation: IsRequiredToSetAuthor: $($validationConfig[0]) | IsRequiredToSetDescription: $($validationConfig[1]) | IsRequiredToSetUpdateDateTime: $($validationConfig[2]) | IsRequiredToSetVersion: $($validationConfig[3]) | SpecifiesCustomProject: $($validationConfig[4])" -ForegroundColor Magenta
                HandlerForCheckValidation $validationConfig[4] $validationConfig
                Write-Host
            }
        }else{
            Write-Host $ValidationConfigFullPath : error -9: 'Return ERROR. The database validation settings file must be returned an array. Sample: "return @(($true, $false, $false, $false, $false, $null), etc...)". Path: ' + $ValidationConfigFullPath
        }
    }

    if (-not ($result[0] -is [array])){
        CheckConfigValidate($result)
    }elseif ($result[0] -is [array]){
        foreach ($validationConfig in $result){
            if ([string]::IsNullOrEmpty($validationConfig[4])){
                Write-Host $ValidationConfigFullPath : error -9: 'Return ERROR. The database validation settings in multi-project (Bidimensional Array) mode, cant''t be null or empty for project path param "SpecifiesCustomProject". Path: ' + $ValidationConfigFullPath
                continue
            }
            CheckConfigValidate($validationConfig)
        }
    }else{
        Write-Host $ValidationConfigFullPath : error -9: 'Return ERROR. The database validation settings file must be returned an array. Sample: "return @(($true, $false, $false, $false, $false, $null), etc...)". Path: ' + $ValidationConfigFullPath
        exit 1
    }

    exit 0
}
catch
{
    $ErrorMessage = $_.Exception.Message
    Write-Host 'DatabaseVersionControlValidation.ps1' : error -9: $ErrorMessage
}