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
            var dataSql = $@"WITH LatestRows AS (
    SELECT source.*,
           MAX(CONVERT(date, DateProduction)) OVER (PARTITION BY GroupMember, CodeSub, NameSub) AS LatestProductionDate
    FROM {TargetTable} source
)
SELECT *
FROM LatestRows
WHERE CONVERT(date, DateProduction) = LatestProductionDate
ORDER BY GroupMember, CodeSub, NameSub, TRY_CONVERT(time(7), MinuteProduction)";
            var dataTable = _database.LoadTable(dataSql);
            var totalRows = dataTable.Rows.Count;

            return new
            {
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
