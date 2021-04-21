using System;
using System.Collections.Generic;
using System.Linq;

namespace Emo.Common.Utils {
    public static class EnumUtil {
        /// <summary>
        /// Allow iterating over all values in an enum
        /// </summary>
        /// <typeparam name="T">The enumeration to change to an enumration</typeparam>
        /// <returns>An enumrable of all values in the passed in enum</returns>
        /// <remarks>borrowed from here: https://stackoverflow.com/questions/972307/how-to-loop-through-all-enum-values-in-c </remarks>
        public static IEnumerable<T> GetValues<T>() {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
