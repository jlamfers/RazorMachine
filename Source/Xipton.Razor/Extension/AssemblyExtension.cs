#region  Microsoft Public License
/* This code is part of Xipton.Razor v3.0
 * (c) Jaap Lamfers, 2013 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

using System;
using System.IO;
using System.Reflection;

namespace Xipton.Razor.Extension
{
    public static class AssemblyExtension
    {
        public static string GetFileName(this Assembly assembly)
        {

#if __MonoCS__
            return assembly == null 
                ? null
                : new DirectoryInfo(IsNotWindows ? assembly.CodeBase.Replace("file:///", "/") : assembly.CodeBase.Replace("file:///", string.Empty)).FullName;
#else
            if (assembly == null) return null;
            var path = assembly.CodeBase.Replace("file:///", string.Empty);
            path = path.Replace("file://", "//"); // Network drive locations (e.g. \\NetworkLocation\App) have a slightly different format which we will account for here
            return new DirectoryInfo(path).FullName;
#endif

        }

        public static bool IsNotWindows
        {
            get
            {
                var p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        //http://msdn.microsoft.com/en-us/library/ms173100.aspx
        public static bool IsManagedAssembly(this string self) {

            if (self == null || !File.Exists(self)) {
                return false;
            }

            try{
                AssemblyName.GetAssemblyName(self);
                return true;
            }
            catch (BadImageFormatException){
                return false;
            }
            catch (FileLoadException){
                return true;
            }
            catch{
                return false;
            }

        }

    }

}
