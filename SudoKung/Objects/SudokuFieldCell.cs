using SudoKung.CellCollections;
using SudoKung.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudoKung.Objects
{
    public class SudokuFieldCell
    {
        private string value = null;

        internal string SafeValue
        {
            set => this.value = value;
        }

        public event SudokuFieldCellValueChanged ValueChanged;
        public event SudokuFieldCellPotentialValuesChanged PotentialValueAdded;
        public event SudokuFieldCellPotentialValuesChanged PotentialValueRemoved;

        public SudokuField ParentField = null;
        public SudokuFieldRow ParentRow = null;
        public SudokuFieldColumn ParentColumn = null;
        public SudokuFieldBox ParentBox = null;
        public (int X, int Y) FieldLocation = (-1, -1);

        public string Value
        {
            get => value;
            set
            {
                string prevValue = this.value;
                this.value = value;
                if (value != null)
                    PotentialValues.Clear();
                OnValueChanged(prevValue, value);
            }
        }
        public List<string> PotentialValues { get; private set; } = new List<string>();

        public SudokuFieldCell() { }
        public SudokuFieldCell(string value)
        {
            Value = value;
        }
        public SudokuFieldCell(List<string> potentialValues)
        {
            PotentialValues = potentialValues;
        }

        public SudokuFieldCell Clone()
        {
            var cloned = new SudokuFieldCell(Value);
            var p = new List<string>();
            foreach (var v in PotentialValues)
                p.Add(v);
            cloned.PotentialValues = p;
            cloned.ParentField = ParentField;
            cloned.ParentRow = ParentRow;
            cloned.ParentColumn = ParentColumn;
            cloned.ParentBox = ParentBox;
            cloned.FieldLocation = FieldLocation;
            return cloned;
        }

        public void AddPotentialValue(string value)
        {
            PotentialValues.Add(value);
            OnPotentialValueAdded(value);
        }
        public void RemovePotentialValue(string value)
        {
            if (PotentialValues.Contains(value))
            {
                PotentialValues.Remove(value);
                OnPotentialValueRemoved(value);
            }
        }

        public void OnValueChanged(string previousValue, string newValue)
        {
            ValueChanged?.Invoke(this, previousValue, newValue);
        }
        public void OnPotentialValueAdded(string changedPotentialValue)
        {
            PotentialValueAdded?.Invoke(this, changedPotentialValue);
        }
        public void OnPotentialValueRemoved(string changedPotentialValue)
        {
            PotentialValueRemoved?.Invoke(this, changedPotentialValue);
        }
    }

    public delegate void SudokuFieldCellValueChanged(SudokuFieldCell cell, string previousValue, string newValue);
    public delegate void SudokuFieldCellPotentialValuesChanged(SudokuFieldCell cell, string changedPotentialValue);
}
