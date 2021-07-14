using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SudoKung.Fields;
using SudoKung.Objects;
using SudoKung.CellCollections;

namespace SudoKung.Generators
{
    public class SudokuFieldGenerator
    {
        /// <summary>The threshold for the wrong cells to start performing the circular swapping on the field generator correction.</summary>
        private int WrongCellThreshold => Field.Size;

        public SudokuField Field;
        
        public SudokuFieldGenerator(int size, (int, int) boxSize, List<string> acceptableValues)
        {
            var cells = new SudokuFieldCell[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    cells[i, j] = new SudokuFieldCell();
            Field = new SudokuField(size, boxSize, cells, acceptableValues);
        }
        
        public virtual void GenerateInitialField()
        {
            for (int a = 0; a < Field.AcceptableValues.Count; a++)
                for (int i = 0; i < Field.BoxHeight; i++)
                    for (int j = 0; j < Field.BoxWidth; j++)
                    {
                        int m = (i * Field.BoxHeight + j + a) % Field.BoxHeight;
                        int n = (a / Field.BoxHeight + i) % 3;
                        Field.Cells[i * 3 + m, j * 3 + n].SafeValue = Field.AcceptableValues[a];
                    }
        }
        public virtual void Generate()
        {
            Random r = new Random();
            GenerateInitialField();
            for (int a = 0; a < Field.Size; a++) // Shuffle a lot
                for (int i = 0; i < Field.Size; i++)
                    for (int j = 0; j < Field.Size; j++)
                    {
                        int m = r.Next(i, Field.Size);
                        int n = r.Next(j, Field.Size);
                        Field.SafeSwap(Field.Cells[i, j], Field.Cells[m, n]);
                    }
            Generate(Field);
        }
        protected virtual bool Generate(SudokuField field)
        {
            var wrong = new List<SudokuFieldCell>();
            var rowValues = new List<string>();
            var columnValues = new List<string>();
            var boxValues = new List<string>();

            void EvaluateWrongCells(SudokuFieldCellCollection collection, ref List<string> values)
            {
                foreach (SudokuFieldCell c in collection.Cells)
                {
                    if (wrong.Contains(c))
                        continue;
                    if (values.Contains(c.Value))
                        wrong.Add(c);
                    values.Add(c.Value);
                }
            }

            for (int i = 0; i < field.Size; i++)
            {
                var a = field.Rows[i];
                var e = field.Columns[i];
                var o = field.Boxes[i % 3, i / 3];
                EvaluateWrongCells(a, ref rowValues);
                EvaluateWrongCells(e, ref columnValues);
                EvaluateWrongCells(o, ref boxValues);
                rowValues.Clear();
                columnValues.Clear();
                boxValues.Clear();
            }
            Console.WriteLine($"Diededed {wrong.Count}");
            SudokuFieldCell x, y;
            for (int i = 0; i < wrong.Count; i++)
            {
                x = wrong[i];
                for (int j = i + 1; j < wrong.Count; j++)
                {
                    y = wrong[j];
                    if (x.Value == y.Value)
                        continue;
                    bool xRow = x.ParentRow.ContainsValueExcept(y.Value, x, y);
                    bool xColumn = x.ParentColumn.ContainsValueExcept(y.Value, x, y);
                    bool xBox = x.ParentBox.ContainsValueExcept(y.Value, x, y);
                    bool yRow = y.ParentRow.ContainsValueExcept(x.Value, x, y);
                    bool yColumn = y.ParentColumn.ContainsValueExcept(x.Value, x, y);
                    bool yBox = y.ParentBox.ContainsValueExcept(x.Value, x, y);
                    bool xInvalid = xRow || xColumn || xBox;
                    bool yInvalid = yRow || yColumn || yBox;
                    if (xInvalid && yInvalid)
                        continue;
                    var a = field[x.FieldLocation];
                    var b = field[y.FieldLocation];
                    field.SafeSwap(a, b);
                    var f = field.Clone();
                    if (Generate(f))
                    {
                        Field = f;
                        return true;
                    }
                }
            }
            if (wrong.Count <= WrongCellThreshold)
                for (int i = 0; i < wrong.Count; i++)
                {
                    x = wrong[i];
                    for (int j = i + 1; j < wrong.Count; j++)
                    {
                        y = wrong[j];
                        if (x.Value == y.Value)
                            continue;
                        if (SwapValues(field, x, y))
                            return true;
                    }
                }
            Console.WriteLine($"Aliven't {wrong.Count}");
            return false;
        }

        private bool SwapValues(SudokuField field, SudokuFieldCell startingA, SudokuFieldCell startingB, SudokuFieldCell a = null, SudokuFieldCell b = null)
        {
            if (a != null && b != null)
                if (EqualsUnordered((a.FieldLocation, b.FieldLocation), (startingA.FieldLocation, startingB.FieldLocation)))
                    return false;
            if (a == null)
            {
                a = startingA;
                b = startingB;
            }
            if (EqualsUnordered((GetOtherCell(a, c => c.ParentRow).FieldLocation, GetOtherCell(b, c => c.ParentRow).FieldLocation), (startingA.FieldLocation, startingB.FieldLocation)))
                return false;
            if (EqualsUnordered((GetOtherCell(a, c => c.ParentColumn).FieldLocation, GetOtherCell(b, c => c.ParentColumn).FieldLocation), (startingA.FieldLocation, startingB.FieldLocation)))
                return false;
            //if (EqualsUnordered((GetOtherCell(a, c => c.ParentBox).FieldLocation, GetOtherCell(b, c => c.ParentBox).FieldLocation), (startingA.FieldLocation, startingB.FieldLocation)))
            //    return false;
            var f = field.Clone();
            var (i, j) = a.FieldLocation;
            var (m, n) = b.FieldLocation;
            SudokuFieldCell x, y;
            f.SafeSwap(x = f.Cells[i, j], y = f.Cells[m, n]);
            var p = x.ParentRow.GetCellWithValue(x.Value);
            var q = y.ParentRow.GetCellWithValue(y.Value);
            if (SwapValues(f, startingA, startingB, p, q))
            {
                Field = f;
                return true;
            }
            var t = x.ParentColumn.GetCellWithValue(x.Value);
            var u = y.ParentColumn.GetCellWithValue(y.Value);
            if (SwapValues(f, startingA, startingB, t, u))
            {
                Field = f;
                return true;
            }
            //var v = x.ParentBox.GetCellWithValue(x.Value);
            //var w = y.ParentBox.GetCellWithValue(y.Value);
            //if (SwapValues(f, startingA, startingB, v, w))
            //{
            //    Field = f;
            //    return true;
            //}
            return false;
        }

        private SudokuFieldCell GetOtherCell(SudokuFieldCell c, Func<SudokuFieldCell, SudokuFieldCellCollection> collection) => collection(c).GetCellWithValue(c.Value);
        private bool EqualsUnordered(((int, int) A, (int, int) B) a, ((int, int) A, (int, int) B) b) => (a.A == b.A && a.B == b.B) || (a.A == b.B && a.B == b.A);
    }
}