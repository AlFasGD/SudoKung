using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudoKung.Objects
{
    public class SudokuFieldCellTuple
    {
        public List<SudokuFieldCell> Cells;
        public string Value;

        public SudokuFieldCellTuple(List<SudokuFieldCell> cells, string value)
        {
            Cells = cells;
            Value = value;
        }
    }
}
