import { test, expect, type Dialog, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';

const maximum = 65536;
const headers = { 'access-control-allow-origin': '*', 'access-control-allow-methods': '*', 'access-control-allow-headers': '*' };
const cases = [
  { route: '/scripting', link: 'Scripting', editor: 'scriptTextArea', original: 'write(7);',
    storage: 'computator.script-document.v1', extension: '.tsl' },
  { route: '/custom-functions', link: 'Custom functions', editor: 'customFunctionsTextArea',
    original: 'static real f(real x) { return x + 7; }', storage: 'computator.custom-functions.v1', extension: '.tslf' }
];

async function visibleSource(page: Page, id: string) {
  return page.locator(`#${id}`).evaluate(element =>
    (element as HTMLElement & { env: { editor: { getValue: () => string } } }).env.editor.getValue());
}
async function expectSource(page: Page, id: string, source: string) {
  // Keep failure messages bounded even for a full-size source document.
  await expect.poll(async () => (await visibleSource(page, id)) === source).toBe(true);
}
async function append(page: Page, id: string, source: string) {
  const input = page.locator(`#${id} textarea`);
  await input.focus(); await input.press('ControlOrMeta+End'); await page.keyboard.insertText(source);
}
async function replace(page: Page, id: string, source: string) {
  const input = page.locator(`#${id} textarea`);
  await input.focus(); await input.press('ControlOrMeta+A'); await page.keyboard.insertText(source);
}
async function stored(page: Page, key: string) {
  return page.evaluate(key => JSON.parse(sessionStorage.getItem(key)!).code as string, key);
}
async function exported(page: Page, extension: string) {
  const pending = page.waitForEvent('download');
  await page.getByRole('button', { name: `Export ${extension}`, exact: true }).click();
  return readFile((await (await pending).path())!, 'utf8');
}
async function reloadAndStay(page: Page) {
  let complete!: (warned: boolean) => void;
  let fail!: (error: unknown) => void;
  let warned = false;
  const outcome = new Promise<boolean>((resolve, reject) => { complete = resolve; fail = reject; });
  const loaded = () => complete(false);
  const dismiss = (dialog: Dialog) => {
    if (dialog.type() !== 'beforeunload') { fail(new Error(`Unexpected ${dialog.type()} dialog`)); return; }
    warned = true; void dialog.dismiss().then(() => complete(true), fail);
  };
  page.on('dialog', dismiss); page.on('load', loaded);
  try {
    void page.reload().catch(error => { if (!warned) fail(error); });
    return await outcome;
  } finally { page.off('dialog', dismiss); page.off('load', loaded); }
}

test.beforeEach(async ({ page }) => {
  await page.route('**/api/**', route => {
    if (route.request().method() === 'OPTIONS') return route.fulfill({ headers, status: 204 });
    const path = new URL(route.request().url()).pathname;
    if (path.includes('/autocomplete/')) return route.fulfill({ headers, json: [] });
    if (path.endsWith('/list-methods')) return route.fulfill({ headers, json: ['Method A'] });
    return route.fulfill({ headers, json: { result: 'accepted' } });
  });
});

for (const { route, link, editor, original, storage, extension } of cases) {
  test(`${link}: repeated oversized edits stay exact through export, navigation and a cancelled reload`, async ({ page }) => {
    await page.goto(route);
    await append(page, editor, original);
    if (extension === '.tslf') await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
    const addition = '\n// ' + 'x'.repeat(maximum);
    let draft = original;
    for (const text of [addition, '\n// second paste — 🙂']) {
      await append(page, editor, text); draft += text;
      await expect(page.getByRole('alert')).toContainText('65,536');
      await expectSource(page, editor, draft);
      if (extension === '.tslf') await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
      expect(await stored(page, storage)).toBe(original);
      expect(await exported(page, extension) === draft).toBe(true);
    }
    if (extension === '.tsl') await expect(page.getByTestId('script-draft-state')).toContainText('memory only');
    else await expect(page.getByRole('status')).toHaveText('Unsaved changes');
    await page.getByRole('link', { name: 'Calculate', exact: true }).click();
    expect(await reloadAndStay(page), 'Oversized edits must retain the reload protection after leaving the editor').toBe(true);
    await page.getByRole('link', { name: link, exact: true }).click();
    await expectSource(page, editor, draft);
    expect(await exported(page, extension) === draft).toBe(true);
    expect(await stored(page, storage)).toBe(original);
  });

  test(`${link}: undo and redo retain exact edits at the character limit and shortening restores saving`, async ({ page }) => {
    await page.goto(route);
    const boundary = original + '\n// ' + 'x'.repeat(maximum - original.length - 4);
    await page.getByLabel(`Import ${extension}`, { exact: true }).setInputFiles({
      name: `boundary${extension}`, mimeType: 'text/plain', buffer: Buffer.from(boundary)
    });
    if (extension === '.tslf') await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
    await expectSource(page, editor, boundary);
    expect((await stored(page, storage)).length).toBe(maximum);
    await append(page, editor, '🙂');
    await expectSource(page, editor, boundary + '🙂');
    await expect(page.getByRole('alert')).toContainText('65,536');
    const input = page.locator(`#${editor} textarea`);
    await input.press('ControlOrMeta+Z');
    await expectSource(page, editor, boundary);
    await expect(page.getByRole('alert')).toHaveCount(0);
    await input.press('ControlOrMeta+Shift+Z');
    await expectSource(page, editor, boundary + '🙂');
    await expect(page.getByRole('alert')).toContainText('65,536');
    const recovered = original + '\n// recovered';
    await replace(page, editor, recovered);
    await expectSource(page, editor, recovered);
    await expect(page.getByRole('alert')).toHaveCount(0);
    if (extension === '.tslf') await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
    expect(await stored(page, storage)).toBe(recovered);
    expect(await reloadAndStay(page)).toBe(false);
    await expectSource(page, editor, recovered);
  });
}

test('an oversized script never runs an older hidden draft and shortening runs the visible source', async ({ page }) => {
  const requests: Record<string, unknown>[] = [];
  page.on('request', request => { if (request.method() === 'POST' && request.url().endsWith('/v2/execute')) requests.push(request.postDataJSON()); });
  await page.goto('/scripting');
  const original = 'write(7);';
  await append(page, 'scriptTextArea', original);
  const draft = original + '\n// ' + 'x'.repeat(maximum);
  await append(page, 'scriptTextArea', draft.slice(original.length));
  await page.getByRole('button', { name: 'Process script', exact: true }).click();
  await expect(page.getByRole('region', { name: 'Output', exact: true }).getByRole('alert')).toContainText('Shorten it before running');
  await expectSource(page, 'scriptTextArea', draft);
  expect(requests).toEqual([]);
  const recovered = 'write(42);';
  await replace(page, 'scriptTextArea', recovered);
  await page.getByRole('button', { name: 'Process script', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('accepted');
  expect(requests).toEqual([expect.objectContaining({ operation: 'script', expression: recovered })]);
});

test('oversized custom definitions block all execution tools and both real and surface charts until shortened', async ({ page }) => {
  const requests: Record<string, unknown>[] = [];
  page.on('request', request => { if (request.method() === 'POST' && /\/v2\/(execute|chart)$/.test(request.url())) requests.push(request.postDataJSON()); });
  await page.goto('/custom-functions');
  const original = 'static real f(real x) { return x + 7; }';
  await append(page, 'customFunctionsTextArea', original);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await append(page, 'customFunctionsTextArea', '\n// ' + 'x'.repeat(maximum));
  for (const [link, id, source, button] of [
    ['Calculate', 'expressionTextArea', 'f(x)', 'Calculate!'],
    ['Numerical calculations', 'expressionTextArea', 'f(x)', 'Compute!'],
    ['Scripting', 'scriptTextArea', 'write(f(1));', 'Process script'],
    ['Chart', 'expressionTextArea', 'f(x)', 'Draw chart!'],
    ['Chart', 'expressionTextArea', 'f(x) + y', 'Draw chart!']
  ]) {
    await page.getByRole('link', { name: link, exact: true }).click();
    await replace(page, id, source);
    await page.getByRole('button', { name: button, exact: true }).click();
    await expect(page.getByRole('alert')).toContainText('Shorten them in Custom functions');
    expect(requests).toEqual([]);
  }
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  const recovered = 'static real f(real x) { return x + 42; }';
  await replace(page, 'customFunctionsTextArea', recovered);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveCount(0);
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await page.getByRole('button', { name: 'Calculate!', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('accepted');
  expect(requests).toEqual([expect.objectContaining({ operation: 'calculate', expression: 'f(x)', customFunctions: recovered })]);
});
