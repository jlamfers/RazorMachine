#region  Microsoft Public License
/* This code is part of Xipton.Razor v3.0
 * (c) Jaap Lamfers, 2013 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

using System;
using System.Collections.Generic;
using System.Web.Razor;
using Xipton.Razor.Core;

namespace Xipton.Razor.Config
{
    public interface IRazorConfigInitializer
    {
        IRazorConfigInitializer InitializeByValues(
            Type baseType = null,
            string rootOperatorPath = null,
            RazorCodeLanguage language = null,
            string defaultExtension = null,
            string autoIncludeNameWithoutExtension = null,
            string sharedLocation = null,
            bool? includeGeneratedSourceCode = null,
            bool? htmlEncode = null,
            IEnumerable<string> references = null,
            IEnumerable<string> namespaces = null,
            IEnumerable<Func<IContentProvider>> contentProviders = null,
            bool replaceReferences = false,
            bool replaceNamespaces = false,
            bool replaceContentProviders = false
            );

        IRazorConfigInitializer InitializeByXmlContent(string xmlContent);

        IRazorConfigInitializer InitializeByXmlFileName(string fileName);
        IRazorConfigInitializer TryInitializeFromConfig();
        RazorConfig AsReadOnly();
    }
}