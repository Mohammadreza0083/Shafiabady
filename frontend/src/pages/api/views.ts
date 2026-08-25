import type { APIRoute } from 'astro';
import { mockArticles, mockBooks, mockCourses } from '../../data/mockData';

export const prerender = false;

// In-memory view cache for demo / hybrid mode
const viewCounts: Record<string, number> = {};
const lastIpViews: Record<string, number> = {}; // key: "ip:entityId" -> timestamp
const TWENTY_FOUR_HOURS_MS = 24 * 60 * 60 * 1000;

// Initialize counts from mock data
mockArticles.forEach((a) => (viewCounts[a.id] = a.viewsCount));
mockBooks.forEach((b) => (viewCounts[b.id] = b.viewsCount));
mockCourses.forEach((c) => (viewCounts[c.id] = c.viewsCount));

export const POST: APIRoute = async ({ request, clientAddress }) => {
  try {
    const body = await request.json();
    const { entityId, increment = true } = body;

    if (!entityId) {
      return new Response(JSON.stringify({ error: 'entityId is required' }), {
        status: 400,
        headers: { 'Content-Type': 'application/json' },
      });
    }

    const ip = clientAddress || request.headers.get('x-forwarded-for') || '127.0.0.1';
    const trackingKey = `${ip}:${entityId}`;
    const now = Date.now();
    const lastView = lastIpViews[trackingKey] || 0;

    let isNewView = false;

    // Check 24 hour IP rate limit
    if (increment && now - lastView >= TWENTY_FOUR_HOURS_MS) {
      viewCounts[entityId] = (viewCounts[entityId] || 0) + 1;
      lastIpViews[trackingKey] = now;
      isNewView = true;
    }

    return new Response(
      JSON.stringify({
        entityId,
        viewsCount: viewCounts[entityId] || 0,
        isNewView,
      }),
      {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      }
    );
  } catch (error) {
    return new Response(JSON.stringify({ error: 'Invalid request body' }), {
      status: 400,
      headers: { 'Content-Type': 'application/json' },
    });
  }
};
