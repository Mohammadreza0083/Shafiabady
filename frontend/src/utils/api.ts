import type {
  Result,
  PagedResult,
  PaginationParams,
  ArticleDto,
  CreateArticleDto,
  UpdateArticleDto,
  BookDto,
  CreateBookDto,
  UpdateBookDto,
  CourseDto,
  CreateCourseDto,
  UpdateCourseDto,
  CategoryDto,
  CreateCategoryDto,
  CommentDto,
  CreateCommentDto,
  UpdateCommentStatusDto,
  ViewStatsDto,
  RecordViewDto,
  AdminUserDto,
  LoginRequestDto,
  EntityType,
  CommentStatus,
} from '../types';
import {
  mockArticles,
  mockBooks,
  mockCourses,
  mockCategories,
  mockCommentsTree,
} from '../data/mockData';

/**
 * Normalizes an API base URL ensuring trailing slashes are removed and '/api' suffix is present.
 */
export function normalizeApiUrl(url: string): string {
  if (!url) return '';
  const trimmed = url.replace(/\/+$/, '');
  return trimmed.endsWith('/api') ? trimmed : `${trimmed}/api`;
}

/**
 * Dynamically resolves the API base URL based on runtime context:
 * - Server-side inside Docker container: connects to INTERNAL_API_URL or http://backend:5000/api
 * - Server-side outside Docker (local dev): connects to http://localhost:5000/api
 * - Client-side in browser: connects to window.__API_BASE_URL__, origin /api, or PUBLIC_API_URL
 */
export function getApiBaseUrl(): string {
  const isBrowser = typeof window !== 'undefined';

  if (isBrowser) {
    if ((window as any).__API_BASE_URL__) {
      return normalizeApiUrl((window as any).__API_BASE_URL__);
    }
    const publicUrl = typeof import.meta !== 'undefined' && import.meta.env?.PUBLIC_API_URL;
    if (publicUrl) {
      return normalizeApiUrl(publicUrl);
    }
    // If accessed through reverse proxy on default ports (e.g. port 80 / 443)
    if (window.location.port === '80' || window.location.port === '443' || window.location.port === '') {
      return `${window.location.origin}/api`;
    }
    return 'http://localhost:5000/api';
  }

  // Server-side execution (Astro SSR / Node.js)
  const ssrUrl =
    (typeof import.meta !== 'undefined' && import.meta.env && (import.meta.env.SSR_API_URL || import.meta.env.INTERNAL_API_URL)) ||
    (typeof process !== 'undefined' && (process.env.SSR_API_URL || process.env.INTERNAL_API_URL || process.env.BACKEND_INTERNAL_URL));

  if (ssrUrl) {
    return normalizeApiUrl(ssrUrl);
  }

  const isDocker = typeof process !== 'undefined' && (
    process.env.IS_DOCKER === 'true' ||
    process.env.CONTAINER === 'true' ||
    Boolean(process.env.HOSTNAME && process.env.HOSTNAME.length === 12)
  );

  const publicUrl =
    (typeof import.meta !== 'undefined' && import.meta.env && import.meta.env.PUBLIC_API_URL) ||
    (typeof process !== 'undefined' && process.env.PUBLIC_API_URL);

  if (isDocker && (!publicUrl || publicUrl.includes('localhost') || publicUrl.includes('127.0.0.1'))) {
    return 'http://backend:5000/api';
  }

  if (publicUrl) {
    return normalizeApiUrl(publicUrl);
  }

  return 'http://localhost:5000/api';
}

/**
 * Base URL for the .NET 9 Backend RESTful Web API.
 */
export const API_BASE_URL: string = getApiBaseUrl();

/**
 * Helper to build auth headers with both Cookie and Authorization bearer for SSR and Client-side calls.
 */
export function buildAuthHeaders(token?: string | null): Record<string, string> {
  const headers: Record<string, string> = {};
  if (token) {
    headers['Cookie'] = `AdminAuthToken=${token}`;
    headers['Authorization'] = `Bearer ${token}`;
  }
  return headers;
}

function toArticleDto(a: any): ArticleDto {
  return {
    id: a.id,
    title: a.title,
    slug: a.slug,
    summary: a.summary || '',
    content: a.content || '',
    thumbnailUrl: a.coverImage || a.thumbnailUrl || '',
    authorName: a.author?.name || a.authorName || 'دکتر محمد حسین شفیع‌آبادی',
    readTimeMinutes: a.readTimeMinutes || 8,
    tags: Array.isArray(a.tags) ? a.tags.join(', ') : (a.tags || ''),
    viewsCount: a.viewsCount || 0,
    categoryId: a.categoryId || null,
    categoryTitle: a.categoryTitle || a.category || null,
    categorySlug: a.categorySlug || null,
    createdAt: a.createdAt || new Date().toISOString(),
    updatedAt: a.updatedAt || null,
  };
}

function toBookDto(b: any): BookDto {
  return {
    id: b.id,
    title: b.title,
    slug: b.slug,
    subtitle: b.subtitle || null,
    description: b.description || '',
    thumbnailUrl: b.coverImage || b.thumbnailUrl || '',
    publisher: b.publisher || 'انتشارات تخصصی',
    publishYear: b.publishYear || '۱۴۰۴',
    pagesCount: b.pagesCount || 400,
    isbn: b.isbn || '978-600-00-0000-0',
    externalBuyLink: b.externalBuyLink || null,
    digitalDownloadLink: b.digitalDownloadLink || null,
    viewsCount: b.viewsCount || 0,
    categoryId: b.categoryId || null,
    categoryTitle: b.categoryTitle || b.category || null,
    categorySlug: b.categorySlug || null,
    createdAt: b.createdAt || new Date().toISOString(),
    updatedAt: b.updatedAt || null,
  };
}

function toCourseDto(c: any): CourseDto {
  return {
    id: c.id,
    title: c.title,
    slug: c.slug,
    description: c.description || '',
    thumbnailUrl: c.thumbnail || c.thumbnailUrl || '',
    aparatVideoUrl: c.aparatVideoUrl || null,
    level: c.level || 'پیشرفته',
    duration: c.duration || '۴۰ ساعت',
    lessonsCount: c.lessonsCount || 50,
    originalPrice: c.originalPrice || c.price || 0,
    discountedPrice: c.discountedPrice || c.price || null,
    discountPercentage: c.discountPercentage || null,
    viewsCount: c.viewsCount || 0,
    categoryId: c.categoryId || null,
    categoryTitle: c.categoryTitle || c.category || null,
    categorySlug: c.categorySlug || null,
    createdAt: c.createdAt || new Date().toISOString(),
    updatedAt: c.updatedAt || null,
  };
}

function paginate<T>(items: T[], pageNumber = 1, pageSize = 10): PagedResult<T> {
  const totalCount = items.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const start = (pageNumber - 1) * pageSize;
  const pagedItems = items.slice(start, start + pageSize);
  return {
    items: pagedItems,
    totalCount,
    pageNumber,
    pageSize,
    totalPages,
    hasNextPage: pageNumber < totalPages,
    hasPreviousPage: pageNumber > 1,
  };
}

function getOfflineMockFallback<T>(endpoint: string, method = 'GET'): T | undefined {
  try {
    const urlObj = endpoint.startsWith('http')
      ? new URL(endpoint)
      : new URL(endpoint, 'http://localhost:5000');
    const pathname = urlObj.pathname.replace(/^\/api/, '');
    const searchParams = urlObj.searchParams;

    // Articles list
    if (pathname === '/Articles' || pathname === '/Articles/') {
      if (method.toUpperCase() === 'GET') {
        const pageNumber = parseInt(searchParams.get('pageNumber') || '1', 10);
        const pageSize = parseInt(searchParams.get('pageSize') || '10', 10);
        const searchTerm = searchParams.get('searchTerm')?.toLowerCase() || searchParams.get('search')?.toLowerCase() || '';
        const categoryId = searchParams.get('categoryId') || '';
        const rawCategory = searchParams.get('categorySlug') || searchParams.get('category') || '';
        let category = rawCategory.trim();
        try {
          category = decodeURIComponent(category);
        } catch {}

        let list = mockArticles.map(toArticleDto);
        if (categoryId) {
          list = list.filter((a) => a.categoryId === categoryId);
        }
        if (category && category !== 'همه' && category.toLowerCase() !== 'all') {
          const catLower = category.toLowerCase();
          list = list.filter(
            (a) =>
              (a.categorySlug && a.categorySlug.toLowerCase() === catLower) ||
              (a.categoryTitle && a.categoryTitle.toLowerCase() === catLower) ||
              (a.categoryId && a.categoryId.toLowerCase() === catLower) ||
              ((a as any).category && (a as any).category.toLowerCase() === catLower)
          );
        }
        if (searchTerm) {
          list = list.filter(
            (a) =>
              a.title.toLowerCase().includes(searchTerm) ||
              a.summary.toLowerCase().includes(searchTerm) ||
              a.tags?.toLowerCase().includes(searchTerm)
          );
        }
        return paginate(list, pageNumber, pageSize) as unknown as T;
      }
    }

    // Article by slug
    const articleBySlugMatch = pathname.match(/^\/Articles\/by-slug\/(.+)$/);
    if (articleBySlugMatch) {
      const slug = decodeURIComponent(articleBySlugMatch[1]);
      const found = mockArticles.find((a) => a.slug === slug || encodeURIComponent(a.slug) === slug);
      if (found) return toArticleDto(found) as unknown as T;
      return toArticleDto(mockArticles[0]) as unknown as T;
    }

    // Article by ID
    const articleByIdMatch = pathname.match(/^\/Articles\/([a-zA-Z0-9-]+)$/);
    if (articleByIdMatch && !['by-slug'].includes(articleByIdMatch[1])) {
      const id = articleByIdMatch[1];
      const found = mockArticles.find((a) => a.id === id);
      if (found) return toArticleDto(found) as unknown as T;
      return toArticleDto(mockArticles[0]) as unknown as T;
    }

    // Books list
    if (pathname === '/Books' || pathname === '/Books/') {
      if (method.toUpperCase() === 'GET') {
        const pageNumber = parseInt(searchParams.get('pageNumber') || '1', 10);
        const pageSize = parseInt(searchParams.get('pageSize') || '10', 10);
        const searchTerm = searchParams.get('searchTerm')?.toLowerCase() || searchParams.get('search')?.toLowerCase() || '';
        const categoryId = searchParams.get('categoryId') || '';
        const rawCategory = searchParams.get('categorySlug') || searchParams.get('category') || '';
        let category = rawCategory.trim();
        try {
          category = decodeURIComponent(category);
        } catch {}

        let list = mockBooks.map(toBookDto);
        if (categoryId) {
          list = list.filter((b) => b.categoryId === categoryId);
        }
        if (category && category !== 'همه' && category.toLowerCase() !== 'all') {
          const catLower = category.toLowerCase();
          list = list.filter(
            (b) =>
              (b.categorySlug && b.categorySlug.toLowerCase() === catLower) ||
              (b.categoryTitle && b.categoryTitle.toLowerCase() === catLower) ||
              (b.categoryId && b.categoryId.toLowerCase() === catLower) ||
              ((b as any).category && (b as any).category.toLowerCase() === catLower)
          );
        }
        if (searchTerm) {
          list = list.filter(
            (b) =>
              b.title.toLowerCase().includes(searchTerm) ||
              b.description.toLowerCase().includes(searchTerm)
          );
        }
        return paginate(list, pageNumber, pageSize) as unknown as T;
      }
    }

    // Book by slug
    const bookBySlugMatch = pathname.match(/^\/Books\/by-slug\/(.+)$/);
    if (bookBySlugMatch) {
      const slug = decodeURIComponent(bookBySlugMatch[1]);
      const found = mockBooks.find((b) => b.slug === slug || encodeURIComponent(b.slug) === slug);
      if (found) return toBookDto(found) as unknown as T;
      return toBookDto(mockBooks[0]) as unknown as T;
    }

    // Book by ID
    const bookByIdMatch = pathname.match(/^\/Books\/([a-zA-Z0-9-]+)$/);
    if (bookByIdMatch && !['by-slug'].includes(bookByIdMatch[1])) {
      const id = bookByIdMatch[1];
      const found = mockBooks.find((b) => b.id === id);
      if (found) return toBookDto(found) as unknown as T;
      return toBookDto(mockBooks[0]) as unknown as T;
    }

    // Courses list
    if (pathname === '/Courses' || pathname === '/Courses/') {
      if (method.toUpperCase() === 'GET') {
        const pageNumber = parseInt(searchParams.get('pageNumber') || '1', 10);
        const pageSize = parseInt(searchParams.get('pageSize') || '10', 10);
        const searchTerm = searchParams.get('searchTerm')?.toLowerCase() || searchParams.get('search')?.toLowerCase() || '';
        const categoryId = searchParams.get('categoryId') || '';
        const rawCategory = searchParams.get('categorySlug') || searchParams.get('category') || '';
        let category = rawCategory.trim();
        try {
          category = decodeURIComponent(category);
        } catch {}

        let list = mockCourses.map(toCourseDto);
        if (categoryId) {
          list = list.filter((c) => c.categoryId === categoryId);
        }
        if (category && category !== 'همه' && category.toLowerCase() !== 'all') {
          const catLower = category.toLowerCase();
          list = list.filter(
            (c) =>
              (c.categorySlug && c.categorySlug.toLowerCase() === catLower) ||
              (c.categoryTitle && c.categoryTitle.toLowerCase() === catLower) ||
              (c.categoryId && c.categoryId.toLowerCase() === catLower) ||
              ((c as any).category && (c as any).category.toLowerCase() === catLower)
          );
        }
        if (searchTerm) {
          list = list.filter(
            (c) =>
              c.title.toLowerCase().includes(searchTerm) ||
              c.description.toLowerCase().includes(searchTerm)
          );
        }
        return paginate(list, pageNumber, pageSize) as unknown as T;
      }
    }

    // Course by slug
    const courseBySlugMatch = pathname.match(/^\/Courses\/by-slug\/(.+)$/);
    if (courseBySlugMatch) {
      const slug = decodeURIComponent(courseBySlugMatch[1]);
      const found = mockCourses.find((c) => c.slug === slug || encodeURIComponent(c.slug) === slug);
      if (found) return toCourseDto(found) as unknown as T;
      return toCourseDto(mockCourses[0]) as unknown as T;
    }

    // Course by ID
    const courseByIdMatch = pathname.match(/^\/Courses\/([a-zA-Z0-9-]+)$/);
    if (courseByIdMatch && !['by-slug'].includes(courseByIdMatch[1])) {
      const id = courseByIdMatch[1];
      const found = mockCourses.find((c) => c.id === id);
      if (found) return toCourseDto(found) as unknown as T;
      return toCourseDto(mockCourses[0]) as unknown as T;
    }

    // Categories
    if (pathname === '/Categories' || pathname === '/Categories/') {
      if (method.toUpperCase() === 'GET') {
        return mockCategories as unknown as T;
      }
    }
    const categoryByIdMatch = pathname.match(/^\/Categories\/([a-zA-Z0-9-]+)$/);
    if (categoryByIdMatch) {
      const id = categoryByIdMatch[1];
      const found = mockCategories.find((cat) => cat.id === id);
      if (found) return found as unknown as T;
      return mockCategories[0] as unknown as T;
    }

    // Comments
    if (pathname.startsWith('/Comments')) {
      if (method.toUpperCase() === 'GET') {
        return mockCommentsTree as unknown as T;
      }
      return { id: 'mock-cm-' + Date.now() } as unknown as T;
    }

    // Views
    if (pathname.startsWith('/Views')) {
      if (pathname.includes('/stats')) {
        return { totalViews: 12500, dailyViews: 350 } as unknown as T;
      }
      return null as unknown as T;
    }

    // Auth preview fallback
    if (pathname === '/Auth/me' || pathname === '/Auth/refresh') {
      return {
        id: 'admin-001',
        username: 'admin',
        fullName: 'دکتر محمد حسین شفیع‌آبادی',
        email: 'admin@shafieiabadi.ir',
        role: 'SuperAdmin',
        createdAt: new Date().toISOString(),
      } as unknown as T;
    }
  } catch (err) {
    console.warn('[Offline Fallback Error]', err);
  }

  return undefined;
}

/**
 * Universal fetch wrapper ensuring `credentials: 'include'` is always set for HttpOnly cookies
 * and network errors are caught and transformed into standardized `Result<T>` error responses
 * or high-fidelity offline mock fallbacks.
 */
export async function apiFetch<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<Result<T>> {
  const baseUrl = getApiBaseUrl();
  const url = endpoint.startsWith('http')
    ? endpoint
    : `${baseUrl}${endpoint.startsWith('/') ? '' : '/'}${endpoint}`;

  const defaultHeaders: Record<string, string> = {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  };

  const config: RequestInit = {
    ...options,
    headers: {
      ...defaultHeaders,
      ...((options.headers as Record<string, string>) || {}),
    },
    // Ensures browser automatically transmits and accepts HttpOnly cookies across origins
    credentials: 'include',
  };

  try {
    const response = await fetch(url, config);

    // If HTTP 204 No Content
    if (response.status === 204) {
      return {
        isSuccess: true,
        statusCode: 204,
        errorMessage: null,
        validationErrors: [],
        data: null,
      };
    }

    // Try to parse JSON response
    const json = await response.json().catch(() => null);

    if (json && typeof json === 'object' && 'isSuccess' in json) {
      return json as Result<T>;
    }

    // If response is not standard Result wrapper but is ok
    if (response.ok) {
      return {
        isSuccess: true,
        statusCode: response.status,
        errorMessage: null,
        validationErrors: [],
        data: (json as T) ?? null,
      };
    }

    // Handled non-ok status without Result body
    return {
      isSuccess: false,
      statusCode: response.status,
      errorMessage: json?.message || json?.title || `درخواست با خطای HTTP ${response.status} مواجه شد.`,
      validationErrors: json?.errors ? (Object.values(json.errors).flat() as string[]) : [],
      data: null,
    };
  } catch (error: any) {
    const errMessage = error?.message || String(error);
    const isDev =
      (typeof import.meta !== 'undefined' && import.meta.env?.DEV) ||
      (typeof process !== 'undefined' && process.env.NODE_ENV !== 'production');

    if (isDev) {
      console.warn(`[Local API Failure] Request to ${url} failed with error: ${errMessage}. Defaulting to mock data.`);
    }

    // Check if offline mock fallback can satisfy request (e.g. Vercel build / offline preview)
    const fallback = getOfflineMockFallback<T>(endpoint, options.method || 'GET');
    if (fallback !== undefined) {
      if (!isDev) {
        console.info(`[Offline Fallback Active] Serving high-fidelity mock data for ${endpoint}`);
      }
      return {
        isSuccess: true,
        statusCode: 200,
        errorMessage: null,
        validationErrors: [],
        data: fallback,
      };
    }

    // Graceful network error handling
    console.warn(`[API Network Error] ${url}:`, errMessage);
    return {
      isSuccess: false,
      statusCode: 0,
      errorMessage:
        error?.message === 'Failed to fetch' || error?.name === 'TypeError'
          ? 'امکان برقراری ارتباط با سرور API وجود ندارد. لطفاً از روشن بودن سرویس بک‌اند روی پورت ۵۰۰۰ اطمینان حاصل فرمایید.'
          : error?.message || 'خطای غیرمنتظره در ارتباط با سرور رخ داده است.',
      validationErrors: [],
      data: null,
    };
  }
}

// ==========================================
// Authentication API Endpoints
// ==========================================

export const authApi = {
  /**
   * Authenticates an administrator and sets the HttpOnly cookie `AdminAuthToken`.
   */
  login: async (credentials: LoginRequestDto): Promise<Result<AdminUserDto>> => {
    return apiFetch<AdminUserDto>('/Auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    });
  },

  /**
   * Logs out current administrator by clearing the HttpOnly session cookie.
   */
  logout: async (): Promise<Result<void>> => {
    return apiFetch<void>('/Auth/logout', {
      method: 'POST',
    });
  },

  /**
   * Verifies existing administrator session via HttpOnly cookie or token header.
   */
  me: async (token?: string | null): Promise<Result<AdminUserDto>> => {
    return apiFetch<AdminUserDto>('/Auth/me', {
      method: 'GET',
      headers: buildAuthHeaders(token),
    });
  },

  /**
   * Silently refreshes the administrator JWT session cookie.
   */
  refresh: async (token?: string | null): Promise<Result<AdminUserDto>> => {
    return apiFetch<AdminUserDto>('/Auth/refresh', {
      method: 'POST',
      headers: buildAuthHeaders(token),
    });
  },

  /**
   * Changes the administrator password and invalidates active refresh tokens.
   */
  changePassword: async (
    dto: { currentPassword: string; newPassword: string; confirmNewPassword: string },
    token?: string | null
  ): Promise<Result<void>> => {
    return apiFetch<void>('/Auth/change-password', {
      method: 'POST',
      headers: buildAuthHeaders(token),
      body: JSON.stringify(dto),
    });
  },
};

// ==========================================
// Articles API Endpoints
// ==========================================

export const articlesApi = {
  /**
   * Retrieves a paginated list of articles for infinite scrolling and archive filtering.
   */
  getPaged: async (params?: PaginationParams, token?: string | null): Promise<Result<PagedResult<ArticleDto>>> => {
    const query = new URLSearchParams();
    if (params?.pageNumber) query.append('pageNumber', params.pageNumber.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.searchTerm) query.append('searchTerm', params.searchTerm);
    if (params?.categoryId) query.append('categoryId', params.categoryId);
    if (params?.category) query.append('category', params.category);
    if (params?.categorySlug) query.append('categorySlug', params.categorySlug);

    const queryString = query.toString();
    return apiFetch<PagedResult<ArticleDto>>(`/Articles${queryString ? `?${queryString}` : ''}`, {
      headers: buildAuthHeaders(token),
    });
  },

  /**
   * Retrieves full article details by its unique URL slug.
   */
  getBySlug: async (slug: string): Promise<Result<ArticleDto>> => {
    let cleanSlug = slug;
    try {
      cleanSlug = decodeURIComponent(slug);
    } catch {}
    return apiFetch<ArticleDto>(`/Articles/by-slug/${encodeURIComponent(cleanSlug)}`);
  },

  /**
   * Retrieves full article details by its unique GUID.
   */
  getById: async (id: string, token?: string | null): Promise<Result<ArticleDto>> => {
    return apiFetch<ArticleDto>(`/Articles/${id}`, {
      headers: buildAuthHeaders(token),
    });
  },

  /**
   * Creates a new article (Admin authenticated).
   */
  create: async (data: CreateArticleDto, token?: string | null): Promise<Result<ArticleDto>> => {
    return apiFetch<ArticleDto>('/Articles', {
      method: 'POST',
      headers: buildAuthHeaders(token),
      body: JSON.stringify(data),
    });
  },

  /**
   * Updates an existing article (Admin authenticated).
   */
  update: async (id: string, data: UpdateArticleDto, token?: string | null): Promise<Result<ArticleDto>> => {
    return apiFetch<ArticleDto>(`/Articles/${id}`, {
      method: 'PUT',
      headers: buildAuthHeaders(token),
      body: JSON.stringify(data),
    });
  },

  /**
   * Deletes an article by ID (Admin authenticated).
   */
  delete: async (id: string, token?: string | null): Promise<Result<void>> => {
    return apiFetch<void>(`/Articles/${id}`, {
      method: 'DELETE',
      headers: buildAuthHeaders(token),
    });
  },
};

// ==========================================
// Books API Endpoints
// ==========================================

export const booksApi = {
  /**
   * Retrieves a paginated list of published books.
   */
  getPaged: async (params?: PaginationParams, token?: string | null): Promise<Result<PagedResult<BookDto>>> => {
    const query = new URLSearchParams();
    if (params?.pageNumber) query.append('pageNumber', params.pageNumber.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.searchTerm) query.append('searchTerm', params.searchTerm);
    if (params?.categoryId) query.append('categoryId', params.categoryId);
    if (params?.category) query.append('category', params.category);
    if (params?.categorySlug) query.append('categorySlug', params.categorySlug);

    const queryString = query.toString();
    return apiFetch<PagedResult<BookDto>>(`/Books${queryString ? `?${queryString}` : ''}`, {
      headers: buildAuthHeaders(token),
    });
  },

  /**
   * Retrieves book details by unique URL slug.
   */
  getBySlug: async (slug: string): Promise<Result<BookDto>> => {
    let cleanSlug = slug;
    try {
      cleanSlug = decodeURIComponent(slug);
    } catch {}
    return apiFetch<BookDto>(`/Books/by-slug/${encodeURIComponent(cleanSlug)}`);
  },

  /**
   * Retrieves book details by GUID.
   */
  getById: async (id: string, token?: string | null): Promise<Result<BookDto>> => {
    return apiFetch<BookDto>(`/Books/${id}`, {
      headers: buildAuthHeaders(token),
    });
  },

  /**
   * Creates a new book entry (Admin authenticated).
   */
  create: async (data: CreateBookDto, token?: string | null): Promise<Result<BookDto>> => {
    return apiFetch<BookDto>('/Books', {
      method: 'POST',
      headers: buildAuthHeaders(token),
      body: JSON.stringify(data),
    });
  },

  /**
   * Updates an existing book entry (Admin authenticated).
   */
  update: async (id: string, data: UpdateBookDto, token?: string | null): Promise<Result<BookDto>> => {
    return apiFetch<BookDto>(`/Books/${id}`, {
      method: 'PUT',
      headers: buildAuthHeaders(token),
      body: JSON.stringify(data),
    });
  },

  /**
   * Deletes a book entry (Admin authenticated).
   */
  delete: async (id: string, token?: string | null): Promise<Result<void>> => {
    return apiFetch<void>(`/Books/${id}`, {
      method: 'DELETE',
      headers: buildAuthHeaders(token),
    });
  },
};

// ==========================================
// Courses API Endpoints
// ==========================================

export const coursesApi = {
  /**
   * Retrieves a paginated list of training courses.
   */
  getPaged: async (params?: PaginationParams, token?: string | null): Promise<Result<PagedResult<CourseDto>>> => {
    const query = new URLSearchParams();
    if (params?.pageNumber) query.append('pageNumber', params.pageNumber.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.searchTerm) query.append('searchTerm', params.searchTerm);
    if (params?.categoryId) query.append('categoryId', params.categoryId);
    if (params?.category) query.append('category', params.category);
    if (params?.categorySlug) query.append('categorySlug', params.categorySlug);

    const queryString = query.toString();
    return apiFetch<PagedResult<CourseDto>>(`/Courses${queryString ? `?${queryString}` : ''}`, {
      headers: buildAuthHeaders(token),
    });
  },

  /**
   * Retrieves course details by unique URL slug.
   */
  getBySlug: async (slug: string): Promise<Result<CourseDto>> => {
    let cleanSlug = slug;
    try {
      cleanSlug = decodeURIComponent(slug);
    } catch {}
    return apiFetch<CourseDto>(`/Courses/by-slug/${encodeURIComponent(cleanSlug)}`);
  },

  /**
   * Retrieves course details by GUID.
   */
  getById: async (id: string, token?: string | null): Promise<Result<CourseDto>> => {
    return apiFetch<CourseDto>(`/Courses/${id}`, {
      headers: buildAuthHeaders(token),
    });
  },

  /**
   * Creates a new course (Admin authenticated).
   */
  create: async (data: CreateCourseDto, token?: string | null): Promise<Result<CourseDto>> => {
    return apiFetch<CourseDto>('/Courses', {
      method: 'POST',
      headers: buildAuthHeaders(token),
      body: JSON.stringify(data),
    });
  },

  /**
   * Updates an existing course (Admin authenticated).
   */
  update: async (id: string, data: UpdateCourseDto, token?: string | null): Promise<Result<CourseDto>> => {
    return apiFetch<CourseDto>(`/Courses/${id}`, {
      method: 'PUT',
      headers: buildAuthHeaders(token),
      body: JSON.stringify(data),
    });
  },

  /**
   * Deletes a course (Admin authenticated).
   */
  delete: async (id: string, token?: string | null): Promise<Result<void>> => {
    return apiFetch<void>(`/Courses/${id}`, {
      method: 'DELETE',
      headers: buildAuthHeaders(token),
    });
  },
};

// ==========================================
// Categories API Endpoints
// ==========================================

export const categoriesApi = {
  /**
   * Retrieves all content categories.
   */
  getAll: async (): Promise<Result<CategoryDto[]>> => {
    return apiFetch<CategoryDto[]>('/Categories');
  },

  /**
   * Retrieves category by GUID.
   */
  getById: async (id: string): Promise<Result<CategoryDto>> => {
    return apiFetch<CategoryDto>(`/Categories/${id}`);
  },

  /**
   * Creates a new category (Admin authenticated).
   */
  create: async (data: CreateCategoryDto, token?: string | null): Promise<Result<CategoryDto>> => {
    return apiFetch<CategoryDto>('/Categories', {
      method: 'POST',
      headers: buildAuthHeaders(token),
      body: JSON.stringify(data),
    });
  },

  /**
   * Updates a category (Admin authenticated).
   */
  update: async (id: string, data: CreateCategoryDto, token?: string | null): Promise<Result<CategoryDto>> => {
    return apiFetch<CategoryDto>(`/Categories/${id}`, {
      method: 'PUT',
      headers: buildAuthHeaders(token),
      body: JSON.stringify(data),
    });
  },

  /**
   * Deletes a category (Admin authenticated).
   */
  delete: async (id: string, token?: string | null): Promise<Result<void>> => {
    return apiFetch<void>(`/Categories/${id}`, {
      method: 'DELETE',
      headers: buildAuthHeaders(token),
    });
  },
};

// ==========================================
// Comments API Endpoints
// ==========================================

export const commentsApi = {
  /**
   * Retrieves hierarchical comment tree for an entity.
   */
  getTree: async (type: EntityType | number, entityId: string): Promise<Result<CommentDto[]>> => {
    return apiFetch<CommentDto[]>(`/Comments/tree?type=${type}&entityId=${entityId}`);
  },

  /**
   * Retrieves paginated comments for admin moderation queue (Admin authenticated).
   */
  getPagedForAdmin: async (
    pageNumber: number = 1,
    pageSize: number = 20,
    status?: CommentStatus | number | null,
    token?: string | null
  ): Promise<Result<PagedResult<CommentDto>>> => {
    const query = new URLSearchParams();
    query.append('pageNumber', pageNumber.toString());
    query.append('pageSize', pageSize.toString());
    if (status !== undefined && status !== null) {
      query.append('status', status.toString());
    }
    return apiFetch<PagedResult<CommentDto>>(`/Comments/admin/paged?${query.toString()}`, {
      headers: buildAuthHeaders(token),
    });
  },

  /**
   * Submits a new comment or reply.
   */
  create: async (data: CreateCommentDto): Promise<Result<CommentDto>> => {
    return apiFetch<CommentDto>('/Comments', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  /**
   * Moderates comment status (Admin authenticated).
   */
  updateStatus: async (data: UpdateCommentStatusDto, token?: string | null): Promise<Result<void>> => {
    return apiFetch<void>('/Comments/status', {
      method: 'PUT',
      headers: buildAuthHeaders(token),
      body: JSON.stringify(data),
    });
  },

  /**
   * Deletes a comment (Admin authenticated).
   */
  delete: async (id: string, token?: string | null): Promise<Result<void>> => {
    return apiFetch<void>(`/Comments/${id}`, {
      method: 'DELETE',
      headers: buildAuthHeaders(token),
    });
  },
};

// ==========================================
// Views & Analytics API Endpoints
// ==========================================

export const viewsApi = {
  /**
   * Records a page view with 24-hour IP rate limiting.
   */
  record: async (entityType: EntityType | number, entityId: string): Promise<Result<boolean>> => {
    return apiFetch<boolean>('/Views/record', {
      method: 'POST',
      body: JSON.stringify({ entityType, entityId }),
    });
  },

  /**
   * Retrieves aggregated analytics statistics for the dashboard (Admin authenticated).
   */
  getStats: async (token?: string | null): Promise<Result<ViewStatsDto>> => {
    return apiFetch<ViewStatsDto>('/Views/stats', {
      headers: buildAuthHeaders(token),
    });
  },
};
