const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

// Viewport configurations
const viewports = [
  { name: 'mobile', width: 375, height: 667, deviceScaleFactor: 2 },
  { name: 'tablet', width: 768, height: 1024, deviceScaleFactor: 2 },
  { name: 'desktop', width: 1920, height: 1080, deviceScaleFactor: 1 }
];

// Pages to test
const pages = [
  { name: 'home', url: 'http://localhost:5173/' },
  { name: 'claims-search', url: 'http://localhost:5173/claims/search' },
  { name: 'dashboard', url: 'http://localhost:5173/dashboard' }
];

// Test results
let testResults = [];
let issues = [];

(async () => {
  const browser = await puppeteer.launch({
    headless: true,
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });

  try {
    console.log('🎨 Starting Visual Regression Tests\n');

    // Create screenshots directory
    const screenshotsDir = path.join(__dirname, 'screenshots');
    if (!fs.existsSync(screenshotsDir)) {
      fs.mkdirSync(screenshotsDir, { recursive: true });
    }

    const page = await browser.newPage();

    for (const pageConfig of pages) {
      console.log(`\n📄 Testing page: ${pageConfig.name}`);

      for (const viewport of viewports) {
        console.log(`  📱 Testing ${viewport.name} (${viewport.width}x${viewport.height})`);

        // Set viewport
        await page.setViewport(viewport);

        try {
          // Navigate to page
          await page.goto(pageConfig.url, {
            waitUntil: 'networkidle0',
            timeout: 30000
          });

          // Wait for any animations to complete
          await page.waitForTimeout(1000);

          // Check for horizontal scroll
          const hasHorizontalScroll = await page.evaluate(() => {
            return document.documentElement.scrollWidth > document.documentElement.clientWidth;
          });

          if (hasHorizontalScroll) {
            issues.push({
              page: pageConfig.name,
              viewport: viewport.name,
              issue: 'Horizontal scroll detected',
              severity: 'high'
            });
          }

          // Check for overflow issues
          const overflowElements = await page.evaluate(() => {
            const elements = document.querySelectorAll('*');
            const overflowing = [];
            elements.forEach(el => {
              if (el.scrollWidth > el.clientWidth || el.scrollHeight > el.clientHeight) {
                const rect = el.getBoundingClientRect();
                if (rect.width > 0 && rect.height > 0) {
                  overflowing.push({
                    tag: el.tagName,
                    class: el.className,
                    overflow: true
                  });
                }
              }
            });
            return overflowing.length;
          });

          if (overflowElements > 0) {
            issues.push({
              page: pageConfig.name,
              viewport: viewport.name,
              issue: `${overflowElements} elements with overflow`,
              severity: 'medium'
            });
          }

          // Check for responsive utilities
          const responsiveClasses = await page.evaluate(() => {
            const classPatterns = ['sm:', 'md:', 'lg:', 'xl:', '2xl:'];
            let found = 0;
            document.querySelectorAll('[class]').forEach(el => {
              const classes = el.className;
              if (typeof classes === 'string') {
                classPatterns.forEach(pattern => {
                  if (classes.includes(pattern)) found++;
                });
              }
            });
            return found;
          });

          // Take screenshot
          const screenshotPath = path.join(
            screenshotsDir,
            `${pageConfig.name}-${viewport.name}.png`
          );

          await page.screenshot({
            path: screenshotPath,
            fullPage: true
          });

          // Record test result
          testResults.push({
            page: pageConfig.name,
            viewport: viewport.name,
            screenshot: screenshotPath,
            hasHorizontalScroll,
            overflowElements,
            responsiveClasses,
            status: hasHorizontalScroll || overflowElements > 5 ? 'FAIL' : 'PASS'
          });

          console.log(`    ✅ Screenshot captured`);
          console.log(`    📊 Responsive classes found: ${responsiveClasses}`);
          console.log(`    📐 Horizontal scroll: ${hasHorizontalScroll ? '❌ Yes' : '✅ No'}`);
          console.log(`    📦 Overflow elements: ${overflowElements}`);

        } catch (error) {
          console.error(`    ❌ Error: ${error.message}`);
          issues.push({
            page: pageConfig.name,
            viewport: viewport.name,
            issue: `Failed to test: ${error.message}`,
            severity: 'critical'
          });
        }
      }
    }

    // Generate report
    console.log('\n📊 Generating Visual Regression Report...');

    const report = `# Visual Regression Test Report

**Test Date**: ${new Date().toISOString()}
**Test Environment**: http://localhost:5173/

## Test Summary
- **Total Tests**: ${testResults.length}
- **Passed**: ${testResults.filter(r => r.status === 'PASS').length}
- **Failed**: ${testResults.filter(r => r.status === 'FAIL').length}
- **Issues Found**: ${issues.length}

## Viewport Test Results

| Page | Viewport | Status | Horizontal Scroll | Overflow Elements | Responsive Classes |
|------|----------|--------|------------------|-------------------|-------------------|
${testResults.map(r =>
  `| ${r.page} | ${r.viewport} | ${r.status} | ${r.hasHorizontalScroll ? '❌ Yes' : '✅ No'} | ${r.overflowElements} | ${r.responsiveClasses} |`
).join('\n')}

## Issues Detected

${issues.length > 0 ? issues.map(i =>
  `### ${i.page} - ${i.viewport}
- **Issue**: ${i.issue}
- **Severity**: ${i.severity}
`).join('\n') : '✅ No issues detected'}

## Screenshot Gallery

### Mobile (375px)
- Home: \`tests/visual-regression/screenshots/home-mobile.png\`
- Claims Search: \`tests/visual-regression/screenshots/claims-search-mobile.png\`
- Dashboard: \`tests/visual-regression/screenshots/dashboard-mobile.png\`

### Tablet (768px)
- Home: \`tests/visual-regression/screenshots/home-tablet.png\`
- Claims Search: \`tests/visual-regression/screenshots/claims-search-tablet.png\`
- Dashboard: \`tests/visual-regression/screenshots/dashboard-tablet.png\`

### Desktop (1920px)
- Home: \`tests/visual-regression/screenshots/home-desktop.png\`
- Claims Search: \`tests/visual-regression/screenshots/claims-search-desktop.png\`
- Dashboard: \`tests/visual-regression/screenshots/dashboard-desktop.png\`

## Responsive Design Analysis

- **Mobile Optimization**: ${testResults.filter(r => r.viewport === 'mobile' && r.status === 'PASS').length}/${pages.length} pages pass
- **Tablet Optimization**: ${testResults.filter(r => r.viewport === 'tablet' && r.status === 'PASS').length}/${pages.length} pages pass
- **Desktop Optimization**: ${testResults.filter(r => r.viewport === 'desktop' && r.status === 'PASS').length}/${pages.length} pages pass

## Recommendations

${issues.filter(i => i.severity === 'critical').length > 0 ?
`### Critical Issues
${issues.filter(i => i.severity === 'critical').map(i => `- Fix ${i.issue} on ${i.page} (${i.viewport})`).join('\n')}
` : ''}

${issues.filter(i => i.severity === 'high').length > 0 ?
`### High Priority Issues
${issues.filter(i => i.severity === 'high').map(i => `- Fix ${i.issue} on ${i.page} (${i.viewport})`).join('\n')}
` : ''}

${issues.filter(i => i.severity === 'medium').length > 0 ?
`### Medium Priority Issues
${issues.filter(i => i.severity === 'medium').map(i => `- Review ${i.issue} on ${i.page} (${i.viewport})`).join('\n')}
` : ''}

${issues.length === 0 ? '✅ All visual regression tests passed successfully!' : ''}

---
*Generated by Visual Regression Test Suite*
`;

    fs.writeFileSync(path.join(__dirname, '..', '..', 'visual-regression-report.md'), report);
    console.log('✅ Report saved to visual-regression-report.md');

  } catch (error) {
    console.error('❌ Test suite error:', error);
  } finally {
    await browser.close();
    console.log(`\n📈 Visual Regression Tests Complete`);
    console.log(`   Issues found: ${issues.length}`);
    process.exit(issues.filter(i => i.severity === 'critical').length > 0 ? 1 : 0);
  }
})();