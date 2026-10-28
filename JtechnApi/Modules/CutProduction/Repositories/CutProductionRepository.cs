using System.Data;
using JtechnApi.Shares.Connects;

namespace JtechnApi.Modules.CutProduction.Repositories
{
    public class CutProductionRepository
    {
        private const int MaxRows = 5000;
        private const string TargetTable = "dbo.Control_ProgramCalculatorCut_Visualize_ByDay";
        private readonly AdoJtecDB_Khoa _database;

        public CutProductionRepository(AdoJtecDB_Khoa database)
        {
            _database = database;
        }

        public object GetSnapshot()
        {
            var countTable = _database.LoadTable($"SELECT COUNT_BIG(1) AS TotalRows FROM {TargetTable}");
            var dataTable = _database.LoadTable($"SELECT TOP ({MaxRows}) * FROM {TargetTable} ORDER BY DateProduction DESC, Machine, MinuteProduction");
            var totalRows = Convert.ToInt64(countTable.Rows[0]["TotalRows"]);

            return new
            {
                TotalRows = totalRows,
                IsTruncated = totalRows > MaxRows,
                Columns = dataTable.Columns.Cast<DataColumn>()
                    .Select(column => column.ColumnName)
                    .ToArray(),
                Rows = dataTable.AsEnumerable()
                    .Select(row => dataTable.Columns.Cast<DataColumn>()
                        .ToDictionary(
                            column => column.ColumnName,
                            column => row.IsNull(column) ? null : row[column],
                            StringComparer.OrdinalIgnoreCase))
                    .ToList()
            };
        }
    }
}
