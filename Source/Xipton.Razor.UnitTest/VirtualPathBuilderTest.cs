using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xipton.Razor.Core;
using Xipton.Razor.Extension;

namespace Xipton.Razor.UnitTest
{
    [TestClass]
    public class VirtualPathBuilderTest {

        #region Helper methods
        private static void MonkeyTest(string appName){
            var pb = new VirtualPathBuilder(appName);
            Assert.AreEqual("", pb.ToString());
            Assert.AreEqual((appName+"/").Replace("//","/"), pb.ApplicationRoot);
            Assert.AreEqual(pb.Length, 0);
            Assert.IsTrue(pb == "");
            Assert.IsFalse(pb.IsAbsolutePath());
            Assert.IsFalse(pb.IsApplicationRoot());
            Assert.IsFalse(pb.IsRelativePath());
            Assert.IsFalse(pb.HasRootOperator());
            Assert.IsFalse(pb.HasExtension());
            Assert.IsFalse(pb.HasTrailingSlash());
            Assert.IsFalse(pb.IsValidAbsolutePath());
        }

        private static VirtualPathBuilder NormalizeWorks(string appName, string part, params string[] normalizeResults) {
            var pb = new VirtualPathBuilder(appName);
            pb.CombineWith(part);
            Assert.IsTrue(pb == part);

            if (normalizeResults.Length > 0) {
                foreach (var normalizeResult in normalizeResults) {
                    Assert.IsTrue(pb == normalizeResult);
                }
                pb = pb.Normalize();
            }
            else {
                var caught = false;
                try {
                    pb.Normalize();
                }
                catch (InvalidOperationException ex) {
                    Debug.WriteLine("Expected error: {0}".FormatWith(ex.Message));
                    caught = true;
                }
                Assert.IsTrue(caught);
                return null;
            }
            return pb;
        }

        private static void AssertIsEmpty(VirtualPathBuilder pb) {
            Assert.IsFalse(pb.IsAbsolutePath());
            Assert.IsFalse(pb.IsApplicationRoot());
            Assert.IsFalse(pb.IsRelativePath());
            Assert.IsFalse(pb.HasRootOperator());
            Assert.IsFalse(pb.HasExtension());
            Assert.IsFalse(pb.HasTrailingSlash());
            Assert.IsFalse(pb.IsValidAbsolutePath());
        }

        private static void AssertIsAbsolute(VirtualPathBuilder pb){
            Assert.IsTrue(pb.IsAbsolutePath());
            Assert.IsFalse(pb.IsRelativePath());
            Assert.IsFalse(pb.HasRootOperator());
            Assert.IsFalse(pb.HasExtension());
            Assert.IsFalse(pb.HasTrailingSlash());
            Assert.IsTrue(pb.IsValidAbsolutePath());
        }

        private static void AssertIsValidRoot(VirtualPathBuilder pb) {
            Assert.IsTrue(pb.IsAbsolutePath());
            Assert.IsTrue(pb.IsApplicationRoot());
            Assert.IsFalse(pb.IsRelativePath());
            Assert.IsFalse(pb.HasRootOperator());
            Assert.IsFalse(pb.HasExtension());
            Assert.IsFalse(pb.HasTrailingSlash(ignoreRoot:true));
            Assert.IsTrue(pb.IsValidAbsolutePath());
        }
        #endregion


        [TestMethod]
        public void MonkeyTest()
        {
            MonkeyTest("/");
            MonkeyTest("/Foo");
            MonkeyTest("/Foo/");

            AssertIsEmpty(NormalizeWorks("/",".",""));
            AssertIsEmpty(NormalizeWorks("/Foo", ".", ""));
            AssertIsEmpty(NormalizeWorks("/Foo/", ".", ""));

            AssertIsAbsolute(NormalizeWorks("/", "doo", "Doo"));
            AssertIsAbsolute(NormalizeWorks("/Foo", "doo", "dOO"));
            AssertIsAbsolute(NormalizeWorks("/Foo/", "doo", "DOO"));

            Assert.IsNull(NormalizeWorks("/", ".."));
            Assert.IsNull(NormalizeWorks("/Foo", ".."));
            Assert.IsNull(NormalizeWorks("/Foo/", ".."));

            AssertIsValidRoot(NormalizeWorks("/", "/", "/", "~/"));
            AssertIsValidRoot(NormalizeWorks("/Foo", "/FOO/", "/Foo", "~/", "/FOO/"));
            AssertIsValidRoot(NormalizeWorks("/Foo", "/Foo/", "/Foo", "~/", "/FOO/"));
            AssertIsValidRoot(NormalizeWorks("/Foo", "/Foo", "/Foo", "~/", "/FOO/"));
            AssertIsValidRoot(NormalizeWorks("/Foo", "/Foo", "/Foo", "~/", "/foo/"));

        }
        [TestMethod]
        public void ClearWorks() {
            var pb = new VirtualPathBuilder("/Foo");
            Assert.AreEqual(string.Empty, pb.ToString());
            Assert.AreEqual("/Foo/", pb.ApplicationRoot);
            pb.CombineWith("Doo");
            Assert.AreEqual("Doo", pb.ToString());
            Assert.AreEqual("/Foo/Doo", pb.Normalize().ToString());
            pb.Clear();
            Assert.AreEqual(string.Empty, pb.ToString());
            pb.CombineWith(".");
            Assert.AreEqual("", pb.Normalize().ToString());
        }
        [TestMethod]
        public void HasRootOperatorWorks(){
            var pb = new VirtualPathBuilder().CombineWith("~/");
            Assert.IsTrue(pb.HasRootOperator());
            Assert.IsFalse(pb.Normalize().HasRootOperator());
            Assert.IsTrue(pb == "/");
            Assert.IsTrue(pb.WithRootOperator().HasRootOperator());

            pb = new VirtualPathBuilder().CombineWith("~");
            Assert.IsTrue(pb.HasRootOperator());
            Assert.IsFalse(pb.Normalize().HasRootOperator());
            Assert.IsTrue(pb == "/");
            Assert.IsTrue(pb.WithRootOperator().HasRootOperator());

            pb = new VirtualPathBuilder().CombineWith("~/Foo");
            Assert.IsTrue(pb.HasRootOperator());
            Assert.IsFalse(pb.Normalize().HasRootOperator());
            Assert.IsTrue(pb == "/Foo");
            Assert.IsTrue(pb.WithRootOperator().HasRootOperator());
        }
        [TestMethod]
        public void IsAbsolutePathWorks() {
            var pb = new VirtualPathBuilder().CombineWith("~/");
            Assert.IsFalse(pb.IsAbsolutePath());
            pb.Normalize();
            Assert.IsTrue(pb.IsAbsolutePath());

            pb = new VirtualPathBuilder().CombineWith("~");
            Assert.IsFalse(pb.IsAbsolutePath());
            pb.Normalize();
            Assert.IsTrue(pb.IsAbsolutePath());

            pb = new VirtualPathBuilder().CombineWith("~/Foo");
            Assert.IsFalse(pb.IsAbsolutePath());
            pb.Normalize();
            Assert.IsTrue(pb.IsAbsolutePath());

            pb = new VirtualPathBuilder("/Foo");
            Assert.IsFalse(pb.IsAbsolutePath());
            Assert.IsTrue(pb.CombineWith("/Foo/").IsAbsolutePath());
            Assert.IsTrue(pb.Clear().CombineWith("/Foo").IsAbsolutePath());
            Assert.IsTrue(pb.Clear().CombineWith("/Foo/Doo").IsAbsolutePath());
            Assert.IsTrue(pb.Clear().CombineWith("/Foo/Doo/Boo").IsAbsolutePath());
            Assert.IsTrue(pb.CombineWith("../..") == "/Foo");
            Assert.IsTrue(pb.WithRootOperator().HasRootOperator());

            pb = new VirtualPathBuilder("/Foo");
            pb.CombineWith("/Doo/Foo");
            Assert.IsTrue(pb.IsAbsolutePath());
            Assert.IsFalse(pb.IsValidAbsolutePath());
        }
        [TestMethod]
        public void IsApplicationRootWorks(){
            var pb = new VirtualPathBuilder();
            Assert.IsFalse(pb.IsApplicationRoot());
            Assert.IsTrue(pb.Clear().CombineWith("/").Normalize().IsApplicationRoot());
            Assert.IsTrue(pb.Clear().CombineWith("~").Normalize().IsApplicationRoot());
            Assert.IsTrue(pb.Clear().CombineWith("~/").Normalize().IsApplicationRoot());

            pb = new VirtualPathBuilder("/Foo");
            Assert.IsFalse(pb.IsApplicationRoot());
            Assert.IsTrue(pb.Clear().CombineWith("/Foo").Normalize().IsApplicationRoot());
            Assert.IsTrue(pb.Clear().CombineWith("/Foo/").Normalize().IsApplicationRoot());
            Assert.IsTrue(pb.Clear().CombineWith("~").Normalize().IsApplicationRoot());
            Assert.IsTrue(pb.Clear().CombineWith("~/").Normalize().IsApplicationRoot());
        }
        [TestMethod]
        public void IsValidAbsolutePathWorks() {
            var pb = new VirtualPathBuilder();
            Assert.IsFalse(pb.IsApplicationRoot());
            Assert.IsFalse(pb.Clear().CombineWith(".").Normalize().IsValidAbsolutePath());
            Assert.IsTrue(pb.Clear().CombineWith("/").Normalize().IsValidAbsolutePath());
            Assert.IsTrue(pb.Clear().CombineWith("~").Normalize().IsValidAbsolutePath());
            Assert.IsTrue(pb.Clear().CombineWith("~/").Normalize().IsValidAbsolutePath());

            pb = new VirtualPathBuilder("/Foo");
            Assert.IsFalse(pb.IsApplicationRoot());
            Assert.IsFalse(pb.Clear().CombineWith(".").Normalize().IsValidAbsolutePath());
            Assert.IsFalse(pb.Clear().CombineWith("/").Normalize().IsValidAbsolutePath());
            Assert.IsTrue(pb.Clear().CombineWith("/Foo").Normalize().IsValidAbsolutePath());
            Assert.IsTrue(pb.Clear().CombineWith("/Foo/").Normalize().IsValidAbsolutePath());
            Assert.IsFalse(pb.Clear().CombineWith("/Foo2/").Normalize().IsValidAbsolutePath());
            Assert.IsTrue(pb.Clear().CombineWith("~").Normalize().IsValidAbsolutePath());
            Assert.IsTrue(pb.Clear().CombineWith("~/Anything").Normalize().IsValidAbsolutePath());
        }
        [TestMethod]
        public void IsRelativePathWorks() {
            var pb = new VirtualPathBuilder();
            Assert.IsFalse(pb.IsRelativePath());
            pb.CombineWith(".");
            Assert.IsTrue(pb.IsRelativePath());
            pb.Clear().CombineWith("Foo");
            Assert.IsTrue(pb.IsRelativePath());
            pb.Normalize();
            Assert.IsFalse(pb.IsRelativePath());
            Assert.IsTrue(pb.IsAbsolutePath());
        }
        [TestMethod]
        public void ResolveRootOperatorWorks() {
            var pb = new VirtualPathBuilder();
            Assert.IsTrue(pb.CombineWith("Foo").ResolveRootOperator().ToString() == "/Foo");
            Assert.IsTrue(pb.Clear().CombineWith("~/").ResolveRootOperator().ToString() == "/");
            Assert.IsTrue(pb.Clear().CombineWith("~").ResolveRootOperator().ToString() == "/");
            pb = new VirtualPathBuilder("/Foo");
            Assert.IsTrue(pb.CombineWith("~/Doo").ResolveRootOperator().ToString() == "/Foo/Doo");
            var caught = false;
            try{
                pb.Clear().CombineWith("/Oops").ResolveRootOperator();
            }
            catch(InvalidOperationException){
                caught = true;
            }
            Assert.IsTrue(caught);
        }
        [TestMethod]
        public void WithRootOperatorWorks() {
            var pb = new VirtualPathBuilder("/Foo");
            var caught = false;
            try{
                pb.WithRootOperator();
            }
            catch(InvalidOperationException){
                caught = true;
            }
            Assert.IsTrue(caught);
            Assert.IsTrue(pb.CombineWith("/Foo").WithRootOperator() == "~/");
            Assert.IsTrue(pb.Clear().CombineWith("Doo").WithRootOperator() == "~/Doo");
        }
        [TestMethod]
        public void NormalizeWorks(){
            var pb = new VirtualPathBuilder().Normalize();
            Assert.IsTrue(pb == "");
            Assert.IsTrue(pb.ToString() == "");

            pb.Clear().CombineWith(".").Normalize();
            Assert.IsTrue(pb == "");
            Assert.IsTrue(pb.ToString() == "");

            var caught = false;
            try{
                pb.Clear().CombineWith("..").Normalize();
            }
            catch(InvalidOperationException){
                caught = true;
            }
            Assert.IsTrue(caught);

            CombineWithWorks();

        }
        [TestMethod]
        public void CombineWithWorks(){
            var pb = new VirtualPathBuilder("/foo").CombineWith("oops/../doo/.").Normalize();
            Assert.IsTrue(pb == "/foo/doo");
            pb = new VirtualPathBuilder("/foo").CombineWith("oops", "..", "doo", ".", ".").Normalize();
            Assert.IsTrue(pb == "/foo/doo");
            pb = new VirtualPathBuilder("/foo").CombineWith("oops", "..", "doo", ".", ".","~/").Normalize();
            Assert.IsTrue(pb == "/foo");
            Assert.IsTrue(pb == "~/");
        }
        [TestMethod]
        public void HasExtensionWorks() {
            var pb = new VirtualPathBuilder().CombineWith(".");
            Assert.IsTrue(pb == ".");
            Assert.IsFalse(pb.HasExtension());
            pb.Clear().CombineWith("~/oops.cshtml");
            Assert.IsTrue(pb.HasExtension());
            pb.Normalize();
            Assert.IsTrue(pb.HasExtension());
        }
        [TestMethod]
        public void GetExtensionWorks() {
            var pb = new VirtualPathBuilder().CombineWith(".");
            Assert.IsTrue(pb == ".");
            Assert.IsFalse(pb.HasExtension());
            Assert.IsNull(pb.GetExtension());
            pb.Clear().CombineWith("~/oops.cshtml");
            Assert.IsTrue(pb.HasExtension());
            pb.Normalize();
            Assert.IsTrue(pb.HasExtension());
            Assert.IsTrue(pb.GetExtension() == "cshtml");
            Assert.IsTrue(pb.HasExtension());
            Assert.IsTrue(pb.GetExtension(true) == "cshtml");
            Assert.IsFalse(pb.HasExtension());
        }
        [TestMethod]
        public void AddOrReplaceExtensionWorks() {
            var pb = new VirtualPathBuilder().CombineWith("~/t.cs");
            Assert.IsTrue(pb.GetExtension() == "cs");
            pb.AddOrReplaceExtension("vb");
            Assert.IsTrue(pb.GetExtension() == "vb");
            pb.Clear().CombineWith("foo");
            Assert.IsFalse(pb.HasExtension());
            pb.AddOrReplaceExtension("vb");
            Assert.IsTrue(pb.GetExtension() == "vb");
        }
        [TestMethod]
        public void AddOrKeepExtensionWorks() {
            var pb = new VirtualPathBuilder().CombineWith("~/t.cs");
            Assert.IsTrue(pb.GetExtension() == "cs");
            pb.AddOrKeepExtension("vb");
            Assert.IsTrue(pb.GetExtension() == "cs");
            pb.Clear().CombineWith("foo");
            Assert.IsFalse(pb.HasExtension());
            pb.AddOrKeepExtension("vb");
            Assert.IsTrue(pb.GetExtension() == "vb");
        }
        [TestMethod]
        public void RemoveExtensionWorks() {
            var pb = new VirtualPathBuilder();
            pb.RemoveExtension();
            pb.CombineWith("oops.cs");
            Assert.IsTrue(pb.GetExtension() == "cs");
            pb.RemoveExtension();
            Assert.IsNull(pb.GetExtension());
        }
        [TestMethod]
        public void GetFirstPartWorks() {
            var pb = new VirtualPathBuilder("/foo").CombineWith("~/app/path/");
            Assert.IsTrue(pb.GetFirstPart() == "~");
            Assert.IsTrue(pb.GetFirstPart(true) == "~");
            Assert.IsTrue(pb.GetFirstPart() == "/app");
            Assert.IsTrue(pb.GetFirstPart(true) == "/app");
            Assert.IsTrue(pb.GetFirstPart() == "/path");
            Assert.IsTrue(pb.GetFirstPart(true) == "/path");
            Assert.IsTrue(pb.GetFirstPart(true) == "/");

            pb = new VirtualPathBuilder("/foo").CombineWith("~/app/path/").Normalize();
            Assert.IsTrue(pb.GetFirstPart() == "/foo");
            Assert.IsTrue(pb.GetFirstPart(true) == "/foo");
            Assert.IsTrue(pb.GetFirstPart() == "/app");
            Assert.IsTrue(pb.GetFirstPart(true) == "/app");
            Assert.IsTrue(pb.GetFirstPart() == "/path");
            Assert.IsTrue(pb.GetFirstPart(true) == "/path");
            Assert.IsTrue(pb.GetFirstPart(true) == "");
        }
        [TestMethod]
        public void GetLastPartWorks() {
            var pb = new VirtualPathBuilder("/foo").CombineWith("~/app/path/");
            Assert.IsTrue(pb.GetLastPart() == "path");
            Assert.IsTrue(pb.GetLastPart(true) == "path");
            Assert.IsTrue(pb.GetLastPart() == "app");
            Assert.IsTrue(pb.GetLastPart(true) == "app");
            Assert.IsTrue(pb.GetLastPart() == "~/");
            Assert.IsTrue(pb.GetLastPart(true) == "~/");
            Assert.IsTrue(pb.GetLastPart(true) == "");

            pb = new VirtualPathBuilder("/foo").CombineWith("~/app/path/").Normalize();
            Assert.IsTrue(pb.GetLastPart() == "path");
            Assert.IsTrue(pb.GetLastPart(true) == "path");
            Assert.IsTrue(pb.GetLastPart() == "app");
            Assert.IsTrue(pb.GetLastPart(true) == "app");
            Assert.IsTrue(pb.GetLastPart() == "foo");
            Assert.IsTrue(pb.GetLastPart(true) == "foo");
            Assert.IsTrue(pb.GetLastPart(true) == "/");
        }
        [TestMethod]
        public void HasTrailingSlashWorks(){
            var pb = new VirtualPathBuilder().CombineWith("/").Normalize();
            Assert.IsTrue(pb.HasTrailingSlash());
            Assert.IsFalse(pb.HasTrailingSlash(true));
            pb.Clear().CombineWith("/foo/");
            Assert.IsTrue(pb.HasTrailingSlash());
            pb.Clear().CombineWith("/foo");
            Assert.IsFalse(pb.HasTrailingSlash());

        }
        [TestMethod]
        public void AppendTrailingSlashWorks() {
            var pb = new VirtualPathBuilder().CombineWith("/").AppendTrailingSlash();
            Assert.IsTrue(pb == "/");
            pb.Clear().CombineWith("/foo").AppendTrailingSlash();
            Assert.IsTrue(pb.HasTrailingSlash());
            Assert.IsTrue(pb == "/foo/");
        }
        [TestMethod]
        public void RemoveTrailingSlashWorks() {
            var pb = new VirtualPathBuilder().CombineWith("/").RemoveTrailingSlash();
            Assert.IsTrue(pb == "");
            pb.Clear().CombineWith("/").RemoveTrailingSlash(true);
            Assert.IsTrue(pb == "/");
        }
        [TestMethod]
        public void PathBuilderWorksWithRootPartAdded()
        {
            var pb = new VirtualPathBuilder();
            pb.CombineWith("/");
            pb.CombineWith("/");
            Assert.AreEqual("/", pb.ToString());
        }
        [TestMethod]
        public void NormalizingToRootRelativeWorksForCurrentDirectory()
        {
            var pb = new VirtualPathBuilder();
            pb
                .CombineWith("/")
                .CombineWith("/./././.")
                .Normalize()
                .WithRootOperator();
            Assert.AreEqual("~/", pb.ToString());
        }
        [TestMethod]
        public void NormalizingToRootRelativeWorksForBackDirectory()
        {
            var pb = new VirtualPathBuilder();
            pb
                .CombineWith("/", "/foo/..")
                .Normalize()
                .WithRootOperator();
            Assert.AreEqual("~/", pb.ToString());
        }
        [TestMethod]
        public void PathBuilderCanBeCreatedFromTwoParts()
        {
            var pb = new VirtualPathBuilder();
            pb.CombineWith("~/part1", "part2").Normalize();
            Assert.AreEqual("/part1/part2",pb.ToString());
            pb.WithRootOperator();
            Assert.AreEqual("~/part1/part2", pb.ToString());
        }

    }
}
