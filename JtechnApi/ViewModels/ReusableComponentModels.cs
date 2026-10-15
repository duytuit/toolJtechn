using System.Collections.Generic;

namespace JtechnApi.ViewModels
{
    public class DataTableModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Caption { get; set; }
        public string SearchPlaceholder { get; set; } = "Tìm kiếm...";
        public string EmptyMessage { get; set; } = "Không có dữ liệu phù hợp.";
        public int PageSize { get; set; } = 10;
        public bool EnableSearch { get; set; } = true;
        public bool EnablePagination { get; set; } = true;
        public bool EnableEdit { get; set; }
        public bool EnableDelete { get; set; }
        public IEnumerable<DataTableColumnModel> Columns { get; set; } = new List<DataTableColumnModel>();
    }

    public class DataTableColumnModel
    {
        public string Field { get; set; }
        public string Header { get; set; }
        public string Format { get; set; }
        public string SecondaryField { get; set; }
        public string CssClass { get; set; }
    }

    public class EntityFormModel
    {
        public string Id { get; set; }
        public string PanelId { get; set; }
        public string Title { get; set; }
        public string Endpoint { get; set; }
        public string IdFieldName { get; set; } = "__entityId";
        public string EntityKeyField { get; set; } = "id";
        public string InsertTitle { get; set; } = "Thêm mới";
        public string UpdateTitle { get; set; } = "Cập nhật";
        public string InsertButtonText { get; set; } = "Lưu";
        public string UpdateButtonText { get; set; } = "Cập nhật";
        public string CancelButtonText { get; set; } = "Hủy";
        public string MessageId { get; set; }
        public bool ShowCancelButton { get; set; }
        public IEnumerable<EntityFormFieldModel> Fields { get; set; } = new List<EntityFormFieldModel>();
    }

    public class EntityFormFieldModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; } = "text";
        public string Placeholder { get; set; }
        public string Value { get; set; }
        public string Endpoint { get; set; }
        public string QueryParameter { get; set; } = "q";
        public string ValueField { get; set; } = "id";
        public string LabelField { get; set; } = "name";
        public string DataPath { get; set; }
        public bool Required { get; set; }
        public IEnumerable<CompactSelectOption> Options { get; set; } = new List<CompactSelectOption>();
    }
}
