﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.Enums
{
    public static class StartTimes
    {
        /// <summary>
        /// return list of valid start times for matches/leagues
        /// </summary>
        /// <returns></returns>
        public static List<string> GetStartTimes()
        {
            return new List<string>
            {
            "06:00",
            "06:30",
            "07:00",
            "07:30",
            "08:00",
            "08:30",
            "09:00",
            "09:30",
            "10:00",
            "10:30",
            "11:00",
            "11:30",
            "12:00",
            "12:30",
            "13:00",
            "13:30",
            "14:00",
            "14:30",
            "15:00",
            "15:30",
            "16:00",
            "16:30",
            "17:00",
            "17:30",
            "18:00",
            "18:30",
            "19:00",
            "19:30",
            "20:00",
            "20:30",
            "21:00",
            "21:30",
            "22:00",
            "22:30",
            "23:00"
            };
        }
    }
}
