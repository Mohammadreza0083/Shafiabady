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
  telegramId?: string;
  message: string;
}

export interface ViewCounterResponse {
  entityId: string;
  viewsCount: number;
  isNewView: boolean;
}
