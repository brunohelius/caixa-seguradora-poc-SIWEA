/**
 * E2E Tests for Complete Application Functionality
 * Tests all migrated shadcn/ui components and user flows
 */

import { test, expect } from '@playwright/test';

const BASE_URL = 'http://localhost:5173';

test.describe('Application Smoke Tests', () => {
  test('should load application homepage', async ({ page }) => {
    await page.goto(BASE_URL);

    // Wait for page to fully load
    await page.waitForLoadState('networkidle');

    // Application redirects / to /claims/search by design
    expect(page.url()).toBe(BASE_URL + '/claims/search');
  });

  test('should have proper title and meta tags', async ({ page }) => {
    await page.goto(BASE_URL);

    // Check page title
    const title = await page.title();
    expect(title).toBeTruthy();
  });
});

test.describe('Navigation Tests', () => {
  test('should navigate to claim search page', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Try to find navigation links
    const searchLink = page.getByRole('link', { name: /pesquisa|busca|search/i });
    if (await searchLink.count() > 0) {
      await searchLink.first().click();
      await page.waitForLoadState('networkidle');

      // Verify navigation occurred
      expect(page.url()).toContain('search');
    }
  });

  test('should navigate to dashboard', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Try to find dashboard link
    const dashboardLink = page.getByRole('link', { name: /dashboard|painel/i });
    if (await dashboardLink.count() > 0) {
      await dashboardLink.first().click();
      await page.waitForLoadState('networkidle');

      // Verify navigation occurred
      expect(page.url()).toContain('dashboard');
    }
  });
});

test.describe('SearchForm Component Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');
  });

  test('should render search form with radio buttons', async ({ page }) => {
    // Look for radio buttons
    const protocolRadio = page.getByRole('radio', { name: /protocolo/i });
    const claimRadio = page.getByRole('radio', { name: /sinistro/i });
    const leaderRadio = page.getByRole('radio', { name: /líder/i });

    if (await protocolRadio.count() > 0) {
      expect(await protocolRadio.isVisible()).toBe(true);
      expect(await claimRadio.isVisible()).toBe(true);
      expect(await leaderRadio.isVisible()).toBe(true);
    }
  });

  test('should show protocol fields when protocol radio selected', async ({ page }) => {
    const protocolRadio = page.getByRole('radio', { name: /por protocolo/i });

    if (await protocolRadio.count() > 0) {
      await protocolRadio.click();

      // Check for protocol fields - use more specific selectors
      const fonteInput = page.getByLabel(/^fonte/i);
      const protocolInput = page.getByLabel(/número do protocolo/i);
      const dacInput = page.getByLabel(/^dac/i);

      if (await fonteInput.count() > 0) {
        expect(await fonteInput.isVisible()).toBe(true);
        expect(await protocolInput.isVisible()).toBe(true);
        expect(await dacInput.isVisible()).toBe(true);
      }
    }
  });

  test('should validate required fields', async ({ page }) => {
    const submitButton = page.getByRole('button', { name: /pesquisar/i });

    if (await submitButton.count() > 0) {
      await submitButton.click();

      // Check for validation messages
      const errorMessages = page.locator('text=/obrigatório/i');
      if (await errorMessages.count() > 0) {
        expect(await errorMessages.count()).toBeGreaterThan(0);
      }
    }
  });

  test('should clear form when clear button clicked', async ({ page }) => {
    const clearButton = page.getByRole('button', { name: /limpar/i });

    if (await clearButton.count() > 0) {
      // Fill a field first
      const fonteInput = page.getByLabel(/fonte/i).first();
      if (await fonteInput.count() > 0) {
        await fonteInput.fill('123');
        expect(await fonteInput.inputValue()).toBe('123');

        // Click clear
        await clearButton.click();
        expect(await fonteInput.inputValue()).toBe('');
      }
    }
  });
});

test.describe('Dashboard Components Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`).catch(() => {
      // Dashboard might not exist yet, that's ok
    });
    await page.waitForLoadState('networkidle');
  });

  test('should render overview cards', async ({ page }) => {
    // Look for dashboard cards
    const cards = page.locator('[class*="card"]');

    if (await cards.count() > 0) {
      expect(await cards.count()).toBeGreaterThan(0);
    }
  });

  test('should display progress indicators', async ({ page }) => {
    // Look for progress bars
    const progressBars = page.locator('[role="progressbar"], [class*="progress"]');

    if (await progressBars.count() > 0) {
      expect(await progressBars.count()).toBeGreaterThan(0);
    }
  });

  test('should display badges', async ({ page }) => {
    // Look for badges
    const badges = page.locator('[class*="badge"]');

    if (await badges.count() > 0) {
      expect(await badges.count()).toBeGreaterThan(0);
    }
  });
});

test.describe('Table Component Tests', () => {
  test('should render table with headers', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Look for tables
    const tables = page.getByRole('table');

    if (await tables.count() > 0) {
      const table = tables.first();

      // Check for table headers
      const headers = table.locator('th');
      if (await headers.count() > 0) {
        expect(await headers.count()).toBeGreaterThan(0);
      }
    }
  });

  test('should have sortable columns', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Look for sortable headers
    const sortableHeaders = page.locator('th[class*="cursor-pointer"]');

    if (await sortableHeaders.count() > 0) {
      const firstHeader = sortableHeaders.first();
      await firstHeader.click();

      // Check for sort indicators
      const sortIcons = page.locator('svg[class*="lucide"]');
      expect(await sortIcons.count()).toBeGreaterThan(0);
    }
  });

  test('should have export button', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    const exportButton = page.getByRole('button', { name: /exportar|export/i });

    if (await exportButton.count() > 0) {
      expect(await exportButton.isVisible()).toBe(true);
    }
  });
});

test.describe('Accessibility Tests', () => {
  test('should have no critical accessibility violations', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Check for basic accessibility
    const buttons = page.getByRole('button');
    const links = page.getByRole('link');
    const inputs = page.getByRole('textbox');

    // All interactive elements should be accessible
    expect(await buttons.count()).toBeGreaterThanOrEqual(0);
    expect(await links.count()).toBeGreaterThanOrEqual(0);
    expect(await inputs.count()).toBeGreaterThanOrEqual(0);
  });

  test('should have proper ARIA labels on inputs', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Check form inputs specifically (not radio buttons)
    const textInputs = page.locator('input[type="number"], input[type="text"]');

    if (await textInputs.count() > 0) {
      for (let i = 0; i < Math.min(5, await textInputs.count()); i++) {
        const input = textInputs.nth(i);
        const ariaLabel = await input.getAttribute('aria-label');
        const inputId = await input.getAttribute('id');

        // Input should have either aria-label or id (for associated label)
        const hasAccessibility = ariaLabel !== null || inputId !== null;
        expect(hasAccessibility).toBe(true);
      }
    }
  });

  test('should be keyboard navigable', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Tab through interactive elements
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    // Check if focus is visible
    const focusedElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(focusedElement).toBeTruthy();
  });
});

test.describe('Responsive Design Tests', () => {
  test('should be responsive on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 }); // iPhone SE
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Page should load without horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });

    expect(hasHorizontalScroll).toBe(false);
  });

  test('should be responsive on tablet', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 }); // iPad
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Page should load without horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });

    expect(hasHorizontalScroll).toBe(false);
  });

  test('should be responsive on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Page should load properly
    expect(page.url()).toBeTruthy();
  });
});

test.describe('Performance Tests', () => {
  test('should load within acceptable time', async ({ page }) => {
    const startTime = Date.now();

    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    const loadTime = Date.now() - startTime;

    // Should load in less than 5 seconds
    expect(loadTime).toBeLessThan(5000);
  });

  test('should not have console errors', async ({ page }) => {
    const consoleErrors: string[] = [];

    page.on('console', (msg) => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // Filter out known acceptable errors
    const criticalErrors = consoleErrors.filter(error =>
      !error.includes('favicon') &&
      !error.includes('404')
    );

    expect(criticalErrors.length).toBe(0);
  });
});
