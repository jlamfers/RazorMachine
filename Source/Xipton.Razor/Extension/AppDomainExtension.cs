#region  Microsoft Public License
/* This code is part of Xipton.Razor v2.3
 * (c) Jaap Lamfers, 2012 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Razor;

namespace Xipton.Razor.Extension {
    public static class AppDomainExtension {

        private static bool _binAssembliesLoadedBefore;

        /// <summary>
        /// Ensures that the required xipton assemblies are loaded in the excution context.
        /// </summary>
        public static AppDomain EnsureXiptonAssembliesLoaded(this AppDomain domain) {
            typeof(ParserResults).GetType();
            return domain;
        }

        /// <summary>
        /// Ensures that all assemblies in the bin folder have been loaded into the excution context.
        /// </summary>
        public static AppDomain EnsureBinAssembliesLoaded(this AppDomain domain) {

            if (_binAssembliesLoadedBefore)
                return domain;

            var binFolder = !string.IsNullOrEmpty(domain.RelativeSearchPath)
                           ? Path.Combine(domain.BaseDirectory, domain.RelativeSearchPath)
                           : domain.BaseDirectory;

            Directory.GetFiles(binFolder, "*.dll")
                .Union(Directory.GetFiles(binFolder, "*.exe"))
                .ToList()
                .ForEach(EnsureAssemblyLoaded);

            _binAssembliesLoadedBefore = true;

            return domain;
        }

        private static void EnsureAssemblyLoaded(string assemblyFileName){
            var assemblyName = AssemblyName.GetAssemblyName(assemblyFileName);
            if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => AssemblyName.ReferenceMatchesDefinition(assemblyName, a.GetName()))){
                Assembly.Load(assemblyName);
            }
        }

    }
}
