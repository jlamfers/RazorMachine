using System;
using System.Collections.Generic;
using Xipton.Razor.Example.Models;

namespace Xipton.Razor.Example {
    class Program {

        // since the with the first examples we do not want to use any file content from the 
        // ./Views folder so we remove all content providers and thus the file content provider as well
        private static RazorMachine _rm = CreateRazorMachineWithoutContentProviders();

        static void Main(string[] args) {

            // Examples from the CodeProject article
            VerifyExample(1);
            Example1();
            VerifyExample(2);
            Example2();
            VerifyExample(3);
            Example3();
            VerifyExample(4);
            Example4();
            VerifyExample(5);
            Example5();
            VerifyExample(6);
            Example6();
            VerifyExample(7);
            Example7();
            VerifyExample(8);
            Example8();
            VerifyExample(9);
            Example9();
            VerifyExample(10);
            Example10();

            VerifyExample("RunEmbeddedTemplate");
            RunEmbeddedTemplate();

            VerifyExample("RunProductListTemplateFromFile");
            RunProductListTemplateFromFile();

            VerifyExample("RunHtmlTemplate");
            RunHtmlTemplate();


            Console.Write("Completed all examples. Press any key to exit...");
            Console.ReadLine();
        }

        private static void VerifyExample(int exampleNr) {
            VerifyExample(exampleNr.ToString());
        }
        private static void VerifyExample(string name) {
            Console.WriteLine("\r\n***** Press [Enter] to run example {0} or [Esc] to exit.\r\n",name);
            if (Console.ReadKey().Key == ConsoleKey.Escape){
                Environment.Exit(0);
            }
            
        }

        private static RazorMachine CreateRazorMachineWithoutContentProviders(bool includeGeneratedSourceCode=false, string rootOperatorPath = null, bool htmlEncode = true) {
            var rm = new RazorMachine(includeGeneratedSourceCode: includeGeneratedSourceCode, htmlEncode: htmlEncode, rootOperatorPath: rootOperatorPath);
            rm.Context.TemplateFactory.ContentManager.ClearAllContentProviders();
            return rm;
        }

        // Example 1 - Executing a template
        public static void Example1() {
            ITemplate template = _rm.ExecuteContent("Razor says: Hello @Model.FirstName @Model.LastName", new { FirstName = "John", LastName = "Smith" });
            Console.WriteLine(template.Result);
        }

        // Example 2 - Executing a template using a layout
        public static void Example2() {
            _rm.RegisterTemplate("~/shared/_layout.cshtml", "BEGIN TEMPLATE \r\n @RenderBody() \r\nEND TEMPLATE");
            ITemplate template = _rm.ExecuteContent("@{Layout=\"_layout\";} Razor says: Hello @Model.FirstName @Model.LastName", new { FirstName = "John", LastName = "Smith" });
            Console.WriteLine(template); // template.ToString() evaluates to template.Result
        }

        //Example 3 - Executing a template using a layout and _viewStart
        public static void Example3() {
            _rm.RegisterTemplate("~/shared/_layout.cshtml", "BEGIN TEMPLATE \r\n @RenderBody() \r\nEND TEMPLATE");
            _rm.RegisterTemplate("~/_viewstart.cshtml", "@{Layout=\"_layout\";}");
            ITemplate template = _rm.ExecuteContent("Razor says: Hello @Model.FirstName @Model.LastName", new { FirstName = "John", LastName = "Smith" });
            Console.WriteLine(template); // same result as example 2
        }

        //Example 4 - Executing a template by a virtual path using a layout and _viewStart
        public static void Example4() {

            _rm.RegisterTemplate("~/shared/_layout.cshtml", "BEGIN TEMPLATE \r\n @RenderBody() \r\nEND TEMPLATE");
            _rm.RegisterTemplate("~/_viewstart.cshtml", "@{Layout=\"_layout\";}");
            _rm.RegisterTemplate("~/simpleTemplate.cshtml", "Razor says: Hello @Model.FirstName @Model.LastName");

            ITemplate template = _rm.ExecuteUrl("~/simpleTemplate.cshtml", new { FirstName = "John", LastName = "Smith" });
            Console.WriteLine(template); // same result as example 2

        }

        //Example 5 - Returning information from anywhere (even from a layout) to the caller using the ViewBag
        public static void Example5() {
            _rm.RegisterTemplate("~/shared/_layout.cshtml", "@{ViewBag.PiValue=3.1415927;}");
            _rm.RegisterTemplate("~/_viewstart.cshtml", "@{Layout=\"_layout\";}");
            ITemplate template = _rm.ExecuteContent("Anything");
            Console.WriteLine(template.ViewBag.PiValue); // => writes 3.1415927
        }

        //Example 6 - Adding information from anywhere to a predefined ViewBag and returning it to the caller
        public static void Example6() {
            _rm.RegisterTemplate("~/shared/_layout.cshtml", "@{ViewBag.Values.Add(3.1415927);}");
            _rm.RegisterTemplate("~/_viewstart.cshtml", "@{Layout=\"_layout\";}");
            ITemplate template = _rm.ExecuteContent("Anything", viewbag: new { Values = new List<double> { 0, 1, 2 } });
            Console.WriteLine(template.ViewBag.Values[3]); // => writes 3.1415927
        }

        //Example 7 - Show generated code
        public static void Example7() {
            var rm = CreateRazorMachineWithoutContentProviders(includeGeneratedSourceCode: true);
            rm.RegisterTemplate("~/shared/_layout.cshtml", "BEGIN TEMPLATE \r\n @RenderBody() \r\nEND TEMPLATE");
            rm.RegisterTemplate("~/_viewstart.cshtml", "@{Layout=\"_layout\";}");
            ITemplate template = rm.ExecuteContent("Razor says: Hello @Model.FirstName @Model.LastName", new { FirstName = "John", LastName = "Smith" });
            Console.WriteLine(template); // writes output result
            Console.WriteLine(template.GeneratedSourceCode); // writes generated source for template
            Console.WriteLine(template.Childs[0].GeneratedSourceCode); // writes generated source for layout
        }

        //Example 8 - HTML encoding
        public static void Example8() {
            _rm.RegisterTemplate("~/shared/_layout.cshtml", "@RenderBody()"); // replace _layout by "RenderBody only" template to assure output

            // not encoded since all output is literal content
            Console.WriteLine(_rm.ExecuteContent("Tom & Jerry").Result);

            // encoded since the content is written as a string value
            // and by default HtmlEncode is on for written content
            Console.WriteLine(_rm.ExecuteContent("@Model.Text", new { Text = "Tom & Jerry" }).Result);

            // not encoded since content is a written as a raw string
            Console.WriteLine(_rm.ExecuteContent("@Raw(Model.Text)", new { Text = "Tom & Jerry" }).Result);

            // not encoded since HtmlEncoding is turend off in code
            Console.WriteLine(_rm.ExecuteContent("@{HtmlEncode=false;} @Model.Text", new { Text = "Tom & Jerry" }).Result);

            var rm = CreateRazorMachineWithoutContentProviders(htmlEncode: false);
            rm.Context.TemplateFactory.ContentManager.ClearAllContentProviders();
            // not encoded since now html encoding if off by default, still you can set it on in code
            Console.WriteLine(rm.ExecuteContent("@Model.Text", new { Text = "Tom & Jerry" }).Result);
        }

        //Example 9 - Root operator is resolved directly
        public static void Example9() {
            var rm = CreateRazorMachineWithoutContentProviders(rootOperatorPath: "/MyAppName");
            rm.RegisterTemplate("~/MyTemplate", "<a href='~/SomeLink'>Some Link</a>");
            var template = rm.ExecuteUrl("/MyAppName/MyTemplate");
            // same result as:
            template = rm.ExecuteUrl("~/MyTemplate");
            Console.WriteLine(template); // writes: <a href='/MyAppName/SomeLink'>Some Link</a>

        }

        //Example 10 - Razor 2: Attributes with null values are not rendered
        public static void Example10() {
            var template = _rm.ExecuteContent("<a href='~/SomeLink' data-brand='@Model.Brand' data-not-rendered='@Model.NullValue'>Some Link</a>", new { Brand = "Toyota", NullValue = (string)null });
            Console.WriteLine(template); // writes: <a href='/SomeLink' data-brand='Toyota'>Some Link</a>
        }


        // ********************* Other examples ********************************************************************************* //

        public static void RunEmbeddedTemplate() {
            // this example only works if the embedded content provider has been configured. Here it is configured at app.config, includeGeneratedSourceCode is forced to true
            var rm = new RazorMachine(includeGeneratedSourceCode:true); // => default configuration from app.config is loaded
            var t = rm.ExecuteUrl("~/embeddedTemplate");
            Console.WriteLine("Generated source code:");
            Console.WriteLine(t.GeneratedSourceCode);
            Console.WriteLine("Generated output:");
            Console.WriteLine(t); // => writes the embedded template's result
            //NOTE: since now all content providers (file content providers as well) are loaded the ~/ViewStart.cshtml from the ./Views folder is executed as well 
        }

        /// <summary>
        /// This example creates a product list using a simple product grid productGrid (at ./Views/Reports/).
        /// The grid implicitly "knows" the product model because the grid is a child of Products, which is executed together with the MyProductList().
        /// The layout is _layout2 which is set in memory at "~/_ViewStart", thus override the file ./Views/_ViewStart.cshtml
        /// </summary>
        public static void RunProductListTemplateFromFile() {
            var rm = new RazorMachine(includeGeneratedSourceCode: true); // => default configuration from app.config is loaded, includeGeneratedSourceCode is forced to true
            rm.RegisterTemplate("~/_ViewStart", "@{Layout=\"_Layout2\";}"); // => change (override) layout in memory to _layout2

            var t = rm.ExecuteUrl("~/Reports/Products", new MyProductList());

            Console.WriteLine("Generated template source code:");
            Console.WriteLine(t.GeneratedSourceCode);

            Console.WriteLine("Generated layout source code:");
            Console.WriteLine(t.Childs[t.Childs.Count - 1].GeneratedSourceCode); // layout always is the last child bacause it is rendered at last

            Console.WriteLine("Generated grid source code:");
            Console.WriteLine(t.Childs[0].GeneratedSourceCode);

            Console.WriteLine("Generated output:");
            Console.WriteLine(t); // => writes the embedded template's result
        }

        public static void RunHtmlTemplate(){
            Console.WriteLine(new RazorMachine(includeGeneratedSourceCode: true).ExecuteUrl("/Other/Html",new{FirstName="Dick",LastName="Tracy"}));
        }



    }
}
