document.addEventListener('DOMContentLoaded', function () {
    'use strict';
    const panel = document.querySelector('#employeeDepartmentPanel');
    if (!panel) return;
    const form = document.querySelector('#employeeDepartmentForm');
    const employee = document.querySelector('#assignmentEmployee');
    const employeePicker = document.querySelector('#assignmentEmployeePicker');
    const employeeSearch = document.querySelector('#assignmentEmployee-search');
    const fields = document.querySelector('#assignmentFields');
    const container = document.querySelector('#assignmentDepartments');
    const search = document.querySelector('#assignmentSearch');
    const count = document.querySelector('#assignmentCount');
    const message = document.querySelector('#assignmentMessage');
    const loadMessage = document.querySelector('#assignmentLoadMessage');
    const submit = document.querySelector('#assignmentSave');
    const token = form.querySelector('[name="__RequestVerificationToken"]').value;
    let employees = [], departments = [], ready = false, saving = false, dirty = false, generation = 0, selectedEmployee = '';

    const getErrorMessage = async response => {
        if (response.status === 401) return 'Phiên đăng nhập đã hết. Vui lòng đăng nhập lại.';
        if (response.status === 403) return 'Bạn không có quyền thực hiện thao tác này.';
        const body = await response.json().catch(() => ({}));
        return body.message || (body.errors && Object.values(body.errors).flat().join(' ')) || `Yêu cầu thất bại (HTTP ${response.status}).`;
    };
    const setMessage = (element, text, error = false) => {
        element.textContent = text;
        element.className = `small mb-3 ${error ? 'text-danger' : 'text-muted'}`;
    };
    const setEditable = () => {
        const active = employees.find(item => String(item.id) === employee.value)?.isActive;
        const allowed = ready && active && panel.dataset.canUpdate === 'true' && !saving;
        fields.disabled = !allowed;
        if (submit) submit.disabled = !allowed;
    };
    const filter = () => {
        const keyword = search.value.trim().toLocaleLowerCase();
        let visible = 0;
        container.querySelectorAll('[data-department]').forEach(item => {
            item.hidden = !item.textContent.toLocaleLowerCase().includes(keyword);
            if (!item.hidden) visible++;
        });
        document.querySelector('#assignmentEmpty').hidden = visible > 0;
        count.textContent = `Đã chọn ${container.querySelectorAll('input:checked').length} / ${departments.length} bộ phận`;
    };
    const render = selected => {
        container.replaceChildren();
        departments.forEach(department => {
            const column = document.createElement('div');
            column.className = 'col-md-6 col-xl-4'; column.dataset.department = department.id;
            const label = document.createElement('label'); label.className = 'form-check border rounded p-3 ps-5 w-100';
            const input = document.createElement('input'); input.type = 'checkbox'; input.className = 'form-check-input';
            input.value = department.id; input.checked = selected.includes(department.id);
            const text = document.createElement('span'); text.textContent = `${department.code} - ${department.name}`;
            label.append(input, text); column.append(label); container.append(column);
        });
        filter();
    };
    const loadAssignment = async () => {
        selectedEmployee = employee.value;
        const current = ++generation;
        ready = false; dirty = false; setEditable(); render([]); setMessage(message, '');
        if (!employee.value) { setMessage(loadMessage, 'Chọn nhân viên để xem bộ phận.'); return; }
        setMessage(loadMessage, 'Đang tải bộ phận nhân viên...');
        try {
            const response = await fetch(`/api/employees/${employee.value}/departments`, { headers: { Accept: 'application/json' } });
            if (!response.ok) throw new Error(await getErrorMessage(response));
            const data = await response.json();
            if (current !== generation) return;
            render(data.departments.map(item => item.id)); ready = true; setEditable();
            const active = employees.find(item => String(item.id) === employee.value)?.isActive;
            setMessage(loadMessage, !active ? 'Nhân viên đã ngừng hoạt động. Chỉ có thể xem bộ phận.' : panel.dataset.canUpdate !== 'true' ? 'Bạn đang có quyền xem.' : 'Chọn hoặc bỏ chọn bộ phận rồi nhấn Lưu.');
        } catch (error) {
            if (current !== generation) return;
            setMessage(loadMessage, error.message, true); window.JtechnUI.toast(error.message, 'error');
        }
    };
    employee.addEventListener('change', async () => {
        if (dirty) {
            employeePicker.disabled = true;
            try {
                const confirmed = await window.JtechnUI.confirm({
                    title: 'Thay đổi chưa lưu', message: 'Bạn muốn bỏ các lựa chọn chưa lưu để chuyển nhân viên?',
                    confirmText: 'Bỏ thay đổi', confirmVariant: 'danger'
                });
                if (!confirmed) {
                    employee.value = selectedEmployee;
                    const previous = employees.find(item => String(item.id) === selectedEmployee);
                    employeeSearch.value = previous ? `${previous.employeeCode} - ${previous.fullName}` : '';
                    setEditable();
                    return;
                }
            } finally { employeePicker.disabled = false; }
        }
        await loadAssignment();
    });
    employeeSearch.addEventListener('input', setEditable);
    search.addEventListener('input', filter);
    container.addEventListener('change', () => { dirty = true; filter(); });
    form.addEventListener('submit', async event => {
        event.preventDefault();
        if (!ready || !employee.value || saving || !submit || submit.disabled || !form.reportValidity()) return;
        saving = true; employeePicker.disabled = true; setEditable();
        try {
            const departmentIds = [...container.querySelectorAll('input:checked')].map(input => Number(input.value));
            const response = await fetch(`/api/employees/${employee.value}/departments`, {
                method: 'PUT', headers: { 'Content-Type': 'application/json', Accept: 'application/json', RequestVerificationToken: token },
                body: JSON.stringify({ departmentIds })
            });
            if (!response.ok) throw new Error(await getErrorMessage(response));
            dirty = false; setMessage(message, 'Đã lưu bộ phận nhân viên.'); window.JtechnUI.toast('Đã lưu bộ phận nhân viên.', 'success');
        } catch (error) { setMessage(message, error.message, true); window.JtechnUI.toast(error.message, 'error'); }
        finally { saving = false; employeePicker.disabled = false; setEditable(); }
    });
    window.addEventListener('beforeunload', event => { if (dirty) { event.preventDefault(); event.returnValue = ''; } });
    (async () => {
        try {
            const response = await fetch('/api/employees/department-options', { headers: { Accept: 'application/json' } });
            if (!response.ok) throw new Error(await getErrorMessage(response));
            const data = await response.json(); employees = data.employees; departments = data.departments;
            employeePicker.disabled = false;
            const initial = new URLSearchParams(location.search).get('employeeId');
            if (employees.some(item => String(item.id) === initial)) {
                employee.value = initial;
                const selected = employees.find(item => String(item.id) === initial);
                employeeSearch.value = `${selected.employeeCode} - ${selected.fullName}`;
            }
            await loadAssignment();
        } catch (error) { setMessage(loadMessage, error.message, true); window.JtechnUI.toast(error.message, 'error'); }
    })();
});
