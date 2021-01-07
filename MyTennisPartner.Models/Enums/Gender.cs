using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MyTennisPartner.Models.Enums
{
    public enum Gender
    {
        [Description("Male")]
        Male = 1,
        [Description("Female")]
        Female = 2,
        [Description("Uknown")]
        Unknown = 3
    }
}
