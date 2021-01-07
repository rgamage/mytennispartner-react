using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace MyTennisPartner.Models.Enums
{
    public enum Frequency
    {
        [Description("Weekly")]
        Weekly,
        [Description("Every other week")]
        BiWeekly,
        [Description("Every third week")]
        TriWeekly,
        [Description("Monthly")]
        Monthly,
        [Description("At random times")]
        AdHoc
    }
}
