
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.SendMails.Dtos;
using Vudaco.SendMails.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.SendMails.Repositories
{
    public interface IEmailTemplateRepositories : IBaseRepository<EmailTemplate>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(EmailTemplateDto EmailTemplateDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<EmailTemplate> GetByCodeAsync(string code);
        Task<EmailTemplate> ShowAsync(int id);
        Task<EmailTemplate> CreateAsync(EmailTemplate EmailTemplate);
        Task<EmailTemplate> UpdateAsync(EmailTemplate EmailTemplate);
        Task<EmailTemplate> DeleteSoftAsync(EmailTemplate EmailTemplate);
    }
}
