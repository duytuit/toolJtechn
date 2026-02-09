using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Vudaco.Notifys.Models;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Shares
{
    public class FcmService
    {
        private readonly VudacoDBContext _context;
        public FcmService(VudacoDBContext context)
        {
            _context = context;
        }
        public async Task<bool> SendMulticastAsync(
            List<int> UserIds,
            string title,
            string body,
            int StorageId = 0,
            int PostId = 0,
            int Type = 0,
            string screen = null,
            Dictionary<string, string> data = null
        )
        {
            var now = DateTime.Now;
            var tokens = await _context.UserDeviceTokens
                .Where(x => UserIds.Contains(x.UserId) && x.IsActive)
                .Select(x => x.DeviceToken)
                .Distinct()
                .ToListAsync();
            _ = Task.Run(() => Helper.SendTelegramMessageAsync($"{string.Join(",", UserIds)} - {tokens.Count} tokens"));
            if (!tokens.Any()) return false;
      
            var message = new MulticastMessage()
            {
                Tokens = tokens,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>()
            };
        
            var getEmployees = await _context.Employees
            .Where(e => e.UserId.HasValue 
                    && UserIds.Contains(e.UserId.Value)
                    && e.StorageId == StorageId)
            .ToListAsync();
            foreach (var emp in getEmployees)
            {
                var notify = new Notify
                {
                    StorageId = emp.StorageId,
                    EmployeeId = emp.Id,
                    PostId = PostId,
                    Title = title,
                    Description = body,
                    Status = 0,
                    Type = Type,
                    Screen = screen,
                    CreatedBy = emp.UserId ?? 0,
                    CreatedAt = now,
                    UpdatedBy = emp.UserId ?? 0,
                    UpdatedAt = now
                };
                _context.Notifys.Add(notify);
            }
            await _context.SaveChangesAsync();
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            // token invalid -> disable
            for (int i = 0; i < response.Responses.Count; i++)
            {
                var r = response.Responses[i];
                if (!r.IsSuccess)
                {
                    var token = tokens[i];

                    // disable token
                    var device = await _context.UserDeviceTokens
                        .FirstOrDefaultAsync(x => x.DeviceToken == token);

                    if (device != null)
                    {
                        device.IsActive = false;
                    }
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
