## RazorMachine ###

    var rm = new RazorMachine();
    var result = 
       rm.Execute("Hello @Model.FirstName @Model.LastName", new {FirstName="John", LastName="Smith"});
    Console.WriteLine(result);
    
RazorMachine is a robust and easy to use .Net Razor 2 template engine. This implementation supports layouts (masterpages) and a _viewStart construct, just like MVC does support these features. The RazorEngine works independently from MVC. It only needs the System.Web.Razor reference. It almost works exacly like Asp.Net MVC. Take a look at the <a href="https://github.com/jlamfers/RazorMachine/wiki/Examples" target="_blank">examples at the wiki</a> to see how easy this framework works.

This RazorEngine originally was published at the <a href="http://www.codeproject.com/Articles/423141/Razor-2-0-template-engine-supporting-layouts" target="_blank">CodeProject</a>

## Install ##

There is a package available at NuGet. To install RazorMachine using NuGet, run the following command in the <a href="http://docs.nuget.org/docs/start-here/using-the-package-manager-console" target="_blank">Package Manager Console</a>
```
PM> Install-Package RazorMachine
```
