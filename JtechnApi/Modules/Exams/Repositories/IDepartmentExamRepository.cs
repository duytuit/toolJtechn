using JtechnApi.Exams.Dtos;
using JtechnApi.Exams.Models;
using JtechnApi.Shares;

namespace JtechnApi.Exams.Repositories
{
    public interface IDepartmentExamRepository
    {
        Task<object> List(int? departmentId);
        Task<RepositoryResult> GetById(int id);
        Task<RepositoryResult> Save(int? id, DepartmentExamInput input);
        Task<RepositoryResult> Delete(int id);
        OnlineExamDefinition LoadDefinition(int id);
        Task<object> Available(int employeeId);
        bool CanTake(int employeeId, int examId);
    }
}
