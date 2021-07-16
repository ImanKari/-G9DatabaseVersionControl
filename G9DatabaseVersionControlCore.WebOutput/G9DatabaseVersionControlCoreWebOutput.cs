using System;
using System.IO;
using System.Linq;
using System.Reflection;
using G9DatabaseVersionControlCore.DataType.AjaxDataType.StepDataType;

namespace G9DatabaseVersionControlCore.WebOutput
{
    public class G9DatabaseVersionControlCoreWebOutput
    {
        /// <summary>
        ///     Get web css data
        /// </summary>
        /// <param name="withoutStyleTag">Specifies need to get data with the style tag</param>
        /// <returns>Web css data</returns>
        public static string GetWebCssData(bool withoutStyleTag = false)
        {
            var assembly = typeof(G9DatabaseVersionControlCoreWebOutput).GetTypeInfo().Assembly;
            const string resourcePath = "G9DatabaseVersionControlCore.WebOutput.ContentFile.G9DatabaseVersionControlCoreWebOutput.css";
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            using (var reader =
                new StreamReader(stream ?? throw new Exception($"Embedded resource not found!\nPath: {resourcePath}")))
            {
                return withoutStyleTag
                    ? reader.ReadToEnd()
                    : $"<style>{reader.ReadToEnd()}</style>";
            }
        }

        /// <summary>
        ///     Get web javascript (Jquery) data
        /// </summary>
        /// <param name="ajaxMethodAddress">Specifies ajax method address for call web app</param>
        /// <param name="withoutScriptTag">Specifies need to get data with the script tag</param>
        /// <returns>web javascript data</returns>
        public static string GetWebJsData(string ajaxMethodAddress, bool withoutScriptTag = false)
        {
            if (string.IsNullOrEmpty(ajaxMethodAddress))
                throw new ArgumentNullException(nameof(ajaxMethodAddress),
                    $"Param '{nameof(ajaxMethodAddress)} can't be null or empty!'");
            var assembly = typeof(G9DatabaseVersionControlCoreWebOutput).GetTypeInfo().Assembly;
            const string resourcePath = "G9DatabaseVersionControlCore.WebOutput.ContentFile.G9DatabaseVersionControlCoreWebOutput.js";
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            using (var reader =
                new StreamReader(stream ?? throw new Exception($"Embedded resource not found!\nPath: {resourcePath}")))
            {
                return withoutScriptTag
                    ? reader.ReadToEnd().Replace("{{G9AjaxMethod}}", ajaxMethodAddress)
                    : $"<script>{reader.ReadToEnd().Replace("{{G9AjaxMethod}}", ajaxMethodAddress)}</script>";
            }
        }

        /// <summary>
        ///     Get web html data
        /// </summary>
        /// <param name="connectionStrings">Specifies collection of connection strings</param>
        /// <returns>Web html data</returns>
        public static string GetWebHtmlData(params G9DtConnectionString[] connectionStrings)
        {
            var options = "<option datasource=\"G9Custom\" userid=\"0\" password=\"\" selected>Custom...</option>";
            if (connectionStrings != null && connectionStrings.Any())
                foreach (var cn in connectionStrings)
                    options +=
                        $"<option datasource=\"{cn.DataSource}\" userid=\"{cn.UserId}\" password=\"{cn.Password}\">{cn.DataSource} | {cn.UserId}</option>";

            var assembly = typeof(G9DatabaseVersionControlCoreWebOutput).GetTypeInfo().Assembly;
            const string resourcePath = "G9DatabaseVersionControlCore.WebOutput.ContentFile.G9DatabaseVersionControlCoreWebOutput.html";
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            using (var reader =
                new StreamReader(stream ?? throw new Exception($"Embedded resource not found!\nPath: {resourcePath}")))
            {
                return reader.ReadToEnd().Replace("{{G9ConnectionString}}", options);
            }
        }
    }
}