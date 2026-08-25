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

/**
 * Base URL for the .NET 9 Backend RESTful Web API.
 * Configured strictly to http://localhost:5000/api.
 */
export const API_BASE_URL: string =
  (typeof import.meta !== 'undefined' && import.meta.env && import.meta.env.PUBLIC_API_URL) ||
  (typeof window !== 'undefined' && (window as any).__API_BASE_URL__) ||
  'http://localhost:5000/api';

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

/**
 * Universal fetch wrapper ensuring `credentials: 'include'` is always set for HttpOnly cookies
 * and network errors are caught and transformed into standardized `Result<T>` error responses.
 */
export async function apiFetch<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<Result<T>> {
  const url = endpoint.startsWith('http')
    ? endpoint
    : `${API_BASE_URL}${endpoint.startsWith('/') ? '' : '/'}${endpoint}`;

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
    // Graceful network error handling
    console.warn(`[API Network Error] ${url}:`, error?.message || error);
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
