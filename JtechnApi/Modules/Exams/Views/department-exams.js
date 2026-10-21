document.addEventListener('DOMContentLoaded', function () {
    'use strict';
    const $ = id => document.getElementById(id), form = $('departmentExamForm');
    const token = document.querySelector('[name="__RequestVerificationToken"]').value;
    let editing = null, busy = false;
    const questions = window.JtechnExamQuestionEditor.create($('examQuestionGroups'), $('examQuestionSummary'), $('examScoreSettings'));
    $('examAddGroup').addEventListener('click', () => { if (!busy) questions.addGroup(); });
    async function api(path = '', method = 'GET', data) {
        const response = await fetch('/api/department-exams' + path, { method, cache: 'no-store',
            headers: { 'Content-Type': 'application/json', RequestVerificationToken: token }, body: data === undefined ? undefined : JSON.stringify(data) });
        if (response.status === 204) return;
        const result = await response.json().catch(() => ({}));
        if (!response.ok) throw new Error(result.message || (result.errors && Object.values(result.errors).flat().join(' ')) || `Yêu cầu thất bại (${response.status}).`);
        return result;
    }
    async function load() {
        const rows = await api($('examDepartmentFilter').value ? '?departmentId=' + $('examDepartmentFilter').value : '');
        window.JtechnDataTable.setRows('departmentExamTable', rows.map(row => ({ ...row, status: row.isActive ? 'Mở thi' : 'Đóng thi' })));
        $('examCount').textContent = `${rows.length} bài thi`;
        $('examLaunchLinks').replaceChildren();
        rows.filter(row => row.isActive).forEach(row => {
            const link = document.createElement('a'); link.className = 'btn btn-sm btn-outline-primary';
            link.href = '/admin/exams?examId=' + row.id; link.textContent = 'Làm bài: ' + row.name;
            $('examLaunchLinks').append(link);
        });
    }
    const fail = error => { $('examListMessage').textContent = error.message; window.JtechnUI.toast(error.message, 'error'); };
    function close() { editing = null; $('examEditor').hidden = true; form.reset(); questions.reset(); }
    $('examNew')?.addEventListener('click', () => {
        if (busy) return; close(); $('examEditor').hidden = false; $('examEditorTitle').textContent = 'Thêm bài thi'; $('examEditorMessage').textContent = '';
        $('examDepartment').value = $('examDepartmentFilter').value; $('examCode').focus();
    });
    $('examCancel').addEventListener('click', () => { if (!busy) close(); });
    $('examDepartmentFilter').addEventListener('change', () => load().catch(fail));
    $('departmentExamTable').addEventListener('datatable:edit', async event => {
        if (busy) return; busy = true;
        try {
            const exam = await api('/' + event.detail.row.id);
            editing = exam.id; $('examEditor').hidden = false; $('examEditorTitle').textContent = 'Cập nhật bài thi'; $('examEditorMessage').textContent = '';
            ['departmentId', 'code', 'name', 'description', 'minutes', 'definitionJson'].forEach(name => {
                const field = form.elements.namedItem(name); field.value = exam[name] ?? ''; field.dispatchEvent(new Event('input', { bubbles: true }));
            });
            questions.load(JSON.parse(exam.definitionJson));
            $('examActive').checked = exam.isActive; $('examEditor').scrollIntoView({ behavior: 'smooth' });
        } catch (error) { fail(error); } finally { busy = false; }
    });
    $('departmentExamTable').addEventListener('datatable:delete', async event => {
        if (busy) return; busy = true;
        try {
            if (!await window.JtechnUI.confirm({ title: 'Xóa bài thi', message: `Xóa bài thi "${event.detail.row.name}"?`, confirmText: 'Xóa bài thi', confirmVariant: 'danger' })) return;
            await api('/' + event.detail.row.id, 'DELETE'); if (editing === event.detail.row.id) close();
            window.JtechnUI.toast('Đã xóa bài thi.', 'success'); await load();
        } catch (error) { fail(error); } finally { busy = false; }
    });
    const applyDefinition = async text => {
        const definition = window.JtechnExamQuestionEditor.normalizeDefinition(JSON.parse(text.replace(/^\uFEFF/, '')));
        if (questions.hasContent() && !await window.JtechnUI.confirm({
            title: 'Import đề thi', message: 'Thay nội dung câu hỏi hiện tại bằng nội dung JSON? Thông tin mã và bộ phận được giữ nguyên.',
            confirmText: 'Thay nội dung', confirmVariant: 'primary'
        })) return;
        questions.load(definition);
        $('examDefinition').value = JSON.stringify(definition, null, 2);
        if (!editing) {
            if (!$('examName').value && definition.title) { $('examName').value = definition.title; $('examName').dispatchEvent(new Event('input', { bubbles: true })); }
            if (Number.isInteger(definition.minutes) && definition.minutes >= 1 && definition.minutes <= 480) $('examMinutes').value = definition.minutes;
        }
        window.JtechnUI.toast('Đã import câu hỏi. Kiểm tra nội dung rồi nhấn Lưu bài thi.', 'success');
    };
    $('examApplyJson').addEventListener('click', async () => {
        if (busy) return; busy = true;
        try { await applyDefinition($('examDefinition').value); }
        catch (error) { window.JtechnUI.toast(error.message, 'error'); }
        finally { busy = false; }
    });
    $('examRefreshJson').addEventListener('click', () => {
        if (!busy) $('examDefinition').value = JSON.stringify(questions.definition($('examName').value, $('examMinutes').value, false), null, 2);
    });
    $('examJsonFile').addEventListener('change', async () => {
        if (busy) return; busy = true;
        try {
            const file = $('examJsonFile').files[0]; if (!file) return;
            if (file.size > 1000000) throw new Error('File JSON tối đa 1 MB.');
            await applyDefinition(await file.text());
        } catch (error) { window.JtechnUI.toast(error.message, 'error'); }
        finally { busy = false; $('examJsonFile').value = ''; }
    });
    form.addEventListener('submit', async event => {
        event.preventDefault(); if (busy || !form.reportValidity()) return;
        const input = Object.fromEntries(new FormData(form).entries()); input.departmentId = Number(input.departmentId); input.minutes = Number(input.minutes); input.isActive = $('examActive').checked;
        try {
            input.definitionJson = JSON.stringify(questions.definition(input.name, input.minutes));
            if (input.definitionJson.length > 1000000) throw new Error('Nội dung đề tối đa 1 triệu ký tự.');
        } catch (error) { $('examEditorMessage').textContent = error.message; window.JtechnUI.toast(error.message, 'error'); return; }
        busy = true; $('departmentExamFields').disabled = true; $('examSave').disabled = true;
        try {
            const result = await api(editing ? '/' + editing : '', editing ? 'PUT' : 'POST', input);
            const savedId = editing ?? result.id;
            // Keep the form open until the database response confirms the requested thresholds.
            editing = savedId;
            const saved = await api('/' + savedId);
            const expected = JSON.parse(input.definitionJson).scores;
            const actual = window.JtechnExamQuestionEditor.normalizeDefinition(JSON.parse(saved.definitionJson)).scores;
            if (JSON.stringify(actual) !== JSON.stringify(expected))
                throw new Error('Máy chủ chưa lưu đúng mốc điểm. Vui lòng khởi động lại ứng dụng để áp dụng mã mới, rồi lưu lại. Nội dung form được giữ nguyên.');
            close(); window.JtechnUI.toast('Đã lưu bài thi và mốc điểm.', 'success'); await load();
        }
        catch (error) { $('examEditorMessage').textContent = error.message; window.JtechnUI.toast(error.message, 'error'); }
        finally { busy = false; $('departmentExamFields').disabled = false; $('examSave').disabled = false; }
    });
    (async () => {
        try {
            const response = await fetch('/api/departments/all'); if (!response.ok) throw new Error('Không tải được danh sách bộ phận.');
            const departments = await response.json();
            departments.forEach(d => { $('examDepartment').add(new Option(d.name, d.id)); $('examDepartmentFilter').add(new Option(d.name, d.id)); });
            $('examCode').pattern = '[A-Za-z0-9][A-Za-z0-9_-]*'; await load();
        } catch (error) { fail(error); }
    })();
});
