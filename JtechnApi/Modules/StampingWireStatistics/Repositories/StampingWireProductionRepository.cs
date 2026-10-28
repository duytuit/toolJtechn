using System.Data;
using JtechnApi.Shares.Connects;

namespace JtechnApi.Modules.StampingWireStatistics.Repositories
{
    public class StampingWireProductionRepository
    {
        private const int MaxRows = 5000;
        private const string TargetTable = "dbo.Control_StampingWire_Visualize_Status";
        private readonly AdoJtecDB_KhoaB _database;

        public StampingWireProductionRepository(AdoJtecDB_KhoaB database)
        {
            _database = database;
        }

        public object GetSnapshot()
        {
            var countSql = $"SELECT COUNT_BIG(1) AS TotalRows FROM {TargetTable}";
            Console.WriteLine($"[StampingWireProduction SQL] {countSql}");
            var countTable = _database.LoadTable(countSql);
            var dataSql = $"SELECT TOP ({MaxRows}) * FROM {TargetTable} ORDER BY DateProduction DESC, MinuteProduction DESC";
            Console.WriteLine($"[StampingWireProduction SQL] {dataSql}");
            var dataTable = _database.LoadTable(dataSql);
            var midDayTargetSql = $"SELECT * FROM {TargetTable} WHERE (TRY_CONVERT(time(7), MinuteProduction) >= CONVERT(time(7), '13:30:00') AND TRY_CONVERT(time(7), MinuteProduction) < CONVERT(time(7), '13:31:00') OR CONVERT(varchar(50), MinuteProduction) LIKE '%13:30%')";
            Console.WriteLine($"[StampingWireProduction SQL] {midDayTargetSql}");
            var midDayTargetRows = _database.LoadTable(midDayTargetSql);
            var targetSql = $"SELECT * FROM {TargetTable} WHERE (TRY_CONVERT(time(7), MinuteProduction) >= CONVERT(time(7), '21:30:00') AND TRY_CONVERT(time(7), MinuteProduction) < CONVERT(time(7), '21:31:00') OR CONVERT(varchar(50), MinuteProduction) LIKE '%21:30%')";
            Console.WriteLine($"[StampingWireProduction SQL] {targetSql}");
            var targetRows = _database.LoadTable(targetSql);
            var totalRows = Convert.ToInt64(countTable.Rows[0]["TotalRows"]);
            var midDayTargetRowValues = ToRows(midDayTargetRows);
            var targetRowValues = ToRows(targetRows);
            Console.WriteLine($"[StampingWireProduction SQL] Kết quả: {totalRows} dòng tổng cộng, {dataTable.Rows.Count} dòng truy vấn chính, {midDayTargetRowValues.Count} dòng target 13:30, {targetRowValues.Count} dòng target 21:30.");
            var rows = ToRows(dataTable)
                .Concat(targetRowValues.Where(targetRow =>
                    !dataTable.AsEnumerable().Any(dataRow =>
                        dataTable.Columns.Cast<DataColumn>().All(column =>
                            Equals(
                                targetRow[column.ColumnName],
                                dataRow.IsNull(column) ? null : dataRow[column])))))
                .ToList();

            return new
            {
                TotalRows = totalRows,
                IsTruncated = totalRows > MaxRows,
                Columns = dataTable.Columns.Cast<DataColumn>()
                    .Select(column => column.ColumnName)
                    .ToArray(),
                MidDayTargetRows = midDayTargetRowValues,
                EndOfDayTargetRows = targetRowValues,
                Rows = rows
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
