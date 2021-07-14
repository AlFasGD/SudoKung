using SudoKung.CellCollections;
using SudoKung.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Convert;

namespace SudoKung.Fields
{
    public class SudokuField
    {
        private SudokuFieldCell[,] cells;
        private SudokuFieldRow[] rows;
        private SudokuFieldColumn[] columns;
        private SudokuFieldBox[,] boxes;
        
        public int KnownValues { get; private set; }
        
        public SudokuFieldCell[,] Cells
        {
            get => cells;
            set
            {
                cells = value;
                for (int i = 0; i < cells.GetLength(0); i++)
                    for (int j = 0; j < cells.GetLength(1); j++)
                    {
                        cells[i, j].ParentField = this;
                        cells[i, j].FieldLocation = (i, j);
                        cells[i, j].ValueChanged += OnValueChanged;
                    }
            }
        }
        
        public int Size { get; set; }
        public (int X, int Y) BoxSize { get; }
        public int BoxWidth => BoxSize.X;
        public int BoxHeight => BoxSize.Y;
        
        public virtual SudokuFieldRow[] Rows => rows;
        public virtual SudokuFieldColumn[] Columns => columns;
        public virtual SudokuFieldBox[,] Boxes => boxes;
        
        public List<string> AcceptableValues { get; private set; }
        
        public SudokuField(int size, (int x, int y) boxSize, SudokuFieldCell[,] cells, List<string> acceptableValues)
        {
            if (size % boxSize.x != 0)
                throw new ArgumentException("The size of the field has to be divisible by the width of the box.");
            if (size % boxSize.y != 0)
                throw new ArgumentException("The size of the field has to be divisible by the height of the box.");
            Size = size;
            BoxSize = boxSize;
            Cells = cells;
            AcceptableValues = acceptableValues;
            foreach (var c in cells)
                KnownValues += ToInt32(c.Value != null);
            GetRows();
            GetColumns();
            GetBoxes();
        }
        
        public SudokuField Clone()
        {
            var clonedCells = new SudokuFieldCell[Size, Size];
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    clonedCells[i, j] = Cells[i, j].Clone();
            return new SudokuField(Size, BoxSize, clonedCells, AcceptableValues);
        }
        
        public void SafeSwap(SudokuFieldCell a, SudokuFieldCell b)
        {
            if (a.ParentField != this || b.ParentField != this)
                throw new Exception("Both cells have to be within this field.");
            var t = a.Value;
            a.SafeValue = b.Value;
            b.SafeValue = t;
        }
        
        public bool Validate()
        {
            if (!ValidateCollection(Rows))
                return false;
            if (!ValidateCollection(Columns))
                return false;
            if (!ValidateCollection(Boxes))
                return false;
            return true;
        }
        public bool ValidateCollection(Array cells)
        {
            foreach (SudokuFieldCellCollection a in cells)
            {
                var values = new List<string>();
                foreach (SudokuFieldCell c in a.Cells)
                {
                    if (values.Contains(c.Value))
                        return false;
                    values.Add(c.Value);
                }
            }
            return true;
        }

        /// <summary>Gets or sets the cell at the specified location.</summary>
        /// <param name="location">The location of the cell to get or set.</param>
        public SudokuFieldCell this[(int X, int Y) location]
        {
            get => Cells[location.X, location.Y];
            internal set => Cells[location.X, location.Y] = value;
        }

        
        private void GetRows()
        {
            rows = new SudokuFieldRow[Size];
            for (int i = 0; i < Size; i++)
            {
                SudokuFieldCell[] cells = new SudokuFieldCell[Size];
                for (int j = 0; j < Size; j++)
                    cells[j] = Cells[i, j];
                rows[i] = new SudokuFieldRow(cells);
                rows[i].ParentField = this;
                rows[i].FieldLocation = i;
            }
        }
        private void GetColumns()
        {
            columns = new SudokuFieldColumn[Size];
            for (int j = 0; j < Size; j++)
            {
                SudokuFieldCell[] cells = new SudokuFieldCell[Size];
                for (int i = 0; i < Size; i++)
                    cells[i] = Cells[i, j];
                columns[j] = new SudokuFieldColumn(cells);
                columns[j].ParentField = this;
                columns[j].FieldLocation = j;
            }
        }
        private void GetBoxes()
        {
            int xBoxes = Size / BoxHeight;
            int yBoxes = Size / BoxWidth;
            boxes = new SudokuFieldBox[xBoxes, yBoxes];
            for (int i = 0, a = 0; i < xBoxes; i++, a += BoxHeight)
                for (int j = 0, b = 0; j < yBoxes; j++, b += BoxWidth)
                {
                    SudokuFieldCell[,] cells = new SudokuFieldCell[BoxHeight, BoxWidth];
                    for (int x = 0; x < BoxHeight; x++)
                        for (int y = 0; y < BoxWidth; y++)
                            cells[x, y] = Cells[a + x, b + y];
                    boxes[i, j] = new SudokuFieldBox(cells);
                    boxes[i, j].ParentField = this;
                    boxes[i, j].FieldLocation = (i, j);
                }
        }
        
        public void OnValueChanged(SudokuFieldCell c, string previousValue, string newValue)
        {
            KnownValues += ToInt32(previousValue == null) - ToInt32(newValue == null);
        }
        
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                    s.Append($"{(cells[i, j].Value ?? "_")} ");
                s.Remove(s.Length - 1, 1);
                s.Append("\n");
            }
            s.Remove(s.Length - 1, 1);
            return s.ToString();
        }
    }
}