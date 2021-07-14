using SudoKung.CellCollections;
using SudoKung.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudoKung.Objects
{
    public class SudokuFieldBoxTuple<T>
        where T : I1DSudokuFieldCellCollection
    {
        public List<SudokuFieldBox> Boxes;
        public T CellCollection;
        public string Value;

        public SudokuFieldBoxTuple(List<SudokuFieldBox> boxes, T cellCollection, string value)
        {
            Boxes = boxes;
            CellCollection = cellCollection;
            Value = value;
        }
    }
}
