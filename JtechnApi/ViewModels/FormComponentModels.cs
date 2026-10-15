namespace JtechnApi.ViewModels
{
    public class CompactInputModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; } = "text";
        public string Value { get; set; }
        public string Placeholder { get; set; }
        public string Autocomplete { get; set; }
        public bool Required { get; set; }
        public string CssClass { get; set; }
    }

    public class CompactSelectOption
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class CompactSelectModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public bool Required { get; set; }
        public string CssClass { get; set; }
        public IEnumerable<CompactSelectOption> Options { get; set; } = Array.Empty<CompactSelectOption>();
    }

    public class DateTimeRangeModel
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string InputType { get; set; } = "datetime-local";
        public string StartLabel { get; set; } = "Từ ngày giờ";
        public string EndLabel { get; set; } = "Đến ngày giờ";
        public string StartName { get; set; }
        public string EndName { get; set; }
        public string StartValue { get; set; }
        public string EndValue { get; set; }
    }

    public class FilterDropdownModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Placeholder { get; set; } = "Nhập để lọc...";
        public string Value { get; set; }
        public bool Required { get; set; }
        public IEnumerable<CompactSelectOption> Options { get; set; } = Array.Empty<CompactSelectOption>();
    }

    public class ApiFilterDropdownModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Endpoint { get; set; }
        public string QueryParameter { get; set; } = "q";
        public string ValueField { get; set; } = "id";
        public string LabelField { get; set; } = "name";
        public string DataPath { get; set; }
        public string Placeholder { get; set; } = "Nhập để tìm...";
        public bool Required { get; set; }
        public int MinCharacters { get; set; } = 0;
        public int DebounceMilliseconds { get; set; } = 250;
    }
}
