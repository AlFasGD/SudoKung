using SudoKung.Extensions;
using SudoKung.Fields;
using SudoKung.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Convert;

namespace SudoKung.CellCollections
{
    // Would it be bad if I said the confusion with generic arrays is retardedly stupid?
    public abstract class SudokuFieldCellCollection
    {
        private Array cells;
        private List<string> containedValues = new List<string>();

        protected virtual string ContainerName => "container";

        public SudokuField ParentField = null;

        public int KnownValues { get; private set; }

        public Array Cells
        {
            get => cells;
            set
            {
                cells = value;
                foreach (SudokuFieldCell c in cells)
                {
                    if (c.Value != null)
                        containedValues.Add(c.Value);
                    LinkParentContainer(c);
                    c.ValueChanged += UpdateValue;
                    KnownValues += ToInt32(c.Value != null);
                }
            }
        }

        public SudokuFieldCellCollection(Array cells)
        {
            Cells = cells;
        }

        public bool Contains(string value)
        {
            if (!ParentField.AcceptableValues.Contains(value))
                throw new ArgumentOutOfRangeException("The provided value is not an acceptable value for that field type.");
            return containedValues.Contains(value);
        }

        /// <summary>This function returns whether this collection contains a cell c whose value is equal to v where q does not contain the cell c.</summary>
        /// <param name="v"></param>
        /// <param name="q"></param>
        public bool ContainsValueExcept(string v, params SudokuFieldCell[] q)
        {
            foreach (SudokuFieldCell c in Cells)
                if (!q.Contains(c) && c.Value == v)
                    return true;
            return false;
        }
        public SudokuFieldCell GetCellWithValue(string value)
        {
            foreach (SudokuFieldCell c in cells)
                if (c.Value == value)
                    return c;
            return null;
        }
        public List<SudokuFieldCell> GetCellsWithPotentialValue(string potentialValue)
        {
            List<SudokuFieldCell> result = new List<SudokuFieldCell>();
            foreach (SudokuFieldCell c in cells)
                if (c.PotentialValues.Contains(potentialValue))
                    result.Add(c);
            return result;
        }
        // O(n^4)
        public List<SudokuFieldCommonCellTuple> GetCommonPotentialValueCellTuples()
        {
            var result = new List<SudokuFieldCommonCellTuple>();
            var l = new List<SudokuFieldCellTuple>();
            var d = new List<SudokuFieldCellTuple>();
            foreach (var value in ParentField.AcceptableValues)
            {
                if (containedValues.Contains(value))
                    continue;
                var c = GetCellsWithPotentialValue(value);
                l.Add(new SudokuFieldCellTuple(c, value));
            }
            foreach (var pair in l)
            {
                if (d.Contains(pair))
                    continue;
                var candidates = new List<SudokuFieldCellTuple> { pair };
                bool done = pair.Cells.Count == candidates.Count;
                for (int i = 0; i < l.Count && !done; i++)
                {
                    var p = l[i];
                    if (pair == p || d.Contains(p))
                        continue;
                    if (pair.Cells.EqualsUnordered(p.Cells))
                        candidates.Add(p);
                    done = pair.Cells.Count == candidates.Count;
                }
                if (done)
                {
                    var values = new List<string>();
                    foreach (var c in candidates)
                        values.Add(c.Value);
                    result.Add(new SudokuFieldCommonCellTuple(pair.Cells, values));
                }
                foreach (var c in candidates)
                    d.Add(c);
            }
            return result;
        }

        protected abstract void LinkParentContainer(SudokuFieldCell cell);

        private void UpdateValue(SudokuFieldCell cell, string previousValue, string newValue)
        {
            if (previousValue != null)
                containedValues.Remove(previousValue);
            if (newValue != null)
            {
                if (containedValues.Contains(newValue))
                    throw new InvalidOperationException($"The {ContainerName} already contains a cell with the value {newValue}.");
                containedValues.Add(newValue);
            }
            KnownValues += ToInt32(previousValue == null) - ToInt32(newValue == null);
        }
    }
}
