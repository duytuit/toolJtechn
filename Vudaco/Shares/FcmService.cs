using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;

namespace Vudaco.Shares
{
    public class FcmService
    {
        public async Task<BatchResponse> SendMulticastAsync(
            List<string> tokens,
            string title,
            string body,
            Dictionary<string, string> data = null
        )
        {
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

            return await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
        }
    }
}
