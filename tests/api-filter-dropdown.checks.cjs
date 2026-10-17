const fs = require('node:fs');
const assert = require('node:assert/strict');
const path = require('node:path');
const source = fs.readFileSync(path.join(__dirname, '../JtechnApi/wwwroot/js/form-components.js'), 'utf8');
const marker = "document.querySelectorAll('[data-api-filter-dropdown]').forEach(dropdown => {";
const body = source.slice(source.indexOf(marker) + marker.length, source.lastIndexOf('        });'));
class Element {
    constructor() { this.value = ''; this.textContent = ''; this.dataset = {}; this.events = {}; this.children = []; }
    addEventListener(name, fn) { this.events[name] = fn; }
    setAttribute() {}
    replaceChildren() { this.children = []; }
    append(child) { this.children.push(child); }
    dispatchEvent() {}
}
const search = new Element(), value = new Element(), list = new Element(), status = new Element();
const dropdown = { dataset: { endpoint: '/search', debounce: '10', minCharacters: '1' },
    querySelector: selector => ({ '[data-filter-search]': search, '[data-filter-value]': value, '[role="listbox"]': list, '.filter-dropdown-status': status })[selector] };
const requests = [];
const fetch = (url, options) => new Promise(resolve => requests.push({ url, options, resolve }));
const document = { createElement: () => new Element(), addEventListener() {} };
const window = { location: { origin: 'http://localhost' }, clearTimeout, setTimeout };
new Function('dropdown', 'document', 'window', 'fetch', 'enableDropdownKeyboard', 'setError', body)(dropdown, document, window, fetch, () => {}, () => {});
const wait = () => new Promise(resolve => setTimeout(resolve, 20));
(async () => {
    search.value = 'a'; search.events.input(); await wait();
    requests[0].resolve({ ok: true, json: async () => [{ id: 7, name: 'Alice' }] }); await wait();
    assert.equal(list.children.length, 1);
    const option = list.children[0];
    search.value = 'b'; search.events.input(); option.events.click(); await wait();
    assert.equal(value.value, '7', 'A queued search must not clear the selected employee');
    assert.equal(requests.length, 1, 'Selection cancels the queued search');
    search.value = 'c'; search.events.input(); await wait();
    search.value = ''; search.events.input();
    requests[1].resolve({ ok: true, json: async () => [{ id: 8, name: 'Carol' }] }); await wait();
    assert.equal(value.value, '');
    assert.equal(list.children.length, 0, 'A stale response must not reopen results after clearing search');
    assert.equal(requests[1].options.signal.aborted, true);
    console.log('API dropdown: 6 regression checks passed.');
})().catch(error => { console.error(error); process.exitCode = 1; });
