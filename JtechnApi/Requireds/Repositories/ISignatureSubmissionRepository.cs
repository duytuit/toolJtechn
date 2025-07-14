
using System.Threading.Tasks;
using JtechnApi.Requireds.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.Requireds.Repositories
{
    public interface ISignatureSubmissionRepository : IBaseRepository<SignatureSubmission>
    {
        Task<PaginatedResult<SignatureSubmission>> GetPaginatedAsync(int page, int pageSize);
        Task<SignatureSubmission> CreateSignatureSubmissiondAsync(SignatureSubmission SignatureSubmission);
        Task<SignatureSubmission> FindByRequired(int required_id, int dept_id, int Signature_id);
    }
}
