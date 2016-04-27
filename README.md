## RazorMachine ###
```csharp
var rm = new RazorMachine();
var result = 
   rm.Execute("Hello @Model.FirstName @Model.LastName", new {FirstName="John", LastName="Smith"});
Console.WriteLine(result);
```
    
RazorMachine is a robust and easy to use .Net Razor v2/v3 template engine. The master branch uses Razor v3. This implementation supports layouts (masterpages) and a _viewStart construct, just like MVC does support these features. The RazorEngine works independently from MVC. It only needs the System.Web.Razor reference. It almost works exacly like Asp.Net MVC. Take a look at https://github.com/jlamfers/RazorMachine/wiki/Examples to see how easily this framework works.

This RazorEngine originally was published at <a href="http://www.codeproject.com/Articles/423141/Razor-2-0-template-engine-supporting-layouts" target="_blank">CodeProject</a>

## Install ##

There is a package available at <a href="https://www.nuget.org/packages/RazorMachine/" target="_blank">NuGet</a>. To install RazorMachine using NuGet, run the following command in the <a href="http://docs.nuget.org/docs/start-here/using-the-package-manager-console" target="_blank">Package Manager Console</a>
```
PM> Install-Package RazorMachine
```
