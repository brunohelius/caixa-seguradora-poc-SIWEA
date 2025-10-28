import { test, expect } from '@playwright/test';

test.describe('Final Verification', () => {
  test('Claims History page should load successfully', async ({ page }) => {
    // Navigate to claims history
    await page.goto('http://localhost:5173/claims/history');

    // Wait for the page to load
    await page.waitForLoadState('networkidle');

    // Check for the main heading
    await expect(page.locator('h1')).toContainText('Histórico de Sinistros', { timeout: 10000 });

    // Check for the search form
    const form = page.locator('form');
    await expect(form).toBeVisible();

    // Take screenshot
    await page.screenshot({ path: 'tests/e2e/screenshots/final-success.png', fullPage: true });

    console.log('✅ Test PASSED: Claims History page loaded successfully!');
  });
});
