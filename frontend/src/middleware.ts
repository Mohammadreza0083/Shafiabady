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

  return next();
});
