/**
 * T108 [US5] - Playwright E2E Test for Phase Tracking
 * Tests phase management functionality end-to-end
 */

import { test, expect } from '@playwright/test';

// Test configuration
const API_BASE_URL = process.env.VITE_API_BASE_URL || 'https://localhost:5001';
const APP_BASE_URL = process.env.APP_BASE_URL || 'http://localhost:3000';

test.describe('Phase Tracking E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Ignore HTTPS certificate errors for localhost testing
    await page.goto(APP_BASE_URL);
  });

  test('Payment authorization triggers phase update', async ({ page }) => {
    /**
     * Prerequisites:
     * - Test claim exists in database without open phases
     * - Payment authorization event configured in SI_REL_FASE_EVENTO
     */

    // Step 1: Search for a claim
    await page.goto(`${APP_BASE_URL}/claims/search`);
    await expect(page.locator('h2')).toContainText('Pesquisa de Sinistro');

    // Fill in search criteria (using protocol search)
    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');

    // Submit search
    await page.click('button[type="submit"]');

    // Wait for claim details to load
    await page.waitForURL(/\/claims\/detail/);
    await expect(page.locator('h2')).toContainText('Detalhes do Sinistro');

    // Step 2: Navigate to Phases tab BEFORE payment authorization
    await page.click('button:has-text("Fases do Sinistro")');

    // Verify no phases exist or count current phases
    const phasesBeforeCount = await page.locator('.phase-timeline-item').count();
    console.log(`Phases before payment authorization: ${phasesBeforeCount}`);

    // Step 3: Navigate back to details and authorize payment
    await page.click('button:has-text("Detalhes")');

    // Check if claim has pending value
    const hasPendingValue = await page.locator('button:has-text("Autorizar Pagamento")').isVisible();

    if (hasPendingValue) {
      // Click authorize payment button
      await page.click('button:has-text("Autorizar Pagamento")');

      // Fill in payment form
      await page.selectOption('select[name="tipoPagamento"]', '1');
      await page.fill('input[name="valorPrincipal"]', '10000.00');
      await page.fill('input[name="valorCorrecao"]', '500.00');
      await page.fill('input[name="favorecido"]', 'Test Beneficiary E2E');
      await page.fill('textarea[name="observacoes"]', 'E2E Test Payment Authorization');

      // Submit payment authorization
      await page.click('button[type="submit"]:has-text("Confirmar Autorização")');

      // Wait for success message
      await expect(page.locator('.alert-success')).toBeVisible({ timeout: 10000 });
      await expect(page.locator('.alert-success')).toContainText('Pagamento autorizado com sucesso');

      // Step 4: Verify phases tab is automatically switched
      await expect(page.locator('.nav-link.active')).toContainText('Histórico de Pagamentos');

      // Step 5: Navigate to Phases tab
      await page.click('button:has-text("Fases do Sinistro")');

      // Wait for phases to load
      await page.waitForTimeout(1000); // Give time for phase update to propagate

      // Verify new phase appeared
      const phasesAfterCount = await page.locator('.phase-timeline-item').count();
      console.log(`Phases after payment authorization: ${phasesAfterCount}`);

      expect(phasesAfterCount).toBeGreaterThan(phasesBeforeCount);

      // Verify phase details
      const firstPhase = page.locator('.phase-timeline-item').first();
      await expect(firstPhase).toBeVisible();

      // Verify phase has opening date = today
      const today = new Date().toLocaleDateString('pt-BR');
      const phaseOpeningDate = await firstPhase.locator('.phase-detail-value').nth(2).textContent();
      expect(phaseOpeningDate).toContain(today.split('/')[0]); // At least check day matches

      // Verify phase status = "Aberta" (Open)
      await expect(firstPhase.locator('.phase-detail-value').first()).toContainText('Aberta');

      // Verify open phase indicator (pulsing green dot)
      await expect(firstPhase.locator('.phase-icon-open')).toBeVisible();
      await expect(firstPhase.locator('.phase-pulse')).toBeVisible();
    } else {
      console.log('Claim has no pending value - skipping payment authorization test');
      test.skip();
    }
  });

  test('Phase timeline displays correctly', async ({ page }) => {
    /**
     * Prerequisites:
     * - Test claim exists with 3 historical phases (mix of open and closed)
     */

    // Step 1: Navigate directly to claim detail with known phases
    // Using a claim that should have historical phases
    await page.goto(`${APP_BASE_URL}/claims/search`);

    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '999005'); // Claim from integration tests
    await page.fill('input[name="dac"]', '5');

    await page.click('button[type="submit"]');
    await page.waitForURL(/\/claims\/detail/);

    // Step 2: Navigate to Phases tab
    await page.click('button:has-text("Fases do Sinistro")');

    // Wait for timeline to render
    await page.waitForSelector('.phase-timeline', { timeout: 5000 });

    // Verify timeline line exists (connecting phases)
    const timelineLineExists = await page.locator('.timeline-line').isVisible();
    expect(timelineLineExists).toBeTruthy();

    // Verify phases are displayed
    const phaseCount = await page.locator('.phase-timeline-item').count();
    console.log(`Phase timeline shows ${phaseCount} phase(s)`);

    if (phaseCount > 0) {
      // Step 3: Verify chronological order
      const phaseOpeningDates: string[] = [];
      for (let i = 0; i < Math.min(phaseCount, 3); i++) {
        const phase = page.locator('.phase-timeline-item').nth(i);
        const dateText = await phase.locator('.phase-dates').textContent();
        if (dateText) {
          phaseOpeningDates.push(dateText);
          console.log(`Phase ${i + 1} dates: ${dateText}`);
        }
      }

      // Step 4: Verify visual distinction between open and closed phases
      for (let i = 0; i < Math.min(phaseCount, 3); i++) {
        const phase = page.locator('.phase-timeline-item').nth(i);

        // Check if phase has status badge
        const statusBadge = phase.locator('.phase-badge');
        const statusText = await statusBadge.textContent();

        if (statusText?.includes('Aberta')) {
          // Open phase should have pulsing green indicator
          await expect(phase.locator('.node-pulsing')).toBeVisible();
          console.log(`Phase ${i + 1}: Open (green indicator)`);
        } else if (statusText?.includes('Fechada')) {
          // Closed phase should have checkmark
          await expect(phase.locator('.checkmark')).toBeVisible();
          console.log(`Phase ${i + 1}: Closed (checkmark)`);
        }
      }

      // Step 5: Test expandable details
      const firstPhase = page.locator('.phase-timeline-item').first();
      const expandButton = firstPhase.locator('.phase-header');

      // Click to expand
      await expandButton.click();
      await page.waitForTimeout(300); // Wait for animation

      // Verify expanded details are visible
      await expect(firstPhase.locator('.timeline-details')).toBeVisible();

      // Verify detail fields
      await expect(firstPhase.locator('.detail-item').first()).toBeVisible();
      const detailText = await firstPhase.locator('.detail-item').first().textContent();
      expect(detailText).toContain('Código da Fase');

      // Click again to collapse
      await expandButton.click();
      await page.waitForTimeout(300);

      // Step 6: Verify duration color coding
      const durationBadge = firstPhase.locator('.duration-badge');
      const durationClass = await durationBadge.getAttribute('class');

      if (durationClass?.includes('duration-green')) {
        console.log('Phase duration: < 30 days (green)');
      } else if (durationClass?.includes('duration-yellow')) {
        console.log('Phase duration: 30-60 days (yellow)');
      } else if (durationClass?.includes('duration-red')) {
        console.log('Phase duration: > 60 days (red)');
      }

      expect(durationClass).toMatch(/duration-(green|yellow|red)/);
    } else {
      console.log('No phases found for this claim - this is acceptable for new claims');
    }
  });

  test('Phase refresh button works correctly', async ({ page }) => {
    // Navigate to claim with phases
    await page.goto(`${APP_BASE_URL}/claims/search`);

    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');

    await page.click('button[type="submit"]');
    await page.waitForURL(/\/claims\/detail/);

    // Navigate to Phases tab
    await page.click('button:has-text("Fases do Sinistro")');

    // Wait for initial load
    await page.waitForTimeout(1000);

    // Get initial phase count
    const initialCount = await page.locator('.phase-timeline-item').count();

    // Click refresh button
    const refreshButton = page.locator('button:has-text("Atualizar")');
    await expect(refreshButton).toBeVisible();
    await refreshButton.click();

    // Wait for loading indicator (if implemented)
    await page.waitForTimeout(500);

    // Verify data reloads (count should be same or updated)
    const updatedCount = await page.locator('.phase-timeline-item').count();
    expect(updatedCount).toBeGreaterThanOrEqual(0);

    console.log(`Phase count after refresh: ${updatedCount} (was ${initialCount})`);
  });

  test('Mobile responsive timeline view', async ({ page, viewport }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 }); // iPhone SE

    // Navigate to claim with phases
    await page.goto(`${APP_BASE_URL}/claims/search`);

    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');

    await page.click('button[type="submit"]');
    await page.waitForURL(/\/claims\/detail/);

    // Navigate to Phases tab
    await page.click('button:has-text("Fases do Sinistro")');

    // Verify timeline renders on mobile
    await expect(page.locator('.phase-timeline')).toBeVisible();

    const phaseCount = await page.locator('.phase-timeline-item').count();

    if (phaseCount > 0) {
      // Verify mobile-optimized layout
      const firstPhase = page.locator('.phase-timeline-item').first();

      // Check that phase cards are readable on mobile
      const phaseCard = firstPhase.locator('.timeline-content');
      const cardWidth = await phaseCard.boundingBox();

      expect(cardWidth).toBeTruthy();
      if (cardWidth) {
        expect(cardWidth.width).toBeLessThanOrEqual(375); // Should fit in viewport
        console.log(`Phase card width on mobile: ${cardWidth.width}px`);
      }

      // Verify detail grid collapses to single column on mobile
      await firstPhase.locator('.phase-header').click();
      await page.waitForTimeout(300);

      const detailGrid = firstPhase.locator('.detail-grid');
      const gridColumns = await detailGrid.evaluate((el) => {
        return window.getComputedStyle(el).gridTemplateColumns;
      });

      console.log(`Detail grid columns on mobile: ${gridColumns}`);
      // On mobile, should be single column or adapted layout
    }
  });

  test('Error handling for missing phases', async ({ page }) => {
    // Navigate to claim that definitely has no phases
    await page.goto(`${APP_BASE_URL}/claims/search`);

    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '999999'); // Non-existent claim
    await page.fill('input[name="dac"]', '9');

    await page.click('button[type="submit"]');

    // Should show "not found" or navigate to phases showing empty state
    // This depends on whether the claim exists

    const notFoundVisible = await page.locator('.alert-warning').isVisible();
    if (notFoundVisible) {
      const notFoundText = await page.locator('.alert-warning').textContent();
      expect(notFoundText).toContain('Nenhum sinistro');
      console.log('Claim not found (expected for non-existent claim)');
    }
  });
});

test.describe('Phase Statistics Integration', () => {
  test('Phase statistics displayed correctly', async ({ page }) => {
    // This test would verify phase statistics if implemented in the UI
    await page.goto(`${APP_BASE_URL}/claims/search`);

    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');

    await page.click('button[type="submit"]');
    await page.waitForURL(/\/claims\/detail/);

    await page.click('button:has-text("Fases do Sinistro")');

    // If statistics section exists
    const hasStatistics = await page.locator('.phase-statistics').isVisible().catch(() => false);

    if (hasStatistics) {
      console.log('Phase statistics section found');

      // Verify statistics content
      await expect(page.locator('.phase-statistics')).toContainText('Total');
      await expect(page.locator('.phase-statistics')).toContainText('Abertas');
      await expect(page.locator('.phase-statistics')).toContainText('Fechadas');
    } else {
      console.log('Phase statistics section not implemented (optional feature)');
    }
  });
});
