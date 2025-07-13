

using System.Collections.Generic;
using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace JtechnApi.Shares
{
    public static class Helper
    {
        public static List<Dictionary<string, object>>  ConfigFormType(int type)
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
                        emp_dept =  new[]
                                    {
                                        new {
                                            id_dept = 2,
                                            code_emp = new List<int> { 240929, 240923 }
                                        },
                                        new {
                                            id_dept = 3,
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
                        user_cat = new List<string> { "240929" },
                        user_dap = new List<string> { "130764" },
                        user_cam = new List<string> { "130206" },
                        user_buredo = new List<string> { "140511" },
                        user_laprap = new List<string> { "10281" },
                        user_kiemtra = new List<string> { "131078" },
                        data_table = new
                        {
                            code = "",
                            quantity = "",
                            size = "",
                            unit_price = "",
                            location_c = "",
                            usage_status = ""
                        }
                    };
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}