import { test, expect } from '@playwright/test';

/**
 * Visual Regression Tests for Caixa Seguradora Claims System
 * Verifies that the UI looks correct across different devices and browsers
 */

test.describe('Claims Search Page Visual Regression', () => {
  test('should render search page correctly on desktop', async ({ page }) => {
    await page.goto('/claims/search');

    // Wait for page to be fully loaded
    await page.waitForLoadState('networkidle');

    // Check that the header is visible
    await expect(page.getByRole('heading', { name: /Pesquisa de Sinistros/i })).toBeVisible();

    // Check that the Caixa logo is visible
    await expect(page.getByAltText('Caixa Seguradora')).toBeVisible();

    // Check navigation links
    await expect(page.getByRole('link', { name: /Pesquisa/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /Dashboard/i })).toBeVisible();

    // Take a full page screenshot
    await expect(page).toHaveScreenshot('search-page-desktop.png', {
      fullPage: true,
    });
  });

  test('should render search form with protocol option selected', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Select "Por Protocolo" radio button (should be selected by default)
    const protocolRadio = page.getByLabel('Por Protocolo');
    await expect(protocolRadio).toBeChecked();

    // Check that protocol fields are visible
    await expect(page.getByLabel('Fonte *')).toBeVisible();
    await expect(page.getByLabel('Número do Protocolo *')).toBeVisible();
    await expect(page.getByLabel('DAC *')).toBeVisible();

    // Take screenshot of form with protocol fields
    await expect(page.locator('form')).toHaveScreenshot('search-form-protocol.png');
  });

  test('should render search form with claim number option', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Select "Por Número do Sinistro" radio button
    await page.getByLabel('Por Número do Sinistro').click();

    // Check that claim number fields are visible
    await expect(page.getByLabel('Tipo Seguro *')).toBeVisible();
    await expect(page.getByLabel('Origem *')).toBeVisible();
    await expect(page.getByLabel('Ramo *')).toBeVisible();
    await expect(page.getByLabel('Número *')).toBeVisible();

    // Take screenshot of form with claim fields
    await expect(page.locator('form')).toHaveScreenshot('search-form-claim.png');
  });

  test('should render search form with leader code option', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Select "Por Código Líder" radio button
    await page.getByLabel('Por Código Líder').click();

    // Check that leader fields are visible
    await expect(page.getByLabel('Código Líder *')).toBeVisible();
    await expect(page.getByLabel('Sinistro Líder *')).toBeVisible();

    // Take screenshot of form with leader fields
    await expect(page.locator('form')).toHaveScreenshot('search-form-leader.png');
  });

  test('should render instructions card', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Check instructions card is visible
    await expect(page.getByRole('heading', { name: /Instruções de Pesquisa/i })).toBeVisible();

    // Take screenshot of instructions card
    const instructionsCard = page.locator('div').filter({ hasText: /Instruções de Pesquisa/ }).last();
    await expect(instructionsCard).toHaveScreenshot('instructions-card.png');
  });
});

test.describe('Navigation and Footer Visual Regression', () => {
  test('should render header with logo and navigation', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Check header elements
    await expect(page.getByRole('banner')).toBeVisible();
    await expect(page.getByAltText('Caixa Seguradora')).toBeVisible();
    await expect(page.getByText('Sistema de Sinistros')).toBeVisible();

    // Take screenshot of header
    await expect(page.locator('header')).toHaveScreenshot('header.png');
  });

  test('should render footer with copyright info', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Check footer elements
    await expect(page.getByRole('contentinfo')).toBeVisible();
    await expect(page.getByText(/© 2025 Caixa Seguradora/i)).toBeVisible();
    await expect(page.getByText(/Sistema de Gestão de Sinistros/i)).toBeVisible();

    // Take screenshot of footer
    await expect(page.locator('footer')).toHaveScreenshot('footer.png');
  });

  test('should highlight active navigation link', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Check that search link is active (has primary background)
    const searchLink = page.getByRole('link', { name: /Pesquisa/i });
    await expect(searchLink).toHaveClass(/bg-primary/);

    // Take screenshot showing active state
    await expect(page.locator('nav')).toHaveScreenshot('nav-search-active.png');
  });
});

test.describe('Responsive Design Visual Regression', () => {
  test('should render correctly on mobile (375x667)', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Check that content is visible and properly laid out
    await expect(page.getByRole('heading', { name: /Pesquisa de Sinistros/i })).toBeVisible();

    // Take full page screenshot
    await expect(page).toHaveScreenshot('search-page-mobile.png', {
      fullPage: true,
    });
  });

  test('should render correctly on tablet (768x1024)', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Check that content is visible and properly laid out
    await expect(page.getByRole('heading', { name: /Pesquisa de Sinistros/i })).toBeVisible();

    // Take full page screenshot
    await expect(page).toHaveScreenshot('search-page-tablet.png', {
      fullPage: true,
    });
  });

  test('should render correctly on large desktop (1920x1080)', async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Check that content is visible and properly laid out
    await expect(page.getByRole('heading', { name: /Pesquisa de Sinistros/i })).toBeVisible();

    // Take full page screenshot
    await expect(page).toHaveScreenshot('search-page-desktop-large.png', {
      fullPage: true,
    });
  });
});

test.describe('Form Interaction Visual States', () => {
  test('should show validation errors with correct styling', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Try to submit empty form
    await page.getByRole('button', { name: /Pesquisar/i }).click();

    // Wait for validation errors to appear
    await expect(page.getByText('Fonte é obrigatório')).toBeVisible();

    // Take screenshot showing error state
    await expect(page.locator('form')).toHaveScreenshot('form-validation-errors.png');
  });

  test('should show loading state when searching', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Fill in form
    await page.getByLabel('Fonte *').fill('1');
    await page.getByLabel('Número do Protocolo *').fill('123456');
    await page.getByLabel('DAC *').fill('5');

    // Mock slow API response
    await page.route('**/api/claims/search**', async route => {
      await new Promise(resolve => setTimeout(resolve, 2000));
      await route.fulfill({
        status: 404,
        body: JSON.stringify({
          sucesso: false,
          mensagem: 'Sinistro não encontrado'
        })
      });
    });

    // Click search and immediately take screenshot of loading state
    const searchPromise = page.getByRole('button', { name: /Pesquisando/i }).waitFor();
    await page.getByRole('button', { name: /Pesquisar/i }).click();

    // Wait for loading state
    await searchPromise;

    // Take screenshot of loading state
    await expect(page.locator('form')).toHaveScreenshot('form-loading-state.png');
  });

  test('should show error alert with correct styling', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Fill in form
    await page.getByLabel('Fonte *').fill('1');
    await page.getByLabel('Número do Protocolo *').fill('123456');
    await page.getByLabel('DAC *').fill('5');

    // Mock API error
    await page.route('**/api/claims/search**', route =>
      route.fulfill({
        status: 404,
        body: JSON.stringify({
          sucesso: false,
          mensagem: 'Sinistro não encontrado'
        })
      })
    );

    // Submit form
    await page.getByRole('button', { name: /Pesquisar/i }).click();

    // Wait for error message
    await expect(page.getByText('Sinistro não encontrado')).toBeVisible();

    // Take screenshot of error state
    await expect(page).toHaveScreenshot('search-error-alert.png', {
      fullPage: true,
    });
  });
});

test.describe('Accessibility and Focus States', () => {
  test('should show focus ring on interactive elements', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Tab through form fields and check focus states
    await page.keyboard.press('Tab'); // Focus on logo/home link
    await page.keyboard.press('Tab'); // Focus on Pesquisa link
    await page.keyboard.press('Tab'); // Focus on Dashboard link
    await page.keyboard.press('Tab'); // Focus on first radio button

    // Take screenshot showing focus ring
    await expect(page).toHaveScreenshot('focus-state-navigation.png');
  });

  test('should render with proper contrast ratios', async ({ page }) => {
    await page.goto('/claims/search');
    await page.waitForLoadState('networkidle');

    // Check accessibility
    // Note: For full accessibility testing, you would use axe-playwright
    // This is just a visual check

    // Take screenshot for manual contrast verification
    await expect(page).toHaveScreenshot('contrast-check.png', {
      fullPage: true,
    });
  });
});
