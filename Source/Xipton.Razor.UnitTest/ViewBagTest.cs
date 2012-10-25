using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Xipton.Razor.Core;
using Xipton.Razor.Extension;

namespace Xipton.Razor.UnitTest
{
    [TestFixture]
    public class ViewBagTest
    {

        private static RazorMachine NewEngine(string virtualRoot = null) {
            var setup = @"
<xipton.razor>

  <rootOperator 
    path=""{0}""
  />
  <namespaces>
    <add namespace=""System.Diagnostics""/>
    <add namespace=""NUnit.Framework""/>
  </namespaces>

</xipton.razor>
".FormatWith(virtualRoot ?? "/");

            return new RazorMachine(setup);
        }

        [Test]
        public void ViewBagValueIsSet()
        {
            var engine = new RazorMachine()
                .RegisterTemplate("~/Test", "@{ViewBag.Foo = 1;} Hello World!");
            var template = engine.ExecuteUrl("~/Test");
            Debug.WriteLine(template.Result); // => prints: Hello World!
            Assert.AreEqual(1, template.ViewBag.Foo);
        }

        [Test]
        public void EmptyViewBagCanBeAssignedTo()
        {
            var engine = NewEngine("/foo");
            engine.RegisterTemplate("~/Test.cshtml", "@{ViewBag.Foo = 1;}");
            var template = engine.ExecuteUrl("/foo/Test");
            Assert.AreEqual(1,template.ViewBag.Foo);
        }

        [Test]
        [ExpectedException(typeof(TemplateBindingException))]
        public void EmptyViewBagThrowsRuntimeBinderExceptionOnInvalidReference()
        {
            var engine = NewEngine();
            engine.RegisterTemplate("/Test", "@{int i = (int)ViewBag.Foo;}");
            engine.ExecuteUrl("/Test");
        }

        [Test]
        public void ViewBagCanBe_AnonymousInstance()
        {
            var engine = NewEngine();
            engine.RegisterTemplate("~/Test/Foo", "@{var i = (int)ViewBag.Foo;Assert.AreEqual(10,i);}");
            engine.ExecuteUrl("/Test/Foo",viewbag:new{Foo = 10});
        }
        [Test]
        public void ViewBagCanBe_KeyValueDictionary()
        {
            var engine = NewEngine();
            engine.RegisterTemplate("~/Test.cshtml", "@{var i = (int)ViewBag.Foo;Assert.AreEqual(10,i);}");
            engine.ExecuteUrl("~/Test", viewbag: new Dictionary<string,object>{{"Foo",10}});

        }
        class FooHolder
        {
// ReSharper disable UnusedMember.Local
            public int Foo { get { return 10; } }
// ReSharper restore UnusedMember.Local
        }
        [Test]
        public void ViewBagCanBe_Class()
        {
            var engine = NewEngine();
            engine.RegisterTemplate("~/Test.cshtml", "@{var i = (int)ViewBag.Foo;Assert.AreEqual(10,i);}");
            engine.ExecuteUrl("~/Test", viewbag: new FooHolder());

        }
        [Test]
        public void ViewBagFromParentIsReferencedImplicitly()
        {
            var engine = NewEngine();
            engine.RegisterTemplate("~/Parent.cshtml", "@{ViewBag.Title=10;} @RenderPage(\"Child\")");
            engine.RegisterTemplate("~/Child.cshtml", "@{var i = (int)ViewBag.Title;Assert.AreEqual(10,i);}");
            engine.ExecuteUrl("~/Parent");

        }

        [Test]
        public void ViewBagAtChildInstantiatesRootViewBagIfChildHasNoViewBag()
        {
            var engine = NewEngine();
            engine.RegisterTemplate("~/Parent.cshtml", "@RenderPage(\"Child\") @{var i = (int)ViewBag.Title;Assert.AreEqual(10,i);}");
            engine.RegisterTemplate("~/Child.cshtml", "@RenderPage(\"Child2\")");
            engine.RegisterTemplate("~/Child2.cshtml", "@{ViewBag.Title=10;}");
            var template = engine.ExecuteUrl("~/Parent");
            Assert.AreEqual("~/Child2.cshtml", template.Childs[0].Childs[0].VirtualPath);

        }


    }
}
