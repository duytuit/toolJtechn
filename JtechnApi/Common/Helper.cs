

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace JtechnApi.Shares
{
    public static class Helper
    {
      
        public static List<Dictionary<string, object>> ConfigFormType(int type)
        {
            var result = new List<Dictionary<string, object>>();
            switch (type)
            {
                case 1:
                    result = new List<Dictionary<string, object>>
                                {
                                    new Dictionary<string, object>
                                    {
                                        ["from_dept"] = 0,
                                        ["to_dept"] = new List<int> { 5,7,4,3,2,6 },
                                        ["confirm_by_type"] = "",
                                        ["confirm_from_dept"] = 0,
                                        ["confirm_to_dept"] = 2,
                                        ["confirm_by_from_dept"] = new List<int> { 3 },
                                        ["confirm_by_to_dept"] = new List<int> { 4, 5 },
                                        ["user_cat"] = new List<string> { "240929" },
                                        ["user_dap"] = new List<string> { "130764" },
                                        ["user_cam"] = new List<string> { "130206"},
                                        ["user_buredo"] = new List<string> { "140511"},
                                        ["user_laprap"] = new List<string> { "10281" },
                                        ["user_kiemtra"] = new List<string> { "131078"},
                                        ["data_table"] = new Dictionary<string, string>
                                        {
                                            ["code"] = "",
                                            ["quantity"] = "",
                                            ["size"] = "",
                                            ["unit_price"] = "",
                                            ["location_c"] = "",
                                            ["usage_status"] = ""
                                        }
                                    }
                                };
                    break;

                case 2:
                    result = new List<Dictionary<string, object>>
                                {
                                    new Dictionary<string, object>
                                    {
                                        ["id"] = 2,
                                        ["from_dept"] = new List<int> { 9, 10 },
                                        ["to_dept"] = new List<int> { 6 },
                                        ["confirm_by_type"] = new List<int> { 1 },
                                        ["confirm_from_dept"] = 1,
                                        ["confirm_to_dept"] = 1,
                                        ["confirm_by_from_dept"] = new List<int> { 2 },
                                        ["confirm_by_to_dept"] = new List<int> { 3 },
                                        ["data_table"] = new Dictionary<string, string>
                                        {
                                            ["code"] = "",
                                            ["quantity"] = "",
                                            ["note"] = ""
                                        }
                                    }
                                };
                    break;

                default:
                    result = new List<Dictionary<string, object>>();
                    break;
            }
            return result;
        }
        public static object ConfigRequiredByType(int type)
        {
            var result = new object();
            switch (type)
            {
                case 1:
                    result = new
                    {
                        from_dept = 0,
                        to_dept = new List<int> { 5, 7, 4, 3, 2, 6 },
                        confirm_by_type = "",
                        confirm_from_dept = 0,
                        confirm_to_dept = 2,
                        confirm_by_from_dept = new List<int> { 3 },
                        confirm_by_to_dept = new List<int> { 4, 5 },
                        emp_dept = new[]
                                    {
                                        new {
                                            id_dept = 5,
                                            code_emp = new List<int> { 240929, 240923 }
                                        },
                                        new {
                                            id_dept = 7,
                                            code_emp = new List<int> { 240930, 240931 }
                                        },
                                        new {
                                            id_dept = 4,
                                            code_emp = new List<int> { 240929, 240923 }
                                        },
                                        new {
                                            id_dept = 3,
                                            code_emp = new List<int> { 240930, 240931 }
                                        },
                                        new {
                                            id_dept = 2,
                                            code_emp = new List<int> { 240929, 240923 }
                                        },
                                        new {
                                            id_dept = 6,
                                            code_emp = new List<int> { 240930, 240931 }
                                        }
                                    }
                    };
                    break;
                case 2:
                    result = new
                    {
                        from_dept = 0,
                        to_dept = new List<int> { 5, 7, 4, 3, 2, 6 },
                        confirm_by_type = "",
                        confirm_from_dept = 0,
                        confirm_to_dept = 2,
                        confirm_by_from_dept = new List<int> { 3 },
                        confirm_by_to_dept = new List<int> { 4, 5 },
                        emp_dept = new[]
                                    {
                                        new {
                                            id_dept = 5,
                                            code_emp = new List<int> { 240929, 240923 },
                                            group="Khánh"
                                        },
                                        new {
                                            id_dept = 7,
                                            code_emp = new List<int> { 240930, 240931 },
                                            group="Cường"
                                        },
                                        new {
                                            id_dept = 4,
                                            code_emp = new List<int> { 240929, 240923 },
                                            group="Hải"
                                        },
                                        new {
                                            id_dept = 3,
                                            code_emp = new List<int> { 240930, 240931 },
                                            group="Quỳnh"
                                        },
                                       //new {
                                       //    id_dept = 2,
                                       //    code_emp = new List<int> { 240929, 240923 },
                                       //    group="Tâm"
                                       //},
                                       //new {
                                       //    id_dept = 6,
                                       //    code_emp = new List<int> { 240930, 240931 },
                                       //    group="Pawa"
                                       //}
                                    },
                        work_group = new[]
                                    {
                                        new {
                                            id=211,
                                            group="Khánh"
                                        },
                                        new {
                                             id=310,
                                            group="Cường"
                                        },
                                        new {
                                             id=288,
                                            group="Hải"
                                        },
                                        new {
                                             id=113,
                                            group="Quỳnh"
                                        },
                                       // new {
                                       //      id=107,
                                       //     group="Tâm"
                                       // },
                                       // new {
                                       //     id=0,
                                       //     group="Pawa"
                                       // }
                                    },
                    };
                    break;
                case 3:
                    result = new
                    {
                        from_dept = 0,
                        to_dept = new List<int> { 5, 7, 4, 3, 2, 6 },
                        confirm_by_type = "",
                        confirm_from_dept = 0,
                        confirm_to_dept = 2,
                        confirm_by_from_dept = new List<int> { 3 },
                        confirm_by_to_dept = new List<int> { 4, 5 },
                        emp_dept = new[]
                                    {
                                        new {
                                            id_dept = 5,
                                            code_emp = new List<int> { 240929, 240923 },
                                            group="Khánh"
                                        },
                                        new {
                                            id_dept = 7,
                                            code_emp = new List<int> { 240930, 240931 },
                                            group="Cường"
                                        },
                                        new {
                                            id_dept = 4,
                                            code_emp = new List<int> { 240929, 240923 },
                                            group="Hải"
                                        },
                                        new {
                                            id_dept = 3,
                                            code_emp = new List<int> { 240930, 240931 },
                                            group="Quỳnh"
                                        }
                                    },
                        work_group = new[]
                                    {
                                        new {
                                            id=128,
                                            group="Huệ"
                                        },
                                        new {
                                             id=149,
                                            group="Bích Liên"
                                        },
                                        new {
                                             id=184,
                                            group="Uyên"
                                        },
                                        new {
                                             id=341,
                                            group="Yến"
                                        }
                                    },
                    };
                    break;
                default:
                    break;
            }
            return result;
        }
        public static async Task<UploadResult> ProcessExamImageAsync(IFormFile file, string webRoot)
        {
            if (file == null || file.Length == 0) return new UploadResult(false, "File rỗng.");
            if (file.Length > 10 * 1024 * 1024) return new UploadResult(false, "Ảnh tối đa 10 MB.");
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(extension))
                return new UploadResult(false, "Chỉ hỗ trợ ảnh JPG, JPEG, PNG, GIF, WEBP.");
            await using var source = file.OpenReadStream();
            var header = new byte[12];
            var count = 0;
            while (count < header.Length)
            {
                var read = await source.ReadAsync(header, count, header.Length - count);
                if (read == 0) break;
                count += read;
            }
            var valid = extension switch
            {
                ".jpg" or ".jpeg" => count >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff,
                ".png" => count >= 8 && header.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
                ".gif" => count >= 6 && (System.Text.Encoding.ASCII.GetString(header, 0, 6) is "GIF87a" or "GIF89a"),
                ".webp" => count >= 12 && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP",
                _ => false
            };
            if (!valid) return new UploadResult(false, "Nội dung file không đúng định dạng ảnh.");
            var name = Guid.NewGuid().ToString("N") + extension;
            var directory = Path.Combine(webRoot, "img", "exams");
            Directory.CreateDirectory(directory);
            await using var target = new FileStream(Path.Combine(directory, name), FileMode.CreateNew);
            await target.WriteAsync(header, 0, count);
            await source.CopyToAsync(target);
            return new UploadResult(true, "OK", "/img/exams/" + name);
        }

        public static async Task<UploadResult> ProcessFileAsync(IFormFile file, string folder, string webRoot)
        {
            long _fileSizeLimit = 50 * 1024 * 1024;            // 50 MB
            string[] _permittedExtensions = { ".jpg", ".png", ".pdf" ,".xls",".xlsx"};
            if (file == null || file.Length == 0)
                return new UploadResult(false, "File rỗng.");

            if (file.Length > _fileSizeLimit)
                return new UploadResult(false, "File quá lớn (max 50 MB).");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            bool found = Array.IndexOf(_permittedExtensions, ext) >= 0;
            if (!found)
                return new UploadResult(false, $"Không hỗ trợ định dạng {ext}.");

            var segments = (folder ?? "").Trim().Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Any(segment => segment == "." || segment == ".." ||
                segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
                return new UploadResult(false, "Thư mục không hợp lệ.");
            var filesRoot = Path.GetFullPath(Path.Combine(webRoot, "files"));
            var targetFolder = Path.GetFullPath(Path.Combine(new[] { filesRoot }.Concat(segments).ToArray()));
            if (targetFolder != filesRoot && !targetFolder.StartsWith(filesRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return new UploadResult(false, "Thư mục không hợp lệ.");

            // tạo thư mục (nếu chưa có)
            Directory.CreateDirectory(targetFolder);
            var fileName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(targetFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.CreateNew);
            await file.CopyToAsync(stream);

            // Đường dẫn trả về cho client
            var relativePath = "/files/" + string.Join("/", segments.Select(Uri.EscapeDataString).Append(fileName));

            return new UploadResult(true, "OK", $"{relativePath}");
        }
        public static string GetClientInfo(IHttpContextAccessor accessor, string clientNameFromBody = null)
        {
            var context = accessor.HttpContext;

            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                     ?? context.Connection.RemoteIpAddress?.ToString();

            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var clientNameFromHeader = context.Request.Headers["X-Client-Name"].ToString();

            string hostName = null;
            try
            {
                var ipAddr = context.Connection.RemoteIpAddress;
                if (ipAddr != null && !IPAddress.IsLoopback(ipAddr))
                {
                    var entry = Dns.GetHostEntry(ipAddr);
                    hostName = entry.HostName;
                }
            }
            catch
            {
                hostName = "Unable to resolve";
            }

            var result = new
            {
                ipAddress = ip,
                userAgent = userAgent,
                clientName_FromHeader = clientNameFromHeader,
                clientName_FromBody = clientNameFromBody,
                hostname_Resolved = hostName
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        public static string RemoteLampSync(RemoteLampRequest request, int timeoutSeconds = 30)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds); // set timeout

                var postFields = new Dictionary<string, string>
                {
                    ["Event"] = request.Event.ToString(),
                    ["Chanel"] = request.Chanel,
                    ["Status"] = request.Status.ToString(),
                    ["MessageText"] = request.MessageText,
                    ["Mode"] = request.Mode.ToString()
                };

                var content = new FormUrlEncodedContent(postFields);

                // Đồng bộ bằng cách dùng .GetAwaiter().GetResult()
                var response = client.PostAsync("http://192.168.207.6:8092/remotelamp/control-lamp", content)
                                     .GetAwaiter().GetResult();

                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }
    }
        public class UploadResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string Path { get; set; }

            public UploadResult(bool success, string message=null, string path=null)
            {
                Success = success;
                Message = message;
                Path = path;
            }
        }
    public class RemoteLampRequest
    {
        public int Event { get; set; }
        public string Chanel { get; set; }
        public int Status { get; set; }
        public string MessageText { get; set; }
        public int Mode { get; set; }
    }
}
