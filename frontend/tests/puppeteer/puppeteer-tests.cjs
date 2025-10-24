const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

// Test report
let testResults = [];
let passedTests = 0;
let failedTests = 0;

// Helper function to log test results
function logTest(testName, passed, details = '') {
  const status = passed ? '✅ PASS' : '❌ FAIL';
  testResults.push({ testName, status, details });
  if (passed) passedTests++;
  else failedTests++;
  console.log(`${status}: ${testName}${details ? ' - ' + details : ''}`);
}

// Helper function to check for CSS classes
async function checkForTailwindClasses(page) {
  return await page.evaluate(() => {
    const elements = document.querySelectorAll('*');
    let hasTailwind = false;
    elements.forEach(el => {
      const classes = el.className;
      if (typeof classes === 'string' &&
          (classes.includes('flex') ||
           classes.includes('bg-') ||
           classes.includes('text-') ||
           classes.includes('p-') ||
           classes.includes('m-'))) {
        hasTailwind = true;
      }
    });
    return hasTailwind;
  });
}

(async () => {
  const browser = await puppeteer.launch({
    headless: true,
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });

  try {
    console.log('🚀 Starting Puppeteer E2E Tests\n');

    // Create screenshots directory
    const screenshotsDir = path.join(__dirname, 'screenshots');
    if (!fs.existsSync(screenshotsDir)) {
      fs.mkdirSync(screenshotsDir, { recursive: true });
    }

    // Test 1: Application loads
    console.log('Testing: Application loads...');
    const page = await browser.newPage();
    await page.setViewport({ width: 1920, height: 1080 });

    let appLoaded = false;
    try {
      const response = await page.goto('http://localhost:5173/', {
        waitUntil: 'networkidle0',
        timeout: 30000
      });
      appLoaded = response.status() === 200;
      logTest('Application loads on port 5173', appLoaded, `Status: ${response.status()}`);
    } catch (error) {
      logTest('Application loads on port 5173', false, error.message);
    }

    // Test 2: Check for Tailwind CSS
    console.log('Testing: Tailwind CSS injection...');
    if (appLoaded) {
      const hasTailwind = await checkForTailwindClasses(page);
      logTest('Tailwind CSS classes present in DOM', hasTailwind);
    } else {
      logTest('Tailwind CSS classes present in DOM', false, 'App not loaded');
    }

    // Test 3: Check for console errors
    console.log('Testing: Console errors...');
    const consoleErrors = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });
    await page.reload();
    await page.waitForTimeout(2000);
    logTest('No console errors', consoleErrors.length === 0,
            consoleErrors.length > 0 ? `Found ${consoleErrors.length} errors` : '');

    // Test 4: Take screenshot of main page
    console.log('Testing: Main page screenshot...');
    try {
      await page.screenshot({
        path: path.join(screenshotsDir, 'main-page.png'),
        fullPage: true
      });
      logTest('Main page screenshot captured', true);
    } catch (error) {
      logTest('Main page screenshot captured', false, error.message);
    }

    // Test 5: Navigate to claims/search
    console.log('Testing: Navigation to /claims/search...');
    try {
      await page.goto('http://localhost:5173/claims/search', {
        waitUntil: 'networkidle0',
        timeout: 30000
      });
      const url = page.url();
      const navigationSuccess = url.includes('/claims/search');
      logTest('Navigate to /claims/search', navigationSuccess, `Current URL: ${url}`);

      if (navigationSuccess) {
        await page.screenshot({
          path: path.join(screenshotsDir, 'claims-search.png'),
          fullPage: true
        });
      }
    } catch (error) {
      logTest('Navigate to /claims/search', false, error.message);
    }

    // Test 6: Check for SearchForm component
    console.log('Testing: SearchForm component presence...');
    const hasSearchForm = await page.evaluate(() => {
      const forms = document.querySelectorAll('form');
      const inputs = document.querySelectorAll('input');
      const buttons = document.querySelectorAll('button');
      return forms.length > 0 && inputs.length > 0 && buttons.length > 0;
    });
    logTest('SearchForm component renders', hasSearchForm);

    // Test 7: Test form interaction
    console.log('Testing: Form interaction...');
    try {
      // Try to find and fill an input field
      const inputSelector = 'input[type="text"], input:not([type="hidden"])';
      const hasInput = await page.$(inputSelector);
      if (hasInput) {
        await page.type(inputSelector, '12345');
        const value = await page.$eval(inputSelector, el => el.value);
        logTest('Form input interaction', value === '12345', `Input value: ${value}`);
      } else {
        logTest('Form input interaction', false, 'No input fields found');
      }
    } catch (error) {
      logTest('Form input interaction', false, error.message);
    }

    // Test 8: Navigate to dashboard
    console.log('Testing: Navigation to /dashboard...');
    try {
      await page.goto('http://localhost:5173/dashboard', {
        waitUntil: 'networkidle0',
        timeout: 30000
      });
      const url = page.url();
      const dashboardSuccess = url.includes('/dashboard');
      logTest('Navigate to /dashboard', dashboardSuccess, `Current URL: ${url}`);

      if (dashboardSuccess) {
        await page.screenshot({
          path: path.join(screenshotsDir, 'dashboard.png'),
          fullPage: true
        });
      }
    } catch (error) {
      logTest('Navigate to /dashboard', false, error.message);
    }

    // Test 9: Check page structure
    console.log('Testing: Page structure...');
    const pageStructure = await page.evaluate(() => {
      return {
        hasHeader: !!document.querySelector('header, [role="banner"], nav'),
        hasMain: !!document.querySelector('main, [role="main"], .main-content'),
        hasFooter: !!document.querySelector('footer, [role="contentinfo"]'),
        hasContainer: !!document.querySelector('.container, .max-w-, [class*="container"]')
      };
    });
    logTest('Page has proper structure',
            pageStructure.hasMain || pageStructure.hasContainer,
            JSON.stringify(pageStructure));

    // Test 10: Check for shadcn components
    console.log('Testing: shadcn/ui components...');
    const hasShadcnComponents = await page.evaluate(() => {
      const shadcnClasses = [
        'rounded-md', 'border', 'bg-background', 'text-foreground',
        'hover:bg-', 'focus:outline-none', 'ring-offset',
        'data-[state=', 'transition-'
      ];
      let found = 0;
      shadcnClasses.forEach(className => {
        if (document.querySelector(`[class*="${className}"]`)) {
          found++;
        }
      });
      return found >= 3; // At least 3 shadcn patterns found
    });
    logTest('shadcn/ui components detected', hasShadcnComponents);

    // Generate report
    console.log('\n📊 Generating Puppeteer test report...');
    const report = `# Puppeteer E2E Test Report

**Test Date**: ${new Date().toISOString()}
**Test Environment**: http://localhost:5173/

## Test Summary
- **Total Tests**: ${passedTests + failedTests}
- **Passed**: ${passedTests}
- **Failed**: ${failedTests}
- **Success Rate**: ${((passedTests / (passedTests + failedTests)) * 100).toFixed(2)}%

## Test Results

| Test Name | Status | Details |
|-----------|--------|---------|
${testResults.map(r => `| ${r.testName} | ${r.status} | ${r.details} |`).join('\n')}

## Screenshots
- Main Page: \`tests/puppeteer/screenshots/main-page.png\`
- Claims Search: \`tests/puppeteer/screenshots/claims-search.png\`
- Dashboard: \`tests/puppeteer/screenshots/dashboard.png\`

## Console Errors
${consoleErrors.length > 0 ? consoleErrors.map(e => `- ${e}`).join('\n') : 'No console errors detected'}

## Recommendations
${failedTests > 0 ? `
- ${failedTests} tests failed and need investigation
- Review failed test details for specific issues
- Check screenshots for visual validation
` : '✅ All tests passed successfully!'}

---
*Generated by Puppeteer Test Suite*
`;

    fs.writeFileSync(path.join(__dirname, '..', '..', 'puppeteer-test-report.md'), report);
    console.log('✅ Report saved to puppeteer-test-report.md');

  } catch (error) {
    console.error('❌ Test suite error:', error);
  } finally {
    await browser.close();
    console.log(`\n📈 Test Results: ${passedTests} passed, ${failedTests} failed`);
    process.exit(failedTests > 0 ? 1 : 0);
  }
})();