using System;

namespace JtecFactory.Navigation
{
    public enum FormOpenMode
    {
        Embedded,
        NewWindow
    }

    public sealed class MenuItem
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string FormName { get; set; }
        public FormOpenMode OpenMode { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; }
    }

    public sealed class MenuPermission
    {
        public int MenuId { get; set; }
        public int RoleId { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }

        public MenuPermission Copy()
        {
            return new MenuPermission
            {
                MenuId = MenuId,
                RoleId = RoleId,
                CanView = CanView,
                CanAdd = CanAdd,
                CanEdit = CanEdit,
                CanDelete = CanDelete,
                CanExport = CanExport
            };
        }
    }

    public sealed class RoleInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}