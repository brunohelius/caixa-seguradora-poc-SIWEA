import { test, expect } from '@playwright/test';

test.describe('Comprehensive Debug', () => {
  test('Deep investigation of page rendering', async ({ page }) => {
    const errors: string[] = [];
    const logs: string[] = [];

    // Capture ALL console messages
    page.on('console', msg => {
      const text = `[${msg.type()}] ${msg.text()}`;
      logs.push(text);
      console.log(text);
    });

    // Capture JavaScript errors
    page.on('pageerror', error => {
      const errorText = `PAGE ERROR: ${error.message}\n${error.stack}`;
      errors.push(errorText);
      console.log(errorText);
    });

    // Capture failed requests
    page.on('requestfailed', request => {
      const failText = `REQUEST FAILED: ${request.url()} - ${request.failure()?.errorText}`;
      errors.push(failText);
      console.log(failText);
    });

    // Navigate to the page
    console.log('Navigating to http://localhost:5173/claims/history...');
    await page.goto('http://localhost:5173/claims/history', {
      waitUntil: 'networkidle',
      timeout: 30000
    });

    // Wait a bit for React to mount
    await page.waitForTimeout(3000);

    // Check HTML structure
    const htmlContent = await page.content();
    console.log('=== HTML CONTENT (first 500 chars) ===');
    console.log(htmlContent.substring(0, 500));

    // Check if root div exists and has content
    const rootHTML = await page.evaluate(() => {
      const root = document.getElementById('root');
      return {
        exists: !!root,
        innerHTML: root?.innerHTML || 'ROOT NOT FOUND',
        hasChildren: (root?.children.length || 0) > 0,
        childrenCount: root?.children.length || 0
      };
    });
    console.log('=== ROOT DIV INFO ===');
    console.log(JSON.stringify(rootHTML, null, 2));

    // Check if React is loaded
    const reactInfo = await page.evaluate(() => {
      return {
        hasReact: typeof (window as any).React !== 'undefined',
        hasReactDOM: typeof (window as any).ReactDOM !== 'undefined',
        scripts: Array.from(document.scripts).map(s => s.src)
      };
    });
    console.log('=== REACT INFO ===');
    console.log(JSON.stringify(reactInfo, null, 2));

    // Check for specific elements
    const elementChecks = await page.evaluate(() => {
      return {
        divCount: document.querySelectorAll('div').length,
        h1Count: document.querySelectorAll('h1').length,
        bodyText: document.body.textContent || 'NO BODY TEXT'
      };
    });
    console.log('=== ELEMENT CHECKS ===');
    console.log(JSON.stringify(elementChecks, null, 2));

    // Take screenshot
    await page.screenshot({
      path: 'tests/e2e/screenshots/comprehensive-debug.png',
      fullPage: true
    });

    // Print all captured errors
    if (errors.length > 0) {
      console.log('=== ALL ERRORS ===');
      errors.forEach((error, index) => {
        console.log(`${index + 1}. ${error}`);
      });
    } else {
      console.log('=== NO ERRORS CAPTURED ===');
    }

    // Print summary
    console.log('=== SUMMARY ===');
    console.log(`Total errors: ${errors.length}`);
    console.log(`Total logs: ${logs.length}`);
    console.log(`Root exists: ${rootHTML.exists}`);
    console.log(`Root has children: ${rootHTML.hasChildren}`);
    console.log(`Root children count: ${rootHTML.childrenCount}`);

    // Fail if there are errors or if root is empty
    expect(errors.length, 'Should have no errors').toBe(0);
    expect(rootHTML.hasChildren, 'Root should have children').toBe(true);
  });
});
