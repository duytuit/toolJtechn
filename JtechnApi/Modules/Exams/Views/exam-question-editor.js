(function (root) {
    'use strict';
    const read = (object, key) => object?.[key] ?? object?.[key[0].toUpperCase() + key.slice(1)];
    function normalizeDefinition(value) {
        const groups = read(value, 'groups');
        if (!Array.isArray(groups)) throw new Error('JSON phải có danh sách groups.');
        const used = new Set();
        return { title: read(value, 'title') || '', minutes: Number(read(value, 'minutes') || 30), groups: groups.map(group => {
            const questions = read(group, 'questions');
            if (!Array.isArray(questions)) throw new Error('Mỗi nhóm phải có danh sách questions.');
            return { title: read(group, 'title') || '', columns: Number(read(group, 'columns') || 1), questions: questions.map(question => {
                const id = Number(read(question, 'id'));
                if (used.size >= 500) throw new Error('Đề thi tối đa 500 câu hỏi.');
                if (!Number.isInteger(id) || id <= 0 || used.has(id)) throw new Error('ID câu hỏi phải là số nguyên dương và không trùng.');
                used.add(id);
                const options = read(question, 'options'), correct = read(question, 'correct');
                if (!Array.isArray(options) || !Array.isArray(correct)) throw new Error(`Câu ${id}: cần options và correct dạng mảng.`);
                const kind = read(question, 'kind');
                if (!['single', 'multiple', 'matching'].includes(kind)) throw new Error(`Câu ${id}: loại câu hỏi không hợp lệ.`);
                return { id, text: read(question, 'text') || '', points: Number(read(question, 'points')), image: read(question, 'image') || '', kind,
                    options: options.map(option => ({ id: String(read(option, 'id') ?? ''), text: String(read(option, 'text') ?? '') })), correct: correct.map(String) };
            }) };
        }) };
    }
    function validateDefinition(definition) {
        const questions = definition.groups.flatMap(group => group.questions);
        if (!definition.groups.length || definition.groups.some(group => !group.questions.length)) throw new Error('Mỗi nhóm phải có ít nhất một câu hỏi.');
        if (questions.length > 500) throw new Error('Đề thi tối đa 500 câu hỏi.');
        let total = 0;
        for (const q of questions) {
            if (!Number.isFinite(q.points) || q.points <= 0) throw new Error(`Câu ${q.id}: điểm phải lớn hơn 0.`);
            if (!q.options.length || q.options.some(option => !option.id.trim() || !option.text.trim()) || new Set(q.options.map(option => option.id)).size !== q.options.length)
                throw new Error(`Câu ${q.id}: nhập đủ lựa chọn và không trùng ID lựa chọn.`);
            if (q.kind === 'matching') {
                if (q.correct.length !== q.options.length || q.correct.some(value => !/^\d+$/.test(value) || Number(value) < 1 || Number(value) > q.options.length) || new Set(q.correct).size !== q.correct.length)
                    throw new Error(`Câu ${q.id}: điền số ghép đúng, không trùng, từ 1 đến ${q.options.length}.`);
            } else if (!q.correct.length || (q.kind === 'single' && q.correct.length !== 1) || q.correct.some(id => !q.options.some(option => option.id === id)))
                throw new Error(`Câu ${q.id}: hãy chọn đáp án đúng.`);
            if (q.image && (!q.image.startsWith('/img/') || /\.\.|[\\?#]/.test(q.image))) throw new Error(`Câu ${q.id}: đường dẫn ảnh phải nằm trong /img/.`);
            total += q.points;
        }
        if (Math.abs(total - 100) > 0.000001) throw new Error(`Tổng điểm hiện tại là ${total}; tổng điểm cần bằng 100.`);
        return definition;
    }
    function create(container, summary) {
        let groups = [], controlId = 0;
        const node = (tag, text, css) => { const result = document.createElement(tag); if (text != null) result.textContent = text; if (css) result.className = css; return result; };
        const button = (text, action, danger = false) => { const result = node('button', text, `btn btn-sm btn-outline-${danger ? 'danger' : 'primary'}`); result.type = 'button'; result.addEventListener('click', action); return result; };
        const update = () => {
            const questions = groups.flatMap(group => group.questions);
            const total = questions.reduce((sum, q) => sum + (Number(q.points) || 0), 0);
            summary.textContent = `${groups.length} nhóm · ${questions.length} câu · ${Number(total.toFixed(4))}/100 điểm`;
            summary.className = Math.abs(total - 100) < 0.000001 ? 'text-success small' : 'text-muted small';
        };
        function field(parent, label, value, onInput, type = 'text') {
            const wrap = node('div', null, 'mb-3');
            const input = node(type === 'textarea' ? 'textarea' : 'input', null, 'form-control');
            input.id = 'exam-builder-' + (++controlId);
            if (type !== 'textarea') input.type = type; else input.rows = 2;
            if (type === 'number') { input.min = '0.0001'; input.step = 'any'; }
            input.value = value ?? '';
            const title = node('label', label, 'form-label'); title.htmlFor = input.id;
            input.addEventListener('input', () => { onInput(input.value); update(); });
            wrap.append(title, input); parent.append(wrap); return input;
        }
        function render() {
            container.replaceChildren();
            groups.forEach((group, groupIndex) => {
                const section = node('section', null, 'border rounded p-3 mb-3');
                const header = node('div', null, 'd-flex justify-content-between align-items-center mb-3');
                header.append(node('h6', `Nhóm ${groupIndex + 1}`, 'mb-0'), button('Xóa nhóm', async () => {
                    if (await root.JtechnUI.confirm({ title: 'Xóa nhóm câu hỏi', message: 'Xóa nhóm và các câu hỏi trong nhóm?', confirmText: 'Xóa nhóm', confirmVariant: 'danger' })) { groups.splice(groupIndex, 1); render(); }
                }, true));
                section.append(header);
                field(section, 'Tên nhóm', group.title, value => group.title = value);
                const columns = field(section, 'Số cột hiển thị (1–3)', group.columns, value => group.columns = Number(value), 'number'); columns.min = '1'; columns.max = '3'; columns.step = '1';
                group.questions.forEach((q, questionIndex) => {
                    const card = node('div', null, 'border-top pt-3 mt-3');
                    const heading = node('div', null, 'd-flex justify-content-between align-items-center mb-3');
                    heading.append(node('strong', `Câu ${questionIndex + 1} · ID ${q.id}`), button('Xóa câu', async () => {
                        if (await root.JtechnUI.confirm({ title: 'Xóa câu hỏi', message: 'Xóa câu hỏi này?', confirmText: 'Xóa câu', confirmVariant: 'danger' })) { group.questions.splice(questionIndex, 1); render(); }
                    }, true)); card.append(heading);
                    field(card, 'Nội dung câu hỏi', q.text, value => q.text = value, 'textarea');
                    field(card, 'Điểm', q.points, value => q.points = Number(value), 'number');
                    field(card, 'Ảnh câu hỏi (ví dụ /img/congdoandap/group2_1.png)', q.image, value => q.image = value.trim());
                    const kindLabel = node('label', 'Loại câu hỏi', 'form-label');
                    const kind = node('select', null, 'form-select mb-3'); kind.id = 'exam-builder-' + (++controlId); kindLabel.htmlFor = kind.id;
                    [['single', 'Chọn một đáp án'], ['multiple', 'Chọn nhiều đáp án'], ['matching', 'Ghép số theo hình']].forEach(([value, text]) => kind.add(new Option(text, value)));
                    kind.value = q.kind; kind.addEventListener('change', () => { q.kind = kind.value; q.correct = q.kind === 'matching' ? q.options.map(() => '') : []; render(); }); card.append(kindLabel, kind);
                    card.append(node('p', q.kind === 'matching' ? 'Điền số trên hình tương ứng mỗi lựa chọn.' : 'Tích vào lựa chọn đúng bên dưới.', 'small text-muted'));
                    q.options.forEach((option, index) => {
                        const row = node('div', null, 'border rounded p-2 mb-2');
                        field(row, `Lựa chọn ${index + 1} (có thể nhập HTML)`, option.text, value => option.text = value, 'textarea');
                        if (q.kind === 'matching') {
                            const input = field(row, 'Số ghép đúng', q.correct[index] || '', value => q.correct[index] = value, 'number'); input.min = '1'; input.max = q.options.length; input.step = '1';
                        } else {
                            const label = node('label', null, 'form-check d-block'); const input = node('input', null, 'form-check-input');
                            input.type = q.kind === 'single' ? 'radio' : 'checkbox'; input.name = 'correct-' + q.id; input.checked = q.correct.includes(option.id);
                            input.addEventListener('change', () => { q.correct = q.kind === 'single' ? [option.id] : q.options.filter((o, i) => i === index ? input.checked : q.correct.includes(o.id)).map(o => o.id); });
                            label.append(input, node('span', 'Đáp án đúng')); row.append(label);
                        }
                        row.append(button('Xóa lựa chọn', () => { q.options.splice(index, 1); q.correct = q.kind === 'matching' ? q.options.map(() => '') : q.correct.filter(id => id !== option.id); render(); }, true)); card.append(row);
                    });
                    card.append(button('+ Thêm lựa chọn', () => {
                        let id = 0; while (q.options.some(option => option.id === String(id))) id++;
                        q.options.push({ id: String(id), text: '' }); if (q.kind === 'matching') q.correct.push(''); render();
                    })); section.append(card);
                });
                section.append(button('+ Thêm câu hỏi', () => {
                    const id = Math.max(0, ...groups.flatMap(g => g.questions).map(q => q.id)) + 1;
                    group.questions.push({ id, text: '', points: 1, image: '', kind: 'single', options: [{ id: '0', text: '' }, { id: '1', text: '' }], correct: [] }); render();
                })); container.append(section);
            });
            if (!groups.length) container.append(node('p', 'Chưa có câu hỏi. Thêm nhóm hoặc import file JSON để bắt đầu.', 'text-muted small'));
            update();
        }
        render();
        return {
            reset() { groups = []; render(); },
            addGroup() { groups.push({ title: '', columns: 1, questions: [] }); render(); },
            load(value) { const definition = normalizeDefinition(value); groups = definition.groups; render(); return definition; },
            hasContent() { return groups.length > 0; },
            definition(title, minutes, validate = true) {
                const definition = { title, minutes: Number(minutes), groups };
                if (validate) validateDefinition(definition);
                return JSON.parse(JSON.stringify(definition));
            }
        };
    }
    const api = { normalizeDefinition, validateDefinition, create };
    if (typeof module !== 'undefined' && module.exports) module.exports = api;
    else root.JtechnExamQuestionEditor = api;
})(typeof window !== 'undefined' ? window : globalThis);
