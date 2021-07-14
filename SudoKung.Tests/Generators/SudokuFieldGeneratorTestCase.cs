using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SudoKung.Generators;

namespace SudoKung.Tests.Generators
{
    internal class SudokuFieldGeneratorTestCase : TestCase
    {
        public SudokuFieldGeneratorTestCase()
        {
            SudokuFieldGenerator g;
            var f = new SudokuFieldGenerator(9, (3, 3), new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" });
            
            f.GenerateInitialField();
            File.WriteAllText("initialTest.sud", f.Field.ToString());
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Initial field generated.");
            Console.ForegroundColor = f.Field.Validate() ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("Validation result");
            
            f.Generate();
            File.WriteAllText("generationTest.sud", f.Field.ToString());
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nField generated.");
            Console.ForegroundColor = f.Field.Validate() ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("Validation result");

            // TODO: Uncomment once the validation passes at least once
            /*
            const int GenerationTests = 100;
            for (int i = 0; i < GenerationTests; i++)
            {
                (g = new SudokuFieldGenerator(9, (3, 3), new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" })).Generate();
                Console.ForegroundColor = g.Field.Validate() ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine("Validation result");
            }
            */
        }
    }
}