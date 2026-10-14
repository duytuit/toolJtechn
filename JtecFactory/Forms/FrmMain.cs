using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JtecFactory.Navigation;
using MenuItem = JtecFactory.Navigation.MenuItem;

namespace JtecFactory.Forms
{
    public sealed partial class FrmMain : Form
    {
        private readonly IMenuService menuService;
        private readonly IPermissionService permissionService;
        private readonly NavigationManager navigationManager = new NavigationManager();
        private readonly Dictionary<string, Func<MenuItem, IPermissionService, Action, Form>> formFactories =
            new Dictionary<string, Func<MenuItem, IPermissionService, Action, Form>>(StringComparer.OrdinalIgnoreCase);
        private Control currentView;

        public bool LogoutRequested { get; private set; }

        public FrmMain()
            : this(new MenuService(), null, false, null)
        {
        }

        public FrmMain(bool administratorMode)
            : this(new MenuService(), null, administratorMode, null)
        {
        }

        public FrmMain(int employeeId, bool administratorMode)
            : this(new MenuService(), null, administratorMode, employeeId)
        {
        }

        public FrmMain(IMenuService menuService, IPermissionService permissionService)
            : this(menuService, permissionService, false, null)
        {
        }

        private FrmMain(IMenuService menuService, IPermissionService permissionService, bool administratorMode, int? employeeId)
        {
            this.menuService = menuService ?? throw new ArgumentNullException("menuService");
            this.permissionService = permissionService ??
                (LicenseManager.UsageMode == LicenseUsageMode.Designtime
                    ? (IPermissionService)new MockPermissionService(menuService)
                    : new PermissionService(menuService, administratorMode));
            if (!administratorMode && employeeId.HasValue)
            {
                int roleId = new Services.EmployeeService()
                    .GetRoleIdsForEmployee(employeeId.Value)
                    .FirstOrDefault();
                this.permissionService.SetCurrentRole(roleId);
            }
            RegisterFunctionForm("FrmProduct", (menu, permissions, back) => new FrmProduct(menu, permissions, back));
            RegisterFunctionForm("FrmRenderQRNgoaiQuan", (menu, permissions, back) => new FrmRenderQRNgoaiQuan(menu, permissions, back));
            RegisterFunctionForm("FrmRolePermission", (menu, permissions, back) => new FrmRolePermission(menuService, permissions, back));
            RegisterFunctionForm("FrmEmployee", (menu, permissions, back) => new FrmEmployee(menu, permissions, back));
            RegisterFunctionForm("FrmEmployeeRoles", (menu, permissions, back) => new FrmEmployeeRoles(menu, permissions, back));
            RegisterFunctionForm("FrmRole", (menu, permissions, back) => new FrmRole(menu, permissions, back));

            InitializeComponent();
            sidebar.ParentSelected += ParentMenuSelected;
            sidebar.LogoutRequested += LogoutButtonClicked;
            this.permissionService.PermissionsChanged += PermissionsChanged;
            List<MenuItem> visibleParents = RefreshParentMenus();

            MenuItem initialMenu = visibleParents.FirstOrDefault(menu => menu.Code == "dashboard") ?? visibleParents.FirstOrDefault();
            if (initialMenu != null)
                ShowParentMenu(initialMenu);
            else
                ShowEmptyView("No accessible modules", "Your current role has no visible modules.");
        }

        public NavigationManager Navigation { get { return navigationManager; } }

        private void LogoutButtonClicked(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            LogoutRequested = true;
            Close();
        }

        public void RegisterFunctionForm(string formName, Func<MenuItem, IPermissionService, Action, Form> factory)
        {
            if (string.IsNullOrWhiteSpace(formName))
                throw new ArgumentException("A form name is required.", "formName");
            if (factory == null)
                throw new ArgumentNullException("factory");
            formFactories[formName] = factory;
        }

        private List<MenuItem> RefreshParentMenus()
        {
            List<MenuItem> visibleParents = menuService.GetParentMenus()
                .Where(parent => permissionService.CanView(parent.Id))
                .Where(parent => parent.FormName != null || menuService.GetChildMenus(parent.Id).Any(child => permissionService.CanView(child.Id)))
                .ToList();
            sidebar.BindParentMenus(visibleParents);

            if (navigationManager.CurrentParentMenu != null && visibleParents.All(menu => menu.Id != navigationManager.CurrentParentMenu.Id))
            {
                MenuItem home = visibleParents.FirstOrDefault(menu => menu.Code == "dashboard");
                if (home != null)
                    ShowParentMenu(home);
                else
                {
                    navigationManager.SelectParent(null);
                    ShowEmptyView("No accessible modules", "Your current role has no visible modules.");
                }
            }

            return visibleParents;
        }

        private void PermissionsChanged(object sender, EventArgs e)
        {
            RefreshParentMenus();
            MenuItem child = navigationManager.CurrentChildMenu;
            if (child != null && navigationManager.CurrentFunctionForm != null && !permissionService.CanView(child.Id))
            {
                navigationManager.CurrentFunctionForm.Close();
                ReturnToChildMenu();
                return;
            }

            if (navigationManager.CurrentParentMenu != null && navigationManager.CurrentFunctionForm == null)
                ShowParentMenu(navigationManager.CurrentParentMenu);
        }

        private void ParentMenuSelected(MenuItem menu)
        {
            ShowParentMenu(menu);
        }

        private void ShowParentMenu(MenuItem parent)
        {
            if (parent == null || !permissionService.CanView(parent.Id))
                return;

            navigationManager.SelectParent(parent);
            sidebar.SetActiveParent(parent.Id);

            List<MenuItem> children = menuService.GetChildMenus(parent.Id)
                .Where(child => permissionService.CanView(child.Id))
                .ToList();

            if (children.Count > 0)
            {
                ShowChildMenu(parent, children, null);
                return;
            }

            if (!string.IsNullOrWhiteSpace(parent.FormName))
            {
                OpenFunction(parent, parent.FormName, parent.OpenMode);
                return;
            }

            ShowEmptyView(parent.Name, "There are no functions available for this module.");
        }

        private void ShowChildMenu(MenuItem parent, IEnumerable<MenuItem> children, int? selectedChildId)
        {
            TableLayoutPanel page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(30, 24, 24, 20)
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel heading = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Label title = new Label
            {
                Text = parent.Name,
                AutoSize = true,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0)
            };
            Label description = new Label
            {
                Text = parent.Description,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(1, 43)
            };
            heading.Controls.Add(title);
            heading.Controls.Add(description);

            FlowLayoutPanel cards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 8, 8, 8)
            };
            foreach (MenuItem child in children)
            {
                MenuItem selectedMenu = child;
                MenuCard card = new MenuCard(selectedMenu) { IsSelected = selectedChildId == selectedMenu.Id };
                card.Selected += delegate { OpenChildMenu(selectedMenu); };
                cards.Controls.Add(card);
            }

            page.Controls.Add(heading, 0, 0);
            page.Controls.Add(cards, 0, 1);
            ShowView(page);
        }

        private void OpenChildMenu(MenuItem menu)
        {
            MenuItem parent = navigationManager.CurrentParentMenu;
            if (parent == null || menu.ParentId != parent.Id || !permissionService.CanView(menu.Id))
                return;

            MenuItem authorizedMenu = menuService.GetChildMenus(parent.Id).FirstOrDefault(child => child.Id == menu.Id);
            if (authorizedMenu == null)
                return;

            navigationManager.SelectChild(authorizedMenu);
            OpenFunction(authorizedMenu, authorizedMenu.FormName, authorizedMenu.OpenMode);
        }

        private void OpenFunction(MenuItem menu, string formName, FormOpenMode openMode)
        {
            if (!permissionService.CanView(menu.Id))
                return;

            Action backAction = null;
            if (openMode != FormOpenMode.NewWindow)
                backAction = ReturnToChildMenu;
            Form form = CreateFunctionForm(menu, formName, backAction);
            navigationManager.SetCurrentFunctionForm(form);

            if (openMode == FormOpenMode.NewWindow)
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormClosed += delegate
                {
                    if (navigationManager.CurrentFunctionForm == form)
                        navigationManager.SetCurrentFunctionForm(null);
                };
                form.Show(this);
                return;
            }

            ShowEmbeddedForm(form);
        }

        private Form CreateFunctionForm(MenuItem menu, string formName, Action backAction)
        {
            if (string.Equals(formName, "FrmDashboard", StringComparison.OrdinalIgnoreCase))
                return new FrmDashboard();

            Func<MenuItem, IPermissionService, Action, Form> factory;
            if (!string.IsNullOrWhiteSpace(formName) && formFactories.TryGetValue(formName, out factory))
                return factory(menu, permissionService, backAction);

            return new GenericFunctionForm(menu, permissionService, backAction);
        }

        private void ReturnToChildMenu()
        {
            MenuItem parent = navigationManager.CurrentParentMenu;
            MenuItem child = navigationManager.CurrentChildMenu;
            navigationManager.ReturnToChildMenu();
            if (parent == null)
                return;

            List<MenuItem> visibleChildren = menuService.GetChildMenus(parent.Id)
                .Where(item => permissionService.CanView(item.Id))
                .ToList();
            ShowChildMenu(parent, visibleChildren, child == null ? (int?)null : child.Id);
        }

        private void ShowEmbeddedForm(Form form)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            ShowView(form);
            form.Show();
        }

        private void ShowEmptyView(string titleText, string messageText)
        {
            Panel view = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(30) };
            Label title = new Label
            {
                Text = titleText,
                AutoSize = true,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(30, 24)
            };
            Label message = new Label
            {
                Text = messageText,
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(31, 72)
            };
            view.Controls.Add(title);
            view.Controls.Add(message);
            ShowView(view);
        }

        private void ShowView(Control view)
        {
            if (currentView != null)
            {
                contentPanel.Controls.Remove(currentView);
                currentView.Dispose();
            }

            currentView = view;
            view.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(view);
            view.BringToFront();
        }
    }
}