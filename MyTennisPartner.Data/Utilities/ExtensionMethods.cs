using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MyTennisPartner.Data.Utilities
{
    public static class ExtensionMethods
    {
        public static string DescriptionAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }


        /// <summary>
        /// true if play format is doubles
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool IsDoubles(this PlayFormat format)
        {
            var result = format == PlayFormat.MensDoubles || format == PlayFormat.WomensDoubles || format == PlayFormat.MixedDoubles;
            return result;
        }
    }
}
