using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MyTennisPartner.Models.Enums
{
    /// <summary>
    /// format of play
    /// </summary>
    public enum PlayFormat
    {
        [Description("Men's Singles")]
        MensSingles,

        [Description("Women's Singles")]
        WomensSingles,

        [Description("Men's Doubles")]
        MensDoubles,

        [Description("Women's Doubles")]
        WomensDoubles,

        [Description("Mixed Doubles")]
        MixedDoubles,

        [Description("Singles Practice")]
        SinglesPractice,

        [Description("Private Lesson")]
        PrivateLesson,

        [Description("Group Lesson/Clinic")]
        GroupLesson
    }
}
