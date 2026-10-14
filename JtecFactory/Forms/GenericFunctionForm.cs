using System.Drawing;
using System.Windows.Forms;
using JtecFactory.Navigation;
using MenuItem = JtecFactory.Navigation.MenuItem;

namespace JtecFactory.Forms
{
    internal sealed class GenericFunctionForm : BaseFunctionForm
    {
        internal GenericFunctionForm(MenuItem menu, IPermissionService permissionService, System.Action backAction)
            : base(menu.Name, menu.Description, menu.Id, permissionService, backAction)
        {
            Label placeholder = new Label
            {
                Text = menu.Name + " workspace",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14F),
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            ContentPanel.Controls.Add(placeholder);
        }
    }
}