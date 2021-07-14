using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SudoKung.Objects;

namespace SudoKung.Fields
{
    public class SudokuField25x25 : SudokuField
    {
        public static new List<string> AcceptableValues => new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        
        public SudokuField25x25(SudokuFieldCell[,] cells)
            : base(25, (5, 5), cells, AcceptableValues)
        {
            
        }
    }
}