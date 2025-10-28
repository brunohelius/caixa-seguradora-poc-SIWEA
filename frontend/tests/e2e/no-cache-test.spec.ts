import { test, expect } from '@playwright/test';

test.describe('No Cache Test', () => {
  test('Claims History page with cache disabled', async ({ browser }) => {
    // Create new context with cache disabled
    const context = await browser.newContext({
      viewport: { width: 1920, height: 1080 },
      // Disable cache
      serviceWorkers: 'block',
    });

    const page = await context.newPage();

    // Disable cache
    await page.route('**/*', route => {
      route.continue({
        headers: {
          ...route.request().headers(),
          'Cache-Control': 'no-cache, no-store, must-revalidate'
        }
      });
    });

    // Navigate with hard reload
    await page.goto('http://localhost:5173/claims/history', {
      waitUntil: 'networkidle'
    });

    // Force reload
    await page.reload({ waitUntil: 'networkidle' });

    // Wait for content
    await page.waitForTimeout(3000);

    // Check for heading
    const h1 = await page.locator('h1').textContent();
    console.log('H1 content:', h1);

    // Take screenshot
    await page.screenshot({ path: 'tests/e2e/screenshots/no-cache-test.png', fullPage: true });

    // Close context
    await context.close();

    // Assert
    expect(h1).toContain('Histórico');
  });
});
