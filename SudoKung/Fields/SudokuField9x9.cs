using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SudoKung.Objects;

namespace SudoKung.Fields
{
    public class SudokuField9x9 : SudokuField
    {
        public static new List<string> AcceptableValues => new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        
        public SudokuField9x9(SudokuFieldCell[,] cells)
            : base(9, (3, 3), cells, AcceptableValues)
        {
            
        }
    }
}