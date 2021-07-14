using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SudoKung.Fields;

namespace SudoKung.Tests
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var types = Assembly.GetAssembly(typeof(Program)).DefinedTypes.Where(t => t.IsSubclassOf(typeof(TestCase)));
            foreach (var t in types)
                t.DeclaredConstructors.First().Invoke(null);

            Console.ReadKey();
        }
    }
}
