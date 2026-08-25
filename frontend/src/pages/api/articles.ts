import type { APIRoute } from 'astro';
import { mockArticles } from '../../data/mockData';

export const prerender = false;

export const GET: APIRoute = async ({ url }) => {
  const page = parseInt(url.searchParams.get('page') || '1', 10);
  const limit = parseInt(url.searchParams.get('limit') || '3', 10);
  const category = url.searchParams.get('category');

  let filtered = [...mockArticles];
  if (category && category !== 'all') {
    filtered = filtered.filter((a) => a.category === category);
  }

  const total = filtered.length;
  const totalPages = Math.ceil(total / limit);
  const startIndex = (page - 1) * limit;
  const endIndex = startIndex + limit;
  const data = filtered.slice(startIndex, endIndex);

  return new Response(
    JSON.stringify({
      data,
      total,
      page,
      totalPages,
      hasMore: page < totalPages,
    }),
    {
      status: 200,
      headers: {
        'Content-Type': 'application/json',
        'Cache-Control': 'no-cache',
      },
    }
  );
};
