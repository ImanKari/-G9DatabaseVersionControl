using System;
using System.Collections.Generic;

namespace G9DatabaseVersionControlCore.DataType
{
    /// <summary>
    ///     Data type fore custom task
    /// </summary>
    public readonly struct G9DtCustomTask
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies nickname of custom task
        /// </summary>
        public readonly string Nickname;

        /// <summary>
        ///     Specifies description of custom task
        /// </summary>
        public readonly string Description;

        /// <summary>
        ///     Func for execute custom task
        ///     <para />
        ///     <para />
        ///     First param: string => Specifies database name (Can't be null)
        ///     <para />
        ///     Second param: Action(string) => Access to action for execute a query on database without result
        ///     <para />
        ///     Third param: Fun(Dictionary(string, object)) => Access to func for execute a query on database with result (List
        ///     specifies rows and dictionary specifies column and value)
        ///     <para />
        ///     Fourth param: Specifies task is successful or no (func answer)
        /// </summary>
        public readonly Func<string, Action<string>, Func<string, List<Dictionary<string, object>>>, G9DtTaskResult>
            CustomTaskFunc;

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Initialize Requirement
        /// </summary>
        /// <param name="nickname">Specifies nickname of custom task</param>
        /// <param name="description">Specifies description of custom task</param>
        /// <param name="customTaskFunc">
        ///     Func for execute custom task
        ///     <para />
        ///     <para />
        ///     First param: string => Specifies database name (Can't be null)
        ///     <para />
        ///     Second param: Action(string) => Access to action for execute a query on database without result
        ///     <para />
        ///     Third param: Fun(Dictionary(string, object)) => Access to func for execute a query on database with result (List
        ///     specifies rows and dictionary specifies column and value)
        ///     <para />
        ///     Fourth param: Specifies task is successful or no (func answer)
        /// </param>
        public G9DtCustomTask(string nickname, string description,
            Func<string, Action<string>, Func<string, List<Dictionary<string, object>>>, G9DtTaskResult> customTaskFunc)
        {
            Nickname = nickname;
            Description = description;
            CustomTaskFunc = customTaskFunc;
        }

        #endregion
    }
}