import { expect, type Locator, type Page, type TestInfo } from '@playwright/test';
import { readFile, writeFile } from 'node:fs/promises';

async function assertMeshInsideImage(page: Page, png: Buffer) {
  // Viridis is colored while the background, grid and axis lettering are
  // neutral. Exclude the rightmost color scale and measure the actual projected
  // mesh pixels rather than accepting camera/layout metadata or a nonblank PNG.
  const bounds = await page.evaluate(async source => {
    const image = new Image(); image.src = source; await image.decode();
    const canvas = document.createElement('canvas'); canvas.width = image.width; canvas.height = image.height;
    const context = canvas.getContext('2d')!; context.drawImage(image, 0, 0);
    const pixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
    let left = canvas.width, right = -1, top = canvas.height, bottom = -1, colored = 0;
    const limit = Math.floor(canvas.width * 0.8);
    for (let y = 0; y < canvas.height; y++) for (let x = 0; x < limit; x++) {
      const offset = (y * canvas.width + x) * 4;
      if (Math.max(pixels[offset], pixels[offset + 1], pixels[offset + 2]) -
          Math.min(pixels[offset], pixels[offset + 1], pixels[offset + 2]) <= 35) continue;
      colored++; left = Math.min(left, x); right = Math.max(right, x);
      top = Math.min(top, y); bottom = Math.max(bottom, y);
    }
    return { width: canvas.width, height: canvas.height, limit, left, right, top, bottom, colored };
  }, 'data:image/png;base64,' + png.toString('base64'));
  expect(bounds.colored, 'The composed image must contain the sampled surface').toBeGreaterThan(1000);
  expect(bounds.left, 'Surface must leave room for the left axis labels').toBeGreaterThan(12);
  expect(bounds.right, 'Surface must fit before the color-scale region').toBeLessThan(bounds.limit - 12);
  expect(bounds.top, 'Surface must not be cropped at the top').toBeGreaterThan(12);
  expect(bounds.bottom, 'Surface must leave room for the ground axis labels').toBeLessThan(bounds.height - 20);
}

export async function assertSurfaceFraming(page: Page, surface: Locator, info: TestInfo, stage: string) {
  const screenshot = info.outputPath(stage + '-viewport.png');
  const viewport = await surface.screenshot({ path: screenshot });
  await info.attach(stage + '-viewport', { path: screenshot, contentType: 'image/png' });
  await assertMeshInsideImage(page, viewport);
  const pending = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Download PNG', exact: true }).click();
  const download = await pending;
  const png = await readFile((await download.path())!);
  expect(download.suggestedFilename()).toBe('computator-chart.png');
  expect(png.subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
  expect([png.readUInt32BE(16), png.readUInt32BE(20)]).toEqual([1200, 800]);
  const exported = info.outputPath(stage + '-export.png');
  await writeFile(exported, png);
  await info.attach(stage + '-export', { path: exported, contentType: 'image/png' });
  await assertMeshInsideImage(page, png);
}
