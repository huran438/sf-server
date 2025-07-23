using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFServer.UI
{
    [HtmlTargetElement("enum-dropdown", Attributes = "asp-for, enum-type")]
    public class EnumDropdownTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression AspFor { get; set; }

        [HtmlAttributeName("enum-type")]
        public Type EnumType { get; set; }

        [HtmlAttributeName("exclude")]
        public string[] Exclude { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!EnumType.IsEnum)
            {
                throw new ArgumentException($"{EnumType} is not an enum type.");
            }

            var selectedValue = AspFor.Model?.ToString();

            var options = Enum.GetValues(EnumType)
                .Cast<Enum>()
                .Where(e => Exclude == null || !Exclude.Contains(e.ToString()))
                .Select(e => new SelectListItem
                {
                    Text = e.GetType()
                        .GetMember(e.ToString())
                        .First()
                        .GetCustomAttribute<DescriptionAttribute>()?.Description ?? e.ToString(),
                    Value = e.ToString(),
                    Selected = e.ToString() == selectedValue
                })
                .ToList();

            var selectTag = new TagBuilder("select");
            selectTag.Attributes["name"] = AspFor.Name;

            // Preserve additional attributes like class or id from the original tag
            foreach (var attr in output.Attributes)
            {
                if (attr.Name is "asp-for" or "enum-type" or "exclude")
                    continue;
                if (attr.Name == "class")
                {
                    selectTag.AddCssClass(attr.Value.ToString());
                }
                else
                {
                    selectTag.Attributes[attr.Name] = attr.Value.ToString();
                }
            }

            selectTag.AddCssClass("form-control");

            foreach (var item in options)
            {
                var option = new TagBuilder("option");
                option.Attributes["value"] = item.Value;
                if (item.Selected)
                    option.Attributes["selected"] = "selected";
                option.InnerHtml.Append(item.Text);
                selectTag.InnerHtml.AppendHtml(option);
            }

            output.TagName = null; // don't render <enum-dropdown>, only the select
            output.Content.SetHtmlContent(selectTag);
        }
    }
}
