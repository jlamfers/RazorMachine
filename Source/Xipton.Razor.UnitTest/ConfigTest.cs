using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xipton.Razor.Config;

namespace Xipton.Razor.UnitTest {
    [TestClass]
    public class ConfigTest {

        [TestMethod]
        public void ConfigCanBeCreated(){
            var config = new RazorConfig();
            Assert.IsNotNull(config.ContentProviders);
            Assert.IsNotNull(config.Namespaces);
            Assert.IsNotNull(config.References);
            Assert.IsNotNull(config.RootOperator);
            Assert.IsNotNull(config.Templates);
            Assert.IsTrue(config.References.Any(r => r.Contains("*")));
        }
        [TestMethod]
        public void ConfigCanBeCreatedWithoutWildcards()
        {
            var config = new RazorConfig(false);
            Assert.IsNotNull(config.ContentProviders);
            Assert.IsNotNull(config.Namespaces);
            Assert.IsNotNull(config.References);
            Assert.IsNotNull(config.RootOperator);
            Assert.IsNotNull(config.Templates);
            Assert.IsFalse(config.References.Any(r => r.Contains("*")));
        }

        [TestMethod]
        public void ConfigIsLoadedFromConfig() {
            var config = new RazorConfig();
            Assert.IsFalse(config.Templates.IncludeGeneratedSourceCode);
            config.Initializer.TryInitializeFromConfig();
            Assert.IsTrue(config.Templates.IncludeGeneratedSourceCode);
        }

        [TestMethod]
        public void ConfigIsLoadedFromFile() {
            var config = new RazorConfig();
            Assert.IsFalse(config.Templates.IncludeGeneratedSourceCode);
            config.Initializer.InitializeByXmlFileName("Xipton.Razor.UnitTest.dll.config");
            Assert.IsTrue(config.Templates.IncludeGeneratedSourceCode);
        }

        [TestMethod]
        public void ConfigIsLoadedFromValues() {
            var config = new RazorConfig();
            config.Initializer.InitializeByValues(defaultExtension: ".vb");
            Assert.AreEqual(config.Templates.DefaultExtension,".vb");
        }

        [TestMethod]
        public void ConfigIsLoadedFromXml() {
            var config = new RazorConfig();
            config.Initializer.InitializeByXmlContent(
                @"
<xipton.razor>
    <rootOperator path=""/foo"" />
</xipton.razor>
"
                );
            Assert.AreEqual(config.RootOperator.Path, "/foo");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConfigCannotBeModifiedAfterRazorMachineInitialization(){

            var rm = new RazorMachine();
            rm.Context.Config.Initializer.InitializeByValues(rootOperatorPath: "/foo");

        }
    }
}
