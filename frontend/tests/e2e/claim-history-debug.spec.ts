import { test, expect } from '@playwright/test';

test.describe('Claims History Page Debug', () => {
  test('should load and show detailed errors', async ({ page }) => {
    // Listen for console errors
    const consoleErrors: string[] = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    // Listen for page errors
    const pageErrors: Error[] = [];
    page.on('pageerror', error => {
      pageErrors.push(error);
    });

    // Navigate to the page
    await page.goto('http://localhost:5173/claims/history');

    // Wait a bit for any async loading
    await page.waitForTimeout(2000);

    // Take a screenshot for debugging
    await page.screenshot({ path: 'tests/e2e/screenshots/claim-history-debug.png', fullPage: true });

    // Check what's actually on the page
    const bodyText = await page.textContent('body');
    console.log('Page body text:', bodyText);

    // Check for specific elements
    const header = await page.locator('h1').first();
    const headerText = await header.textContent().catch(() => 'No header found');
    console.log('Header text:', headerText);

    // Check for search form
    const searchForm = await page.locator('form').count();
    console.log('Number of forms found:', searchForm);

    // Log all errors
    if (consoleErrors.length > 0) {
      console.log('Console errors found:');
      consoleErrors.forEach((error, index) => {
        console.log(`${index + 1}. ${error}`);
      });
    }

    if (pageErrors.length > 0) {
      console.log('Page errors found:');
      pageErrors.forEach((error, index) => {
        console.log(`${index + 1}. ${error.message}`);
        console.log('   Stack:', error.stack);
      });
    }

    // The test should fail if there are errors
    expect(consoleErrors.length, `Found ${consoleErrors.length} console errors`).toBe(0);
    expect(pageErrors.length, `Found ${pageErrors.length} page errors`).toBe(0);

    // Verify page loaded correctly
    await expect(header).toContainText('Histórico de Sinistros');
  });
});
