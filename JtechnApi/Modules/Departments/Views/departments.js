document.addEventListener('DOMContentLoaded', function () {
    'use strict';
    const apiUrl = '/api/departments';
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    const getErrorMessage = async response => {
        if (response.status === 401) return 'Phiên đăng nhập đã hết. Vui lòng đăng nhập lại.';
        try {
            const body = await response.json();
            if (body.message) return body.message;
            if (body.detail) return body.detail;
            if (body.errors) return Object.values(body.errors).flat().join(' ');
        } catch { /* Use HTTP status if the response is not JSON. */ }
        return `Yêu cầu thất bại (HTTP ${response.status}).`;
    };
    const setMessage = (element, text, isError) => {
        element.textContent = text;
        element.className = `small mb-3 ${isError ? 'text-danger' : 'text-success'}`;
    };

    const table = document.querySelector('#departmentDataTable');
    if (table) {
        const search = document.querySelector('#departmentSearch');
        const count = document.querySelector('#departmentCount');
        const message = document.querySelector('#departmentListMessage');
        let departments = [], deleting = false;
        const render = () => {
            const keyword = search.value.trim().toLocaleLowerCase();
            const visible = departments.filter(department => [department.code, department.name]
                .some(value => String(value || '').toLocaleLowerCase().includes(keyword)));
            count.textContent = `${visible.length} / ${departments.length} bộ phận`;
            message.textContent = '';
            window.JtechnDataTable.setRows('departmentDataTable', visible);
        };
        const loadDepartments = async () => {
            const response = await fetch(`${apiUrl}/all`, { headers: { Accept: 'application/json' } });
            if (!response.ok) throw new Error(await getErrorMessage(response));
            const data = await response.json();
            if (!Array.isArray(data)) throw new Error('API bộ phận không trả về danh sách hợp lệ.');
            departments = data;
            render();
        };
        const pageUrl = new URL(window.location.href);
        const savedMessages = { created: 'Đã thêm bộ phận.', updated: 'Đã cập nhật bộ phận.' };
        const saved = pageUrl.searchParams.get('saved');
        if (savedMessages[saved]) {
            pageUrl.searchParams.delete('saved');
            window.history.replaceState(null, '', pageUrl);
            window.JtechnUI.toast(savedMessages[saved], 'success');
        }
        table.addEventListener('datatable:edit', event => {
            window.location.href = `/admin/departments/${event.detail.row.id}/edit`;
        });
        table.addEventListener('datatable:view', event => {
            window.location.href = `/admin/departments/${event.detail.row.id}/employees`;
        });
        const sync = document.querySelector('#departmentSync');
        sync?.addEventListener('click', async () => {
            if (sync.disabled) return;
            sync.disabled = true;
            try {
                const confirmed = await window.JtechnUI.confirm({
                    title: 'Đồng bộ nhân viên vào bộ phận',
                    message: 'Đối chiếu employee.Department với tên hoặc mã bộ phận. Thao tác chỉ thêm hoặc khôi phục liên kết, giữ nguyên các liên kết đã có. Tiếp tục?',
                    confirmText: 'Đồng bộ', confirmVariant: 'primary'
                });
                if (!confirmed) return;
                const response = await fetch('/api/employees/sync-departments', {
                    method: 'POST', headers: { RequestVerificationToken: token, Accept: 'application/json' }
                });
                if (!response.ok) throw new Error(await getErrorMessage(response));
                const result = await response.json();
                document.querySelector('#departmentSyncPanel').hidden = false;
                document.querySelector('#departmentSyncSummary').textContent = `Thêm mới: ${result.added}; khôi phục: ${result.restored}; đã có: ${result.unchanged}; không có bộ phận: ${result.empty}.`;
                const skipped = document.querySelector('#departmentSyncSkipped'); skipped.replaceChildren();
                if (result.unmatched.length) {
                    const list = document.createElement('ul');
                    result.unmatched.forEach(item => {
                        const row = document.createElement('li'); row.textContent = `${item.department}: ${item.count} nhân viên — ${item.reason}`; list.append(row);
                    });
                    skipped.append(list);
                } else skipped.textContent = 'Tất cả giá trị bộ phận có nội dung đều đã khớp.';
                window.JtechnUI.toast('Đã đồng bộ nhân viên vào bộ phận.', 'success');
                try { await loadDepartments(); }
                catch (error) { window.JtechnUI.toast(`Đã đồng bộ nhưng chưa tải lại được số lượng nhân viên: ${error.message}`, 'warning'); }
            } catch (error) { window.JtechnUI.toast(error.message, 'error'); }
            finally { sync.disabled = false; }
        });
        table.addEventListener('datatable:delete', async event => {
            if (deleting) return;
            deleting = true;
            try {
                const department = event.detail.row;
                const confirmed = await window.JtechnUI.confirm({
                    title: 'Xóa bộ phận', message: `Bạn có chắc muốn xóa bộ phận "${department.name}"?`,
                    confirmText: 'Xóa bộ phận', confirmVariant: 'danger'
                });
                if (!confirmed) return;
                const response = await fetch(`${apiUrl}/${department.id}`, {
                    method: 'DELETE', headers: { RequestVerificationToken: token, Accept: 'application/json' }
                });
                if (!response.ok) throw new Error(await getErrorMessage(response));
                window.JtechnUI.toast('Đã xóa bộ phận.', 'success');
                try { await loadDepartments(); }
                catch (error) { window.JtechnUI.toast(`Đã xóa bộ phận nhưng không thể tải lại danh sách: ${error.message}`, 'warning'); }
            } catch (error) { window.JtechnUI.toast(error.message, 'error'); }
            finally { deleting = false; }
        });
        search.addEventListener('input', render);
        loadDepartments().catch(error => {
            count.textContent = 'Không thể tải dữ liệu từ cơ sở dữ liệu.';
            message.className = 'small mt-3 text-danger';
            message.textContent = error.message;
            window.JtechnDataTable.showMessage('departmentDataTable', error.message, true);
        });
    }

    const employeesPanel = document.querySelector('#departmentEmployeesPanel');
    if (employeesPanel) {
        const loadDepartmentEmployees = async () => {
            try {
                const response = await fetch(`${apiUrl}/${employeesPanel.dataset.departmentId}/employees`, { headers: { Accept: 'application/json' } });
                if (!response.ok) throw new Error(await getErrorMessage(response));
                const result = await response.json();
                document.querySelector('#departmentEmployeesTitle').textContent = `${result.department.code} - ${result.department.name}`;
                document.querySelector('#departmentEmployeesCount').textContent = `${result.employees.length} nhân viên`;
                window.JtechnDataTable.setRows('departmentEmployeesTable', result.employees);
            } catch (error) {
                document.querySelector('#departmentEmployeesTitle').textContent = 'Không thể tải dữ liệu';
                const message = document.querySelector('#departmentEmployeesMessage'); message.className = 'small mt-3 text-danger'; message.textContent = error.message;
                window.JtechnDataTable.showMessage('departmentEmployeesTable', error.message, true);
            }
        };
        let removing = false;
        document.querySelector('#departmentEmployeesTable').addEventListener('datatable:delete', async event => {
            if (removing) return;
            removing = true;
            try {
                const employee = event.detail.row;
                const confirmed = await window.JtechnUI.confirm({
                    title: 'Gỡ nhân viên khỏi bộ phận',
                    message: `Gỡ "${employee.employeeCode} - ${employee.fullName}" khỏi bộ phận này? Hồ sơ nhân viên vẫn được giữ nguyên.`,
                    confirmText: 'Gỡ nhân viên', confirmVariant: 'danger'
                });
                if (!confirmed) return;
                const response = await fetch(`${apiUrl}/${employeesPanel.dataset.departmentId}/employees/${employee.id}`, {
                    method: 'DELETE', headers: { RequestVerificationToken: token, Accept: 'application/json' }
                });
                if (!response.ok) throw new Error(await getErrorMessage(response));
                window.JtechnUI.toast('Đã gỡ nhân viên khỏi bộ phận.', 'success');
                await loadDepartmentEmployees();
            } catch (error) { window.JtechnUI.toast(error.message, 'error'); }
            finally { removing = false; }
        });
        loadDepartmentEmployees();
    }
    const panel = document.querySelector('#departmentFormPanel');
    if (panel) {
        const form = document.querySelector('#departmentForm');
        const fields = document.querySelector('#departmentFields');
        const message = document.querySelector('#departmentMessage');
        const loadMessage = document.querySelector('#departmentLoadMessage');
        const submit = form.querySelector('[type="submit"]');
        const id = panel.dataset.departmentId;
        let saving = false;
        form.elements.namedItem('code').pattern = '[A-Za-z0-9][A-Za-z0-9_-]*';
        const loadDepartment = async () => {
            const response = await fetch(`${apiUrl}/${id}`, { headers: { Accept: 'application/json' } });
            if (!response.ok) throw new Error(await getErrorMessage(response));
            const department = await response.json();
            ['code', 'name', 'description'].forEach(name => {
                const field = form.elements.namedItem(name);
                field.value = department[name] || '';
                field.dispatchEvent(new Event('input', { bubbles: true }));
            });
        };
        form.addEventListener('submit', async event => {
            event.preventDefault();
            if (saving || !form.reportValidity()) return;
            const payload = Object.fromEntries(['code', 'name', 'description'].map(name => [name, form.elements.namedItem(name).value.trim()]));
            if (!payload.name) { setMessage(message, 'Vui lòng nhập tên bộ phận.', true); return; }
            saving = true; submit.disabled = true; fields.disabled = true;
            try {
                const response = await fetch(id ? `${apiUrl}/${id}` : apiUrl, {
                    method: id ? 'PUT' : 'POST',
                    headers: { 'Content-Type': 'application/json', Accept: 'application/json', RequestVerificationToken: token },
                    body: JSON.stringify(payload)
                });
                if (!response.ok) throw new Error(await getErrorMessage(response));
                window.location.assign(`/admin/departments?saved=${id ? 'updated' : 'created'}`);
            } catch (error) {
                setMessage(message, error.message, true);
                window.JtechnUI.toast(error.message, 'error');
                saving = false; submit.disabled = false; fields.disabled = false;
            }
        });
        if (id) {
            fields.disabled = true; submit.disabled = true;
            loadDepartment().then(() => { fields.disabled = false; submit.disabled = false; })
                .catch(error => { setMessage(loadMessage, error.message, true); window.JtechnUI.toast(error.message, 'error'); });
        }
    }
});
