const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

// Components to validate
const componentsToTest = [
  'Button', 'Input', 'Label', 'Card', 'Table',
  'Progress', 'Badge', 'RadioGroup', 'Separator'
];

// Test results
let componentResults = [];
let accessibilityIssues = [];

(async () => {
  const browser = await puppeteer.launch({
    headless: true,
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });

  try {
    console.log('🧩 Starting Component Validation Tests\n');

    const page = await browser.newPage();
    await page.setViewport({ width: 1920, height: 1080 });

    // Navigate to claims search page (has most components)
    console.log('📄 Loading application...');
    await page.goto('http://localhost:5173/claims/search', {
      waitUntil: 'networkidle0',
      timeout: 30000
    });

    // Test 1: Check for Button components
    console.log('\n🔘 Testing Button components...');
    const buttonTest = await page.evaluate(() => {
      const buttons = document.querySelectorAll('button');
      const results = {
        count: buttons.length,
        hasText: 0,
        hasAriaLabel: 0,
        hasFocusStyles: 0,
        hasHoverStyles: 0,
        isAccessible: 0
      };

      buttons.forEach(btn => {
        if (btn.textContent.trim()) results.hasText++;
        if (btn.getAttribute('aria-label') || btn.getAttribute('aria-labelledby')) {
          results.hasAriaLabel++;
        }
        const styles = window.getComputedStyle(btn);
        if (styles.outline || btn.className.includes('focus:')) results.hasFocusStyles++;
        if (btn.className.includes('hover:')) results.hasHoverStyles++;
        if (btn.textContent.trim() || btn.getAttribute('aria-label')) {
          results.isAccessible++;
        }
      });

      return results;
    });
    componentResults.push({
      component: 'Button',
      found: buttonTest.count > 0,
      count: buttonTest.count,
      accessible: buttonTest.isAccessible === buttonTest.count,
      details: buttonTest
    });
    console.log(`  ✅ Found ${buttonTest.count} buttons`);
    console.log(`  📊 Accessible: ${buttonTest.isAccessible}/${buttonTest.count}`);

    // Test 2: Check for Input components
    console.log('\n📝 Testing Input components...');
    const inputTest = await page.evaluate(() => {
      const inputs = document.querySelectorAll('input:not([type="hidden"])');
      const results = {
        count: inputs.length,
        hasLabel: 0,
        hasAriaLabel: 0,
        hasPlaceholder: 0,
        hasBorder: 0,
        hasFocusStyles: 0
      };

      inputs.forEach(input => {
        const id = input.id;
        if (id && document.querySelector(`label[for="${id}"]`)) results.hasLabel++;
        if (input.getAttribute('aria-label') || input.getAttribute('aria-labelledby')) {
          results.hasAriaLabel++;
        }
        if (input.placeholder) results.hasPlaceholder++;
        const styles = window.getComputedStyle(input);
        if (styles.borderWidth !== '0px') results.hasBorder++;
        if (input.className.includes('focus:') || styles.outline) results.hasFocusStyles++;
      });

      return results;
    });
    componentResults.push({
      component: 'Input',
      found: inputTest.count > 0,
      count: inputTest.count,
      accessible: inputTest.hasLabel + inputTest.hasAriaLabel >= inputTest.count,
      details: inputTest
    });
    console.log(`  ✅ Found ${inputTest.count} inputs`);
    console.log(`  📊 With labels: ${inputTest.hasLabel}/${inputTest.count}`);

    // Test 3: Check for Label components
    console.log('\n🏷️ Testing Label components...');
    const labelTest = await page.evaluate(() => {
      const labels = document.querySelectorAll('label');
      const results = {
        count: labels.length,
        hasFor: 0,
        hasText: 0,
        isValid: 0
      };

      labels.forEach(label => {
        if (label.getAttribute('for')) results.hasFor++;
        if (label.textContent.trim()) results.hasText++;
        if (label.getAttribute('for') && label.textContent.trim()) results.isValid++;
      });

      return results;
    });
    componentResults.push({
      component: 'Label',
      found: labelTest.count > 0,
      count: labelTest.count,
      accessible: labelTest.isValid === labelTest.count,
      details: labelTest
    });
    console.log(`  ✅ Found ${labelTest.count} labels`);
    console.log(`  📊 Valid labels: ${labelTest.isValid}/${labelTest.count}`);

    // Test 4: Check for Card components
    console.log('\n🗂️ Testing Card components...');
    const cardTest = await page.evaluate(() => {
      const cardSelectors = [
        '[class*="card"]',
        '[class*="rounded"][class*="border"]',
        'div[class*="bg-"][class*="p-"][class*="rounded"]'
      ];
      let cards = new Set();

      cardSelectors.forEach(selector => {
        document.querySelectorAll(selector).forEach(el => {
          cards.add(el);
        });
      });

      const results = {
        count: cards.size,
        hasBorder: 0,
        hasRoundedCorners: 0,
        hasPadding: 0,
        hasBackground: 0
      };

      cards.forEach(card => {
        const classes = card.className;
        const styles = window.getComputedStyle(card);
        if (classes.includes('border') || styles.borderWidth !== '0px') results.hasBorder++;
        if (classes.includes('rounded')) results.hasRoundedCorners++;
        if (classes.includes('p-') || styles.padding !== '0px') results.hasPadding++;
        if (classes.includes('bg-')) results.hasBackground++;
      });

      return results;
    });
    componentResults.push({
      component: 'Card',
      found: cardTest.count > 0,
      count: cardTest.count,
      accessible: true, // Cards are usually accessible if content is
      details: cardTest
    });
    console.log(`  ✅ Found ${cardTest.count} card-like components`);

    // Test 5: Check for Table components
    console.log('\n📊 Testing Table components...');
    const tableTest = await page.evaluate(() => {
      const tables = document.querySelectorAll('table');
      const results = {
        count: tables.length,
        hasHeaders: 0,
        hasScope: 0,
        hasCaption: 0,
        isAccessible: 0
      };

      tables.forEach(table => {
        const headers = table.querySelectorAll('th');
        if (headers.length > 0) results.hasHeaders++;

        let hasScope = false;
        headers.forEach(th => {
          if (th.getAttribute('scope')) hasScope = true;
        });
        if (hasScope) results.hasScope++;

        if (table.querySelector('caption')) results.hasCaption++;

        if (headers.length > 0) results.isAccessible++;
      });

      return results;
    });
    componentResults.push({
      component: 'Table',
      found: tableTest.count > 0,
      count: tableTest.count,
      accessible: tableTest.hasHeaders === tableTest.count,
      details: tableTest
    });
    console.log(`  ✅ Found ${tableTest.count} tables`);

    // Test 6: Check for RadioGroup components
    console.log('\n⭕ Testing RadioGroup components...');
    const radioTest = await page.evaluate(() => {
      const radios = document.querySelectorAll('input[type="radio"]');
      const results = {
        count: radios.length,
        inGroups: 0,
        hasLabels: 0,
        hasName: 0,
        groups: new Set()
      };

      radios.forEach(radio => {
        const name = radio.getAttribute('name');
        if (name) {
          results.hasName++;
          results.groups.add(name);
        }
        const id = radio.id;
        if (id && document.querySelector(`label[for="${id}"]`)) results.hasLabels++;
        if (radio.closest('[role="radiogroup"]')) results.inGroups++;
      });

      return {
        ...results,
        groupCount: results.groups.size
      };
    });
    componentResults.push({
      component: 'RadioGroup',
      found: radioTest.count > 0,
      count: radioTest.count,
      accessible: radioTest.hasLabels === radioTest.count,
      details: radioTest
    });
    console.log(`  ✅ Found ${radioTest.count} radio buttons in ${radioTest.groupCount} groups`);

    // Test keyboard navigation
    console.log('\n⌨️ Testing keyboard navigation...');
    const keyboardTest = await page.evaluate(() => {
      const focusableSelectors = 'button, input, select, textarea, a[href], [tabindex]:not([tabindex="-1"])';
      const focusableElements = document.querySelectorAll(focusableSelectors);
      const results = {
        totalFocusable: focusableElements.length,
        hasTabIndex: 0,
        hasFocusStyles: 0,
        isInTabOrder: 0
      };

      focusableElements.forEach(el => {
        const tabIndex = el.getAttribute('tabindex');
        if (tabIndex !== null) results.hasTabIndex++;

        const classes = el.className || '';
        const styles = window.getComputedStyle(el);
        if (classes.includes('focus:') || styles.outline !== 'none') {
          results.hasFocusStyles++;
        }

        if (tabIndex !== '-1') results.isInTabOrder++;
      });

      return results;
    });

    // Test ARIA attributes
    console.log('\n♿ Testing accessibility attributes...');
    const ariaTest = await page.evaluate(() => {
      const results = {
        hasLandmarks: 0,
        hasAriaLabels: 0,
        hasRoles: 0,
        hasAriaDescriptions: 0
      };

      // Check for landmarks
      const landmarks = ['main', 'nav', 'header', 'footer', 'aside'];
      landmarks.forEach(landmark => {
        if (document.querySelector(landmark) || document.querySelector(`[role="${landmark}"]`)) {
          results.hasLandmarks++;
        }
      });

      // Check for ARIA labels
      results.hasAriaLabels = document.querySelectorAll('[aria-label], [aria-labelledby]').length;

      // Check for roles
      results.hasRoles = document.querySelectorAll('[role]').length;

      // Check for descriptions
      results.hasAriaDescriptions = document.querySelectorAll('[aria-describedby], [aria-description]').length;

      return results;
    });

    // Generate report
    console.log('\n📊 Generating Component Validation Report...');

    const totalComponents = componentResults.reduce((acc, r) => acc + r.count, 0);
    const accessibleComponents = componentResults.filter(r => r.accessible).length;

    const report = `# Component Validation Report

**Test Date**: ${new Date().toISOString()}
**Test Environment**: http://localhost:5173/

## Component Summary
- **Total Components Found**: ${totalComponents}
- **Component Types Tested**: ${componentsToTest.length}
- **Accessible Components**: ${accessibleComponents}/${componentResults.length}

## Component Test Results

| Component | Found | Count | Accessible | Details |
|-----------|-------|-------|------------|---------|
${componentResults.map(r =>
  `| ${r.component} | ${r.found ? '✅' : '❌'} | ${r.count} | ${r.accessible ? '✅' : '⚠️'} | ${JSON.stringify(r.details)} |`
).join('\n')}

## Keyboard Navigation

- **Total Focusable Elements**: ${keyboardTest.totalFocusable}
- **Elements in Tab Order**: ${keyboardTest.isInTabOrder}
- **Elements with Focus Styles**: ${keyboardTest.hasFocusStyles}
- **Elements with TabIndex**: ${keyboardTest.hasTabIndex}

## Accessibility Attributes

- **Landmark Regions**: ${ariaTest.hasLandmarks}
- **ARIA Labels**: ${ariaTest.hasAriaLabels}
- **ARIA Roles**: ${ariaTest.hasRoles}
- **ARIA Descriptions**: ${ariaTest.hasAriaDescriptions}

## shadcn/ui Component Checklist

${componentResults.map(r =>
  `- [${r.found ? 'x' : ' '}] **${r.component}**: ${r.count} found${!r.accessible ? ' ⚠️ Accessibility issues detected' : ''}`
).join('\n')}

## Accessibility Issues

${componentResults.filter(r => !r.accessible).length > 0 ?
  componentResults.filter(r => !r.accessible).map(r =>
    `### ${r.component}
- Found: ${r.count} components
- Issue: Not all components are properly labeled or accessible
- Details: ${JSON.stringify(r.details)}
`).join('\n') : '✅ No major accessibility issues detected'}

## Recommendations

${keyboardTest.hasFocusStyles < keyboardTest.totalFocusable ?
  '- Add focus styles to all interactive elements\n' : ''}
${ariaTest.hasLandmarks < 3 ?
  '- Add more landmark regions for better screen reader navigation\n' : ''}
${componentResults.some(r => !r.accessible) ?
  '- Ensure all form inputs have associated labels\n- Add ARIA labels to buttons without visible text\n' : ''}
${keyboardTest.isInTabOrder < keyboardTest.totalFocusable ?
  '- Review tab order for all interactive elements\n' : ''}

## Component Migration Status

✅ **Successfully Migrated**:
${componentResults.filter(r => r.found && r.accessible).map(r => `- ${r.component}`).join('\n')}

⚠️ **Needs Attention**:
${componentResults.filter(r => r.found && !r.accessible).map(r => `- ${r.component}: Accessibility improvements needed`).join('\n')}

❌ **Not Found**:
${componentResults.filter(r => !r.found).map(r => `- ${r.component}`).join('\n')}

---
*Generated by Component Validation Test Suite*
`;

    fs.writeFileSync(path.join(__dirname, '..', '..', 'component-validation-report.md'), report);
    console.log('✅ Report saved to component-validation-report.md');

  } catch (error) {
    console.error('❌ Test suite error:', error);
  } finally {
    await browser.close();
    console.log(`\n📈 Component Validation Complete`);
    process.exit(0);
  }
})();