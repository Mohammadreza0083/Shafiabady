import type { APIRoute } from 'astro';
import { articlesApi } from '../../utils/api';
import { mockArticles } from '../../data/mockData';

export const prerender = false;

export const GET: APIRoute = async ({ url }) => {
  const page = parseInt(url.searchParams.get('page') || url.searchParams.get('pageNumber') || '1', 10);
  const limit = parseInt(url.searchParams.get('limit') || url.searchParams.get('pageSize') || '10', 10);
  const searchTerm = url.searchParams.get('searchTerm') || undefined;
  const categoryId = url.searchParams.get('categoryId') || undefined;

  try {
    const result = await articlesApi.getPaged({
      pageNumber: page,
      pageSize: limit,
      searchTerm,
      categoryId,
    });

    if (result.isSuccess && result.data) {
      return new Response(
        JSON.stringify({
          data: result.data.items,
          total: result.data.totalCount,
          page: result.data.pageNumber,
          totalPages: result.data.totalPages,
          hasMore: result.data.hasNextPage,
        }),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
            'Cache-Control': 'no-cache',
          },
        }
      );
    }
  } catch (e) {
    console.warn('[Proxy /api/articles] Failed to reach backend, using fallback:', e);
  }

  // Fallback to mock data if backend unreachable
  const startIndex = (page - 1) * limit;
  const endIndex = startIndex + limit;
  const data = mockArticles.slice(startIndex, endIndex);
  const total = mockArticles.length;
  const totalPages = Math.ceil(total / limit);

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
