/**
 * Infinite Scroll Engine for Articles Archive
 * Uses IntersectionObserver on sentinel to dynamically fetch and append paginated articles.
 */

export function setupInfiniteScroll({
  sentinelId = 'scroll-sentinel',
  containerId = 'articles-grid',
  loadingId = 'scroll-loading',
  endMessageId = 'scroll-end',
  initialPage = 1,
  limit = 3,
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

  let currentPage = initialPage;
  let isLoading = false;
  let hasMore = true;

  const renderCardHTML = (article: any) => {
    return `
      <article class="group relative apple-card overflow-hidden flex flex-col justify-between reveal-on-scroll revealed">
        <div class="relative overflow-hidden aspect-[16/10] bg-neutral-100 dark:bg-neutral-800">
          <div class="w-full h-full flex items-center justify-center bg-gradient-to-br from-blue-500/10 via-sky-400/5 to-purple-500/10 p-6">
            <div class="text-apple-blue dark:text-apple-blue-dark font-bold text-4xl opacity-40">
              ${article.category}
            </div>
          </div>
          <div class="absolute top-4 right-4 z-10">
            <span class="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-white/90 dark:bg-black/90 backdrop-blur-md text-apple-blue dark:text-apple-blue-dark shadow-sm border border-black/5 dark:border-white/10">
              ${article.category}
            </span>
          </div>
          <div class="absolute bottom-4 left-4 z-10">
            <span class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-medium bg-black/60 backdrop-blur-md text-white shadow-sm">
              <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>${article.readTime}</span>
            </span>
          </div>
        </div>
        <div class="p-6 sm:p-7 flex flex-col justify-between flex-grow">
          <div>
            <div class="flex items-center gap-2 text-xs text-neutral-400 dark:text-neutral-500 mb-3">
              <span>${new Date(article.createdAt).toLocaleDateString('fa-IR')}</span>
              <span>•</span>
              <span class="flex items-center gap-1">
                <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                <span>${article.viewsCount} بازدید</span>
              </span>
            </div>
            <h3 class="text-xl sm:text-2xl font-bold text-neutral-900 dark:text-white group-hover:text-apple-blue dark:group-hover:text-apple-blue-dark transition-colors duration-200 leading-snug mb-3">
              <a href="/articles/${article.slug}" class="focus:outline-none">
                <span class="absolute inset-0 z-0"></span>
                ${article.title}
              </a>
            </h3>
            <p class="text-sm text-neutral-600 dark:text-neutral-400 line-clamp-3 leading-relaxed mb-4">
              ${article.summary}
            </p>
          </div>
          <div class="pt-4 border-t border-black/5 dark:border-white/5 flex items-center justify-between mt-auto">
            <div class="flex items-center gap-2">
              <div class="w-7 h-7 rounded-full bg-neutral-200 dark:bg-neutral-700 flex items-center justify-center text-xs font-semibold text-neutral-700 dark:text-neutral-300">
                ${article.author?.name ? article.author.name[0] : 'ش'}
              </div>
              <span class="text-xs text-neutral-600 dark:text-neutral-400 font-medium">
                ${article.author?.name || 'دکتر محمد حسین شفیعآبادی'}
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
      const response = await fetch(`/api/articles?page=${nextPage}&limit=${limit}`);
      if (!response.ok) throw new Error('Network error');

      const data = await response.json();
      currentPage = nextPage;
      hasMore = data.hasMore;

      if (data.data && data.data.length > 0) {
        data.data.forEach((article: any) => {
          const wrapper = document.createElement('div');
          wrapper.innerHTML = renderCardHTML(article).trim();
          if (wrapper.firstElementChild) {
            container.appendChild(wrapper.firstElementChild);
          }
        });

        // Trigger animations for new DOM elements
        window.dispatchEvent(new CustomEvent('content-loaded'));
      }

      if (!hasMore) {
        if (loading) loading.classList.add('hidden');
        if (endMessage) endMessage.classList.remove('hidden');
        observer.disconnect();
      }
    } catch (error) {
      console.error('Failed to load more articles:', error);
    } finally {
      isLoading = false;
      if (hasMore && loading) loading.classList.add('hidden');
    }
  };

  const observer = new IntersectionObserver(
    (entries) => {
      if (entries[0].isIntersecting && hasMore && !isLoading) {
        loadMoreArticles();
      }
    },
    { rootMargin: '200px' }
  );

  observer.observe(sentinel);
}
