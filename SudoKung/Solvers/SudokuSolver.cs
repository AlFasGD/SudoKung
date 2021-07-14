//+src=SudokuField.cs
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using SudoKung.Fields;
using SudoKung.CellCollections;
using SudoKung.Objects;

namespace SudoKung.Solvers
{
    public class SudokuSolver
    {
        private bool fieldChanged;
        
        private List<SudokuFieldCell> queuedValueChanges = new List<SudokuFieldCell>();
        private List<SudokuFieldCell> queuedPotentialValueRemovals = new List<SudokuFieldCell>();
        
        public readonly List<string> Log = new List<string>();
        
        public readonly SudokuField Field;
        public SudokuFieldCell[,] Cells => Field.Cells;
        public SudokuFieldRow[] Rows => Field.Rows;
        public SudokuFieldColumn[] Columns => Field.Columns;
        public SudokuFieldBox[,] Boxes => Field.Boxes;
        
        public bool Solved => Field.KnownValues == Field.Size * Field.Size;
        
        public SudokuSolver(SudokuField field)
        {
            Field = field;
        }
        
        public void Solve()
        {
            InitializePotentialValues();
            RemoveInvalidPotentialValues();
            InitializeEvents();
            do
            {
                fieldChanged = false;
                PerformLoggedTask(ConvertSinglePotentialValues, "Status - Converting single candidates");
                PerformLoggedTask(EvaluatePotentialValues, "Status - Evaluating candidates");
                PerformLoggedTask(EvaluatePotentialValueTuples, "Status - Evaluating candidate value tuples");
                PerformLoggedTask(EvaluatePotentialValuesByBox, "Status - Evaluating candidates by box");
            }
            while (!Solved && fieldChanged);
            if (!fieldChanged)
                throw new InvalidDataException("This Sudoku cannot be solved.");
        }

        private void PerformLoggedTask(Action action, string log)
        {
            Log.Add(log);
            action();
        }
        
        private void InitializeEvents()
        {
            foreach (var c in Cells)
            {
                c.ValueChanged += (x, y, z) => 
                {
                    fieldChanged = true;
                    RemoveInvalidPotentialValues(c);
                    Log.Add($"{c.FieldLocation} - Changed value from {y ?? "_"} to {z}");
                };
                c.PotentialValueRemoved += (x, y) =>
                {
                    fieldChanged = true;
                    queuedPotentialValueRemovals.Add(c);
                    Log.Add($"{c.FieldLocation} - Removed candidate {y}");
                };
            }
        }
        // O(n^3) because it adds one element per dimension, for every cell in the grid
        private void InitializePotentialValues()
        {
            foreach (var c in Cells)
                if (c.Value == null)
                    c.PotentialValues.AddRange(Field.AcceptableValues);
        }
        // O(n^4)
        private void RemoveInvalidPotentialValues()
        {
            foreach (var c in Cells)
                RemoveInvalidPotentialValues(c);
            while (queuedPotentialValueRemovals.Count > 0)
            {
                var q = queuedPotentialValueRemovals.First();
                queuedPotentialValueRemovals.RemoveAt(0);
                EvaluatePotentialValues(q);
            }
        }
        // O(n^2)
        private void RemoveInvalidPotentialValues(SudokuFieldCell c)
        {
            if (c.Value != null)
            {
                RemoveInvalidPotentialValues(c.ParentRow, c.Value);
                RemoveInvalidPotentialValues(c.ParentColumn, c.Value);
                RemoveInvalidPotentialValues(c.ParentBox, c.Value);
            }
        }
        // O(n^2)
        private void RemoveInvalidPotentialValues<T>(T container, string value) where T : SudokuFieldCellCollection
        {
            foreach (SudokuFieldCell c in container.Cells)
                if (c.Value == null)
                    c.RemovePotentialValue(value);
        }
        private void ConvertSinglePotentialValues()
        {
            foreach (var c in Cells)
                if (c.PotentialValues.Count == 1)
                    c.Value = c.PotentialValues[0];
        }
        // O(n^4)
        private void EvaluatePotentialValues()
        {
            foreach (var c in Cells)
                if (c.Value == null)
                    EvaluatePotentialValues(c);
        }
        // O(n^2)
        private void EvaluatePotentialValues(SudokuFieldCell c)
        {
            Log.Add($"{c.FieldLocation} - Evaluating candidates ({ToString(c.PotentialValues)})");
            SudokuFieldCellCollection[] collections = { c.ParentRow, c.ParentColumn, c.ParentBox };
            for (int i = 0; i < c.PotentialValues.Count; i++)
                foreach (var col in collections)
                    if (EvaluatePotentialValuesCollection(col, c.PotentialValues[i]))
                        break;
        }
        // O(n)
        private bool EvaluatePotentialValuesCollection(SudokuFieldCellCollection collection, string value)
        {
            var cells = collection.GetCellsWithPotentialValue(value);
            bool result = cells.Count == 1;
            if (result)
                cells[0].Value = value;
            return result;
        }
        // O(n^6)
        private void EvaluatePotentialValuesByBox()
        {
            int boxRows = Boxes.GetLength(0);
            int boxColumns = Boxes.GetLength(1);
            var boxRowCollectionTuples = new SudokuFieldBoxCellCollectionTuple<SudokuFieldRow>[boxRows, boxColumns];
            var boxColumnCollectionTuples = new SudokuFieldBoxCellCollectionTuple<SudokuFieldColumn>[boxRows, boxColumns];
            foreach (var value in Field.AcceptableValues)
            {
                for (int i = 0; i < boxRows; i++)
                    for (int j = 0; j < boxColumns; j++)
                    {
                        GetBoxCellParents(Boxes[i, j], value, out var r, out var c);
                        boxRowCollectionTuples[i, j] = new SudokuFieldBoxCellCollectionTuple<SudokuFieldRow>(Boxes[i, j], r, value);
                        boxColumnCollectionTuples[i, j] = new SudokuFieldBoxCellCollectionTuple<SudokuFieldColumn>(Boxes[i, j], c, value);
                    }
                // --- Move to function below?
                for (int i = 0; i < boxRows; i++)
                {
                    for (int j = 0; j < boxColumns; j++)
                    {
                        var dadRows = boxRowCollectionTuples[i, j];
                        var dadColumns = boxColumnCollectionTuples[i, j];
                        if (dadRows.CellCollections.Count == boxRows && dadColumns.CellCollections.Count == boxColumns)
                            continue;
                        var rowCandidateBoxes = new List<SudokuFieldBox> { Boxes[i, j] };
                        var columnCandidateBoxes = new List<SudokuFieldBox> { Boxes[i, j] };
                        // Rows
                        for (int k = 0; k < boxColumns && rowCandidateBoxes.Count < dadRows.CellCollections.Count; k++)
                        {
                            if (k == j)
                                continue;
                            var rows = boxRowCollectionTuples[i, k];
                            if (dadRows.CellCollections.Count != rows.CellCollections.Count)
                                continue;
                            bool match = true;
                            foreach (var r in rows.CellCollections)
                                if (!(match = dadRows.CellCollections.Contains(r)))
                                    break;
                            if (match)
                                rowCandidateBoxes.Add(rows.Box);
                        }
                        if (rowCandidateBoxes.Count == dadRows.CellCollections.Count)
                        {
                            for (int k = 0; k < boxColumns; k++)
                            {
                                if (k == j)
                                    continue;
                                if (!rowCandidateBoxes.Contains(Boxes[i, k]))
                                    foreach (SudokuFieldCell c in Boxes[i, k].Cells)
                                        if (c.Value == null && c.PotentialValues.Contains(value) && dadRows.CellCollections.Contains(c.ParentRow))
                                            c.RemovePotentialValue(value);
                            }
                        }
                        // Columns
                        for (int k = 0; k < boxRows && columnCandidateBoxes.Count < dadColumns.CellCollections.Count; k++)
                        {
                            if (k == i)
                                continue;
                            var columns = boxColumnCollectionTuples[k, j];
                            if (dadColumns.CellCollections.Count != columns.CellCollections.Count)
                                continue;
                            bool match = true;
                            foreach (var c in columns.CellCollections)
                                if (!(match = dadColumns.CellCollections.Contains(c)))
                                    break;
                            if (match)
                                columnCandidateBoxes.Add(columns.Box);
                        }
                        if (columnCandidateBoxes.Count == dadColumns.CellCollections.Count)
                        {
                            for (int k = 0; k < boxRows; k++)
                            {
                                if (k == j)
                                    continue;
                                if (!columnCandidateBoxes.Contains(Boxes[k, j]))
                                    foreach (SudokuFieldCell c in Boxes[k, j].Cells)
                                        if (c.Value == null && c.PotentialValues.Contains(value) && dadColumns.CellCollections.Contains(c.ParentColumn))
                                            c.RemovePotentialValue(value);
                            }
                        }
                    }
                }
            }
        }
        private void GetBoxCellParents(SudokuFieldBox box, string potentialValue, out List<SudokuFieldRow> rows, out List<SudokuFieldColumn> columns)
        {
            var l = box.GetCellsWithPotentialValue(potentialValue);
            rows = new List<SudokuFieldRow>();
            columns = new List<SudokuFieldColumn>();
            foreach (var c in l)
            {
                if (!rows.Contains(c.ParentRow))
                    rows.Add(c.ParentRow);
                if (!columns.Contains(c.ParentColumn))
                    columns.Add(c.ParentColumn);
                if (rows.Count == Field.BoxHeight && columns.Count == Field.BoxWidth)
                    break;
            }
        }
        // O(n^5)
        private void EvaluatePotentialValueTuples()
        {
            EvaluatePotentialValueTuples(Rows);
            EvaluatePotentialValueTuples(Columns);
            EvaluatePotentialValueTuples(Boxes);
        }
        // O(n^5)
        private void EvaluatePotentialValueTuples(Array collections) // Seriously cut it off with this bullshit
        {
            foreach (SudokuFieldCellCollection c in collections)
            {
                var l = c.GetCommonPotentialValueCellTuples();
                foreach (var p in l)
                    foreach (var cell in p.Cells)
                        KeepSpecificPotentialValues(cell, p.Values);
            }
        }
        private void KeepSpecificPotentialValues(SudokuFieldCell c, List<string> values)
        {
            for (int i = 0; i < c.PotentialValues.Count; i++)
                if (!values.Contains(c.PotentialValues[i]))
                    c.RemovePotentialValue(c.PotentialValues[i--]);
        }
        
        public string ToString<T>(List<T> l)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < l.Count; i++)
                s.Append($"{l[i]}, ");
            s.Remove(s.Length - 2, 2);
            return s.ToString();
        }
    }
}