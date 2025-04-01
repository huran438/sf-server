using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SFServer.UI.Models
{
    public class EnumDropdownViewModel
    {
        public string Name { get; set; }
        public Enum Selected { get; set; }
        public Type EnumType { get; set; }
        public List<Enum> Exclude { get; set; }
    }
}