using JtechnApi.Employees.Repositories;

var names = new[] { "QLSX", "GA", "Cắt", "Lắp ráp", "Kĩ thuật", "Buredo", "KTNQ", "Dập", "KTTM", "Cắm", "Kho xuất", "Kho Nhập" };
var departments = names.Select((name, index) => (Id: index + 1, Name: name, Code: "BP" + index)).ToArray();
var cases = new[] {
    ("PD-QLSX", "QLSX"), ("GA- Humanresorce", "GA"), ("PD-Cắt", "Cắt"), ("PD-Lắp ráp", "Lắp ráp"),
    ("PD-Kĩ thuật", "Kĩ thuật"), ("PD-Buredo", "Buredo"), ("PD-KTNQ", "KTNQ"), ("PD-Dập", "Dập"),
    ("PD-KTTM", "KTTM"), ("PD-Cắm", "Cắm"), ("PD-Kho xuất", "Kho xuất"), ("PD-Kho Nhập", "Kho Nhập"),
    ("KTTM", "KTTM"), ("KHO XUẤT", "Kho xuất"), ("KTNQ", "KTNQ"), ("LẮP RÁP", "Lắp ráp"), ("QLSX", "QLSX"),
    ("  pd - kỹ   thuật ", "Kĩ thuật"), ("Nhân viên kho xuất", "Kho xuất")
};
foreach (var (source, expected) in cases)
{
    var ids = DepartmentNameMatcher.Match(source, departments);
    if (ids.Length != 1 || departments.Single(d => d.Id == ids[0]).Name != expected)
        throw new Exception("Incorrect mapping: " + source);
}
if (DepartmentNameMatcher.Match("Kho xuất / Kho nhập", departments).Length != 2) throw new Exception("Ambiguity must be reported");
if (DepartmentNameMatcher.Match("KT", departments).Length != 0) throw new Exception("Short names must not be guessed");
if (DepartmentNameMatcher.Match("", departments).Length != 0) throw new Exception("Empty source must be skipped");
if (DepartmentNameMatcher.Match("Unknown", departments).Length != 0) throw new Exception("Unknown source must be skipped");
Console.WriteLine("23 matching checks passed.");
