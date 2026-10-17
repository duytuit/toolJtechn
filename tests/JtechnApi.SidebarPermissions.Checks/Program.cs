using JtechnApi.Configuration;
var permissions = SidebarMenu.RequiredPermissions().ToList();
if (permissions.Select(p => p.Code).Distinct().Count() != permissions.Count) throw new Exception("Duplicate permission codes");
foreach (var menu in SidebarMenu.Items)
{
    foreach (var action in new[] { "VIEW", "CREATE", "UPDATE", "DELETE" })
        if (permissions.Count(p => p.Code == menu.Resource + "." + action) != 1) throw new Exception("Missing CRUD permission: " + menu.Resource);
    if (!permissions.Any(p => p.Code == menu.ViewPermission)) throw new Exception("Menu must use its VIEW permission");
}
if (permissions.Any(p => p.Code.Length > 100 || p.Name.Length > 150 || p.Category.Length > 100)) throw new Exception("Permission exceeds database limits");
Console.WriteLine($"Validated {SidebarMenu.Items.Count} sidebar items and {permissions.Count} generated permissions.");
