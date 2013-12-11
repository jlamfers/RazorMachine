// ReSharper disable HeuristicUnreachableCode
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xipton.Razor.UnitTest {
    [TestClass]
    public class LiteralStringTest {

        [TestMethod]
        public void LiteralStringCanBeInstantiated() {
            new LiteralString(null);
            new LiteralString(string.Empty);
            new LiteralString("foo");
        }

        [TestMethod]
        public void LiteralStringCanBeComparedToSystemString() {
            Assert.IsTrue(new LiteralString(null).Equals(null));
            new LiteralString(string.Empty).Equals(string.Empty);
            new LiteralString("foo").Equals("foo");

            Assert.IsTrue(new LiteralString(string.Empty) == string.Empty);
            Assert.IsTrue(new LiteralString("foo") == "foo");

            Assert.IsFalse(new LiteralString(null) != null); // !
            Assert.IsFalse(new LiteralString(string.Empty) != string.Empty);
            Assert.IsFalse(new LiteralString("foo") != "foo");
        }

        [TestMethod]
        public void LiteralStringCanBeAssignedFromSystemString() {
            LiteralString literalString = "foo";
            Assert.AreEqual(literalString,"foo");
            Assert.IsTrue(literalString == "foo");

            string nullString = null;
            literalString = nullString;
            Assert.IsTrue(literalString.Equals(nullString)); //! => literalString object itself is not null
            Assert.IsTrue(literalString == nullString);
            Assert.IsTrue(literalString == null);//! => literalString object itself is not null
        }

        [TestMethod]
        public void StringCanBeAssignedFromLiteralString() {
            LiteralString literalString = "foo";
            string s = literalString;
            Assert.AreEqual(literalString, s);
            Assert.IsTrue(s == "foo");

            literalString = (string) null;
            s = literalString;
            Assert.IsTrue(literalString == s);
            Assert.IsTrue(s == null);
        }

        [TestMethod]
        public void NullStringBehaviorCheck(){
            Assert.IsTrue(new LiteralString(null) == null); // !!
            Assert.IsTrue(new LiteralString(null).ToString() == string.Empty); // !! => ToString() never returns null;

            LiteralString s = (string)null;
            Assert.IsTrue(s == null);
            Assert.IsTrue(s.ToString().Length == 0); //!!

            s = null; // instead of (string)null => no implicit cast operator
            Assert.IsTrue(s == null);
            Exception caught = null;
            try{
                Assert.IsTrue(s.ToString().Length == 0);
            }
            catch (NullReferenceException ex) {
                caught = ex;
                Console.WriteLine(ex.Message);
            }
            Assert.IsNotNull(caught);
        }
    }
}
