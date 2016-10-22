using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Common.Util
{
    public static class EqualsExtensions
    {
        public static bool EqualsSequence<T>(this IRapidList<T> list1, IRapidList<T> list2)
        {
            if (list1 == list2)
                return true;

            if (list1 == null || list2 == null)
                return false;

            var len1 = list1.Count;
            var len2 = list2.Count;

            if (len1 != len2)
                return false;

            for (var i = 0; i < len1; ++i)
            {
                var elem1 = list1[i];
                var elem2 = list2[i];

                if (elem1 == elem2)
                    continue;
                
                if (elem1 == null || elem2 == null)
                    return false;

                if (!elem1.Equals(elem2))
                    return false;
            }

            return true;
        }
    }
}
