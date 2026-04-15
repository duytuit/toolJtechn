using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Vudaco.Notifys.Models;
using Vudaco.Shares.BaseRepository;
using dotAPNS;

namespace Vudaco.Shares
{
    public class FcmService
    {
        private readonly VudacoDBContext _context;
        private readonly ApnsClient _client;
        private readonly string _bundleId;
        public FcmService(VudacoDBContext context, IConfiguration config)
        {
            _context = context;
              var cfg = config.GetSection("Apns");

            _bundleId = cfg["BundleId"];

            _client = ApnsClient.CreateUsingJwt(new ApnsJwtOptions
            {
                TeamId = cfg["TeamId"],
                KeyId = cfg["KeyId"],
                PrivateKey = File.ReadAllText(cfg["P8Path"]),
                UseSandbox = bool.Parse(cfg["UseSandbox"])
            });
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
            var android_tokens = await _context.UserDeviceTokens
                .Where(x => UserIds.Contains(x.UserId) && x.IsActive && x.Platform == "android")
                .Select(x => x.DeviceToken)
                .Distinct()
                .ToListAsync();
            var ios_tokens = await _context.UserDeviceTokens
                .Where(x => UserIds.Contains(x.UserId) && x.IsActive && x.Platform == "ios")
                .Select(x => x.DeviceToken)
                .Distinct()
                .ToListAsync();
            if (ios_tokens.Any())
            {
                 await Task.WhenAll(ios_tokens.Select(token => SendOne(token, title, body, data)));
            }
            _ = Task.Run(() => Helper.SendTelegramMessageAsync($"{string.Join(",", UserIds)} - {android_tokens.Count} tokens"));
           
      
            var message = new MulticastMessage()
            {
                Tokens = android_tokens,
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
         private async Task SendOne(string token, string title, string body, Dictionary<string, string> data)
        {
            var push = new ApplePush(ApplePushType.Alert)
                .AddToken(token)
                .AddAlert(title, body)
                .AddSound("default");

            // apns-topic bắt buộc
            push.AddCustomProperty("apns-topic", _bundleId);

            if (data != null)
            {
                foreach (var d in data)
                    push.AddCustomProperty(d.Key, d.Value);
            }

            var result = await _client.SendAsync(push);

            if (!result.IsSuccessful)
            {
                Console.WriteLine("APNS Error: " + result.Reason);

                // disable token
                var device = await _context.UserDeviceTokens
                    .FirstOrDefaultAsync(x => x.DeviceToken == token);

                if (device != null)
                    device.IsActive = false;
            }
        }
    }
}
