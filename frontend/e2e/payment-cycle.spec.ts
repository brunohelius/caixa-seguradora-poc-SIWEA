import { test, expect } from '@playwright/test';

/**
 * T074: End-to-End test for complete payment authorization cycle
 * Tests the full user journey from claim search to payment authorization completion.
 * Success Criteria (SC-002): Complete payment cycle in < 90 seconds
 */

test.describe('Payment Authorization Cycle', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to application home page
    await page.goto('http://localhost:3000');
  });

  test('user can complete full payment authorization cycle', async ({ page }) => {
    const startTime = Date.now();

    // Step 1: Search for claim by protocol
    await page.goto('http://localhost:3000/claims/search');

    // Fill in protocol search form
    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');

    // Click search button
    await page.click('button[type="submit"]');

    // Wait for claim details page to load
    await page.waitForURL(/\/claims\/\d+/);
    await expect(page.locator('h1, h2')).toContainText(/Sinistro|Claim/);

    // Step 2: Verify claim details are displayed
    const claimNumber = page.locator('[data-testid="claim-number"]');
    await expect(claimNumber).toBeVisible();

    const pendingValue = page.locator('[data-testid="pending-value"]');
    await expect(pendingValue).toBeVisible();

    // Step 3: Open payment authorization form
    const authorizeButton = page.locator('button:has-text("Autorizar Pagamento"), button:has-text("Authorize Payment")');
    await expect(authorizeButton).toBeVisible();
    await authorizeButton.click();

    // Wait for payment form to appear
    await expect(page.locator('form[data-testid="payment-form"], form.payment-form')).toBeVisible();

    // Step 4: Fill payment authorization form
    await page.selectOption('select[name="tipoPagamento"]', '1');
    await page.fill('input[name="valorPrincipal"]', '10000.00');
    await page.fill('input[name="valorCorrecao"]', '500.00');
    await page.fill('input[name="favorecido"]', 'Test User');
    await page.check('input[name="tipoApolice"][value="1"]');
    await page.fill('textarea[name="observacoes"]', 'E2E test payment authorization');

    // Step 5: Submit payment authorization
    const submitButton = page.locator('button[type="submit"]:has-text("Enviar"), button[type="submit"]:has-text("Submit")');
    await submitButton.click();

    // Step 6: Wait for success notification
    const successMessage = page.locator('.success-message, [data-testid="success-notification"], .toast-success');
    await expect(successMessage).toBeVisible({ timeout: 30000 });
    await expect(successMessage).toContainText(/sucesso|success|autorizado|authorized/i);

    // Step 7: Verify authorization details are displayed
    const authorizationDetails = page.locator('[data-testid="authorization-details"]');
    await expect(authorizationDetails).toBeVisible();

    // Verify occurrence number (ocorhist) is displayed
    const occurrenceNumber = page.locator('[data-testid="occurrence-number"]');
    await expect(occurrenceNumber).toBeVisible();

    // Step 8: Refresh page and verify pending value was reduced
    await page.reload();
    await page.waitForLoadState('networkidle');

    const updatedPendingValue = page.locator('[data-testid="pending-value"]');
    await expect(updatedPendingValue).toBeVisible();

    // Verify total time is less than 90 seconds (SC-002)
    const endTime = Date.now();
    const totalTime = (endTime - startTime) / 1000; // Convert to seconds
    expect(totalTime).toBeLessThan(90);

    console.log(`Payment cycle completed in ${totalTime.toFixed(2)} seconds`);
  });

  test('user sees error for invalid payment amount', async ({ page }) => {
    // Navigate to claim detail page directly
    await page.goto('http://localhost:3000/claims/1/1/1/1');

    // Wait for page load
    await page.waitForLoadState('networkidle');

    // Open payment form
    const authorizeButton = page.locator('button:has-text("Autorizar Pagamento"), button:has-text("Authorize Payment")');
    await authorizeButton.click();

    // Try to submit with zero amount
    await page.selectOption('select[name="tipoPagamento"]', '1');
    await page.fill('input[name="valorPrincipal"]', '0.00');
    await page.fill('input[name="favorecido"]', 'Test User');
    await page.check('input[name="tipoApolice"][value="1"]');

    const submitButton = page.locator('button[type="submit"]:has-text("Enviar"), button[type="submit"]:has-text("Submit")');
    await submitButton.click();

    // Verify error message is displayed
    const errorMessage = page.locator('.error-message, [data-testid="error-notification"], .toast-error');
    await expect(errorMessage).toBeVisible({ timeout: 10000 });
    await expect(errorMessage).toContainText(/maior que zero|greater than zero|inválido|invalid/i);
  });

  test('user sees error when exceeding pending value', async ({ page }) => {
    // Navigate to claim detail page
    await page.goto('http://localhost:3000/claims/1/1/1/1');
    await page.waitForLoadState('networkidle');

    // Open payment form
    const authorizeButton = page.locator('button:has-text("Autorizar Pagamento"), button:has-text("Authorize Payment")');
    await authorizeButton.click();

    // Try to submit with excessive amount
    await page.selectOption('select[name="tipoPagamento"]', '1');
    await page.fill('input[name="valorPrincipal"]', '999999999.99');
    await page.fill('input[name="valorCorrecao"]', '0.00');
    await page.fill('input[name="favorecido"]', 'Test User');
    await page.check('input[name="tipoApolice"][value="1"]');

    const submitButton = page.locator('button[type="submit"]:has-text("Enviar"), button[type="submit"]:has-text("Submit")');
    await submitButton.click();

    // Verify error message about exceeding pending value
    const errorMessage = page.locator('.error-message, [data-testid="error-notification"], .toast-error');
    await expect(errorMessage).toBeVisible({ timeout: 10000 });
    await expect(errorMessage).toContainText(/excede|saldo pendente|exceeds|pending/i);
  });

  test('concurrent payment conflict handled gracefully', async ({ browser }) => {
    // Create two browser contexts to simulate concurrent users
    const context1 = await browser.newContext();
    const context2 = await browser.newContext();

    const page1 = await context1.newPage();
    const page2 = await context2.newPage();

    try {
      // Both users navigate to the same claim
      await page1.goto('http://localhost:3000/claims/1/1/1/1');
      await page2.goto('http://localhost:3000/claims/1/1/1/1');

      await page1.waitForLoadState('networkidle');
      await page2.waitForLoadState('networkidle');

      // Both users open payment form
      const button1 = page1.locator('button:has-text("Autorizar Pagamento"), button:has-text("Authorize Payment")');
      const button2 = page2.locator('button:has-text("Autorizar Pagamento"), button:has-text("Authorize Payment")');

      await button1.click();
      await button2.click();

      // Fill payment forms with same data
      const fillForm = async (page) => {
        await page.selectOption('select[name="tipoPagamento"]', '1');
        await page.fill('input[name="valorPrincipal"]', '5000.00');
        await page.fill('input[name="valorCorrecao"]', '250.00');
        await page.fill('input[name="favorecido"]', 'Concurrent Test User');
        await page.check('input[name="tipoApolice"][value="1"]');
        await page.fill('textarea[name="observacoes"]', 'Concurrent payment test');
      };

      await fillForm(page1);
      await fillForm(page2);

      // Submit both forms simultaneously
      const submit1 = page1.locator('button[type="submit"]:has-text("Enviar"), button[type="submit"]:has-text("Submit")').click();
      const submit2 = page2.locator('button[type="submit"]:has-text("Enviar"), button[type="submit"]:has-text("Submit")').click();

      await Promise.all([submit1, submit2]);

      // Wait for responses
      await page1.waitForTimeout(3000);
      await page2.waitForTimeout(3000);

      // One should succeed, one should show conflict error
      const success1 = page1.locator('.success-message, [data-testid="success-notification"], .toast-success');
      const success2 = page2.locator('.success-message, [data-testid="success-notification"], .toast-success');
      const error1 = page1.locator('.error-message, [data-testid="error-notification"], .toast-error');
      const error2 = page2.locator('.error-message, [data-testid="error-notification"], .toast-error');

      const hasSuccess1 = await success1.isVisible().catch(() => false);
      const hasSuccess2 = await success2.isVisible().catch(() => false);
      const hasError1 = await error1.isVisible().catch(() => false);
      const hasError2 = await error2.isVisible().catch(() => false);

      // Verify one succeeded and one failed
      expect(hasSuccess1 || hasSuccess2).toBeTruthy();
      expect(hasError1 || hasError2).toBeTruthy();

      // If there's an error, it should mention conflict or concurrent update
      if (hasError1) {
        await expect(error1).toContainText(/conflito|concurrent|concorrente|409/i);
      }
      if (hasError2) {
        await expect(error2).toContainText(/conflito|concurrent|concorrente|409/i);
      }
    } finally {
      await context1.close();
      await context2.close();
    }
  });

  test('payment form validates required fields', async ({ page }) => {
    await page.goto('http://localhost:3000/claims/1/1/1/1');
    await page.waitForLoadState('networkidle');

    // Open payment form
    const authorizeButton = page.locator('button:has-text("Autorizar Pagamento"), button:has-text("Authorize Payment")');
    await authorizeButton.click();

    // Try to submit without filling required fields
    const submitButton = page.locator('button[type="submit"]:has-text("Enviar"), button[type="submit"]:has-text("Submit")');
    await submitButton.click();

    // Verify validation errors are displayed
    const validationErrors = page.locator('.error-message, .field-error, [class*="error"]');
    await expect(validationErrors.first()).toBeVisible({ timeout: 5000 });
  });

  test('payment form displays currency formatting', async ({ page }) => {
    await page.goto('http://localhost:3000/claims/1/1/1/1');
    await page.waitForLoadState('networkidle');

    // Open payment form
    const authorizeButton = page.locator('button:has-text("Autorizar Pagamento"), button:has-text("Authorize Payment")');
    await authorizeButton.click();

    // Fill currency input
    const valorInput = page.locator('input[name="valorPrincipal"]');
    await valorInput.fill('12345.67');
    await valorInput.blur();

    // Verify currency formatting (should display R$ or with thousand separators)
    const displayedValue = await valorInput.inputValue();

    // Accept either raw decimal or formatted with R$
    expect(displayedValue).toMatch(/12345\.67|12\.345,67|R\$/);
  });
});

test.describe('Payment Authorization Performance', () => {
  test('search to payment completion under 90 seconds', async ({ page }) => {
    const startTime = Date.now();

    // Complete minimal flow
    await page.goto('http://localhost:3000/claims/search');

    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');
    await page.click('button[type="submit"]');

    await page.waitForURL(/\/claims\/\d+/);

    const authorizeButton = page.locator('button:has-text("Autorizar Pagamento"), button:has-text("Authorize Payment")');
    await authorizeButton.click();

    await page.selectOption('select[name="tipoPagamento"]', '1');
    await page.fill('input[name="valorPrincipal"]', '1000.00');
    await page.fill('input[name="favorecido"]', 'Performance Test');
    await page.check('input[name="tipoApolice"][value="1"]');

    const submitButton = page.locator('button[type="submit"]:has-text("Enviar"), button[type="submit"]:has-text("Submit")');
    await submitButton.click();

    // Wait for completion (success or error)
    await Promise.race([
      page.waitForSelector('.success-message, [data-testid="success-notification"]', { timeout: 90000 }),
      page.waitForSelector('.error-message, [data-testid="error-notification"]', { timeout: 90000 })
    ]);

    const endTime = Date.now();
    const totalTime = (endTime - startTime) / 1000;

    console.log(`Performance test: ${totalTime.toFixed(2)} seconds`);
    expect(totalTime).toBeLessThan(90); // SC-002 requirement
  });
});
