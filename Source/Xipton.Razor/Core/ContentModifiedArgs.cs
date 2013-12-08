#region  Microsoft Public License
/* This code is part of Xipton.Razor v3.0
 * (c) Jaap Lamfers, 2013 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

using System;

namespace Xipton.Razor.Core
{
    public class ContentModifiedArgs : EventArgs
    {
        public ContentModifiedArgs(string modifiedResourceName)
        {
            if (modifiedResourceName == null) throw new ArgumentNullException("modifiedResourceName");
            ModifiedResourceName = modifiedResourceName;
        }

        public string ModifiedResourceName { get; private set; }
    }
}