

using System.Collections.Generic;
using System.Text.Json;

namespace JtechnApi.Shares
{
    public static class Helper
    {
        public static string ConfigFormType(int type)
        {
            object result = null;
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
                    result = new { error = "Unknown type" };
                    break;
            }

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}