using SudoKung.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudoKung.CellCollections
{
    public class SudokuFieldColumn : SudokuFieldCellCollection, I1DSudokuFieldCellCollection
    {
        public int FieldLocation = -1;

        protected override string ContainerName => "column";

        public SudokuFieldColumn(SudokuFieldCell[] cells) : base(cells) { }

        protected override void LinkParentContainer(SudokuFieldCell cell) => cell.ParentColumn = this;
    }
}
