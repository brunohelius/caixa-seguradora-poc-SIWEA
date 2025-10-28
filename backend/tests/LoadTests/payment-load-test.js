import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

/**
 * T075: Load test for payment authorization endpoint
 * Tests system performance under concurrent load
 *
 * Requirements:
 * - 100 concurrent users
 * - Ramp up over 10 seconds
 * - Sustain for 60 seconds
 * - p50 < 500ms, p95 < 2000ms, p99 < 5000ms
 * - Error rate < 1%
 * - Throughput > 50 req/s
 */

// Custom metrics
const errorRate = new Rate('errors');
const paymentDuration = new Trend('payment_duration');

// Test configuration
export const options = {
  stages: [
    { duration: '10s', target: 100 }, // Ramp up to 100 users over 10 seconds
    { duration: '60s', target: 100 }, // Stay at 100 users for 60 seconds
    { duration: '10s', target: 0 },   // Ramp down to 0 users
  ],
  thresholds: {
    'http_req_duration': ['p(50)<500', 'p(95)<2000', 'p(99)<5000'],
    'errors': ['rate<0.01'], // Less than 1% error rate
    'http_reqs': ['rate>50'], // More than 50 requests per second
    'payment_duration': ['avg<1000', 'p(95)<2000'],
  },
};

// Base URL - can be configured via environment variable
const BASE_URL = __ENV.API_BASE_URL || 'https://localhost:5001';

// Test data pool - simulate different claims
const testClaims = [
  { tipseg: 1, orgsin: 1, rmosin: 1, numsin: 1 },
  { tipseg: 1, orgsin: 1, rmosin: 1, numsin: 2 },
  { tipseg: 1, orgsin: 1, rmosin: 1, numsin: 3 },
  { tipseg: 1, orgsin: 1, rmosin: 1, numsin: 4 },
  { tipseg: 1, orgsin: 1, rmosin: 1, numsin: 5 },
  { tipseg: 1, orgsin: 2, rmosin: 1, numsin: 1 },
  { tipseg: 1, orgsin: 2, rmosin: 1, numsin: 2 },
  { tipseg: 1, orgsin: 2, rmosin: 1, numsin: 3 },
  { tipseg: 1, orgsin: 2, rmosin: 1, numsin: 4 },
  { tipseg: 1, orgsin: 2, rmosin: 1, numsin: 5 },
];

export default function () {
  // Select random claim from pool
  const claim = testClaims[Math.floor(Math.random() * testClaims.length)];

  // Generate random payment data
  const paymentRequest = {
    tipoPagamento: Math.floor(Math.random() * 5) + 1, // 1-5
    valorPrincipal: parseFloat((Math.random() * 10000 + 1000).toFixed(2)),
    valorCorrecao: parseFloat((Math.random() * 500).toFixed(2)),
    favorecido: `Load Test User ${__VU}`,
    tipoApolice: Math.random() > 0.5 ? '1' : '2',
    observacoes: `Load test iteration ${__ITER} by VU ${__VU}`,
  };

  const url = `${BASE_URL}/api/claims/${claim.tipseg}/${claim.orgsin}/${claim.rmosin}/${claim.numsin}/authorize-payment`;

  const params = {
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      // Add authentication header if required
      // 'Authorization': 'Bearer YOUR_TOKEN_HERE',
    },
    timeout: '30s',
  };

  // Measure payment authorization duration
  const startTime = new Date().getTime();

  const response = http.post(url, JSON.stringify(paymentRequest), params);

  const duration = new Date().getTime() - startTime;
  paymentDuration.add(duration);

  // Validate response
  const success = check(response, {
    'status is 201': (r) => r.status === 201,
    'status is 2xx or 4xx': (r) => r.status >= 200 && r.status < 500,
    'response has body': (r) => r.body.length > 0,
    'response time < 5000ms': (r) => r.timings.duration < 5000,
  });

  // Check for specific success response
  if (response.status === 201) {
    const body = JSON.parse(response.body);
    check(body, {
      'has sucesso field': (b) => 'sucesso' in b,
      'sucesso is true': (b) => b.sucesso === true,
      'has ocorhist': (b) => 'ocorhist' in b && b.ocorhist > 0,
      'has operacao': (b) => b.operacao === 1098,
      'has valpri': (b) => 'valpri' in b && b.valpri > 0,
    });
  }

  // Track errors
  errorRate.add(!success);

  // Log failures for debugging
  if (!success || response.status >= 500) {
    console.error(`Failed request: VU=${__VU}, Iter=${__ITER}, Status=${response.status}, Duration=${duration}ms`);
    if (response.body) {
      console.error(`Response body: ${response.body.substring(0, 200)}`);
    }
  }

  // Think time - simulate user delay between requests
  sleep(Math.random() * 2 + 1); // 1-3 seconds
}

// Setup function - runs once at the beginning
export function setup() {
  console.log('Starting payment authorization load test');
  console.log(`Target: ${BASE_URL}`);
  console.log('Configuration:');
  console.log('  - Ramp up: 100 users over 10 seconds');
  console.log('  - Sustain: 100 users for 60 seconds');
  console.log('  - Expected throughput: >50 req/s');
  console.log('  - Expected p95 latency: <2000ms');
  console.log('  - Expected error rate: <1%');

  // Warmup request to verify API is accessible
  const warmupUrl = `${BASE_URL}/api/claims/1/1/1/1`;
  const warmupResponse = http.get(warmupUrl, {
    headers: { 'Accept': 'application/json' },
    timeout: '10s',
  });

  if (warmupResponse.status !== 200 && warmupResponse.status !== 404) {
    console.error(`Warmup failed with status ${warmupResponse.status}`);
    console.error('API may not be accessible');
  } else {
    console.log('Warmup successful - API is accessible');
  }

  return { startTime: new Date().toISOString() };
}

// Teardown function - runs once at the end
export function teardown(data) {
  console.log('Load test completed');
  console.log(`Started at: ${data.startTime}`);
  console.log(`Ended at: ${new Date().toISOString()}`);
  console.log('Check the summary below for detailed metrics');
}

/**
 * How to run this test:
 *
 * 1. Install k6: https://k6.io/docs/getting-started/installation/
 *
 * 2. Run with default settings:
 *    k6 run payment-load-test.js
 *
 * 3. Run with custom API URL:
 *    k6 run -e API_BASE_URL=https://your-api.com payment-load-test.js
 *
 * 4. Run with HTML report:
 *    k6 run --out html=report.html payment-load-test.js
 *
 * 5. Run with JSON output:
 *    k6 run --out json=results.json payment-load-test.js
 *
 * 6. Run with cloud output (requires k6 cloud account):
 *    k6 cloud payment-load-test.js
 *
 * Expected results:
 * - http_req_duration (p50) < 500ms
 * - http_req_duration (p95) < 2000ms
 * - http_req_duration (p99) < 5000ms
 * - Error rate < 1%
 * - Request rate > 50 req/s
 * - No database deadlocks
 * - No connection pool exhaustion
 */
