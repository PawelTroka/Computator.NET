import { test, expect, type Page } from '@playwright/test';

const definitions = 'public static double Code(string value) { return value[0]; }';
const png = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jL1sAAAAASUVORK5CYII=', 'base64');
const headers = { 'access-control-allow-origin': '*', 'access-control-allow-methods': 'GET, POST, OPTIONS', 'access-control-allow-headers': 'content-type' };

test.beforeEach(async ({ page }) => {
  await page.addInitScript(code => {
    sessionStorage.setItem('computator.custom-functions.v1', JSON.stringify({ schemaVersion: 1, code }));
  }, definitions);
  await page.route('**/api/**', route => {
    if (route.request().method() === 'OPTIONS') return route.fulfill({ status: 204, headers });
    if (route.request().url().includes('/autocomplete/')) return route.fulfill({ headers, json: [] });
    if (route.request().url().endsWith('/list-methods')) return route.fulfill({ headers, json: ['Method A'] });
    if (route.request().url().endsWith('/v2/chart')) return route.fulfill({ headers, contentType: 'image/png', body: png });
    return route.fulfill({ headers, json: { result: '42' } });
  });
});

async function submit(page: Page, button = 'Calculate!', endpoint = '/v2/execute') {
  const sent = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith(endpoint));
  await page.getByRole('button', { name: button, exact: true }).click();
  const request = await sent;
  await expect(page.getByRole('button', { name: button, exact: true })).toBeEnabled();
  return request.postDataJSON();
}

async function replaceExpression(page: Page, source: string) {
  const editor = page.locator('#expressionTextArea textarea');
  await editor.press('ControlOrMeta+A');
  await editor.press('Backspace');
  if (source) await page.keyboard.insertText(source);
}

for (const [path, button, endpoint, numericInput] of [
  ['/calculate', 'Calculate!', '/v2/execute', 'x'],
  ['/numerical-calculations', 'Compute!', '/v2/execute', 'a'],
  ['/', 'Draw chart!', '/v2/chart', 'XMIN'],
]) {
  test(`quoted custom-function arguments survive requests and later edits on ${path}`, async ({ page }) => {
    await page.goto(path);
    const editor = page.locator('#expressionTextArea textarea');
    await replaceExpression(page, 'Code("*")');
    expect(await submit(page, button, endpoint)).toMatchObject({ expression: 'Code("*")', customFunctions: definitions });

    // A parent render and an unrelated edit must not rewrite an earlier literal.
    await page.getByRole('spinbutton', { name: numericInput, exact: true }).fill('-2');
    await editor.press('ControlOrMeta+End');
    await page.keyboard.insertText(' + 2*3');
    expect(await submit(page, button, endpoint)).toMatchObject({ expression: 'Code("*") + 2*3', customFunctions: definitions });
  });
}

test('literal and comment forms preserve their exact source, including newlines', async ({ page }) => {
  await page.goto('/calculate');
  const examples = [
    "'*'",
    'Code("\\\"*")',
    'Code(@"C:\\*\\""quoted*""")',
    'Code($"*{2*3}")',
    'Code($@"C:\\*\\{2*3}")',
    'Code("""raw * and "quoted*" text""")',
    'Code("""\nraw * across lines\n""")',
    'Code($$"""* {literal} {{2*3}}""")',
    '2/* * comment */*3',
    '2 // * comment\n+3',
    '2 // * comment\r\n+3',
    '2 /* first\n* second */ * 3',
  ];
  for (const expression of examples) {
    await replaceExpression(page, expression);
    // Ace normalizes pasted CRLF to LF; the expression must retain that line
    // boundary because replacing it with a space changes strings and comments.
    expect((await submit(page)).expression, expression).toBe(expression.replace(/\r\n/g, '\n'));
  }
});

test('typing unfinished literals and comment delimiters does not alter them', async ({ page }) => {
  await page.goto('/calculate');
  const editor = page.locator('#expressionTextArea textarea');
  await editor.pressSequentially("'*'");
  expect((await submit(page)).expression).toBe("'*'");

  await replaceExpression(page, '');
  await editor.pressSequentially('Code("*") + 2*3');
  expect((await submit(page)).expression).toBe('Code("*") + 2*3');

  await replaceExpression(page, '');
  await editor.pressSequentially('2*3 /* * note */');
  expect((await submit(page)).expression).toBe('2·3 /* * note */');
});

test('ordinary mathematical input retains multiplication notation and single-line editing', async ({ page }) => {
  await page.goto('/calculate');
  const editor = page.locator('#expressionTextArea textarea');
  for (const [source, expected] of [['2*3', '2·3'], ['2·3', '2·3'], ['2*\n3', '2· 3']]) {
    await replaceExpression(page, source);
    expect((await submit(page)).expression).toBe(expected);
  }
  await replaceExpression(page, '2');
  await editor.press('End');
  await editor.press('Enter');
  await page.keyboard.insertText('+3');
  expect((await submit(page)).expression).toBe('2+3');
});
