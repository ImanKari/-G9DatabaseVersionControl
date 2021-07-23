<#
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
Important Note: The database validation settings in multi-project (Bidimensional Array) mode, cant't be null or empty for project path param ([String] Parameter 5: "SpecifiesCustomProject")
[Warning]: PowerShell software must be installed for verification and validation of the Database version control file (Only required for development environment on Linux and Mac to access "pwsh" command).
Linux Install: https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux?view=powershell-7.1
Mac Install: https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-macos?view=powershell-7.1
#>
return @(($true, $false, $false, $false, $null))
