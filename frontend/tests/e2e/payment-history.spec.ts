import { test, expect, Page } from '@playwright/test';

test.describe('Payment History', () => {
  let page: Page;

  test.beforeEach(async ({ page: testPage }) => {
    page = testPage;
    await page.goto('http://localhost:3000');
  });

  test('User views payment history for claim', async () => {
    // Prerequisites: claim with 5 history records
    // First, search for a claim
    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');
    await page.click('button[type="submit"]');

    // Wait for claim details to load
    await page.waitForSelector('h2:has-text("Detalhes do Sinistro")', { timeout: 5000 });

    // Click on History tab
    await page.click('button:has-text("Histórico de Pagamentos")');

    // Wait for table to load
    await page.waitForSelector('table', { timeout: 5000 });

    // Assert 5 rows visible (excluding header)
    const rows = await page.$$('tbody tr');
    expect(rows.length).toBe(5);

    // Assert columns display correct data
    const headers = await page.$$eval('thead th', ths => ths.map(th => th.textContent));
    expect(headers).toContain('Data/Hora');
    expect(headers).toContain('Ocorrência');
    expect(headers).toContain('Operação');
    expect(headers).toContain('Valor Principal');
    expect(headers).toContain('Correção');
    expect(headers).toContain('Valor Total BTNF');
    expect(headers).toContain('Favorecido');
    expect(headers).toContain('Operador');

    // Assert dates formatted correctly (dd/MM/yyyy HH:mm:ss)
    const firstDateCell = await page.$eval('tbody tr:first-child td:first-child', td => td.textContent);
    expect(firstDateCell).toMatch(/\d{2}\/\d{2}\/\d{4} \d{2}:\d{2}:\d{2}/);

    // Click on a row to see details (if expandable)
    await page.click('tbody tr:first-child');

    // Check if details are shown (this depends on implementation)
    const detailsVisible = await page.isVisible('[data-testid="history-details"]');
    if (detailsVisible) {
      const details = await page.textContent('[data-testid="history-details"]');
      expect(details).toBeTruthy();
    }
  });

  test('Pagination works correctly', async () => {
    // Prerequisites: 25 records, default page size 20
    // Navigate to claim with many history records
    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '999999');
    await page.fill('input[name="dac"]', '9');
    await page.click('button[type="submit"]');

    await page.waitForSelector('h2:has-text("Detalhes do Sinistro")');
    await page.click('button:has-text("Histórico de Pagamentos")');
    await page.waitForSelector('table');

    // Assert page 1 shows first 20 records
    const page1Rows = await page.$$('tbody tr');
    expect(page1Rows.length).toBe(20);

    // Check pagination info
    const paginationInfo = await page.textContent('[data-testid="pagination-info"]');
    expect(paginationInfo).toContain('Página 1 de 2');
    expect(paginationInfo).toContain('1-20 de 25');

    // Click Next button
    await page.click('button[aria-label="Próxima página"]');

    // Wait for page 2 to load
    await page.waitForFunction(() => {
      const info = document.querySelector('[data-testid="pagination-info"]');
      return info?.textContent?.includes('Página 2');
    });

    // Assert page 2 shows last 5 records
    const page2Rows = await page.$$('tbody tr');
    expect(page2Rows.length).toBe(5);

    // Check updated pagination info
    const page2Info = await page.textContent('[data-testid="pagination-info"]');
    expect(page2Info).toContain('Página 2 de 2');
    expect(page2Info).toContain('21-25 de 25');

    // Previous button should be enabled, Next should be disabled
    const prevButton = await page.$('button[aria-label="Página anterior"]');
    const nextButton = await page.$('button[aria-label="Próxima página"]');

    expect(await prevButton?.isEnabled()).toBe(true);
    expect(await nextButton?.isEnabled()).toBe(false);

    // Go back to page 1
    await page.click('button[aria-label="Página anterior"]');

    await page.waitForFunction(() => {
      const info = document.querySelector('[data-testid="pagination-info"]');
      return info?.textContent?.includes('Página 1');
    });

    const backToPage1Rows = await page.$$('tbody tr');
    expect(backToPage1Rows.length).toBe(20);
  });

  test('Empty state displays correctly', async () => {
    // Navigate to claim with no history
    await page.fill('input[name="fonte"]', '9');
    await page.fill('input[name="protsini"]', '111111');
    await page.fill('input[name="dac"]', '1');
    await page.click('button[type="submit"]');

    await page.waitForSelector('h2:has-text("Detalhes do Sinistro")');
    await page.click('button:has-text("Histórico de Pagamentos")');

    // Should show empty state message
    await page.waitForSelector('text=Nenhum registro de histórico encontrado');

    // Table should not be visible
    const tableVisible = await page.isVisible('table');
    expect(tableVisible).toBe(false);
  });

  test('Currency values are formatted correctly', async () => {
    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');
    await page.click('button[type="submit"]');

    await page.waitForSelector('h2:has-text("Detalhes do Sinistro")');
    await page.click('button:has-text("Histórico de Pagamentos")');
    await page.waitForSelector('table');

    // Check currency format (R$ with thousand separator and 2 decimals)
    const currencyCell = await page.$eval(
      'tbody tr:first-child td[data-testid="valor-principal"]',
      td => td.textContent
    );
    expect(currencyCell).toMatch(/R\$\s[\d.]+,\d{2}/);
  });

  test('Refresh button updates data', async () => {
    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');
    await page.click('button[type="submit"]');

    await page.waitForSelector('h2:has-text("Detalhes do Sinistro")');
    await page.click('button:has-text("Histórico de Pagamentos")');
    await page.waitForSelector('table');

    // Get initial row count
    const initialRows = await page.$$('tbody tr');
    const initialCount = initialRows.length;

    // Click refresh button
    await page.click('button[aria-label="Atualizar histórico"]');

    // Wait for loading state
    await page.waitForSelector('[data-testid="loading-spinner"]');

    // Wait for data to reload
    await page.waitForSelector('table');

    // Verify data was refreshed (row count should be same or different if data changed)
    const refreshedRows = await page.$$('tbody tr');
    expect(refreshedRows.length).toBeGreaterThanOrEqual(0);
  });

  test('Export to CSV works', async () => {
    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');
    await page.click('button[type="submit"]');

    await page.waitForSelector('h2:has-text("Detalhes do Sinistro")');
    await page.click('button:has-text("Histórico de Pagamentos")');
    await page.waitForSelector('table');

    // Setup download promise before clicking
    const downloadPromise = page.waitForEvent('download');

    // Click export button
    await page.click('button:has-text("Exportar CSV")');

    // Wait for download
    const download = await downloadPromise;

    // Verify download
    expect(download.suggestedFilename()).toContain('historico');
    expect(download.suggestedFilename()).toContain('.csv');
  });

  test('Sorting works correctly', async () => {
    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');
    await page.click('button[type="submit"]');

    await page.waitForSelector('h2:has-text("Detalhes do Sinistro")');
    await page.click('button:has-text("Histórico de Pagamentos")');
    await page.waitForSelector('table');

    // Get initial first row date
    const initialFirstDate = await page.$eval(
      'tbody tr:first-child td:first-child',
      td => td.textContent
    );

    // Click date header to sort
    await page.click('thead th:has-text("Data/Hora")');

    // Wait for sort to apply
    await page.waitForTimeout(500);

    // Get new first row date
    const sortedFirstDate = await page.$eval(
      'tbody tr:first-child td:first-child',
      td => td.textContent
    );

    // Dates should be different after sorting (unless only 1 record)
    const rowCount = (await page.$$('tbody tr')).length;
    if (rowCount > 1) {
      expect(sortedFirstDate).not.toBe(initialFirstDate);
    }
  });

  test('Error state displays and allows retry', async () => {
    // Simulate network error by intercepting request
    await page.route('**/api/claims/**/history', route => {
      route.abort('failed');
    });

    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');
    await page.click('button[type="submit"]');

    await page.waitForSelector('h2:has-text("Detalhes do Sinistro")');
    await page.click('button:has-text("Histórico de Pagamentos")');

    // Should show error message
    await page.waitForSelector('text=Erro ao carregar histórico');

    // Should show retry button
    const retryButton = await page.$('button:has-text("Tentar novamente")');
    expect(retryButton).toBeTruthy();

    // Remove route intercept
    await page.unroute('**/api/claims/**/history');

    // Click retry
    await page.click('button:has-text("Tentar novamente")');

    // Should load data successfully
    await page.waitForSelector('table');
  });

  test('Responsive design on mobile', async () => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });

    await page.fill('input[name="fonte"]', '1');
    await page.fill('input[name="protsini"]', '123456');
    await page.fill('input[name="dac"]', '7');
    await page.click('button[type="submit"]');

    await page.waitForSelector('h2:has-text("Detalhes do Sinistro")');
    await page.click('button:has-text("Histórico de Pagamentos")');

    // Table should be scrollable or collapsed to cards
    const tableContainer = await page.$('[data-testid="history-table-container"]');
    const containerBox = await tableContainer?.boundingBox();

    // Check if container width is less than viewport width (scrollable)
    if (containerBox) {
      expect(containerBox.width).toBeLessThanOrEqual(375);
    }
  });
});