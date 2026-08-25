/**
 * Infinite Scroll Engine for Articles Archive
 * Connects to .NET 9 Backend GET /api/Articles endpoint with pagination and error handling.
 */

import { articlesApi } from '../utils/api';
import type { ArticleDto } from '../types';

export function setupInfiniteScroll({
  sentinelId = 'scroll-sentinel',
  containerId = 'articles-grid',
  loadingId = 'scroll-loading',
  endMessageId = 'scroll-end',
  initialPage = 1,
  limit = 10,
}: {
  sentinelId?: string;
  containerId?: string;
  loadingId?: string;
  endMessageId?: string;
  initialPage?: number;
  limit?: number;
} = {}): void {
  const sentinel = document.getElementById(sentinelId);
  const container = document.getElementById(containerId);
  const loading = document.getElementById(loadingId);
  const endMessage = document.getElementById(endMessageId);

  if (!sentinel || !container) return;

  // Read initial configuration from container data attributes
  const hasMoreAttr = container.getAttribute('data-has-more');
  const pageSizeAttr = container.getAttribute('data-page-size');
  const effectiveLimit = pageSizeAttr ? parseInt(pageSizeAttr, 10) : limit;

  let currentPage = initialPage;
  let isLoading = false;
  let hasMore = hasMoreAttr !== 'false';

  // If there are no more pages initially, show end message and do not observe
  if (!hasMore) {
    if (loading) loading.classList.add('hidden');
    if (endMessage) endMessage.classList.remove('hidden');
    return;
  }

  const renderCardHTML = (article: ArticleDto) => {
    const title = article.title || '';
    const slug = article.slug || '';
    const summary = article.summary || '';
    const categoryName = article.categoryTitle || 'معماری نرم‌افزار';
    const readTime = article.readTimeMinutes ? `${article.readTimeMinutes} دقیقه` : '۵ دقیقه';
    const authorName = article.authorName || 'دکتر محمد حسین شفیع‌آبادی';
    const authorInitial = authorName ? authorName.trim().charAt(0) : 'ش';
    const dateFormatted = new Date(article.createdAt || Date.now()).toLocaleDateString('fa-IR');
    const viewsCount = article.viewsCount ?? 0;
    const viewsFormatted = viewsCount.toLocaleString('fa-IR');
    const thumbnailUrl = article.thumbnailUrl || '';

    return `
      <article class="group relative apple-card overflow-hidden flex flex-col justify-between reveal-on-scroll revealed transition-all duration-300 hover:shadow-xl hover:-translate-y-1">
        <!-- Card Image / Header -->
        <div class="relative overflow-hidden aspect-[16/10] bg-neutral-100 dark:bg-neutral-800">
          ${
            thumbnailUrl
              ? `<img
                  src="${thumbnailUrl}"
                  alt="تصویر مقاله ${title}"
                  width="600"
                  height="375"
                  loading="lazy"
                  decoding="async"
                  class="w-full h-full object-cover transition-transform duration-500 ease-out group-hover:scale-105"
                />`
              : `<div class="w-full h-full flex items-center justify-center bg-gradient-to-br from-blue-500/10 via-sky-400/5 to-purple-500/10 p-6">
                  <div class="text-apple-blue dark:text-apple-blue-dark font-bold text-4xl opacity-40">
                    ${categoryName}
                  </div>
                </div>`
          }

          <!-- Category Pill Badge -->
          <div class="absolute top-4 right-4 z-10">
            <span class="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-white/90 dark:bg-black/90 backdrop-blur-md text-apple-blue dark:text-apple-blue-dark shadow-sm border border-black/5 dark:border-white/10">
              ${categoryName}
            </span>
          </div>

          <!-- Reading Time Badge -->
          <div class="absolute bottom-4 left-4 z-10">
            <span class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-medium bg-black/60 backdrop-blur-md text-white shadow-sm">
              <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>${readTime}</span>
            </span>
          </div>
        </div>

        <!-- Card Body -->
        <div class="p-6 sm:p-7 flex flex-col justify-between flex-grow">
          <div>
            <div class="flex items-center gap-2 text-xs text-neutral-400 dark:text-neutral-500 mb-3">
              <span>${dateFormatted}</span>
              <span>•</span>
              <span class="flex items-center gap-1">
                <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                <span class="entity-views-count" data-entity-id="${article.id}">${viewsFormatted} بازدید</span>
              </span>
            </div>

            <h3 class="text-xl sm:text-2xl font-bold text-neutral-900 dark:text-white group-hover:text-apple-blue dark:group-hover:text-apple-blue-dark transition-colors duration-200 leading-snug mb-3">
              <a href="/articles/${slug}" class="focus:outline-none">
                <span class="absolute inset-0 z-0"></span>
                ${title}
              </a>
            </h3>

            <p class="text-sm text-neutral-600 dark:text-neutral-400 line-clamp-3 leading-relaxed mb-4">
              ${summary}
            </p>
          </div>

          <!-- Card Footer / Tags & Author -->
          <div class="pt-4 border-t border-black/5 dark:border-white/5 flex items-center justify-between mt-auto">
            <div class="flex items-center gap-2">
              <div class="w-7 h-7 rounded-full bg-neutral-200 dark:bg-neutral-700 flex items-center justify-center text-xs font-semibold text-neutral-700 dark:text-neutral-300">
                ${authorInitial}
              </div>
              <span class="text-xs text-neutral-600 dark:text-neutral-400 font-medium">
                ${authorName}
              </span>
            </div>

            <span class="text-xs font-semibold text-apple-blue dark:text-apple-blue-dark inline-flex items-center gap-1 group-hover:translate-x-[-2px] transition-transform">
              <span>مطالعه مقاله</span>
              <svg class="w-3.5 h-3.5 transform rotate-180" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </span>
          </div>
        </div>
      </article>
    `;
  };

  const loadMoreArticles = async () => {
    if (isLoading || !hasMore) return;
    isLoading = true;

    if (loading) loading.classList.remove('hidden');

    try {
      const nextPage = currentPage + 1;
      const result = await articlesApi.getPaged({ pageNumber: nextPage, pageSize: effectiveLimit });

      if (result.isSuccess && result.data) {
        currentPage = result.data.pageNumber;
        hasMore = result.data.hasNextPage;

        const items = result.data.items || [];
        if (items.length > 0) {
          items.forEach((article: ArticleDto) => {
            const temp = document.createElement('div');
            temp.innerHTML = renderCardHTML(article).trim();
            if (temp.firstElementChild) {
              container.appendChild(temp.firstElementChild);
            }
          });

          // Trigger smooth reveal animation for newly appended cards
          window.dispatchEvent(new CustomEvent('content-loaded'));
        }
      } else {
        hasMore = false;
      }

      if (!hasMore) {
        if (loading) loading.classList.add('hidden');
        if (endMessage) endMessage.classList.remove('hidden');
        observer.disconnect();
      }
    } catch (error) {
      console.warn('Failed to load more articles:', error);
      hasMore = false;
      if (loading) loading.classList.add('hidden');
      if (endMessage) endMessage.classList.remove('hidden');
      observer.disconnect();
    } finally {
      isLoading = false;
      if (hasMore && loading) loading.classList.add('hidden');
    }
  };

  const observer = new IntersectionObserver(
    (entries) => {
      const entry = entries[0];
      if (entry && entry.isIntersecting && hasMore && !isLoading) {
        loadMoreArticles();
      }
    },
    { rootMargin: '250px' }
  );

  observer.observe(sentinel);
}
