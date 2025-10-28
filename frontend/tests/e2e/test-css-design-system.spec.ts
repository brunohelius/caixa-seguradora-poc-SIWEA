import { test, expect } from '@playwright/test';

test.describe('CSS Design System Validation', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the app
    await page.goto('http://localhost:5173/claims/search');
  });

  test('should load all CSS files correctly', async ({ page }) => {
    // Wait for page to fully load
    await page.waitForLoadState('networkidle');

    // Check if CSS is loaded by verifying computed styles
    const body = page.locator('body');

    // Verify body has the correct background color from design system
    const bgColor = await body.evaluate((el) => {
      return window.getComputedStyle(el).backgroundColor;
    });

    // Should be --surface-secondary: #F9FAFB which is rgb(249, 250, 251)
    expect(bgColor).toMatch(/rgb\(249,\s*250,\s*251\)/);
  });

  test('should apply card-modern styles correctly', async ({ page }) => {
    await page.waitForLoadState('networkidle');

    const card = page.locator('.card-modern').first();
    await expect(card).toBeVisible();

    // Verify card has correct styles
    const styles = await card.evaluate((el) => {
      const computed = window.getComputedStyle(el);
      return {
        borderRadius: computed.borderRadius,
        backgroundColor: computed.backgroundColor,
        boxShadow: computed.boxShadow,
      };
    });

    // Card should have 12px border radius
    expect(styles.borderRadius).toBe('12px');

    // Card should have white background (rgb(255, 255, 255))
    expect(styles.backgroundColor).toMatch(/rgb\(255,\s*255,\s*255\)/);

    // Card should have box shadow
    expect(styles.boxShadow).not.toBe('none');
  });

  test('should apply bg-gradient-caixa to header', async ({ page }) => {
    await page.waitForLoadState('networkidle');

    const header = page.locator('.bg-gradient-caixa').first();
    await expect(header).toBeVisible();

    const backgroundImage = await header.evaluate((el) => {
      return window.getComputedStyle(el).backgroundImage;
    });

    // Should have linear-gradient
    expect(backgroundImage).toContain('linear-gradient');
  });

  test('should apply btn-primary styles correctly', async ({ page }) => {
    await page.waitForLoadState('networkidle');

    const button = page.locator('.btn-primary').first();
    await expect(button).toBeVisible();

    const styles = await button.evaluate((el) => {
      const computed = window.getComputedStyle(el);
      return {
        backgroundColor: computed.backgroundColor,
        color: computed.color,
        borderRadius: computed.borderRadius,
        padding: computed.padding,
      };
    });

    // Button should have Caixa blue background (--caixa-blue-700: #0047BB = rgb(0, 71, 187))
    expect(styles.backgroundColor).toMatch(/rgb\(0,\s*71,\s*187\)/);

    // Button should have white text color
    expect(styles.color).toMatch(/rgb\(255,\s*255,\s*255\)/);

    // Button should have 8px border radius
    expect(styles.borderRadius).toBe('8px');
  });

  test('should apply input-modern styles correctly', async ({ page }) => {
    await page.waitForLoadState('networkidle');

    const input = page.locator('.input-modern').first();
    await expect(input).toBeVisible();

    const styles = await input.evaluate((el) => {
      const computed = window.getComputedStyle(el);
      return {
        borderRadius: computed.borderRadius,
        padding: computed.padding,
        borderWidth: computed.borderWidth,
      };
    });

    // Input should have 8px border radius
    expect(styles.borderRadius).toBe('8px');

    // Input should have border
    expect(styles.borderWidth).toBe('1px');
  });

  test('should have all CSS variables defined in :root', async ({ page }) => {
    await page.waitForLoadState('networkidle');

    const cssVars = await page.evaluate(() => {
      const root = document.documentElement;
      const computed = window.getComputedStyle(root);

      return {
        caixaBlue700: computed.getPropertyValue('--caixa-blue-700').trim(),
        caixaYellow500: computed.getPropertyValue('--caixa-yellow-500').trim(),
        surfacePrimary: computed.getPropertyValue('--surface-primary').trim(),
        surfaceSecondary: computed.getPropertyValue('--surface-secondary').trim(),
        shadowSm: computed.getPropertyValue('--shadow-sm').trim(),
        fontSans: computed.getPropertyValue('--font-sans').trim(),
      };
    });

    // Verify all critical CSS variables are defined
    expect(cssVars.caixaBlue700).toBe('#0047BB');
    expect(cssVars.caixaYellow500).toBe('#FFB81C');
    expect(cssVars.surfacePrimary).toBe('#FFFFFF');
    expect(cssVars.surfaceSecondary).toBe('#F9FAFB');
    expect(cssVars.shadowSm).toContain('rgba');
    expect(cssVars.fontSans).toContain('Inter');
  });

  test('should apply container-modern max-width correctly', async ({ page }) => {
    await page.waitForLoadState('networkidle');

    const container = page.locator('.container-modern').first();
    await expect(container).toBeVisible();

    const maxWidth = await container.evaluate((el) => {
      return window.getComputedStyle(el).maxWidth;
    });

    // Container should have 1400px max-width
    expect(maxWidth).toBe('1400px');
  });

  test('should verify page title is visible', async ({ page }) => {
    await page.waitForLoadState('networkidle');

    const title = page.locator('h2').filter({ hasText: 'Pesquisa de Sinistros' });
    await expect(title).toBeVisible();

    // Verify title has correct styling
    const color = await title.evaluate((el) => {
      return window.getComputedStyle(el).color;
    });

    // Title should be white (in gradient header)
    expect(color).toMatch(/rgb\(255,\s*255,\s*255\)/);
  });

  test('should have proper font rendering', async ({ page }) => {
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');

    const fontFamily = await body.evaluate((el) => {
      return window.getComputedStyle(el).fontFamily;
    });

    // Should use the design system font stack
    expect(fontFamily).toContain('Inter');
  });

  test('should take screenshot for visual verification', async ({ page }) => {
    await page.waitForLoadState('networkidle');

    // Wait a bit more to ensure all styles are applied
    await page.waitForTimeout(1000);

    // Take full page screenshot
    await page.screenshot({
      path: 'tests/e2e/screenshots/design-system-validation.png',
      fullPage: true
    });

    // Take screenshot of just the card
    const card = page.locator('.card-modern').first();
    await card.screenshot({
      path: 'tests/e2e/screenshots/card-modern-validation.png'
    });
  });
});
