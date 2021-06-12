Note: Do not use this file in any way, this file will be replaced with new content in updates.
To maintain database update scripts, you must create folders like this in the 'DatabaseUpdateFiles\ProjectName\' path in your project and keep the scripts inside.
The folder naming format consists of two parts, version, and date, for example:
1.0.0.0-20210601
This naming format is mandatory for folders.
You can then save the scripts in any format inside any folder. The suggested format for script names is to start with a 3-digit number, a hyphen, and then a brief description of the script. like the:
001- Update View Vw_Customers.sql
Tip 1: Folder formatting is mandatory, database version, and database update date are set based on folder names.
Tip 2: The priority of executing scripts is based on the name, so you can set their execution priority by using numbers in first the name of the script.
Tip 3: In each script, you can use the following example to write explanations to update and change this script, this description is kept as a log.
-- <Author>Iman Kari</Author>
-- <Description>Updated view for fixing problems in task number 226</Description>
-- <UpdateDateTime>1990-09-01 12:45:00</UpdateDateTime>
