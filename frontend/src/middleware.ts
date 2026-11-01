import { defineMiddleware } from 'astro:middleware';

/**
 * Server-Side Authentication Guard Middleware for Protected Admin Routes.
 * Intercepts all requests targeting `/admin/*` and verifies the presence of
 * the HttpOnly JWT cookie `AdminAuthToken`.
 */
export const onRequest = defineMiddleware(async (context, next) => {
  const { pathname } = context.url;

  // Protect all /admin routes except /admin/login
  if (pathname.startsWith('/admin') && pathname !== '/admin/login' && pathname !== '/admin/login/') {
    const token = context.cookies.get('AdminAuthToken')?.value;
    if (!token) {
      return context.redirect('/admin/login');
    }
  }

  // If already authenticated and navigating to login page, redirect to dashboard
  if (pathname === '/admin/login' || pathname === '/admin/login/') {
    const token = context.cookies.get('AdminAuthToken')?.value;
    if (token) {
      return context.redirect('/admin');
    }
  }

  const response = await next();

  // Enforce enterprise security headers on all responses
  response.headers.set('X-Frame-Options', 'DENY');
  response.headers.set('X-Content-Type-Options', 'nosniff');
  response.headers.set('Referrer-Policy', 'strict-origin-when-cross-origin');
  response.headers.set('Permissions-Policy', 'camera=(), microphone=(), geolocation=()');
  response.headers.set(
    'Content-Security-Policy',
    "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https: blob: http://localhost:*; font-src 'self' data: https:; connect-src 'self' http://localhost:* ws://localhost:* https://shafieiabadi.ir https://*.shafieiabadi.ir; frame-ancestors 'none';"
  );

  return response;
});
