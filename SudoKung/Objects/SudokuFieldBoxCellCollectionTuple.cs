using SudoKung.CellCollections;
using SudoKung.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudoKung.Objects
{
    public class SudokuFieldBoxCellCollectionTuple<T>
        where T : I1DSudokuFieldCellCollection
    {
        public SudokuFieldBox Box;
        public List<T> CellCollections;
        public string Value;

        public SudokuFieldBoxCellCollectionTuple(SudokuFieldBox box, List<T> cellCollections, string value)
        {
            Box = box;
            CellCollections = cellCollections;
            Value = value;
        }
    }
}
