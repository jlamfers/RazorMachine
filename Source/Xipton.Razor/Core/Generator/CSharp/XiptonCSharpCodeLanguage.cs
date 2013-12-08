#region  Microsoft Public License
/* This code is part of Xipton.Razor v3.0
 * (c) Jaap Lamfers, 2013 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

using System.Web.Razor;
using System.Web.Razor.Parser;

namespace Xipton.Razor.Core.Generator.CSharp
{
    public class XiptonCSharpCodeLanguage : CSharpRazorCodeLanguage, IXiptonCodeLanguage
    {
        public override ParserBase CreateCodeParser()
        {
            return new XiptonCSharpCodeParser();
        }
    }
}