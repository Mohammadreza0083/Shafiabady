// ==========================================
// Standardized API Response & Generic Wrappers
// ==========================================

export interface Result {
  isSuccess: boolean;
  statusCode: number;
  errorMessage?: string | null;
  validationErrors: string[];
}

export interface Result<T = any> {
  isSuccess: boolean;
  statusCode: number;
  errorMessage?: string | null;
  validationErrors: string[];
  data?: T | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: string;
  category?: string;
  categorySlug?: string;
}

// ==========================================
// Enums
// ==========================================

export enum EntityType {
  Article = 1,
  Book = 2,
  Course = 3,
}

export enum CommentStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
}

// ==========================================
// Domain DTOs (Data Transfer Objects)
// ==========================================

export interface CategoryDto {
  id: string;
  title: string;
  slug: string;
  description?: string | null;
  displayOrder: number;
  articlesCount: number;
  booksCount: number;
  coursesCount: number;
}

export interface CreateCategoryDto {
  title: string;
  slug: string;
  description?: string | null;
  displayOrder?: number;
}

export interface ArticleDto {
  id: string;
  title: string;
  slug: string;
  summary: string;
  content: string;
  thumbnailUrl: string;
  authorName: string;
  readTimeMinutes: number;
  tags: string;
  viewsCount: number;
  categoryId?: string | null;
  categoryTitle?: string | null;
  categorySlug?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateArticleDto {
  title: string;
  slug: string;
  summary: string;
  content: string;
  thumbnailUrl: string;
  authorName?: string;
  readTimeMinutes?: number;
  tags?: string;
  categoryId?: string | null;
}

export interface UpdateArticleDto extends CreateArticleDto {}

export interface BookDto {
  id: string;
  title: string;
  slug: string;
  subtitle?: string | null;
  description: string;
  thumbnailUrl: string;
  publisher: string;
  publishYear: string;
  pagesCount: number;
  isbn: string;
  externalBuyLink?: string | null;
  digitalDownloadLink?: string | null;
  viewsCount: number;
  categoryId?: string | null;
  categoryTitle?: string | null;
  categorySlug?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateBookDto {
  title: string;
  slug: string;
  subtitle?: string | null;
  description: string;
  thumbnailUrl: string;
  publisher: string;
  publishYear: string;
  pagesCount: number;
  isbn: string;
  externalBuyLink?: string | null;
  digitalDownloadLink?: string | null;
  categoryId?: string | null;
}

export interface UpdateBookDto extends CreateBookDto {}

export interface CourseDto {
  id: string;
  title: string;
  slug: string;
  description: string;
  thumbnailUrl: string;
  aparatVideoUrl?: string | null;
  level: string;
  duration: string;
  lessonsCount: number;
  originalPrice: number;
  discountedPrice?: number | null;
  discountPercentage?: number | null;
  viewsCount: number;
  categoryId?: string | null;
  categoryTitle?: string | null;
  categorySlug?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateCourseDto {
  title: string;
  slug: string;
  description: string;
  thumbnailUrl: string;
  aparatVideoUrl?: string | null;
  level?: string;
  duration: string;
  lessonsCount?: number;
  originalPrice?: number;
  discountedPrice?: number | null;
  discountPercentage?: number | null;
  categoryId?: string | null;
}

export interface UpdateCourseDto extends CreateCourseDto {}

export interface CommentDto {
  id: string;
  guestName: string;
  email: string;
  text: string;
  status: CommentStatus;
  parentCommentId?: string | null;
  articleId?: string | null;
  bookId?: string | null;
  courseId?: string | null;
  createdAt: string;
  replies: CommentDto[];
}

export interface CreateCommentDto {
  guestName: string;
  email: string;
  text: string;
  parentCommentId?: string | null;
  articleId?: string | null;
  bookId?: string | null;
  courseId?: string | null;
}

export interface UpdateCommentStatusDto {
  commentId: string;
  status: CommentStatus;
}

export interface RecordViewDto {
  entityType: EntityType;
  entityId: string;
}

export interface ViewStatsDto {
  totalViews: number;
  totalArticlesViews: number;
  totalBooksViews: number;
  totalCoursesViews: number;
  topArticleTitle: string;
  topArticleViews: number;
  topBookTitle: string;
  topBookViews: number;
  topCourseTitle: string;
  topCourseViews: number;
  pendingCommentsCount: number;
}

export interface AdminUserDto {
  username: string;
  fullName: string;
  email: string;
  lastLoginAt?: string | null;
}

export interface LoginRequestDto {
  usernameOrEmail: string;
  password: string;
  rememberMe?: boolean;
}

export interface LoginResultDto {
  user: AdminUserDto;
  token: string;
  expiresAt: string;
}

export interface AuthStatusDto {
  isAuthenticated: boolean;
  username?: string | null;
  fullName?: string | null;
  email?: string | null;
}

// ==========================================
// Legacy / UI-Mapped Types (For backward compatibility)
// ==========================================

export interface BaseEntity {
  id: string;
  createdAt: string;
  updatedAt?: string;
}

export interface Article extends BaseEntity {
  slug: string;
  title: string;
  summary: string;
  content: string;
  coverImage: string;
  readTime: string;
  category: string;
  tags: string[];
  viewsCount: number;
  commentsCount: number;
  author: {
    name: string;
    avatar: string;
    role: string;
  };
}

export interface Book extends BaseEntity {
  slug: string;
  title: string;
  subtitle: string;
  description: string;
  coverImage: string;
  pagesCount: number;
  publisher: string;
  publishYear: string;
  downloadLink?: string;
  viewsCount: number;
}

export interface Course extends BaseEntity {
  slug: string;
  title: string;
  description: string;
  thumbnail: string;
  duration: string;
  lessonsCount: number;
  level: 'مقدماتی' | 'متوسط' | 'پیشرفته' | 'جامع';
  price: number;
  originalPrice?: number;
  discountedPrice?: number;
  discountPercentage?: number;
  isFree: boolean;
  viewsCount: number;
}

export interface Comment extends BaseEntity {
  authorName: string;
  authorEmail: string;
  authorAvatar?: string;
  content: string;
  parentId?: string | null;
  replies?: Comment[];
}

export interface ContactFormData {
  fullName: string;
  email: string;
  phoneNumber?: string;
  baleId?: string;
  message: string;
}

export interface ViewCounterResponse {
  entityId: string;
  viewsCount: number;
  isNewView: boolean;
}
