using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Vudaco.Notifys.Models;
using dotAPNS;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Shares
{
    public class FcmService
    {
        private readonly VudacoDBContext _context;
        private readonly ApnsClient _client;
        private readonly string _bundleId;
        private readonly bool _useSandbox;

        public FcmService(VudacoDBContext context, IConfiguration config)
        {
            _context = context;

            var cfg = config.GetSection("Apns");
            _useSandbox = bool.Parse(cfg["UseSandbox"] ?? "false");

            _bundleId = cfg["BundleId"];

            var keyContent = File.ReadAllText(cfg["P8Path"]);

            var httpClient = new HttpClient();

            _client = ApnsClient.CreateUsingJwt(
                httpClient,
                new ApnsJwtOptions
                {
                    TeamId = cfg["TeamId"],
                    KeyId = cfg["KeyId"],
                    CertContent = keyContent, // ✅ dùng content
                    BundleId = _bundleId
                });
          

        }
        public async Task<bool> SendMulticastAsync(
            List<int> userIds,
            string title,
            string body,
            int storageId = 0,
            int postId = 0,
            int type = 0,
            string screen = null,
            string data = null
        )
        {
            var now = DateTime.Now;
            // 🔥 Lấy tất cả token (Android + iOS)
            var tokens = await _context.UserDeviceTokens
                .Where(x => userIds.Contains(x.UserId) && x.IsActive)
                .Select(x => x.DeviceToken)
                .Distinct()
                .ToListAsync();

            if (tokens.Any())
            {
              
                // 🔥 chia batch (max 500 tokens/request)
                var chunks = SplitList(tokens, 500);

                foreach (var chunk in chunks)
                {
                    var message = new MulticastMessage()
                    {
                        Tokens = chunk.ToList(),

                        Notification = new Notification
                        {
                            Title = title,
                            Body = body
                        },

                        Data = new Dictionary<string, string>
                        {
                            { "type", type.ToString() },
                            { "screen", screen ?? "" },
                            { "data", data ?? "" }
                        },

                        // ✅ iOS config
                        Apns = new ApnsConfig
                        {
                            Aps = new Aps
                            {
                                Sound = "default",
                            }
                        },

                        // ✅ Android config
                        Android = new AndroidConfig
                        {
                            Priority = Priority.High
                        }
                    };

                    var response = await FirebaseMessaging.DefaultInstance
                        .SendEachForMulticastAsync(message);

                    // 🔥 xử lý token lỗi
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        if (!response.Responses[i].IsSuccess)
                        {
                            var badToken = chunk.ElementAt(i);

                            var device = await _context.UserDeviceTokens
                                .FirstOrDefaultAsync(x => x.DeviceToken == badToken);

                            if (device != null)
                                device.IsActive = false;
                               _ = Task.Run(() => Helper.SendTelegramMessageAsync($"Error:{response.Responses[i].Exception?.Message}-FCM UserIds: {string.Join(", ", badToken)}"));
                            // log lỗi (nên giữ)
                            Console.WriteLine($"FCM Error: {response.Responses[i].Exception?.Message}");
                        }
                    }
                }
            }

            // ================= SAVE DB =================
            var employees = await _context.Employees
                .Where(e => e.UserId.HasValue &&
                            userIds.Contains(e.UserId.Value) &&
                            e.StorageId == storageId)
                .ToListAsync();

            foreach (var emp in employees)
            {
                _context.Notifys.Add(new Notify
                {
                    StorageId = emp.StorageId,
                    EmployeeId = emp.Id,
                    PostId = postId,
                    Title = title,
                    Description = body,
                    Status = 0,
                    Type = type,
                    Screen = screen,
                    CreatedBy = emp.UserId ?? 0,
                    CreatedAt = now,
                    UpdatedBy = emp.UserId ?? 0,
                    UpdatedAt = now
                });
            }

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> SendMulticastAsyncV2(
            List<int> userIds,
            string title,
            string body,
            int storageId = 0,
            int postId = 0,
            int type = 0,
            string screen = null,
            string data = null
        )
        {
            var now = DateTime.Now;

            // ================= ANDROID =================
            var androidTokens = await _context.UserDeviceTokens
                .Where(x => userIds.Contains(x.UserId) && x.IsActive && x.Platform == "android")
                .Select(x => x.DeviceToken)
                .Distinct()
                .ToListAsync();

            if (androidTokens.Any())
            {
                var message = new MulticastMessage()
                {
                    Tokens = androidTokens,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = new Dictionary<string, string>
                    {
                        { "type", type.ToString() },
                        { "screen", screen },
                        { "data", data }
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

                for (int i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        var token = androidTokens[i];

                        var device = await _context.UserDeviceTokens
                            .FirstOrDefaultAsync(x => x.DeviceToken == token);

                        if (device != null)
                            device.IsActive = false;
                    }
                }
            }

            // ================= IOS =================
            var iosProdTokens = await _context.UserDeviceTokens
                .Where(x => userIds.Contains(x.UserId) && x.IsActive && x.Platform == "ios" && x.Env == "prod")
                .Select(x => x.DeviceToken)
                .Distinct()
                .ToListAsync();

            foreach (var token in iosProdTokens) // ✅ tránh DbContext crash
            {
                _ = Task.Run(() => Helper.SendTelegramMessageAsync($"{string.Join(",", token)} - tokens"));
                await SendOne(token, title, body, type, screen, data);
            }
            var iosDevTokens = await _context.UserDeviceTokens
                .Where(x => userIds.Contains(x.UserId) && x.IsActive && x.Platform == "ios" && x.Env == "dev")
                .Select(x => x.DeviceToken)
                .Distinct()
                .ToListAsync();
            foreach (var token in iosDevTokens) // ✅ tránh DbContext crash
            {
                _ = Task.Run(() => Helper.SendTelegramMessageAsync($"{string.Join(",", token)} - tokens"));
                await SendOneDev(token, title, body, type, screen, data);
            }
            // ================= SAVE DB =================
            var employees = await _context.Employees
                .Where(e => e.UserId.HasValue &&
                            userIds.Contains(e.UserId.Value) &&
                            e.StorageId == storageId)
                .ToListAsync();

            foreach (var emp in employees)
            {
                _context.Notifys.Add(new Notify
                {
                    StorageId = emp.StorageId,
                    EmployeeId = emp.Id,
                    PostId = postId,
                    Title = title,
                    Description = body,
                    Status = 0,
                    Type = type,
                    Screen = screen,
                    CreatedBy = emp.UserId ?? 0,
                    CreatedAt = now,
                    UpdatedBy = emp.UserId ?? 0,
                    UpdatedAt = now
                });
            }

            await _context.SaveChangesAsync();

            return true;
        }
        private async Task SendOne(string token, string title, string body, int type = 0,
            string screen = null,  string data = null)
        {
            var push = new ApplePush(ApplePushType.Alert)
                .AddToken(token)
                .AddAlert(title, body)
                .AddCustomProperty("type", type.ToString())
                .AddCustomProperty("screen", screen)
                .AddSound("default");
            // thêm data
            if (!string.IsNullOrEmpty(data))
            {
                push.AddCustomProperty("data", data);
            }

            var result = await _client.SendAsync(push);

            if (!result.IsSuccessful)
            {
                var reason = result.Reason.ToString();

                Console.WriteLine($"APNS Error: {reason} | Token: {token}");

                // gửi telegram
                _ = Task.Run(() =>
                    Helper.SendTelegramMessageAsync($"APNS Error: {reason} | Token: {token}")
                );

                // ✅ chỉ disable token chết thật
                if (reason == "BadDeviceToken" || reason == "Unregistered")
                {
                    var device = await _context.UserDeviceTokens
                        .FirstOrDefaultAsync(x => x.DeviceToken == token);

                    if (device != null)
                    {
                        device.IsActive = false;
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
        private async Task SendOneDev(string token, string title, string body, int type = 0,
            string screen = null,  string data = null)
        {
            var push = new ApplePush(ApplePushType.Alert)
                .AddToken(token)
                .AddAlert(title, body)
                .AddCustomProperty("type", type.ToString())
                .AddCustomProperty("screen", screen)
                .AddSound("default");
                 push.SendToDevelopmentServer();
            // thêm data
            if (!string.IsNullOrEmpty(data))
            {
                push.AddCustomProperty("data", data);
            }

            var result = await _client.SendAsync(push);

            if (!result.IsSuccessful)
            {
                var reason = result.Reason.ToString();

                Console.WriteLine($"APNS Error: {reason} | Token: {token}");

                // gửi telegram
                _ = Task.Run(() =>
                    Helper.SendTelegramMessageAsync($"APNS Error: {reason} | Token: {token}")
                );

                // ✅ chỉ disable token chết thật
                if (reason == "BadDeviceToken" || reason == "Unregistered")
                {
                    var device = await _context.UserDeviceTokens
                        .FirstOrDefaultAsync(x => x.DeviceToken == token);

                    if (device != null)
                    {
                        device.IsActive = false;
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
        public static IEnumerable<List<T>> SplitList<T>(List<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
            {
                yield return source.GetRange(i, Math.Min(size, source.Count - i));
            }
        }
    }
}