using System.Collections.Generic;

namespace Xipton.Razor
{
    public interface IRazorMachine
    {
        /// <summary>
        /// Executes the template by its virtual path and returns the resulting executed template instance.
        /// </summary>
        /// <param name="templateVirtualPath">The requested virtual path for the template.</param>
        /// <param name="model">The optional model.</param>
        /// <param name="viewbag">The optional viewbag.</param>
        /// <param name="skipLayout">if set true then any layout setting is ignored</param>
        /// <param name="throwExceptionOnVirtualPathNotFound">
        ///   Optional. If set to <c>true</c> an exception is thrown if the requested path could not be resolved. 
        ///   If set to false then null is returned if the requested path could not be resolved.
        /// </param>
        /// <returns>An executed template instance. The corresponding rendered result can be found at <see cref="ITemplate.Result"/></returns>
        ITemplate ExecuteUrl(string templateVirtualPath, object model = null, object viewbag = null, bool skipLayout = false, bool throwExceptionOnVirtualPathNotFound = true);

        /// <summary>
        /// Renders the specified template content's result with the passed model instance and returns the template's rendered result.
        /// A corresponding virtual path is generated internally for being able to keep the compiled template type cached.
        /// </summary>
        /// <param name="templateContent">Content of the template.</param>
        /// <param name="model">The model</param>
        /// <param name="viewbag">The optional viewbag</param>
        /// <param name="skipLayout">if set to <c>true</c> then any layout setting (probably at _ViewStart) is ignored.</param>
        /// <returns>
        /// The rendered string
        /// </returns>
        ITemplate ExecuteContent(string templateContent, object model = null, object viewbag = null, bool skipLayout = false);

        /// <summary>
        /// Hybrid convenience executer. It decides whether to execute content or an url.
        /// </summary>
        /// <param name="templateVirtualPathOrContent">Content of the template URL or.</param>
        /// <param name="model">The model.</param>
        /// <param name="viewbag">The viewbag.</param>
        /// <param name="skipLayout">if set to <c>true</c> [skip layout].</param>
        /// <param name="throwExceptionOnVirtualPathNotFound">if set to <c>true</c> [throw exception on virtual path not found].</param>
        /// <returns></returns>
        ITemplate Execute(string templateVirtualPathOrContent, object model = null, object viewbag = null, bool skipLayout = false, bool throwExceptionOnVirtualPathNotFound = true);

        RazorContext Context { get; }
        RazorMachine RegisterTemplate(string virtualPath, string content);
        RazorMachine RemoveTemplate(string virtualPath);
        IDictionary<string, string> GetRegisteredInMemoryTemplates();
        RazorMachine ClearTypeCache();
    }
}