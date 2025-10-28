# Caixa Seguradora Frontend Testing

This directory contains automated tests for the Caixa Seguradora Claims System frontend.

## Test Structure

```
tests/
├── e2e/                          # End-to-end tests with Playwright
│   └── visual-regression.spec.ts # Visual regression tests
└── README.md                     # This file
```

## Visual Regression Testing with Playwright

### Setup

Playwright is already installed. To install browsers for testing:

```bash
npx playwright install
```

### Running Tests

```bash
# Run all tests
npm run test:e2e

# Run tests in headed mode (see browser)
npx playwright test --headed

# Run tests in UI mode (interactive)
npx playwright test --ui

# Run specific test file
npx playwright test tests/e2e/visual-regression.spec.ts

# Run tests for specific browser
npx playwright test --project=chromium-desktop

# Run tests for mobile
npx playwright test --project=mobile-chrome
```

### Updating Baseline Screenshots

When you intentionally change the UI, update the baseline screenshots:

```bash
npx playwright test --update-snapshots
```

### Test Reports

After running tests, view the HTML report:

```bash
npx playwright show-report
```

## Test Coverage

### Visual Regression Tests

- ✅ Claims Search Page (desktop, tablet, mobile)
- ✅ Header/Navigation with logo
- ✅ Footer with copyright
- ✅ Search form (all 3 modes: protocol, claim number, leader code)
- ✅ Instructions card
- ✅ Form validation errors
- ✅ Loading states
- ✅ Error alerts
- ✅ Focus states (accessibility)
- ✅ Responsive layouts (375px, 768px, 1920px)

### Browser Coverage

- ✅ Chrome (Desktop)
- ✅ Firefox (Desktop)
- ✅ Safari (Desktop - WebKit)
- ✅ Chrome (Mobile - Pixel 5)
- ✅ Safari (Mobile - iPhone 12)
- ✅ Safari (Tablet - iPad Pro)

## Screenshot Organization

Baseline screenshots are stored in:
```
tests/e2e/visual-regression.spec.ts-snapshots/
├── chromium-desktop/
│   ├── search-page-desktop.png
│   ├── search-form-protocol.png
│   └── ...
├── mobile-chrome/
│   ├── search-page-mobile.png
│   └── ...
└── ...
```

## CI/CD Integration

The tests are configured to run in CI environments with:
- Automatic retries (2x)
- JSON reporter for metrics
- HTML reporter for debugging
- Screenshots on failure
- Video recording on failure

## Performance Benchmarks

Target performance metrics for visual tests:
- Page load: < 3 seconds
- Form interaction: < 500ms
- Navigation: < 1 second

## Troubleshooting

### Port conflicts

If the dev server fails to start on port 5175:
1. Check `playwright.config.ts` `webServer.url`
2. Update to match your actual dev server port
3. Or stop other processes using that port

### Screenshot differences

Small pixel differences can occur due to:
- Font rendering differences across OS
- Browser version updates
- Anti-aliasing variations

Use threshold settings to tolerate minor differences:
```typescript
await expect(page).toHaveScreenshot('name.png', {
  maxDiffPixels: 100,
  threshold: 0.2,
});
```

### Flaky tests

If tests are flaky:
1. Add explicit waits: `await page.waitForLoadState('networkidle')`
2. Wait for specific elements: `await page.getByText('...').waitFor()`
3. Increase timeout: `test.setTimeout(60000)`

## Future Test Coverage

- [ ] Claim Detail Page visual tests
- [ ] Migration Dashboard visual tests
- [ ] Payment authorization form tests
- [ ] Payment history component tests
- [ ] Phase management component tests
- [ ] Cross-browser compatibility matrix
- [ ] Performance regression tests
- [ ] Accessibility audit with axe-playwright

## Resources

- [Playwright Documentation](https://playwright.dev)
- [Visual Regression Testing Guide](https://playwright.dev/docs/test-snapshots)
- [Best Practices](https://playwright.dev/docs/best-practices)
