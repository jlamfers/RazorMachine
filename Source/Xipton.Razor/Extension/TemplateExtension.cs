#region  Microsoft Public License
/* This code is part of Xipton.Razor v3.0
 * (c) Jaap Lamfers, 2013 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

using System;

namespace Xipton.Razor.Extension
{
    //pre compile view extension
    public static class RazorMachineExtension
    {
        public static void EnsureViewCompiled(this RazorMachine self, string path, bool throwException = true)
        {
            if (self == null) throw new ArgumentNullException("self");
            self.Context.TemplateFactory.CreateTemplateInstance(path, throwException);
        }
    }
}
