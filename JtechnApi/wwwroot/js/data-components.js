(function () {
    'use strict';

    const tables = new Map();

    const readField = (row, path) => path.split('.').reduce((value, key) => value?.[key], row);

    const formatValue = (value, format) => {
        if (value === null || value === undefined || value === '') return '—';
        if (format !== 'date') return String(value);

        const [year, month, day] = String(value).slice(0, 10).split('-').map(Number);
        if (!year || !month || !day) return String(value);
        return new Intl.DateTimeFormat('vi-VN', { dateStyle: 'short' }).format(new Date(year, month - 1, day));
    };

    const createCell = (row, column) => {
        const cell = document.createElement('td');
        if (column.cssClass) cell.className = column.cssClass;

        const value = readField(row, column.field);
        const text = formatValue(value, column.format);
        if (column.format === 'image') {
            if (typeof value === 'string' && value.startsWith('/img/') &&
                !value.includes('..') && !value.includes('\\') &&
                !value.includes('?') && !value.includes('#')) {
                const image = document.createElement('img');
                image.src = value;
                image.alt = column.imageAltField
                    ? String(readField(row, column.imageAltField) ?? '')
                    : 'Hình minh họa';
                image.loading = 'lazy';
                image.className = 'data-table-thumbnail';
                cell.append(image);
            } else {
                cell.textContent = '—';
            }
        } else if (column.format === 'badge' && text !== '—') {
            const badge = document.createElement('span');
            badge.className = 'status-pill';
            badge.textContent = text;
            cell.append(badge);
        } else if (column.format === 'strong') {
            const strong = document.createElement('strong');
            strong.textContent = text;
            cell.append(strong);
        } else {
            const primary = document.createElement('span');
            primary.textContent = text;
            cell.append(primary);
        }

        if (column.secondaryField) {
            const secondary = document.createElement('small');
            secondary.className = 'd-block text-muted';
            secondary.textContent = formatValue(readField(row, column.secondaryField));
            cell.append(secondary);
        }

        return cell;
    };

    const createActionCell = (row, state) => {
        const cell = document.createElement('td');
        cell.className = 'text-end text-nowrap';

        [['view', state.enableView, 'Xem nhân viên', 'btn-outline-secondary'],
            ['edit', state.enableEdit, 'Sửa', 'btn-outline-primary'],
            ['delete', state.enableDelete, 'Xóa', 'btn-outline-danger']]
            .filter(action => action[1])
            .forEach(([action, , label, style]) => {
                const button = document.createElement('button');
                button.type = 'button';
                button.className = `btn btn-sm ${style} ms-1`;
                button.textContent = label;
                button.addEventListener('click', function () {
                    state.element.dispatchEvent(new CustomEvent(`datatable:${action}`, {
                        bubbles: true,
                        detail: { row }
                    }));
                });
                cell.append(button);
            });

        return cell;
    };

    const render = (state) => {
        const query = state.search?.value.trim().toLocaleLowerCase() || '';
        const filtered = !state.enableServerPagination && query
            ? state.rows.filter(row => state.columns.some(column =>
                `${readField(row, column.field) ?? ''} ${column.secondaryField ? readField(row, column.secondaryField) ?? '' : ''}`
                    .toLocaleLowerCase().includes(query)))
            : state.rows;
        const totalItems = state.enableServerPagination ? state.totalItems : filtered.length;
        const pageCount = state.enablePagination ? Math.max(1, Math.ceil(totalItems / state.pageSize)) : 1;
        state.page = Math.min(state.page, pageCount);
        const start = state.enablePagination ? (state.page - 1) * state.pageSize : 0;
        const pageRows = state.enableServerPagination
            ? filtered
            : state.enablePagination ? filtered.slice(start, start + state.pageSize) : filtered;

        state.body.replaceChildren();
        if (pageRows.length === 0) {
            const row = document.createElement('tr');
            const cell = document.createElement('td');
            cell.colSpan = state.columns.length + (state.enableView || state.enableEdit || state.enableDelete ? 1 : 0);
            cell.className = 'text-center text-muted py-4';
            cell.textContent = state.emptyMessage;
            row.append(cell);
            state.body.append(row);
        } else {
            pageRows.forEach(item => {
                const row = document.createElement('tr');
                state.columns.forEach(column => row.append(createCell(item, column)));
                if (state.enableView || state.enableEdit || state.enableDelete) row.append(createActionCell(item, state));
                state.body.append(row);
            });
        }

        if (state.summary) {
            const first = totalItems === 0 ? 0 : start + 1;
            const last = start + pageRows.length;
            state.summary.textContent = `${first}–${last} / ${totalItems}`;
            state.pageLabel.textContent = `${state.page} / ${pageCount}`;
            state.previous.disabled = state.page <= 1;
            state.next.disabled = state.page >= pageCount;
        }
    };

    const initialize = (element) => {
        try {
            const columnsElement = element.querySelector('[data-table-columns]');
            const columns = JSON.parse(columnsElement.textContent);
            const state = {
                element,
                columns,
                rows: [],
                body: element.querySelector('[data-table-body]'),
                search: element.querySelector('[data-table-search]'),
                summary: element.querySelector('[data-table-summary]'),
                pageLabel: element.querySelector('[data-table-page]'),
                previous: element.querySelector('[data-table-previous]'),
                next: element.querySelector('[data-table-next]'),
                pageSize: Math.max(1, Number(element.dataset.pageSize) || 10),
                page: 1,
                totalItems: 0,
                emptyMessage: element.dataset.emptyMessage || 'Không có dữ liệu phù hợp.',
                enablePagination: element.dataset.enablePagination === 'true',
                enableServerPagination: element.dataset.enableServerPagination === 'true',
                enableView: element.dataset.enableView === 'true',
                enableEdit: element.dataset.enableEdit === 'true',
                enableDelete: element.dataset.enableDelete === 'true'
            };
            if (!Array.isArray(columns) || !state.body) {
                throw new Error('Cấu hình cột hoặc phần thân của datatable không hợp lệ.');
            }

            state.search?.addEventListener('input', () => {
                state.page = 1;
                if (state.enableServerPagination) {
                    state.element.dispatchEvent(new CustomEvent('datatable:page', {
                        bubbles: true,
                        detail: { page: state.page, pageSize: state.pageSize, search: state.search.value.trim() }
                    }));
                    return;
                }
                render(state);
            });
            state.previous?.addEventListener('click', () => {
                state.page = Math.max(1, state.page - 1);
                if (state.enableServerPagination) {
                    state.element.dispatchEvent(new CustomEvent('datatable:page', {
                        bubbles: true,
                        detail: { page: state.page, pageSize: state.pageSize, search: state.search?.value.trim() || '' }
                    }));
                    return;
                }
                render(state);
            });
            state.next?.addEventListener('click', () => {
                state.page++;
                if (state.enableServerPagination) {
                    state.element.dispatchEvent(new CustomEvent('datatable:page', {
                        bubbles: true,
                        detail: { page: state.page, pageSize: state.pageSize, search: state.search?.value.trim() || '' }
                    }));
                    return;
                }
                render(state);
            });

            tables.set(element.id, state);
            render(state);
        } catch (error) {
            const body = element.querySelector('[data-table-body]');
            if (body) {
                const row = document.createElement('tr');
                const cell = document.createElement('td');
                cell.colSpan = Math.max(1, element.querySelectorAll('thead th').length);
                cell.className = 'text-center text-danger py-4';
                cell.textContent = `Không thể khởi tạo datatable: ${error.message}`;
                row.append(cell);
                body.replaceChildren(row);
            }
            console.error(error);
        }
    };

    window.JtechnDataTable = {
        setRows(id, rows) {
            const state = tables.get(id);
            if (!state) throw new Error(`Không tìm thấy datatable "${id}".`);
            if (!Array.isArray(rows)) throw new TypeError('Dữ liệu datatable phải là một mảng.');
            state.rows = rows;
            state.page = 1;
            render(state);
        },
        setServerRows(id, rows, totalItems, page) {
            const state = tables.get(id);
            if (!state) throw new Error(`Không tìm thấy datatable "${id}".`);
            if (!state.enableServerPagination) throw new Error(`Datatable "${id}" chưa bật phân trang server.`);
            if (!Array.isArray(rows) || !Number.isInteger(totalItems) || totalItems < 0 ||
                !Number.isInteger(page) || page < 1) {
                throw new TypeError('Dữ liệu phân trang server không hợp lệ.');
            }
            state.rows = rows;
            state.totalItems = totalItems;
            state.page = page;
            render(state);
        },
        showMessage(id, message, isError = false) {
            const state = tables.get(id);
            if (!state) throw new Error(`Không tìm thấy datatable "${id}".`);
            const row = document.createElement('tr');
            const cell = document.createElement('td');
            cell.colSpan = state.columns.length + (state.enableView || state.enableEdit || state.enableDelete ? 1 : 0);
            cell.className = `text-center py-4 ${isError ? 'text-danger' : 'text-muted'}`;
            cell.textContent = message;
            row.append(cell);
            state.body.replaceChildren(row);
        }
    };

    window.JtechnEntityForm = {
        reset(form) {
            if (typeof form === 'string') form = document.getElementById(form);
            if (!form?.matches('[data-api-entity-form]')) throw new Error('Không tìm thấy form entity hợp lệ.');
            form.reset();
            const idField = form.elements.namedItem(form.dataset.idField);
            if (idField) idField.value = '';
            this.setMode(form, false);
            form.querySelectorAll('[data-inline-validation]').forEach(field => {
                field.removeAttribute('aria-invalid');
                field.classList.remove('is-invalid');
                delete field.dataset.validationAttempted;
            });
            form.querySelectorAll('[data-validation-message]').forEach(message => {
                message.textContent = '';
            });
            form.querySelectorAll('[data-floating-field]').forEach(wrapper => {
                wrapper.querySelector('[data-floating-input]')?.dispatchEvent(new Event('input', { bubbles: true }));
            });
        },
        setValues(form, values) {
            if (typeof form === 'string') form = document.getElementById(form);
            if (!form?.matches('[data-api-entity-form]')) throw new Error('Không tìm thấy form entity hợp lệ.');
            if (!values || typeof values !== 'object') throw new TypeError('Dữ liệu để sửa phải là một object.');

            Object.entries(values).forEach(([name, value]) => {
                const field = form.elements.namedItem(name);
                if (!field || field.name === form.dataset.idField) return;
                const fieldValue = value == null ? '' : String(value);
                field.value = field.type === 'date' ? fieldValue.slice(0, 10) : fieldValue;
                const dropdown = field.closest('[data-api-filter-dropdown]');
                const search = dropdown?.querySelector('[data-filter-search]');
                if (search && fieldValue) search.value = fieldValue;
                field.dispatchEvent(new Event('input', { bubbles: true }));
                field.dispatchEvent(new Event('change', { bubbles: true }));
            });

            const idField = form.elements.namedItem(form.dataset.idField);
            const key = values[form.dataset.entityKeyField];
            if (idField) idField.value = key == null ? '' : String(key);
            this.setMode(form, Boolean(idField?.value));
        },
        setMode(form, editing) {
            const panel = form.closest('.panel-card');
            const title = panel?.querySelector('[data-entity-form-title]');
            const submit = form.querySelector('[data-entity-form-submit]');
            const cancel = panel?.querySelector('[data-entity-form-cancel]');
            if (title) title.textContent = editing ? title.dataset.updateTitle : title.dataset.insertTitle;
            if (submit) submit.textContent = editing ? form.dataset.updateButtonText : form.dataset.insertButtonText;
            cancel?.classList.toggle('d-none', !editing);
        }
    };

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-data-table]').forEach(initialize);

        document.querySelectorAll('[data-api-entity-form]').forEach(form => {
            const panel = form.closest('.panel-card');
            const message = form.querySelector('[role="status"]');
            const idField = form.elements.namedItem(form.dataset.idField);
            const submit = form.querySelector('[data-entity-form-submit]');

            panel?.querySelector('[data-entity-form-cancel]')?.addEventListener('click', () => {
                form.dispatchEvent(new CustomEvent('entity-form:cancel', { bubbles: true }));
            });

            form.addEventListener('submit', async function (event) {
                event.preventDefault();
                if (form.querySelector('[aria-invalid="true"]')) return;

                const id = idField?.value;
                const payload = {};
                Array.from(new FormData(form).entries()).forEach(([key, value]) => {
                    if (key !== form.dataset.idField) payload[key] = value;
                });

                const method = id ? 'PUT' : 'POST';
                const endpoint = form.dataset.endpoint.replace(/\/$/, '');
                const url = id ? `${endpoint}/${encodeURIComponent(id)}` : endpoint;
                if (submit) {
                    submit.disabled = true;
                    submit.textContent = 'Đang lưu...';
                }
                if (message) {
                    message.className = 'small mt-3 text-muted';
                    message.textContent = '';
                }

                try {
                    const response = await fetch(url, {
                        method,
                        credentials: 'same-origin',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    });
                    const responseText = response.status === 204 ? '' : await response.text();
                    let result = null;
                    if (responseText) {
                        try {
                            result = JSON.parse(responseText);
                        } catch {
                            result = { message: responseText };
                        }
                    }
                    if (!response.ok) throw new Error(result?.message || `Không thể lưu dữ liệu (${response.status}).`);

                    if (message) {
                        message.className = 'small mt-3 text-success';
                        message.textContent = result?.message || 'Đã lưu dữ liệu thành công.';
                    }
                    window.JtechnEntityForm.reset(form);
                    form.dispatchEvent(new CustomEvent('entity-form:saved', {
                        bubbles: true,
                        detail: { method, id, result }
                    }));
                } catch (error) {
                    if (message) {
                        message.className = 'small mt-3 text-danger';
                        message.textContent = error.message || 'Không thể lưu dữ liệu.';
                    }
                    form.dispatchEvent(new CustomEvent('entity-form:error', {
                        bubbles: true,
                        detail: { error }
                    }));
                } finally {
                    if (submit) {
                        submit.disabled = false;
                        window.JtechnEntityForm.setMode(form, Boolean(idField?.value));
                    }
                }
            });
        });
    });
})();
