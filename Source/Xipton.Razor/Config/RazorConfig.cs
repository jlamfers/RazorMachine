#region  Microsoft Public License
/* This code is part of Xipton.Razor v3.0
 * (c) Jaap Lamfers, 2013 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Razor;
using System.Xml;
using System.Xml.Linq;
using Xipton.Razor.Core;
using Xipton.Razor.Core.ContentProvider;
using Xipton.Razor.Extension;

namespace Xipton.Razor.Config {
    /// <summary>
    /// Holds all configuration settings. The configuration is initialized together with the RazorMachine class.
    /// 
    /// If the configuration is initialized by default (with no parameters) it is checked if a configuration section exists at 
    /// the default application's configuration named xipton.razor.config. If such a section exists then that configuration is used. Else the configuration
    /// is initialized as default, i.e., using the C# compiler and using MVC complient settings.
    /// 
    /// If you pass explicit settings with the constructor then at first the configuration is initialized as by default, as with the default constructor. 
    /// After that all passed non null arguments override their correponding existing settings. For namespaces and references hold that by default 
    /// these are merged with the already present configured (or default) namespaces and references, unless you specify that these arguments must replace
    /// the corresponding present settings.
    /// </summary>
    public sealed class RazorConfig : IRazorConfigInitializer
    {
        private readonly bool 
            _allowWildcardReferences;

        private const string 
            _rootElementName = "xipton.razor";

        private bool 
            _isReadOnly;

        public RazorConfig(bool allowWildcardReferences = true)
        {
            _allowWildcardReferences = allowWildcardReferences;
            LoadDefaults();
        }

        #region Configuration Properties
        public RootOperatorElement RootOperator { get; private set; }
        public TemplatesElement Templates { get; private set; }
        public IList<string> Namespaces { get; private set; }
        public IList<string> References { get; private set; }
        public IList<Func<IContentProvider>> ContentProviders { get; private set; }
        public IRazorConfigInitializer Initializer{
            get{
                EnsureNotReadonly();
                return this;
            }
        }
        #endregion

        #region IRazorConfigInitializer

        IRazorConfigInitializer IRazorConfigInitializer.InitializeByValues(
            Type baseType,
            string rootOperatorPath,
            RazorCodeLanguage language,
            string defaultExtension,
            string autoIncludeNameWithoutExtension,
            string sharedLocation,
            bool? includeGeneratedSourceCode,
            bool? htmlEncode,
            IEnumerable<string> references,
            IEnumerable<string> namespaces,
            IEnumerable<Func<IContentProvider>> contentProviders,
            bool replaceReferences,
            bool replaceNamespaces,
            bool replaceContentProviders
            )
        {

            EnsureNotReadonly();

            RootOperator.Path = rootOperatorPath ?? RootOperator.Path;

            Templates.BaseType = baseType ?? Templates.BaseType;
            Templates.Language = language ?? Templates.Language;
            Templates.DefaultExtension = (defaultExtension ?? Templates.DefaultExtension).EmptyAsNull();
            Templates.AutoIncludeName = (autoIncludeNameWithoutExtension ?? Templates.AutoIncludeName).EmptyAsNull();
            Templates.SharedLocation = (sharedLocation ?? Templates.SharedLocation).EmptyAsNull();
            if (includeGeneratedSourceCode != null)
                Templates.IncludeGeneratedSourceCode = includeGeneratedSourceCode.Value;
            if (htmlEncode != null)
                Templates.HtmlEncode = htmlEncode.Value;
            if (references != null) {
                References = replaceReferences ? references.ToList().AsReadOnly() : References.Union(references, StringComparer.InvariantCultureIgnoreCase).ToList().AsReadOnly();
            }
            if (namespaces != null) {
                Namespaces = replaceNamespaces ? namespaces.ToList().AsReadOnly() : Namespaces.Union(namespaces).ToList().AsReadOnly();
            }
            if (contentProviders != null) {
                ContentProviders = replaceContentProviders ? contentProviders.ToList().AsReadOnly() : ContentProviders.Union(contentProviders).ToList().AsReadOnly();
            }
            return this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "StringReader is disposed on XmlTextReader.Dispose()")]
        IRazorConfigInitializer IRazorConfigInitializer.InitializeByXmlContent(string xmlContent) {
            if (xmlContent == null) throw new ArgumentNullException("xmlContent");
            EnsureNotReadonly();
            using (var reader = new XmlTextReader(new StringReader(xmlContent))){
                LoadSettingsFromXDocumentOrElseDefaults(XDocument.Load(reader));
            }
            return this;
        }

        IRazorConfigInitializer IRazorConfigInitializer.InitializeByXmlFileName(string fileName) {
            if (fileName == null) throw new ArgumentNullException("fileName");
            EnsureNotReadonly();
            if (!File.Exists(fileName)){
                throw new FileNotFoundException("Configuration file '{0}' not found.".FormatWith(fileName), fileName);
            }
            using (var reader = new XmlTextReader(fileName))
                LoadSettingsFromXDocumentOrElseDefaults(XDocument.Load(reader));
            return this;
        }

        IRazorConfigInitializer IRazorConfigInitializer.TryInitializeFromConfig() {
            EnsureNotReadonly();
            var section = ConfigurationManager.GetSection("xipton.razor.config");
            XElement innerXml;
            if (section != null && (innerXml = section.CastTo<XmlConfigurationSection>().InnerXml) != null)
                LoadSettingsFromXDocumentOrElseDefaults(new XDocument(innerXml));
            return this;
        }

        RazorConfig IRazorConfigInitializer.AsReadOnly(){
            return AsReadonly();
        }

        #endregion

        #region Internal
        internal RazorConfig AsReadonly() {
            if (!_isReadOnly) {
                _isReadOnly = true;
                TryResolveWildcardReferences();
            }
            return this;
        }
        #endregion

        #region Private
        private void EnsureNotReadonly() {
            if (_isReadOnly)
                throw new InvalidOperationException("Configuration is read-only, after it has been registered inside the razor context, and cannot be modified anymore.");
        }

        private static IList<string> CreateDefaultNamespaces() {
            return
                new List<string>
                    {
                        "System",
                        "System.Collections",
                        "System.Collections.Generic",
                        "System.Dynamic",
                        "System.IO",
                        "System.Linq",
                        "Xipton.Razor.Extension"
                    }.AsReadOnly();
        }
        private IList<string> CreateDefaultReferences() {
            var references =
                new List<string>
                    {
                        "mscorlib.dll", 
                        "system.dll", 
                        "system.core.dll", 
                        "microsoft.csharp.dll"
                        
                    };
            if (_allowWildcardReferences)
            {
                references.AddRange(new[]
                {
                    "*.dll",
                    "*.exe"
                });
            }
            return references.AsReadOnly();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "created instances are disposed on CompositeContentProvider.Dispose()")]
        private static IList<Func<IContentProvider>> CreateDefaultContentProviders() {
            return new List<Func<IContentProvider>> { () => new FileContentProvider(Directory.Exists("./Views".MakeAbsoluteDirectoryPath()) ? "./Views" : ".") }.AsReadOnly();
        }

        private void LoadSettingsFromXDocumentOrElseDefaults(XContainer config) {
            try {

                var rootDescendents = config.Descendants(_rootElementName);

                var xElements = rootDescendents as XElement[] ?? rootDescendents.ToArray();
                RootOperator = ConfigElement
                    .Create<RootOperatorElement>()
                    .TryLoadElement(xElements
                        .Descendants("rootOperator")
                        .SingleOrDefault()
                     )
                    .CastTo<RootOperatorElement>();

                Templates = ConfigElement
                    .Create<TemplatesElement>()
                    .TryLoadElement(xElements
                        .Descendants("templates")
                        .SingleOrDefault()
                     )
                    .CastTo<TemplatesElement>();

                Namespaces = xElements.HasClearChildElement("namespaces") ? new List<string>() : CreateDefaultNamespaces();
                References = xElements.HasClearChildElement("references") ? new List<string>() : CreateDefaultReferences();
                ContentProviders = xElements.HasClearChildElement("contentProviders") ? new List<Func<IContentProvider>>() : CreateDefaultContentProviders();

                Namespaces = xElements
                    .Descendants("namespaces")
                    .SingleOrDefault(new XElement("namespaces"))
                    .Descendants("add")
                    .Select(xe => xe.GetAttributeValue("namespace"))
                    .Union(Namespaces)
                    .ToList()
                    .AsReadOnly();

                References = xElements
                    .Descendants("references")
                    .SingleOrDefault(new XElement("references"))
                    .Descendants("add")
                    .Select(xe => xe.GetAttributeValue("reference"))
                    .Union(References, StringComparer.InvariantCultureIgnoreCase)
                    .ToList()
                    .AsReadOnly();

                var contentProviderElements = 
                    xElements
                    .Descendants("contentProviders")
                    .SingleOrDefault(new XElement("contentProviders"))
                    .Descendants("add");

                ContentProviders = ContentProviders.ToList();
                foreach (var e in contentProviderElements) {
                    var el = e;
                    ContentProviders.Add(
                        () => Activator.CreateInstance(Type.GetType(el.GetAttributeValue("type"), true), true)
                                .CastTo<IContentProvider>()
                                .InitFromConfig(el)
                    );
                }
                ContentProviders = ContentProviders.CastTo<List<Func<IContentProvider>>>().AsReadOnly();
            }
            catch(TypeLoadException ex){
                throw new TemplateConfigurationException("Unable to load type named '{0}' at your xipton.razor configuration. Please correct the corresponding configuration setting.".FormatWith(ex.TypeName),ex);
            }
            catch(Exception ex){
                throw new TemplateConfigurationException("Unable to load the xipton.razor configuration. {0}. Please correct your xipton.razor configuration. Take a look at the inner exception(s) for details.".FormatWith(ex.Message),ex);
            }

        }
        private void LoadDefaults() {
            RootOperator = ConfigElement.Create<RootOperatorElement>();
            Templates = ConfigElement.Create<TemplatesElement>();
            Namespaces = CreateDefaultNamespaces();
            References = CreateDefaultReferences();
            ContentProviders = CreateDefaultContentProviders();
        }
        private void TryResolveWildcardReferences()
        {

            // ensure xipton assemblies to be loaded in the execution context
            AppDomain.CurrentDomain
                .EnsureXiptonAssembliesLoaded();

            if (!References.Any(s => s.Contains("*.")))
            {
                // no need to resolve references
                return;
            }

            if (!_allowWildcardReferences)
            {
                throw new TemplateConfigurationException("AllowWildcardReferences == false -> References cannot be configured using wildcards. All references must be configured explicitely.");
            }

            // ensure all bin assemblies to be loaded in the execution context
            AppDomain.CurrentDomain.EnsureBinAssembliesLoaded();

            var domainAssemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .ToList();

            var referencedAssemblyFileNames = new List<string>();

            FindAssemblyFileNamesByPattern("*.dll", domainAssemblies, referencedAssemblyFileNames);
            FindAssemblyFileNamesByPattern("*.exe", domainAssemblies, referencedAssemblyFileNames);

            References =
                References
                    .Where(referenceName => 
                        !referenceName.Contains("*") &&
                        !referencedAssemblyFileNames.Any(filename => filename
                            .TrimEnd()
                            .EndsWith(EnsureLeadingBackSlash(referenceName), StringComparison.OrdinalIgnoreCase)
                         )
                    )
                    .Concat(
                        referencedAssemblyFileNames
                    )
                    .ToList()
                    .AsReadOnly();
        }
        private void FindAssemblyFileNamesByPattern(string pattern, IEnumerable<Assembly> domainAssemblies, List<string> referencedAssemblyFileNames){
            if (References.Contains(pattern, StringComparer.OrdinalIgnoreCase)) {
                // if the configuration contains any <add reference="[pattern]"/> entry, then all collected DLLs are added to the referenced assemblies set
                referencedAssemblyFileNames.AddRange(
                    domainAssemblies
                    .Select(assembly => assembly.GetFileName())
                    .Where(filename => filename.EndsWith(pattern.Substring(1), StringComparison.OrdinalIgnoreCase))
                );
            }
        }
        private static string EnsureLeadingBackSlash(string referenceName){
            if (referenceName == null) throw new ArgumentNullException("referenceName");
            return referenceName.Contains("\\") ? referenceName : "\\" + referenceName;
        }

        #endregion


    }

}

