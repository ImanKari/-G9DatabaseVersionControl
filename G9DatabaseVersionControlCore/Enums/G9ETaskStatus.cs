namespace G9DatabaseVersionControlCore.Enums
{
    /// <summary>
    /// Specifies status of task
    /// </summary>
    public enum G9ETaskStatus : byte
    {
        SetConnectionString,
        SetInstallData,
        CheckInstallData,
        RestoreEmptyDataBase,
        ConverFromOldDbToNewDb,
        UpdateDataBase,
        InstallFinished
    }
}