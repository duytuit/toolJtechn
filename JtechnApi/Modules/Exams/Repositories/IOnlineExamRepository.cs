using JtechnApi.Exams.Models;

namespace JtechnApi.Exams.Repositories
{
    public interface IOnlineExamAttemptStore
    {
        Task<OnlineExamAttempt> Get(Guid id, string employeeId);
        Task<List<OnlineExamAttempt>> History(string employeeId);
        Task Save(OnlineExamAttempt attempt);
    }

    public interface IOnlineExamRepository
    {
        Task<object> Catalog(string employeeId, int? examId = null);
        Task<object> GetCandidate(Guid id, string employeeId);
        Task<object> StartCandidate(string employeeId, string name, int? examId = null, OnlineExamStartInput input = null);
        Task<object> SaveCandidate(Guid id, string employeeId, Dictionary<int, string[]> answers, bool submit);
        OnlineExamDefinition Definition(int? examId = null);
        string[] MissingImages(int? examId = null);
        Task<List<OnlineExamAttempt>> History(string employeeId);
        Task<OnlineExamAttempt> Start(string employeeId, string name, int? examId = null, DateTime? examDate = null, bool isNewEmployee = false);
        Task<OnlineExamAttempt> Get(Guid id, string employeeId);
        Task<OnlineExamAttempt> Update(Guid id, string employeeId, Dictionary<int, string[]> answers, bool submit);
    }
}
