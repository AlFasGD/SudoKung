using SudoKung.Fields;
using SudoKung.Objects;
using System;
using System.Collections.Generic;
using System.Net;

namespace SudoKung.Tests.Sudoku25x25Scrapper
{
    public class WebsiteScrapper
    {
        private Sudoku25x25Database database = new Sudoku25x25Database();

        public const string URL = "https://www.sudoku-puzzles-online.com/alphadoku/play-25x25-sudoku.php";

        public WebsiteScrapper()
        {

        }

        public SudokuField25x25 GetPuzzle(Difficulty d, Symmetry s, int index = -1)
        {
            if (index < 0)
                index = new Random().Next(1, 10001);
            string myParameters = $"niv={(int)d}&sym={(int)s}&num={index}";

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string result = client.UploadString(URL, myParameters);

                var cells = new SudokuFieldCell[25, 25];

                int startIndex = result.IndexOf("<input name=\"eso\" type=\"hidden\" value=\"") + 1;
                for (int i = 0; i < 25 * 25; i++)
                {
                    char c = result[startIndex + i];
                    int value = c == '0' ? 0 : c - 'A' + 1;
                    cells[i / 25, i % 25] = new SudokuFieldCell(value.ToString()); // please remove that string thing
                }

                var generated = new SudokuField25x25(cells);

                database[d, s, index] = generated;
                return generated;
            }
        }

        public enum Difficulty : byte
        {
            Beginner = 1,
            Contained = 2,
            Expert = 3,
        }

        public enum Symmetry : byte
        {
            Asymmetric = 1,
            Symmetric = 2,
        }

        public class Sudoku25x25Database
        {
            private Dictionary<FieldKey, SudokuField25x25> fields = new Dictionary<FieldKey, SudokuField25x25>();

            public SudokuField25x25 this[Difficulty d, Symmetry s, int index]
            {
                get => fields[(d, s, index)];
                set
                {
                    FieldKey key = (d, s, index);
                    if (fields.ContainsKey(key))
                        fields[key] = value;
                    else
                        fields.Add(key, value);
                }
            }
        }
        public struct FieldKey
        {
            public Difficulty Difficulty;
            public Symmetry Symmetry;
            public int Index;

            public FieldKey(Difficulty d, Symmetry s, int index)
            {
                Difficulty = d;
                Symmetry = s;
                Index = index;
            }

            public static implicit operator FieldKey((Difficulty Difficulty, Symmetry Symmetry, int Index) t) => new FieldKey(t.Difficulty, t.Symmetry, t.Index);
        }
    }
}
