(function () {
    'use strict';

    const validationMessage = (field) => {
        if (field.dataset.errorMessage) return field.dataset.errorMessage;
        if (field.validity.valueMissing) {
            const label = field.labels?.[0]?.textContent?.trim();
            return label ? `Vui lòng nhập ${label.toLocaleLowerCase()}` : 'Trường này là bắt buộc.';
        }
        if (field.validity.typeMismatch) return 'Vui lòng nhập đúng định dạng.';
        if (field.validity.rangeUnderflow || field.validity.rangeOverflow) return 'Giá trị nằm ngoài phạm vi cho phép.';
        if (field.validity.tooShort || field.validity.tooLong) return 'Độ dài chưa hợp lệ.';
        return 'Vui lòng kiểm tra lại thông tin.';
    };

    const requiredDropdownMessage = (field, search) => {
        const label = search.labels?.[0]?.textContent?.trim();
        return label ? `Vui lòng chọn ${label.toLocaleLowerCase()}` : 'Vui lòng chọn một giá trị trong danh sách.';
    };

    const messageElement = (field) => {
        const describedBy = (field.getAttribute('aria-describedby') || '').split(/\s+/);
        const existing = describedBy
            .map(id => document.getElementById(id))
            .find(element => element?.hasAttribute('data-validation-message'));
        if (existing) return existing;

        const message = document.createElement('span');
        message.className = 'field-validation-error';
        message.setAttribute('data-validation-message', '');
        message.setAttribute('aria-live', 'polite');
        field.insertAdjacentElement('afterend', message);
        return message;
    };

    const setError = (field, message) => {
        const target = messageElement(field);
        target.textContent = message || '';
        field.setAttribute('aria-invalid', message ? 'true' : 'false');
        field.classList.toggle('is-invalid', Boolean(message));
    };

    const validateField = (field) => {
        if (field.disabled || field.type === 'hidden' || field.dataset.filterSearch !== undefined) {
            setError(field, '');
            return true;
        }

        const valid = field.checkValidity();
        setError(field, valid ? '' : validationMessage(field));
        return valid;
    };

    const validateRange = (range) => {
        const start = range.querySelector('[data-range-start]');
        const end = range.querySelector('[data-range-end]');
        if (!start || !end || !start.value || !end.value) return true;

        const valid = start.value <= end.value;
        const message = valid ? '' : 'Thời gian bắt đầu phải nhỏ hơn hoặc bằng thời gian kết thúc.';
        setError(end, message);
        return valid;
    };

    const validateDropdowns = (form) => {
        let valid = true;
        form.querySelectorAll('[data-filter-value][required]').forEach(valueField => {
            if (valueField.value) return;
            const search = valueField.closest('.filter-dropdown')?.querySelector('[data-filter-search]');
            if (search) {
                setError(search, requiredDropdownMessage(valueField, search));
                valid = false;
            }
        });
        return valid;
    };

    const enableDropdownKeyboard = (search, list, closeList) => {
        search.addEventListener('keydown', function (event) {
            if (event.key === 'ArrowDown' && !list.hidden) {
                event.preventDefault();
                list.querySelector('button:not([hidden])')?.focus();
            } else if (event.key === 'Escape') {
                closeList();
            }
        });
        list.addEventListener('keydown', function (event) {
            const options = Array.from(list.querySelectorAll('button:not([hidden])'));
            const index = options.indexOf(document.activeElement);
            if (event.key === 'ArrowDown' && index < options.length - 1) {
                event.preventDefault();
                options[index + 1].focus();
            } else if (event.key === 'ArrowUp') {
                event.preventDefault();
                if (index > 0) options[index - 1].focus();
                else search.focus();
            } else if (event.key === 'Escape') {
                closeList();
                search.focus();
            } else if (event.key === 'Enter') {
                event.preventDefault();
                document.activeElement.click();
            }
        });
    };

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-floating-field]').forEach(wrapper => {
            const field = wrapper.querySelector('[data-floating-input]');
            if (!field) return;

            const updateFloatingLabel = () => {
                wrapper.classList.toggle('is-floating', field.value.length > 0);
            };

            updateFloatingLabel();
            field.addEventListener('input', updateFloatingLabel);
            field.addEventListener('change', updateFloatingLabel);
            field.addEventListener('blur', updateFloatingLabel);
        });

        document.querySelectorAll('form').forEach(form => {
            form.noValidate = true;
            form.addEventListener('submit', function (event) {
                let valid = true;
                form.querySelectorAll('[data-inline-validation]').forEach(field => {
                    if (!validateField(field)) valid = false;
                });
                form.querySelectorAll('[data-datetime-range]').forEach(range => {
                    if (!validateRange(range)) valid = false;
                });
                if (!validateDropdowns(form)) valid = false;

                if (!valid) {
                    event.preventDefault();
                    event.stopImmediatePropagation();
                    form.querySelector('[aria-invalid="true"]')?.focus();
                }
            }, true);
        });

        document.addEventListener('input', function (event) {
            const field = event.target.closest('[data-inline-validation]');
            if (field && field.dataset.validationAttempted === 'true') validateField(field);
        });
        document.addEventListener('change', function (event) {
            const field = event.target.closest('[data-inline-validation]');
            if (field) validateField(field);
            const range = event.target.closest('[data-datetime-range]');
            if (range) validateRange(range);
        });
        document.addEventListener('focusout', function (event) {
            const field = event.target.closest('[data-inline-validation]');
            if (field) {
                field.dataset.validationAttempted = 'true';
                validateField(field);
            }
        });

        document.querySelectorAll('[data-filter-dropdown]').forEach(dropdown => {
            const search = dropdown.querySelector('[data-filter-search]');
            const value = dropdown.querySelector('[data-filter-value]');
            const options = Array.from(dropdown.querySelectorAll('.filter-dropdown-option'));
            const list = dropdown.querySelector('[role="listbox"]');

            const selected = options.find(option => option.dataset.value === value.value);
            if (selected) search.value = selected.textContent;

            const filterOptions = () => {
                const query = search.value.trim().toLocaleLowerCase();
                let matches = 0;
                options.forEach(option => {
                    const visible = option.textContent.toLocaleLowerCase().includes(query);
                    option.hidden = !visible;
                    if (visible) matches++;
                });
                list.hidden = matches === 0;
                search.setAttribute('aria-expanded', String(matches > 0));
            };

            enableDropdownKeyboard(search, list, () => {
                list.hidden = true;
                search.setAttribute('aria-expanded', 'false');
            });

            search.addEventListener('input', function () {
                value.value = '';
                filterOptions();
            });
            search.addEventListener('focus', filterOptions);
            options.forEach(option => {
                option.addEventListener('click', function () {
                    value.value = option.dataset.value;
                    search.value = option.textContent;
                    list.hidden = true;
                    search.setAttribute('aria-expanded', 'false');
                    setError(search, '');
                    value.dispatchEvent(new Event('change', { bubbles: true }));
                });
            });
            document.addEventListener('click', function (event) {
                if (!dropdown.contains(event.target)) {
                    list.hidden = true;
                    search.setAttribute('aria-expanded', 'false');
                }
            });
        });

        document.querySelectorAll('[data-api-filter-dropdown]').forEach(dropdown => {
            const search = dropdown.querySelector('[data-filter-search]');
            const value = dropdown.querySelector('[data-filter-value]');
            const list = dropdown.querySelector('[role="listbox"]');
            const status = dropdown.querySelector('.filter-dropdown-status');
            const minCharacters = Number(dropdown.dataset.minCharacters) || 0;
            const debounce = Math.max(0, Number(dropdown.dataset.debounce) || 0);
            let timer;
            let requestNumber = 0;
            let controller;

            const closeList = () => {
                list.hidden = true;
                search.setAttribute('aria-expanded', 'false');
            };

            enableDropdownKeyboard(search, list, closeList);

            const showOptions = (items) => {
                list.replaceChildren();
                items.forEach(item => {
                    const option = document.createElement('button');
                    option.type = 'button';
                    option.className = 'filter-dropdown-option';
                    option.setAttribute('role', 'option');
                    option.textContent = String(item.label ?? '');
                    option.dataset.value = String(item.value ?? '');
                    option.addEventListener('click', function () {
                        window.clearTimeout(timer);
                        requestNumber++;
                        if (controller) controller.abort();
                        value.value = option.dataset.value;
                        search.value = option.textContent;
                        status.textContent = '';
                        setError(search, '');
                        closeList();
                        value.dispatchEvent(new Event('change', { bubbles: true }));
                    });
                    list.append(option);
                });
                list.hidden = items.length === 0;
                search.setAttribute('aria-expanded', String(items.length > 0));
            };

            const searchApi = async () => {
                const query = search.value.trim();
                list.replaceChildren();
                closeList();
                status.textContent = '';

                if (query.length < minCharacters) return;

                const currentRequest = ++requestNumber;
                if (controller) controller.abort();
                controller = new AbortController();
                const url = new URL(dropdown.dataset.endpoint, window.location.origin);
                url.searchParams.set(dropdown.dataset.queryParameter || 'q', query);

                try {
                    status.textContent = 'Đang tải...';
                    const response = await fetch(url, {
                        credentials: 'same-origin',
                        signal: controller.signal
                    });
                    if (!response.ok) throw new Error(`Không tải được danh sách (${response.status}).`);

                    let result = await response.json();
                    if (dropdown.dataset.dataPath) {
                        for (const key of dropdown.dataset.dataPath.split('.')) result = result?.[key];
                    } else if (!Array.isArray(result)) {
                        result = result?.items ?? result?.data;
                    }
                    if (!Array.isArray(result)) throw new Error('API cần trả về danh sách hoặc cấu hình đúng đường dẫn dữ liệu.');
                    if (currentRequest !== requestNumber) return;

                    const valueField = dropdown.dataset.valueField || 'id';
                    const labelField = dropdown.dataset.labelField || 'name';
                    const items = result.map(item => ({
                        value: item?.[valueField],
                        label: item?.[labelField]
                    })).filter(item => item.value !== undefined
                        && item.value !== null
                        && item.label !== undefined
                        && item.label !== null
                        && String(item.label).toLocaleLowerCase().includes(query.toLocaleLowerCase()));
                    showOptions(items);
                    status.textContent = items.length ? '' : 'Không tìm thấy kết quả.';
                } catch (error) {
                    if (error.name === 'AbortError' || currentRequest !== requestNumber) return;
                    status.textContent = error.message || 'Không thể tải danh sách.';
                    closeList();
                }
            };

            search.addEventListener('input', function () {
                value.value = '';
                requestNumber++;
                if (controller) controller.abort();
                window.clearTimeout(timer);
                timer = window.setTimeout(searchApi, debounce);
            });
            document.addEventListener('click', function (event) {
                if (!dropdown.contains(event.target)) closeList();
            });
        });
    });
})();
