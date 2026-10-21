using System.Data;
using JtechnApi.Modules.AssemblyTargetStatistics.Models;
using JtechnApi.Shares.Connects;

namespace JtechnApi.Modules.AssemblyTargetStatistics.Repositories
{
    public class AssemblyCombineTargetStatisticsRepository
    {
        private const int MaxRows = 5000;
        private const string TargetTable = "dbo.Control_Assembled_Combine_Visualize_30";
        private readonly AdoJtecDB_KhoaB _database;

        public AssemblyCombineTargetStatisticsRepository(AdoJtecDB_KhoaB database)
        {
            _database = database;
        }

        public AssemblyCombineTargetSnapshot GetSnapshot(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            const string dateFilter = " WHERE DateProduction >= @startDate AND DateProduction < @endDate";
            var parameterNames = new[] { "@startDate", "@endDate" };
            var parameterValues = new object[] { startDate, endDate };
            var statusStartDate = DateTime.Today;
            var statusEndDate = statusStartDate.AddDays(1);
            const string statusDateFilter = " WHERE TimeInput >= @statusStartDate AND TimeInput < @statusEndDate";
            var statusParameterNames = new[] { "@statusStartDate", "@statusEndDate" };
            var statusParameterValues = new object[] { statusStartDate, statusEndDate };

            var countTable = _database.LoadTable(
                "SELECT COUNT_BIG(1) AS TotalRows FROM " + TargetTable + dateFilter,
                parameterNames,
                parameterValues,
                parameterNames.Length);
            var dataTable = _database.LoadTable(
                $"SELECT TOP ({MaxRows}) * FROM {TargetTable}{dateFilter} ORDER BY TimeInput DESC",
                parameterNames,
                parameterValues,
                parameterNames.Length);
            var statusTable = _database.LoadTable(
                $"SELECT TOP ({MaxRows}) * FROM dbo.Control_Assembled_Status{statusDateFilter} ORDER BY TimeInput DESC",
                statusParameterNames,
                statusParameterValues,
                statusParameterNames.Length);
            var statusCountTable = _database.LoadTable(
                "SELECT COUNT_BIG(1) AS TotalRows FROM dbo.Control_Assembled_Status" + statusDateFilter,
                statusParameterNames,
                statusParameterValues,
                statusParameterNames.Length);
            var statusSummaryTable = _database.LoadTable(
                "SELECT Status, COUNT_BIG(1) AS TotalRows FROM dbo.Control_Assembled_Status" + statusDateFilter + " GROUP BY Status",
                statusParameterNames,
                statusParameterValues,
                statusParameterNames.Length);

            return new AssemblyCombineTargetSnapshot
            {
                TotalRows = Convert.ToInt64(countTable.Rows[0]["TotalRows"]),
                IsTruncated = Convert.ToInt64(countTable.Rows[0]["TotalRows"]) > MaxRows,
                Columns = dataTable.Columns.Cast<DataColumn>()
                    .Select(column => column.ColumnName)
                    .ToArray(),
                Rows = ToRows(dataTable),
                StatusRows = ToRows(statusTable),
                StatusCounts = ToRows(statusSummaryTable),
                StatusTotalRows = Convert.ToInt64(statusCountTable.Rows[0]["TotalRows"]),
                StatusIsTruncated = Convert.ToInt64(statusCountTable.Rows[0]["TotalRows"]) > MaxRows
            };
        }

        private static List<Dictionary<string, object>> ToRows(DataTable table)
        {
            return table.AsEnumerable()
                .Select(row => table.Columns.Cast<DataColumn>()
                    .ToDictionary(
                        column => column.ColumnName,
                        column => row.IsNull(column) ? null : row[column],
                        StringComparer.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
