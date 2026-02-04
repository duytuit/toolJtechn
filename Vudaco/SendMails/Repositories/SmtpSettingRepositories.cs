using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.SendMails.Dtos;
using Vudaco.SendMails.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.SendMails.Repositories
{
    public class SmtpSettingRepositories : BaseRepository<SmtpSetting>, ISmtpSettingRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        private readonly IEmailTemplateRepositories _templateRepo;
        public SmtpSettingRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis, IEmailTemplateRepositories templateRepo) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
            _templateRepo = templateRepo;
        }

        public Task<SmtpSetting> CreateAsync(SmtpSetting SmtpSetting)
        {
              _context.SmtpSettings.Add(SmtpSetting);
            _context.SaveChanges();
            return Task.FromResult(SmtpSetting);
        }
        public async Task<SmtpSetting> GetDefaultAsync()
        {
            return await _context.SmtpSettings
                .FirstOrDefaultAsync(x => x.IsDefault && x.IsActive);
        }

        public async Task<SmtpSetting> GetByCodeAsync(string code)
        {
            return await _context.SmtpSettings
                .FirstOrDefaultAsync(x => x.Code == code && x.IsActive);
        }
        public async Task<SendMailResult> SendAsync(SendMailRequest request)
        {
            var result = new SendMailResult
            {
                To = request.To,
                TemplateCode = request.TemplateCode,
                SmtpCode = request.SmtpCode,
                SentAt = DateTime.UtcNow
            };

            try
            {
                // 1. Load SMTP
                var smtp = string.IsNullOrEmpty(request.SmtpCode)
                    ? await _context.SmtpSettings.FirstOrDefaultAsync(x => x.IsDefault && x.IsActive)
                    : await _context.SmtpSettings.FirstOrDefaultAsync(x => x.Code == request.SmtpCode && x.IsActive);

                if (smtp == null)
                    throw new Exception("SMTP config not found");

                // 2. Load Template
                var template = await _templateRepo.GetByCodeAsync(request.TemplateCode);
                if (template == null)
                    throw new Exception("Email template not found");

                // 3. Replace param
                var subject = Replace(template.Subject, request.Parameters);
                var body = Replace(template.Body, request.Parameters);

                // 4. Send mail
                using var message = new MailMessage
                {
                    From = new MailAddress(smtp.FromEmail, smtp.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(request.To);

                using var client = new SmtpClient(smtp.Host, smtp.Port)
                {
                    Credentials = new NetworkCredential(
                        smtp.Username,
                        smtp.Password),
                    EnableSsl = smtp.EnableSsl
                };

                await client.SendMailAsync(message);

                // 5. Success
                result.Success = true;
                result.Message = "Send mail successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Send mail failed";
                result.Error = ex.Message;
            }

            return result;
        }

        private string Replace(string content, Dictionary<string, string> parameters)
        {
            if (parameters == null) return content;

            foreach (var p in parameters)
            {
                content = content.Replace(
                    $"{{{{{p.Key}}}}}",
                    p.Value ?? "");
            }
            return content;
        }
        public Task<SmtpSetting> DeleteSoftAsync(SmtpSetting SmtpSetting)
        {
              _context.SmtpSettings.Update(SmtpSetting);
            _context.SaveChanges();
            return Task.FromResult(SmtpSetting);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(SmtpSettingDto SmtpSettingDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            // if (DepartmentDto.StorageId > 0)
            //     whereEquals["storage_id"] = DepartmentDto.StorageId;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "departments",
                        new[] { "id","code","name","parent_id","status","storage_id","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at","permissions"},
                        offset: null,
                        limit: null,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        redisCache: _redis,
                        includeCount: false,
                        cancellationToken: cancellationToken
                    );
            int totalItems = results.Count;
            var objectList = new List<object>();
            objectList.AddRange(results.Data);
            var _results = new PaginatedResultReact<object>
            {
                PageNum = page,
                PageSize = pageSize,
                First = (int)Math.Ceiling((double)totalItems / pageSize),
                Total = totalItems,
                Data = objectList,
            };
            objectList = null;
            results = null;
            whereEquals?.Clear(); whereLikes?.Clear(); whereDateRange?.Clear(); orderByList?.Clear();
            return _results;
        }

        public Task<SmtpSetting> ShowAsync(int id)
        {
            return _context.SmtpSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<SmtpSetting> UpdateAsync(SmtpSetting SmtpSetting)
        {
              _context.SmtpSettings.Update(SmtpSetting);
            _context.SaveChanges();
            return Task.FromResult(SmtpSetting);
        }
    }
}
