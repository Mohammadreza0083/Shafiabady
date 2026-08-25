export interface ViewRecord {
  timestamp: number;
}

const STORAGE_KEY = 'entity_views_cache';
const TWENTY_FOUR_HOURS_MS = 24 * 60 * 60 * 1000;

/**
 * Tracks a view for an entity (Article, Book, Course) with a client-side and server-side
 * 24-hour rate limit to ensure 1 view per 24 hours per user.
 */
export async function trackView(entityId: string, entityType: 'article' | 'book' | 'course'): Promise<{ viewsCount: number; isNew: boolean }> {
  if (typeof window === 'undefined' || !entityId) {
    return { viewsCount: 0, isNew: false };
  }

  const now = Date.now();
  let cache: Record<string, number> = {};

  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) {
      cache = JSON.parse(raw);
    }
  } catch (e) {
    cache = {};
  }

  const lastViewTime = cache[entityId] || 0;
  const isEligibleForNewView = (now - lastViewTime) >= TWENTY_FOUR_HOURS_MS;

  try {
    const response = await fetch('/api/views', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        entityId,
        entityType,
        increment: isEligibleForNewView,
      }),
    });

    if (response.ok) {
      const data = await response.json();
      if (isEligibleForNewView && data.isNewView) {
        cache[entityId] = now;
        localStorage.setItem(STORAGE_KEY, JSON.stringify(cache));
      }
      return { viewsCount: data.viewsCount, isNew: data.isNewView };
    }
  } catch (error) {
    console.error('Failed to update view counter:', error);
  }

  return { viewsCount: 0, isNew: false };
}
