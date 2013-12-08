#region  Microsoft Public License
/* This code is part of Xipton.Razor v3.0
 * (c) Jaap Lamfers, 2013 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

namespace Xipton.Razor.Core
{

    public interface ITemplateController : ITemplate
    {
        ITemplateController SetModel(object model);
        ITemplateController SetViewBag(object viewBag);
        ITemplateController ApplyLayout(ITemplateController layoutTemplate);
        ITemplateController SetVirtualPath(string virualPath);
        ITemplateController SetContext(RazorContext context);
        ITemplateController SetParent(ITemplateController parent);
        ITemplateController AddChild(ITemplateController child);
        ITemplateController Execute();
        ITemplateController TryApplyLayout();
        ITemplateController SetGeneratedSourceCode(string generatedSourceCode);
        string RenderSectionByChildRequest(string sectionName);
    }

}