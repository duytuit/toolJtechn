namespace JtechnApi.Configuration
{
    public sealed record SidebarMenuItem(string Resource, string Title, string Url, string Icon, string Group = null)
    {
        public string ViewPermission => Resource + ".VIEW";
    }

    // Add a sidebar entry here; permission synchronization reads the same catalog.
    public static class SidebarMenu
    {
        public static IEnumerable<(string Code, string Name, string Category)> RequiredPermissions()
        {
            var actions = new[] { ("VIEW", "Xem"), ("CREATE", "Thêm"), ("UPDATE", "Cập nhật"), ("DELETE", "Xóa") };
            foreach (var menu in Items.DistinctBy(m => m.Resource))
                foreach (var (action, title) in actions)
                    yield return (menu.Resource + "." + action, title + " " + menu.Title, menu.Title);
        }
        public static IReadOnlyList<SidebarMenuItem> Items { get; } = Array.AsReadOnly(new[]
        {
            new SidebarMenuItem("DASHBOARD", "Dashboard", "/admin", "▣"),
            new SidebarMenuItem("HOME", "Trang chủ", "/", "◫"),
            new SidebarMenuItem("EMPLOYEES", "Danh sách nhân viên", "/admin/employees", "◍", "Nhân sự"),
            new SidebarMenuItem("DEPARTMENTS", "Danh sách bộ phận", "/admin/departments", "◍", "Nhân sự"),
            new SidebarMenuItem("EMPLOYEE_DEPARTMENTS", "Bộ phận nhân viên", "/admin/employee-departments", "◍", "Nhân sự"),
            new SidebarMenuItem("ROLES", "Vai trò và quyền", "/admin/roles", "◍", "Nhân sự"),
            new SidebarMenuItem("EMPLOYEE_ROLES", "Gán vai trò nhân viên", "/admin/employee-roles", "◍", "Nhân sự"),
            new SidebarMenuItem("PRODUCTION_PLANS", "Kế hoạch sản xuất", "#", "◌", "Sản xuất"),
            new SidebarMenuItem("PRODUCTION_PROGRESS", "Tiến độ thực hiện", "#", "◌", "Sản xuất"),
            new SidebarMenuItem("DAILY_REPORTS", "Báo cáo ngày", "#", "◎", "Báo cáo"),
            new SidebarMenuItem("INCIDENT_REPORTS", "Báo cáo sự cố", "#", "◎", "Báo cáo"),
            new SidebarMenuItem("ONLINE_EXAMS", "Thi trực tuyến", "/admin/exams", "✓"),
            new SidebarMenuItem("DEPARTMENT_EXAMS", "Bài thi theo bộ phận", "/admin/department-exams", "✓")
        });
    }
}
