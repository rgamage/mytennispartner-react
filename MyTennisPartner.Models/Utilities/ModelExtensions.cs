using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.ComponentModel;
using System.Linq;
using MyTennisPartner.Models.ViewModels;
using System.Collections;
using MyTennisPartner.Models.Enums;
using System.Reflection;

namespace MyTennisPartner.Models.Utilities
{
    public static class ModelExtensions
    {

        public static string GetDescription<T>(this T value) where T : IConvertible
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }

        //public static string GetDescription<T>(this T e) where T : IConvertible
        //{
        //    if (e is Enum)
        //    {
        //        Type type = e.GetType();
        //        Array values = Enum.GetValues(type);

        //        foreach (int val in values)
        //        {
        //            if (val == e.ToInt32(CultureInfo.InvariantCulture))
        //            {
        //                var memInfo = type.GetMember(type.GetEnumName(val));

        //                if (memInfo[0]
        //                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
        //                    .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
        //                {
        //                    return descriptionAttribute.Description;
        //                }
        //            }
        //        }
        //    }
        //    return e.ToString();
        //    //return string.Empty;
        //}

        /// <summary>
        /// given an enum type, return all description attributes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<string> GetAllDescriptions<T>(this T e) where T : IConvertible
        {
            var result = new List<string>();
            if (e is Enum)
            {
                foreach (T value in (T[])Enum.GetValues(typeof(T)))
                {
                    result.Add(value.GetDescription());
                }
            }
            result.Sort();
            return result;
        }

        /// <summary>
        /// given a play format, return gender, to use for searching for compatible players, given a league/match format
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Gender GenderFromFormat(this PlayFormat format)
        {
            switch (format)
            {
                case PlayFormat.MensDoubles:
                case PlayFormat.MensSingles:
                    return Gender.Male;
                case PlayFormat.WomensDoubles:
                case PlayFormat.WomensSingles:
                    return Gender.Female;
                default:
                    return Gender.Unknown;
            }
        }
    }
}

