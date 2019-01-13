using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Entity
{
    public class ProjectStyle
    {
        public string _class { get; set; }
        public string name { get; set; }
        public LastBuild lastbuild { get; set; }
    }
}