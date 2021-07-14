//+src=SudokuField9x9.cs
//+src=SudokuSolver.cs
//+src=SudokuField.cs
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SudoKung.Fields;
using SudoKung.Solvers;
using static System.Convert;
using SudoKung.Objects;

namespace SudoKung.Parsers
{
    public static class Program
    {
        public static string path = "";
        
        public static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Enter the path of the file containing the Sudoku puzzle (.sud)");
            Console.ForegroundColor = ConsoleColor.Cyan;
            while (!path.EndsWith(".sud"))
                path = Console.ReadLine();
            SudokuField f = ParseSudokuFile(path);
            SudokuSolver solver = new SudokuSolver(f);
            Timer t = new Timer(o => WriteLog(solver, path, ""), null, 0, 100);
            try
            {
                solver.Solve();
                t.Dispose();
                string solvedPath = path.Substring(0, path.Length - 4) + " - Solved.sud";
                WriteSudokuFile(solver.Field, solvedPath);
                WriteLog(solver, path, "");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nThe Sudoku has been solved and the result was saved to {solvedPath}.");
            }
            catch (Exception e)
            {
                t.Dispose();
                WriteLog(solver, path, $"\n\nException message:\n{e.Message}\n\nException trace:\n{e.StackTrace}\n\n{e.Data}\n\n{e.Source}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nThe solver encountered an error while solving the Sudoku and the log was saved to the file's local directory.");
            }
        }
        public static void WriteLog(SudokuSolver solver, string path, string extraContent)
        {
            string logPath = path.Substring(0, path.Length - 4) + " - Solution Log.log";
            string[] log = new string[solver.Log.Count + 2];
            int line = 0;
            for (; line < solver.Log.Count; line++)
                log[line] = solver.Log[line];
            log[line] = $"Field's last state:\n{solver.Field.ToString()}{extraContent}";
            File.WriteAllLines(logPath, log);
        }
        
        public static SudokuField ParseSudokuFile(string path)
        {
            SudokuFieldCell[,] cells = new SudokuFieldCell[0, 0];
            int size = 0;
            (int, int) boxSize = (0, 0);
            List<string> acceptableValues = new List<string>();
            
            /*
            
            The form of the file is the following:
            
            AcceptableValues: 1, 2, 3, 4, 5, 6, 7, 8, 9, A, B, C, D, E, F
            Size: 16
            BoxSize: 4x4
            Cells:
            1 3 _ 4 2 _ _ 6 C _ E _ F 9 _ _
            _ _ 7 _ 3 1 5 2 _ _ _ B _ _ D A
            ...
            
            Note 1: The underscore signifies an empty cell, thus cannot be used as an acceptable cell value.
            Note 2: The cells token has to be the final token in the file. Any other tokens past that will be ignored.
            Note 3: For the following known field sizes, AcceptableValues is unnecessary.
            Known field sizes: 4, 6, 9, 12, 16, 20, 25
            // This will be implemented at later stages of the library's development
            
            */
            
            string[] lines = File.ReadAllLines(path);
            bool foundCellsToken = false;
            for (int i = 0; i < lines.Length && !foundCellsToken; i++)
            {
                string[] token = lines[i].Split(':');
                string key = token[0].RemoveEdgeSpaces();
                if (!(foundCellsToken = key == "Cells"))
                {
                    string value = token[1].RemoveEdgeSpaces();
                    switch (key)
                    {
                        case "AcceptableValues":
                        {
                            acceptableValues = new List<string>();
                            string[] values = value.Split(',');
                            foreach (var v in values)
                                acceptableValues.Add(v.RemoveEdgeSpaces());
                            break;
                        }
                        case "Size":
                        {
                            size = ToInt32(value);
                            break;
                        }
                        case "BoxSize":
                        {
                            string[] b = value.Split('x');
                            boxSize = (ToInt32(b[0]), ToInt32(b[1]));
                            break;
                        }
                        default:
                            throw new ArgumentException($"Invalid key \"{key}\" in file.");
                    }
                }
                else
                {
                    i++;
                    cells = new SudokuFieldCell[size, size];
                    for (int x = 0; x < size; x++)
                    {
                        string[] c = lines[i + x].Split(' ');
                        for (int y = 0; y < size; y++)
                            cells[x, y] = new SudokuFieldCell(c[y] == "_" ? null : c[y]);
                    }
                }
            }
            if (size == 9)
                return new SudokuField9x9(cells);
            return new SudokuField(size, boxSize, cells, acceptableValues);
        }
        public static void WriteSudokuFile(SudokuField field, string path) => File.WriteAllText(path, field.ToString());
        
        public static string RemoveEdgeSpaces(this string s)
        {
            int start = 0;
            int end = s.Length;
            while (s[start] == ' ')
                start++;
            while (s[end - 1] == ' ')
                end--;
            return s.Substring(start, end - start);
        }
    }
}