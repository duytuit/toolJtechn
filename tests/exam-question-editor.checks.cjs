const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const editor = require('../JtechnApi/Modules/Exams/Views/exam-question-editor.js');
const original = JSON.parse(fs.readFileSync(path.join(__dirname, '../JtechnApi/Modules/Exams/Data/stamping-new.json'), 'utf8'));
const normalize = value => editor.normalizeDefinition(value);
const definition = normalize(original);
assert.equal(definition.groups.flatMap(g => g.questions).length, 53);
assert.doesNotThrow(() => editor.validateDefinition(definition));
const pascal = value => Array.isArray(value) ? value.map(pascal) : value && typeof value === 'object'
    ? Object.fromEntries(Object.entries(value).map(([key, child]) => [key[0].toUpperCase() + key.slice(1), pascal(child)])) : value;
assert.deepEqual(normalize(pascal(original)), definition, 'Database PascalCase JSON must populate the same form');
assert.deepEqual(normalize(JSON.parse(JSON.stringify(definition))), definition, 'Import and export preserve IDs and answer keys');
assert.ok(definition.groups.flatMap(g => g.questions).some(q => q.options.some(o => o.text.includes('<br>'))), 'HTML answers are preserved as source text');
const invalid = mutate => { const copy = normalize(original); mutate(copy); return copy; };
assert.throws(() => normalize({}), /groups/);
assert.throws(() => normalize({ groups: [{ questions: null }] }), /questions/);
assert.throws(() => normalize(invalid(d => d.groups[1].questions[0].id = d.groups[0].questions[0].id)), /ID/);
assert.throws(() => editor.validateDefinition(invalid(d => d.groups[0].questions[0].points = 0)), /điểm/i);
assert.throws(() => editor.validateDefinition(invalid(d => d.groups[0].questions[0].points += 1)), /100/);
assert.throws(() => editor.validateDefinition(invalid(d => d.groups[0].questions[0].correct = [])), /đáp án/);
assert.throws(() => editor.validateDefinition(invalid(d => d.groups[0].questions[0].image = '/img/../secret')), /ảnh/);
const matching = definition.groups.flatMap(g => g.questions).find(q => q.kind === 'matching');
assert.equal(matching.correct.length, matching.options.length);
assert.throws(() => editor.validateDefinition(invalid(d => {
    const q = d.groups.flatMap(g => g.questions).find(q => q.kind === 'matching'); q.correct[0] = q.correct[1];
})), /ghép/);
assert.deepEqual(definition.scores, [80, 60]);
assert.deepEqual(normalize({ ...original, scores: [96, 90] }).scores, [96, 90]);
assert.equal(normalize({ ...original, scores: null }).scores, null);
for (const scores of [[96], [90, 96], [90, 90], [101, 90], [96, -1], ['96', 90], [NaN, 90]]) {
    assert.throws(() => editor.validateDefinition({ ...definition, scores }), /Mốc điểm/);
}
console.log('Exam question editor: all checks passed.');
