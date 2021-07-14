using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudoKung.Objects
{
    public class SudokuFieldCommonCellTuple
    {
        public List<SudokuFieldCell> Cells;
        public List<string> Values;

        public SudokuFieldCommonCellTuple(List<SudokuFieldCell> cells, List<string> values)
        {
            if (cells.Count != values.Count)
                throw new ArgumentException("The list of cells and the list of values have to have the same length.");
            Cells = cells;
            Values = values;
        }
    }
}
