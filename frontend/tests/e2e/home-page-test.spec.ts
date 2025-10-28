import { test, expect } from '@playwright/test';

test.describe('Home Page Test', () => {
  test('should load home page', async ({ page }) => {
    // Listen for console errors
    const consoleErrors: string[] = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    // Navigate to home page
    await page.goto('http://localhost:5173/');

    // Wait a bit
    await page.waitForTimeout(3000);

    // Check what's on the page
    const bodyText = await page.textContent('body');
    console.log('Home page body text:', bodyText?.substring(0, 200));

    // Log errors if any
    if (consoleErrors.length > 0) {
      console.log('Console errors:');
      consoleErrors.forEach((error, index) => {
        console.log(`${index + 1}. ${error}`);
      });
    }

    // Take screenshot
    await page.screenshot({ path: 'tests/e2e/screenshots/home-page.png', fullPage: true });

    expect(consoleErrors.length, `Found ${consoleErrors.length} console errors`).toBe(0);
  });
});
