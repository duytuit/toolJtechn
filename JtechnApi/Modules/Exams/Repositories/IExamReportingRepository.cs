using JtechnApi.Exams.Models;

namespace JtechnApi.Exams.Repositories
{
    public interface IExamReportingRepository
    {
        Task<ExamResultFilterOptions> ResultFilterOptions();
        Task<ExamResultPage> Results(int page, int pageSize, int? departmentId, int? examId, string search);
        Task<IReadOnlyList<ExamResultItem>> ExportResults(int? departmentId, int? examId, string search);
        Task<ExamQuickStatistics> QuickStatistics(int? departmentId, int? examId);
        Task<IReadOnlyList<ExamQuickStatisticsEmployee>> ExportQuickStatistics(int period, string category, int? departmentId, int? examId);
        Task<bool> DeleteResult(int id, string deletedBy);
        Task<ExamStatistics> Statistics(int page, int pageSize, string search);
    }
}
