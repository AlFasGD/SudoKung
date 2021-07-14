using SudoKung.CellCollections;
using SudoKung.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudoKung.Objects
{
    public class SudokuFieldCommonBoxTuple<T>
        where T : I1DSudokuFieldCellCollection
    {
        public List<SudokuFieldBox> Boxes;
        public List<T> CellCollections;
        public string Value;

        public SudokuFieldCommonBoxTuple(List<SudokuFieldBox> boxes, List<T> cellCollections, string value)
        {
            if (boxes.Count != cellCollections.Count)
                throw new ArgumentException("The list of boxes and the list of cell collections have to have the same length.");
            Boxes = boxes;
            CellCollections = cellCollections;
            Value = value;
        }
    }
}
