using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudoKung.Extensions
{
    public static class ListExtensions
    {
        public static bool EqualsUnordered<T>(this List<T> l, List<T> a)
        {
            if (l.Count != a.Count)
                return false;
            var t = new List<T>();
            foreach (var c in a)
                t.Add(c);
            foreach (var c in l)
                if (t.Contains(c))
                    t.Remove(c);
                else
                    return false;
            return true;
        }
    }
}
