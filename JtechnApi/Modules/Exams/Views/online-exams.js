document.addEventListener('DOMContentLoaded', function () {
    'use strict';
    const $ = id => document.getElementById(id);
    const selectedExamId = new URLSearchParams(location.search).get('examId');
    const attemptUrl = id => selectedExamId ? `?examId=${encodeURIComponent(selectedExamId)}&attempt=${encodeURIComponent(id)}` : `?attempt=${encodeURIComponent(id)}`;
    let attempt, timer, offset = 0, queue = Promise.resolve(), dirty = false, submitting = false, pending = 0;
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    $('examDate').value = new Intl.DateTimeFormat('sv-SE', { timeZone: 'Asia/Bangkok', year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date());
    const message = text => { $('examMessage').textContent = text; $('examMessage').className = 'small mb-3 text-danger'; };
    const element = (tag, text, className) => { const e = document.createElement(tag); if (text != null) e.textContent = text; if (className) e.className = className; return e; };
    // Rebuild formatting nodes instead of inserting question-bank HTML into the live DOM.
    const answerContent = html => {
        const template = document.createElement('template');
        template.innerHTML = String(html || '');
        const allowed = new Set(['BR', 'DIV', 'P', 'SPAN', 'STRONG', 'B', 'EM', 'I', 'U', 'S', 'SUB', 'SUP', 'UL', 'OL', 'LI', 'TABLE', 'THEAD', 'TBODY', 'TR', 'TH', 'TD']);
        const blocked = new Set(['SCRIPT', 'STYLE', 'IFRAME', 'OBJECT', 'EMBED', 'SVG', 'MATH', 'TEMPLATE', 'FORM', 'INPUT', 'BUTTON', 'SELECT', 'TEXTAREA', 'LINK', 'META', 'BASE']);
        const copy = (source, target) => {
            for (const node of source.childNodes) {
                if (node.nodeType === Node.TEXT_NODE) { target.append(document.createTextNode(node.textContent)); continue; }
                if (node.nodeType !== Node.ELEMENT_NODE || blocked.has(node.tagName)) continue;
                if (!allowed.has(node.tagName)) { copy(node, target); continue; }
                const clean = document.createElement(node.tagName.toLowerCase());
                const indent = node.style.marginLeft;
                if (/^\d+(?:\.\d+)?px$/.test(indent)) clean.style.marginLeft = `${Math.min(100, parseFloat(indent))}px`;
                copy(node, clean);
                target.append(clean);
            }
        };
        const content = element('div', null, 'exam-option-content');
        copy(template.content, content);
        return content;
    };
    async function api(path = '', method = 'GET', body) {
        if (selectedExamId && (path === '' || path === '/start')) path += '?examId=' + encodeURIComponent(selectedExamId);
        const response = await fetch('/api/online-exams' + path, { method, credentials: 'same-origin', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token }, body: body === undefined ? undefined : JSON.stringify(body) });
        if (response.status === 401) throw new Error('Phiên đăng nhập đã hết. Hãy đăng nhập lại ở tab khác rồi thử lại.');
        if (response.status === 403) throw new Error('Bài thi không thuộc bộ phận của bạn, tài khoản đã ngừng hoạt động hoặc bài thi đã đóng.');
        const data = await response.json().catch(() => ({}));
        if (!response.ok) throw new Error(data.message || 'Không thể thực hiện yêu cầu. Vui lòng thử lại.');
        return data;
    }
    const questions = () => attempt.definition.groups.flatMap(g => g.questions);
    function answers() {
        const out = {};
        for (const q of questions()) {
            const inputs = [...document.querySelectorAll(`[data-question="${q.id}"]`)];
            out[q.id] = q.kind === 'matching' ? inputs.map(i => i.value.trim()) : inputs.filter(i => i.checked).map(i => i.value);
        }
        return out;
    }
    function progress() {
        const values = answers(); let count = 0;
        for (const q of questions()) {
            const done = values[q.id].length > 0 && values[q.id].every(v => v !== '');
            $('map-' + q.id).classList.toggle('answered', done);
            if (done) count++;
        }
        $('examProgress').textContent = `Đã trả lời ${count}/${questions().length} câu`;
        return count;
    }
    function result() {
        clearInterval(timer); $('examTimer').textContent = 'Đã nộp';
        document.querySelectorAll('#examForm input, #examForm button, #submitExam').forEach(e => e.disabled = true);
        $('examResult').hidden = false; $('examResult').replaceChildren(element('h2', attempt.classification), element('div', `${attempt.score}/100 điểm`, 'exam-result-score'), element('p', 'Kết quả đã được lưu. Bạn có thể xem lại trong lịch sử bài thi.'));
        $('saveStatus').textContent = 'Bài thi đã nộp thành công.';
        $('examResult').scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
    function render(data) {
        attempt = data; offset = Date.parse(data.serverNow) - Date.now();
        $('attemptInfo').textContent = `Ngày thi: ${data.examDate ? data.examDate.slice(0, 10).split('-').reverse().join('/') : 'Chưa ghi nhận'} · Ngày tạo lượt: ${new Date(data.startedAt).toLocaleString('vi-VN')} · Nhân viên mới: ${data.isNewEmployee == null ? 'Chưa ghi nhận' : data.isNewEmployee ? 'Có' : 'Không'}`;
        $('examCatalog').hidden = true; $('examWorkspace').hidden = false;
        $('examQuestions').replaceChildren(); $('examMap').replaceChildren();
        let number = 0;
        for (const group of data.definition.groups) {
            const section = element('section', null, 'panel-card exam-group');
            const heading = element('div', null, 'panel-head');
            heading.append(element('h5', group.title, 'mb-0'));
            section.append(heading);
            const grid = element('div', null, 'exam-grid'); grid.style.setProperty('--columns', Math.min(3, Math.max(1, group.columns)));
            for (const q of group.questions) {
                number++;
                const field = element('fieldset', null, 'exam-question'); field.id = 'question-' + q.id;
                field.append(element('legend', `Câu ${number}. ${q.text || 'Chọn đáp án đúng theo hình ảnh'}`), element('small', `${q.points} điểm · ${q.kind === 'multiple' ? 'Chọn nhiều đáp án' : q.kind === 'matching' ? 'Điền số trên hình vào mỗi ô tương ứng' : 'Chọn một đáp án'}`));
                if (q.image) { const img = element('img'); img.src = q.image; img.alt = `Hình câu ${number}`; img.loading = 'lazy'; img.addEventListener('error', () => { message('Không tải được ảnh đề thi. Kiểm tra kết nối và liên hệ quản lý; không đoán đáp án theo ảnh thiếu.'); }); field.append(img); }
                const saved = data.answers[q.id] || [];
                q.options.forEach((option, index) => {
                    const label = element('label', null, 'exam-option'), input = element('input');
                    const content = answerContent(option.text);
                    input.dataset.question = q.id; input.name = 'question-' + q.id;
                    if (q.kind === 'matching') { input.type = 'number'; input.className = 'form-control form-control-sm'; input.min = '1'; input.max = q.options.length; input.step = '1'; input.value = saved[index] || ''; input.setAttribute('aria-label', content.textContent); }
                    else { input.type = q.kind === 'multiple' ? 'checkbox' : 'radio'; input.className = 'form-check-input'; input.value = option.id; input.checked = saved.includes(option.id); }
                    label.append(input, content); field.append(label);
                });
                grid.append(field);
                const link = element('a', String(number)); link.id = 'map-' + q.id; link.href = '#question-' + q.id; link.setAttribute('aria-label', `Đến câu ${number}`); $('examMap').append(link);
            }
            section.append(grid); $('examQuestions').append(section);
        }
        progress();
        if (data.submittedAt) { result(); return; }
        const tick = () => {
            const remaining = Math.max(0, Math.ceil((Date.parse(attempt.expiresAt) - Date.now() - offset) / 1000));
            $('examTimer').textContent = `${String(Math.floor(remaining / 60)).padStart(2, '0')}:${String(remaining % 60).padStart(2, '0')}`;
            if (!remaining && !submitting) submit(true);
        };
        clearInterval(timer); timer = setInterval(tick, 1000); tick();
    }
    function save() {
        if (!attempt || attempt.submittedAt || submitting || !dirty) return;
        const values = answers(); dirty = false; pending++; $('saveStatus').textContent = 'Đang lưu…';
        queue = queue.catch(() => {}).then(async () => {
            const data = await api(`/attempts/${attempt.id}/save`, 'POST', { answers: values });
            if (data.submittedAt) { attempt = data; result(); }
            else $('saveStatus').textContent = 'Đã lưu lúc ' + new Date().toLocaleTimeString('vi-VN');
        }).catch(error => { dirty = true; $('saveStatus').textContent = 'Chưa lưu được. Đang chờ thử lại.'; message(error.message); }).finally(() => { pending--; });
    }
    async function submit(automatic = false) {
        if (!attempt || attempt.submittedAt || submitting) return;
        submitting = true;
        if (!automatic) {
            try {
                const confirmed = await window.JtechnUI.confirm({
                    title: 'Nộp bài thi',
                    message: `Bạn đã trả lời ${progress()}/${questions().length} câu. Bạn có chắc muốn nộp bài?`,
                    confirmText: 'Nộp bài', confirmVariant: 'primary'
                });
                if (!confirmed) { submitting = false; return; }
            } catch (error) { submitting = false; message(error.message); return; }
        }
        document.querySelectorAll('.exam-submit, #submitExam').forEach(b => b.disabled = true);
        try {
            await queue;
            attempt = await api(`/attempts/${attempt.id}/submit`, 'POST', { answers: answers() });
            message(''); result();
            window.JtechnUI.toast('Đã nộp bài thi.', 'success');
        } catch (error) { message(error.message); document.querySelectorAll('.exam-submit, #submitExam').forEach(b => b.disabled = false); }
        finally { submitting = false; }
    }
    async function init() {
        try {
            const id = new URLSearchParams(location.search).get('attempt');
            if (id) { render(await api('/attempts/' + encodeURIComponent(id))); return; }
            const catalog = await api();
            if (catalog.selectExam) {
                $('examSelection').hidden = false;
                $('availableExamsEmpty').hidden = catalog.exams.length > 0;
                $('availableExams').replaceChildren();
                for (const exam of catalog.exams) {
                    const column = element('div', null, 'col-md-6 col-xl-4');
                    const card = element('div', null, 'border rounded p-3 h-100');
                    const link = element('a', 'Chọn bài thi', 'btn btn-outline-primary');
                    link.href = '/admin/exams?examId=' + encodeURIComponent(exam.id);
                    card.append(element('p', exam.departmentName, 'section-kicker'), element('h5', exam.name), element('p', `${exam.code} · ${exam.minutes} phút`, 'small text-muted'), link);
                    column.append(card); $('availableExams').append(column);
                }
                return;
            }
            $('examTitle').textContent = catalog.title;
            $('examMeta').textContent = `${catalog.totalQuestions} câu hỏi  ·  ${catalog.minutes} phút  ·  ${catalog.totalPoints} điểm`;
            if (catalog.missingImages) { $('imageWarning').hidden = false; $('imageWarning').textContent = `Đề còn thiếu ${catalog.missingImages} ảnh. Quản lý cần bổ sung ảnh trước khi mở thi.`; }
            const active = catalog.attempts.find(a => !a.submittedAt);
            $('examStartFields').disabled = Boolean(active);
            if (active) {
                if (active.examDate) $('examDate').value = active.examDate.slice(0, 10);
                $('examNewEmployee').checked = Boolean(active.isNewEmployee);
            }
            $('startExam').textContent = active ? 'Tiếp tục bài đang làm' : 'Bắt đầu làm bài';
            $('startExam').disabled = !active && catalog.missingImages > 0;
            $('examHistory').replaceChildren();
            $('examHistoryCount').textContent = `${catalog.attempts.length} lượt thi`;
            if (!catalog.attempts.length) {
                const row = element('tr'), cell = element('td', 'Bạn chưa có lượt thi nào.', 'text-center text-muted py-4');
                cell.colSpan = 6; row.append(cell); $('examHistory').append(row);
            }
            for (const a of catalog.attempts) {
                const row = element('tr'), link = element('a', a.submittedAt ? 'Xem bài' : 'Tiếp tục', 'btn btn-sm btn-outline-primary');
                link.href = attemptUrl(a.id);
                const status = element('td'), actions = element('td', null, 'text-end');
                status.append(element('span', a.submittedAt ? a.classification : 'Đang làm', 'badge-soft'));
                actions.append(link);
                row.append(element('td', a.examDate ? a.examDate.slice(0, 10).split('-').reverse().join('/') : 'Chưa ghi nhận'),
                    element('td', new Date(a.startedAt).toLocaleString('vi-VN')),
                    element('td', a.isNewEmployee == null ? 'Chưa ghi nhận' : a.isNewEmployee ? 'Có' : 'Không'),
                    element('td', a.submittedAt ? `${a.score}/100` : '—'), status, actions);
                $('examHistory').append(row);
            }
            $('examCatalog').hidden = false;
        } catch (error) { message(error.message); }
        finally { $('examLoading').hidden = true; }
    }
    $('examStartForm').addEventListener('submit', async event => {
        event.preventDefault();
        if ($('startExam').disabled || !$('examStartForm').reportValidity()) return;
        $('startExam').disabled = true;
        try {
            const data = await api('/start', 'POST', { examDate: $('examDate').value, isNewEmployee: $('examNewEmployee').checked });
            location.assign(attemptUrl(data.id));
        }
        catch (error) { message(error.message); $('startExam').disabled = false; }
    });
    $('examForm').addEventListener('input', () => { dirty = true; progress(); });
    $('examForm').addEventListener('change', save);
    $('examForm').addEventListener('submit', e => { e.preventDefault(); submit(); });
    $('submitExam').addEventListener('click', () => submit());
    setInterval(save, 5000);
    window.addEventListener('beforeunload', e => { if ((dirty || pending) && attempt && !attempt.submittedAt) { e.preventDefault(); e.returnValue = ''; } });
    init();
});
