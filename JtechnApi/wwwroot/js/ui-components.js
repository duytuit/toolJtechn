(function () {
    'use strict';

    const toastContainer = document.getElementById('appToastContainer');
    const confirmModalElement = document.getElementById('appConfirmModal');
    if (!toastContainer || !confirmModalElement || !window.bootstrap?.Toast || !window.bootstrap?.Modal) {
        throw new Error('Không thể khởi tạo các component thông báo của ứng dụng.');
    }

    const toastStyles = {
        success: { title: 'Thành công', style: 'text-bg-success', role: 'status' },
        error: { title: 'Lỗi', style: 'text-bg-danger', role: 'alert' },
        warning: { title: 'Cảnh báo', style: 'text-bg-warning', role: 'alert' },
        info: { title: 'Thông tin', style: 'text-bg-primary', role: 'status' }
    };

    const confirmModal = window.bootstrap.Modal.getOrCreateInstance(confirmModalElement);
    const confirmTitle = confirmModalElement.querySelector('#appConfirmTitle');
    const confirmMessage = confirmModalElement.querySelector('#appConfirmMessage');
    const confirmButton = confirmModalElement.querySelector('[data-confirm-accept]');
    const cancelButton = confirmModalElement.querySelector('[data-confirm-cancel]');
    let resolveConfirmation = null;
    let confirmationResult = false;

    const createToastElement = (message, style) => {
        const toast = document.createElement('div');
        toast.className = `toast align-items-center border-0 ${style.style}`;
        toast.setAttribute('role', style.role);
        toast.setAttribute('aria-live', style.role === 'alert' ? 'assertive' : 'polite');
        toast.setAttribute('aria-atomic', 'true');

        const content = document.createElement('div');
        content.className = 'd-flex';

        const body = document.createElement('div');
        body.className = 'toast-body';

        const title = document.createElement('strong');
        title.className = 'd-block';
        title.textContent = style.title;

        const text = document.createElement('span');
        text.textContent = message;
        body.append(title, text);

        const close = document.createElement('button');
        close.type = 'button';
        close.className = 'btn-close btn-close-white me-2 m-auto';
        close.setAttribute('data-bs-dismiss', 'toast');
        close.setAttribute('aria-label', 'Đóng');

        content.append(body, close);
        toast.append(content);
        return toast;
    };

    const settleConfirmation = () => {
        if (!resolveConfirmation) return;
        const resolve = resolveConfirmation;
        resolveConfirmation = null;
        resolve(confirmationResult);
        confirmationResult = false;
    };

    confirmButton.addEventListener('click', function () {
        confirmationResult = true;
        confirmModal.hide();
    });

    confirmModalElement.addEventListener('hidden.bs.modal', settleConfirmation);
    cancelButton.addEventListener('click', function () {
        confirmationResult = false;
    });

    window.JtechnUI = {
        toast(message, type = 'info') {
            const style = toastStyles[type];
            if (!style) throw new TypeError(`Loại thông báo không hợp lệ: ${type}`);
            if (!message) throw new TypeError('Nội dung thông báo không được để trống.');

            const element = createToastElement(String(message), style);
            toastContainer.append(element);
            const toast = window.bootstrap.Toast.getOrCreateInstance(element, { delay: 5000 });
            element.addEventListener('hidden.bs.toast', () => {
                toast.dispose();
                element.remove();
            }, { once: true });
            toast.show();
        },

        confirm(options) {
            if (resolveConfirmation) {
                throw new Error('Một hộp thoại xác nhận khác đang được mở.');
            }
            if (!options || !options.message) {
                throw new TypeError('Cần cung cấp nội dung xác nhận.');
            }

            confirmTitle.textContent = options.title || 'Xác nhận';
            confirmMessage.textContent = options.message;
            confirmButton.textContent = options.confirmText || 'Xác nhận';
            confirmButton.className = `btn ${options.confirmVariant === 'danger' ? 'btn-danger' : 'btn-primary'}`;
            cancelButton.textContent = options.cancelText || 'Hủy';

            return new Promise(resolve => {
                resolveConfirmation = resolve;
                confirmationResult = false;
                confirmModal.show();
            });
        }
    };
})();
