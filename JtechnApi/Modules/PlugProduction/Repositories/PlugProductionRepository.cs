using System.Data;
using JtechnApi.Shares.Connects;

namespace JtechnApi.Modules.PlugProduction.Repositories
{
    public class PlugProductionRepository
    {
        private const string TargetTable = "dbo.Control_ProgramPlug_Visualize";
        private readonly AdoJtecDB_Khoa _database;

        public PlugProductionRepository(AdoJtecDB_Khoa database)
        {
            _database = database;
        }

        public object GetSnapshot()
        {
            var dataSql = $@"SELECT *
FROM {TargetTable}
WHERE DateProduction >= CONVERT(date, GETDATE())
  AND DateProduction < DATEADD(day, 1, CONVERT(date, GETDATE()))
ORDER BY GroupMember, CodeSub, NameSub, TRY_CONVERT(time(7), MinuteProduction)";
            var dataTable = _database.LoadTable(dataSql);
            var totalRows = dataTable.Rows.Count;
            var qualityTable = _database.LoadTable(
                "SELECT LastNameSub, TargetError, InputError FROM dbo.Control_ProgramPlug_Target ORDER BY LastNameSub");

            return new
            {
                QualityRows = qualityTable.AsEnumerable().Select(row => new
                {
                    LastNameSub = row.IsNull("LastNameSub") ? null : row["LastNameSub"],
                    TargetError = row.IsNull("TargetError") ? null : row["TargetError"],
                    InputError = row.IsNull("InputError") ? null : row["InputError"]
                }).ToList(),
                TotalRows = totalRows,
                IsTruncated = false,
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
