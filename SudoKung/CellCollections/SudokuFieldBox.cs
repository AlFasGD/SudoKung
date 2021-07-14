using SudoKung.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudoKung.CellCollections
{
    public class SudokuFieldBox : SudokuFieldCellCollection, I2DSudokuFieldCellCollection
    {
        public (int X, int Y) FieldLocation = (-1, -1);

        protected override string ContainerName => "box";

        public SudokuFieldBox(SudokuFieldCell[,] cells) : base(cells) { }

        protected override void LinkParentContainer(SudokuFieldCell cell) => cell.ParentBox = this;
    }
}
