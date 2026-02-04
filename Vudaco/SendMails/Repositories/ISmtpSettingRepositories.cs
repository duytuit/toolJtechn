
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.SendMails.Dtos;
using Vudaco.SendMails.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.SendMails.Repositories
{
    public interface ISmtpSettingRepositories : IBaseRepository<SmtpSetting>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(SmtpSettingDto SmtpSettingDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<SmtpSetting> ShowAsync(int id);
        Task<SendMailResult> SendAsync(SendMailRequest request);
        Task<SmtpSetting> GetDefaultAsync();
        Task<SmtpSetting> GetByCodeAsync(string code);
        Task<SmtpSetting> CreateAsync(SmtpSetting SmtpSetting);
        Task<SmtpSetting> UpdateAsync(SmtpSetting SmtpSetting);
        Task<SmtpSetting> DeleteSoftAsync(SmtpSetting SmtpSetting);
    }
}
