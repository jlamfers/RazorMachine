#region  Microsoft Public License
/* This code is part of Xipton.Razor v3.0
 * (c) Jaap Lamfers, 2013 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Razor;

namespace Xipton.Razor.Extension {
    public static class AppDomainExtension {

        private static readonly ConcurrentDictionary<string,bool> 
            _binAssembliesLoadedBefore = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Ensures that the required xipton assemblies are loaded in the excution context.
        /// </summary>
        public static AppDomain EnsureXiptonAssembliesLoaded(this AppDomain domain) {
#pragma warning disable 168
            var t = typeof(ParserResults);
#pragma warning restore 168
            return domain;
        }

        /// <summary>
        /// Ensures that all assemblies in the bin folder have been loaded into the excution context.
        /// </summary>
        public static AppDomain EnsureBinAssembliesLoaded(this AppDomain domain) {

            if (_binAssembliesLoadedBefore.ContainsKey(domain.FriendlyName))
                return domain;

            var binFolder = !string.IsNullOrEmpty(domain.RelativeSearchPath)
                           ? Path.Combine(domain.BaseDirectory, domain.RelativeSearchPath)
                           : domain.BaseDirectory;

            Directory.GetFiles(binFolder, "*.dll")
                .Union(Directory.GetFiles(binFolder, "*.exe"))
                .ToList()
                .ForEach(domain.EnsureAssemblyIsLoaded);

            _binAssembliesLoadedBefore[domain.FriendlyName] = true;

            return domain;
        }

        public static Assembly GetOrLoadAssembly(this AppDomain domain, string assemblyFileNameOrAssemblyDisplayName) {
            var assemblyName = assemblyFileNameOrAssemblyDisplayName.IsFileName()
                ? AssemblyName.GetAssemblyName(assemblyFileNameOrAssemblyDisplayName)
                : new AssemblyName(assemblyFileNameOrAssemblyDisplayName);
            return domain
                .GetAssemblies()
                .FirstOrDefault(a => ReferenceMatchesDefinitionEx(assemblyName, a.GetName())) ?? domain.Load(assemblyName);
        }

        private static void EnsureAssemblyIsLoaded(this AppDomain domain, string assemblyFileName) {
            try{
                var assemblyName = AssemblyName.GetAssemblyName(assemblyFileName);
                if (!domain.GetAssemblies().Any(a => ReferenceMatchesDefinitionEx(assemblyName, a.GetName()))){
                    domain.Load(assemblyName);
                }
            }
            catch (BadImageFormatException){
                // thrown by GetAssemblyName
                // ignore this assembly since it is an unmanaged assembly
            }
        }

        private static bool ReferenceMatchesDefinitionEx(AssemblyName reference, AssemblyName definition)
        {
#if __MonoCS__
            return string.Equals(reference.ToString(), definition.ToString(), StringComparison.OrdinalIgnoreCase);
#else
            return AssemblyName.ReferenceMatchesDefinition(reference, definition);
#endif
        }



    }
}
