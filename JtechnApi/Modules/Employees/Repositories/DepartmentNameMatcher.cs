using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace JtechnApi.Employees.Repositories
{
    public static class DepartmentNameMatcher
    {
        public static string Normalize(string value)
        {
            var decomposed = (value ?? "").Normalize(NormalizationForm.FormD);
            var text = new string(decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c)
                != UnicodeCategory.NonSpacingMark).ToArray()).Replace('đ', 'd').Replace('Đ', 'D').ToUpperInvariant();
            var key = Regex.Replace(text, @"[^A-Z0-9]+", " ").Trim();
            if (key.StartsWith("PD ", StringComparison.Ordinal)) key = key[3..].Trim();
            key = key.Replace("KY THUAT", "KI THUAT");
            if (key is "GA HUMANRESORCE" or "GA HUMANRESOURCE" or "GA HUMANRESOURCES" or "GA HUMAN RESOURCE" or "GA HUMAN RESOURCES")
                return "GA";
            return key;
        }

        public static int[] Match(string source, IEnumerable<(int Id, string Name, string Code)> departments)
        {
            var key = Normalize(source);
            if (key.Length == 0) return Array.Empty<int>();
            var candidates = departments.Select(d => new { d.Id, Name = Normalize(d.Name), Code = Normalize(d.Code) }).ToArray();
            // Prefer the actual department name over a code or partial match.
            var exact = candidates.Where(d => d.Name == key).Select(d => d.Id).Distinct().ToArray();
            if (exact.Length > 0) return exact;
            var codes = candidates.Where(d => d.Code == key).Select(d => d.Id).Distinct().ToArray();
            if (codes.Length > 0) return codes;
            // Whole-word containment only; never guess from short codes or spelling distance.
            return candidates.Where(d => d.Name.Length >= 3 && (" " + key + " ").Contains(" " + d.Name + " ", StringComparison.Ordinal))
                .Select(d => d.Id).Distinct().ToArray();
        }
    }
}
