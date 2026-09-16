import http from 'k6/http';
import { check, sleep, group } from 'k6';

/**
 * ==============================================================================
 * k6 Automated Performance & Load Testing Suite
 * Platform: Dr. Mohammad Hossein Shafiei Abadi Educational & Branding Stack
 * ==============================================================================
 *
 * Execution Profiles:
 * 1. Docker execution:
 *    docker run --rm -i --network=shafieiabadi-net grafana/k6 run - <tests/k6/load-test.js
 *
 * 2. Host CLI / npm execution:
 *    npm run test:load
 *    or: k6 run tests/k6/load-test.js
 */

export const options = {
  // 4-Stage Traffic Ramp Profile
  stages: [
    { duration: '30s', target: 20 },  // Stage 1: Warm-up & Ramp-up (0 -> 20 VUs)
    { duration: '1m',  target: 50 },  // Stage 2: Sustained Peak Load (20 -> 50 VUs)
    { duration: '30s', target: 100 }, // Stage 3: Extreme Stress Spike (50 -> 100 VUs)
    { duration: '20s', target: 0 },   // Stage 4: Cool-down Recovery (100 -> 0 VUs)
  ],

  // Production Quality Gate SLAs / Thresholds
  thresholds: {
    // 95% of requests must complete in under 500ms
    http_req_duration: ['p(95)<500'],
    // Cumulative error rate must be strictly below 1%
    http_req_failed: ['rate<0.01'],
  },
};

// Base URLs configurable via environment variables
const BASE_URL = __ENV.BASE_URL || 'http://localhost';
const API_URL = __ENV.API_URL || `${BASE_URL}/api`;
const ADMIN_TOKEN = __ENV.ADMIN_TOKEN || 'mock-loadtest-admin-token';

export default function () {
  const defaultHeaders = {
    'Accept': 'application/json, text/html, */*',
    'User-Agent': 'k6-load-tester/1.0 (ShafieiAbadi Benchmark)',
  };

  // ----------------------------------------------------------------------------
  // Scenario 1: Public Frontend Pages (Astro SSR)
  // ----------------------------------------------------------------------------
  group('01_Frontend_Public_Pages', () => {
    const pages = [
      { name: 'Landing Page', path: '/' },
      { name: 'Articles Catalog', path: '/articles' },
      { name: 'Books Catalog', path: '/books' },
      { name: 'Courses Catalog', path: '/courses' },
      { name: 'About Biography', path: '/about' },
    ];

    const randomPage = pages[Math.floor(Math.random() * pages.length)];
    const res = http.get(`${BASE_URL}${randomPage.path}`, {
      headers: defaultHeaders,
      tags: { name: `Frontend_${randomPage.name}` },
    });

    check(res, {
      [`${randomPage.name} status is 200`]: (r) => r.status === 200,
      'response has valid content': (r) => r.body && r.body.length > 200,
    });
  });

  sleep(1);

  // ----------------------------------------------------------------------------
  // Scenario 2: Public Backend RESTful API Endpoints
  // ----------------------------------------------------------------------------
  group('02_Backend_Public_APIs', () => {
    // 2.1 Articles Paged API
    const articlesRes = http.get(`${API_URL}/Articles?pageNumber=1&pageSize=10`, {
      headers: defaultHeaders,
      tags: { name: 'API_GetArticlesPaged' },
    });

    check(articlesRes, {
      'Articles API status is 200': (r) => r.status === 200,
      'Articles API returned json': (r) => r.headers['Content-Type'] && r.headers['Content-Type'].includes('json'),
      'Articles API isSuccess': (r) => {
        try {
          const json = JSON.parse(r.body);
          return json.isSuccess === true;
        } catch {
          return false;
        }
      },
    });

    // 2.2 Books Paged API
    const booksRes = http.get(`${API_URL}/Books?pageNumber=1&pageSize=10`, {
      headers: defaultHeaders,
      tags: { name: 'API_GetBooksPaged' },
    });

    check(booksRes, {
      'Books API status is 200': (r) => r.status === 200,
      'Books API isSuccess': (r) => {
        try {
          const json = JSON.parse(r.body);
          return json.isSuccess === true;
        } catch {
          return false;
        }
      },
    });

    // 2.3 Categories List API
    const catRes = http.get(`${API_URL}/Categories`, {
      headers: defaultHeaders,
      tags: { name: 'API_GetCategories' },
    });

    check(catRes, {
      'Categories API status is 200': (r) => r.status === 200,
    });
  });

  sleep(1);

  // ----------------------------------------------------------------------------
  // Scenario 3: Protected Admin Endpoints & Security Checks
  // ----------------------------------------------------------------------------
  group('03_Admin_Protected_Endpoints', () => {
    const adminHeaders = Object.assign({}, defaultHeaders, {
      'Authorization': `Bearer ${ADMIN_TOKEN}`,
      'Cookie': `AdminAuthToken=${ADMIN_TOKEN}`,
    });

    // Query admin comments management endpoint
    const adminCommentsRes = http.get(`${API_URL}/Comments/admin/paged?pageNumber=1&pageSize=20`, {
      headers: adminHeaders,
      tags: { name: 'API_Admin_Comments' },
      responseCallback: http.expectedStatuses(200, 401),
    });

    check(adminCommentsRes, {
      'Admin Comments response is acceptable (200 OK or 401 Unauthorized if unauthenticated)': (r) =>
        r.status === 200 || r.status === 401,
    });
  });

  sleep(1);
}
