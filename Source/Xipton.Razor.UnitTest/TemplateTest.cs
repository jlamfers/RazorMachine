using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Xipton.Razor.Core;

namespace Xipton.Razor.UnitTest
{
    public class Person
    {
        public class AddressType
        {
            public string Street { get; set; }
        }

        public AddressType Address { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [TestFixture]
    public class TemplateTest {

        // contains a few global unit tests

        [Test]
        public void SpacesArePreservedWithinAttributes(){
            var m = new RazorMachine();
            var t = m.ExecuteContent("<Tag attribute=\"@Model.FirstName   @Model.LastName\"></Tag>", new { FirstName = "John", LastName = "Smith" });
            Assert.AreEqual("<Tag attribute=\"John   Smith\"></Tag>", t.Result);
        }

        [Test]
        public void NestedAnonymousTypesAreSupported(){
            var m = new RazorMachine();
            const string streetName = "Main Street";
            var t = m.Execute("@Model.Address.Street",new Person {Address = new Person.AddressType {Street = streetName}});
            Assert.AreEqual(streetName,t.Result);

            t = m.Execute("@Model.Address.Street",new { Address = new Person.AddressType { Street = streetName } });
            Assert.AreEqual(streetName, t.Result);

            t = m.Execute("@Model.Address.Street",new { Address = new { Street = streetName } });
            Assert.AreEqual(streetName, t.Result);
        }



        [Test]
        public void Performance_Uncompiled_vs_Compiled() {
            var rm = new RazorMachine(includeGeneratedSourceCode:false);
            rm.ExecuteContent(Guid.NewGuid().ToString("N")); // to warm up the engine
            var list = new List<string>();
            const int count = 50;
            for (var i = 0; i < count; i++) {
                // generate templates that display a guid value
                list.Add(Guid.NewGuid().ToString("N"));
            }
            var sw = new Stopwatch();
            sw.Start();
            for(var i = 0; i < count; i++){
                rm.ExecuteContent(list[i]);
            }
            sw.Stop();
            Console.WriteLine("Elapsed compile and run {0} times: {1} ms", count, sw.ElapsedMilliseconds);

            sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < count; i++) {
                rm.ExecuteContent(list[i]);
            }
            sw.Stop();
            Console.WriteLine("Elapsed run again {0} times: {1} ms", count, sw.ElapsedMilliseconds == 0 ? "< 1" : sw.ElapsedMilliseconds.ToString());
        }

        [Test]
        public void Performance_Compiled_Template_Tree() {
            var rm = new RazorMachine();
            rm.RegisterTemplate("~/Parent.cshtml", "@RenderPage(\"Child\") @{var i = (int)ViewBag.Title;}");
            rm.RegisterTemplate("~/Child.cshtml", "@RenderPage(\"Child2\") child");
            rm.RegisterTemplate("~/Child2.cshtml", "@{ViewBag.Title=10;} child2");
            Debug.WriteLine("Result: " + rm.ExecuteUrl("~/Parent").Result); // to warm up

            var sw = new Stopwatch();
            sw.Start();
            const int count = 10000;
            for (var i = 0; i < count; i++) {
                rm.ExecuteUrl("~/Parent");
            }
            sw.Stop();
            Debug.WriteLine("Elapsed on {0} iterations: {1} ms", count, sw.ElapsedMilliseconds);

        }



        [Test]
        public void TemplateCanBeExcecutedDirectlyByContent() {
            var rm = new RazorMachine();
            var executedTemplate = rm.ExecuteContent("Hello @Model.FirstName @Model.LastName", new { FirstName = "Dick", LastName = "Tracy" });
            Assert.AreEqual("Hello Dick Tracy", executedTemplate.Result);
            executedTemplate = rm.Execute("Hello @Model.FirstName @Model.LastName", new { FirstName = "Dick", LastName = "Tracy" });
            Assert.AreEqual("Hello Dick Tracy", executedTemplate.Result);
        }

        [Test]
        public void TemplateCanBeExcecutedIndirectlyByUrl(){
            var rm = new RazorMachine();
            rm.RegisterTemplate("~/simpleTemplate.cshrml", "Hello @Model.FirstName @Model.LastName");
            var executedTemplate = rm.ExecuteUrl("~/simpleTemplate.cshrml", new {FirstName = "Dick", LastName = "Tracy"});
            Assert.AreEqual("Hello Dick Tracy", executedTemplate.Result);
            executedTemplate = rm.ExecuteUrl("/simpleTemplate.cshrml", new { FirstName = "Dick", LastName = "Tracy" });
            Assert.AreEqual("Hello Dick Tracy", executedTemplate.Result);
            executedTemplate = rm.Execute("~/simpleTemplate.cshrml", new { FirstName = "Dick", LastName = "Tracy" });
            Assert.AreEqual("Hello Dick Tracy", executedTemplate.Result);
            executedTemplate = rm.Execute("/simpleTemplate.cshrml", new { FirstName = "Dick", LastName = "Tracy" });
            Assert.AreEqual("Hello Dick Tracy", executedTemplate.Result);
        }

        [Test]
        public void TemplateCanBeExcecutedIndirectlyByUrlAdOmittingDefaultExtension() {
            var rm = new RazorMachine();
            rm.RegisterTemplate("~/simpleTemplate", "Hello @Model.FirstName @Model.LastName");
            var executedTemplate = rm.ExecuteUrl("~/simpleTemplate", new { FirstName = "Dick", LastName = "Tracy" });
            Assert.AreEqual("Hello Dick Tracy", executedTemplate.Result);
        }

        [Test]
        public void ViewStartIsExecuted() {
            var rm = new RazorMachine();
            rm.RegisterTemplate("~/_ViewStart", "@{ViewBag.ViewStartExecuted = true;}");
            rm.RegisterTemplate("~/simpleTemplate", "Hello @Model.FirstName @Model.LastName");
            var executedTemplate = rm.ExecuteUrl("~/simpleTemplate", new { FirstName = "Dick", LastName = "Tracy" });
            Assert.AreEqual("Hello Dick Tracy", executedTemplate.Result.Trim());
            Assert.AreEqual(true, executedTemplate.ViewBag.ViewStartExecuted); 
        }

        [Test]
        public void LayoutIsApplied() {
            var rm = new RazorMachine(includeGeneratedSourceCode:true);
            rm.RegisterTemplate("~/Shared/_layout", "Layout says: @RenderBody()");
            rm.RegisterTemplate("~/simpleTemplate", "@{Layout=\"_layout\";}Hello @Model.FirstName @Model.LastName");
            var executedTemplate = rm.ExecuteUrl("~/simpleTemplate", new { FirstName = "Dick", LastName = "Tracy" });
            Debug.WriteLine("Template source code: \r\n"+executedTemplate.GeneratedSourceCode);
            Debug.WriteLine("Layout source code: \r\n" + executedTemplate.Childs[0].GeneratedSourceCode); // because layout is executed as a child of executedTemplate
            Assert.AreEqual("Layout says: Hello Dick Tracy", executedTemplate.Result.Trim());

            rm.RegisterTemplate("~/simpleTemplate", "@model dynamic\r\n@{Layout=\"_layout\";}Hello again @Model.FirstName @Model.LastName"); // "old" template type is removed from cache internally. Check to see what "@model dynamic" does with the generated source code.
            executedTemplate = rm.ExecuteUrl("~/simpleTemplate", new { FirstName = "Dick", LastName = "Tracy" });
            Debug.WriteLine("Template source code: \r\n" + executedTemplate.GeneratedSourceCode);
            Assert.AreEqual("Layout says: Hello again Dick Tracy", executedTemplate.Result.Trim());
        }

        [Test]
        public void RenderPageWorks(){
            var rm = new RazorMachine();
            rm.RegisterTemplate("~/someControl", "Hello @Model.Name");
            rm.RegisterTemplate("~/somePage", "@RenderPage(\"someControl\",skipLayout:true)");
            rm.RegisterTemplate("~/shared/someLayout", ">>@RenderBody()");
            rm.RegisterTemplate("~/_viewStart", "@{Layout=\"someLayout\";}");
            var t = rm.ExecuteUrl("/somePage",new {Name="John"});
            Assert.AreEqual(t.Result,">>Hello John");
        }

        [Test]
        public void RecursionErrorWorks() {
            var caught = false;
            try {
                var rm = new RazorMachine();
                rm.RegisterTemplate("~/somePage", "Oops");
                rm.RegisterTemplate("~/shared/_someLayout1", "@{Layout=\"_someLayout2\";} >>@RenderBody()");
                rm.RegisterTemplate("~/shared/_someLayout2", "@{Layout=\"_someLayout1\";} >>@RenderBody()");
                rm.RegisterTemplate("~/_viewStart", "@{Layout=\"_someLayout1\";}");
                var t = rm.ExecuteUrl("/somePage");
            }
            catch(TemplateTreeException ex){
                Console.WriteLine(ex.Message);
                caught = true;
            }
            Assert.IsTrue(caught);
        }

        [Test]
        public void LayoutIsAppliedImplicitlyByViewStart() {
            var rm = new RazorMachine();
            rm.RegisterTemplate("~/_ViewStart", "@{/* _ViewStart*/ Layout=\"_layout\";}");
            rm.RegisterTemplate("~/Shared/_layout", "Layout says: @RenderBody()");
            rm.RegisterTemplate("~/simpleTemplate", "Hello @Model.FirstName @Model.LastName");
            var executedTemplate = rm.ExecuteUrl("~/simpleTemplate", new { FirstName = "Dick", LastName = "Tracy" });
            Debug.WriteLine("Template source code: \r\n" + executedTemplate.GeneratedSourceCode);
            Debug.WriteLine("Layout source code: \r\n" + executedTemplate.Childs[0].GeneratedSourceCode); // layout is executed as a child of executedTemplate
            Assert.AreEqual("Layout says: Hello Dick Tracy", executedTemplate.Result.Trim());
        }

        [Test]
        public void LayoutIsAppliedAndSectionIsAppliedImplicitlyByViewStart() {
            var rm = new RazorMachine();
            rm.RegisterTemplate("~/_ViewStart", @"
@{
   Layout=""_layout"";
}
@section footer{
   Contact me on any crime at 666-666-666
}
");
            rm.RegisterTemplate("~/Shared/_layout", "Layout says: @RenderBody() @RenderSection(\"footer\")");
            rm.RegisterTemplate("~/simpleTemplate", "Hello @Model.FirstName @Model.LastName");
            var executedTemplate = rm.ExecuteUrl("~/simpleTemplate", new { FirstName = "Dick", LastName = "Tracy" });
            Debug.WriteLine("Template source code: \r\n" + executedTemplate.GeneratedSourceCode);
            Debug.WriteLine("Layout source code: \r\n" + executedTemplate.Childs[0].GeneratedSourceCode); // layout is executed as a child of executedTemplate
            Debug.WriteLine(executedTemplate.Result);
            Assert.IsTrue(executedTemplate.Result.Trim().StartsWith("Layout says:"));
        }

        [Test]
        public void SectionWorks() {
            var engine = new RazorMachine()
                .RegisterTemplate("~/Main", @"
@section s1{
This is section 1.
}
This is main content")
                .RegisterTemplate("~/Shared/_layout", "TOP\r\n@RenderBody()\r\n@if(IsSectionDefined(\"s1\")){ @RenderSection(\"s1\");}\r\nEND")
                .RegisterTemplate("~/_viewStart", "@{Layout=\"_layout\";}");
            var t = engine.ExecuteUrl("/Main");
            Assert.IsTrue(t.Result.Trim().StartsWith("TOP"));
            Assert.IsTrue(t.Result.TrimEnd().EndsWith("\r\nEND"));
            Assert.IsTrue(t.Result.Contains("\r\nThis is main content\r\n"));
            Debug.WriteLine(t.Result);

        }


        [Test]
        public void LastSectionHolds() {
            var rm = new RazorMachine();
            rm.RegisterTemplate("~/_ViewStart", @"
@{
   Layout=""_layout"";
}
@section footer{
   Contact me on any crime at 666-666-666
}
");
            rm.RegisterTemplate("~/Shared/_layout", "Layout says: @RenderBody() @RenderSection(\"footer\")");
            rm.RegisterTemplate("~/simpleTemplate", @"
Hello @Model.FirstName @Model.LastName
@section footer{
   corrected: Contact @Model.FirstName on any crime at 777-777-777
}"
                );
            var executedTemplate = rm.ExecuteUrl("~/simpleTemplate", new { FirstName = "Dick", LastName = "Tracy" });
            Debug.WriteLine("Template source code: \r\n" + executedTemplate.GeneratedSourceCode);
            Debug.WriteLine("Layout source code: \r\n" + executedTemplate.Childs[0].GeneratedSourceCode); // layout is executed as a child of executedTemplate
            Debug.WriteLine(executedTemplate.Result);
            Assert.IsTrue(executedTemplate.Result.Trim().StartsWith("Layout says:"));
            Assert.IsTrue(executedTemplate.Result.Contains("corrected"));
        }



        [Test]
        public void Html5IsHandled() {
            var rm = new RazorMachine();
            rm.RegisterTemplate("~/Test.cshtml", @"
@{string myref=""/oops""; ViewBag.Ref=""/oops2"";}
@{<input name=""foo""><p>A different tag</p>}
@{<input name=""foo""/><p>A different tag</p>}
@{<input name=""foo""></input><p>A different tag</p>}
@* The following does not work for the void element input *@
@{<input2 name=""foo""><p>A different tag</p></input2><p>A different tag</p>}
<a href=""/favicon.ico"">oops</a>
<a href=""~/""/>
<a href=""@ViewBag.Ref"">oops</a>
");
            var template = rm.ExecuteUrl("/Test");
            Debug.WriteLine("Result:\r\n" + template.Result);

        }

        [Test]
        public void VBWorksAsWell() {
            var config = @"
<xipton.razor>
   <templates language='VB' defaultExtension='.vbhtml'/>
</xipton.razor>
".Replace("'", "\"");
            var rm = new RazorMachine(config);
            // or...
            //var rm = new RazorMachine(language:new XiptonVBCodeLanguage(),defaultExtension:".vbhtml");

            rm.RegisterTemplate("~/shared/_layout", @"
BEGIN TEMPLATE _layout
@RenderBody()
END TEMPLATE _layout");

            rm.RegisterTemplate("~/_viewStart", @"
BEGIN INCLUDE
@Code
    Layout = ""_layout""
    ViewBag.Title = ""My Title""
End Code
END INCLUDE");

            rm.RegisterTemplate("~/vbTemplate", @"
@ModelType Xipton.Razor.UnitTest.Person
@Imports System.Collections.Concurrent
VB template says: Hello @Model.FirstName @Model.LastName");

            Console.WriteLine(rm.ExecuteUrl("/vbTemplate", new Person { FirstName = "Dick", LastName = "Tracy" }));


        }



    }
}