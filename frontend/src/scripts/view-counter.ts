/**
 * View Counter Engine with Client & Server Rate Limiting
 * Connects to .NET 9 Backend POST /api/Views/record endpoint.
 */

import { viewsApi } from '../utils/api';
import { EntityType } from '../types';

export interface ViewRecord {
  timestamp: number;
}

const STORAGE_KEY = 'entity_views_cache';
const TWENTY_FOUR_HOURS_MS = 24 * 60 * 60 * 1000;

function resolveEntityType(type: 'article' | 'book' | 'course' | EntityType | number): EntityType {
  if (typeof type === 'number') return type as EntityType;
  switch (type) {
    case 'article':
      return EntityType.Article;
    case 'book':
      return EntityType.Book;
    case 'course':
      return EntityType.Course;
    default:
      return EntityType.Article;
  }
}

/**
 * Tracks a view for an entity (Article, Book, Course) with client-side localStorage
 * and backend IP-based 24-hour rate limiting.
 */
export async function trackView(
  entityId: string,
  entityType: 'article' | 'book' | 'course' | EntityType | number
): Promise<{ viewsCount: number; isNew: boolean }> {
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
  const isEligibleForNewView = now - lastViewTime >= TWENTY_FOUR_HOURS_MS;

  try {
    const typeEnum = resolveEntityType(entityType);

    // Call .NET 9 Backend POST /api/Views/record
    const result = await viewsApi.record(typeEnum, entityId);

    if (result.isSuccess) {
      const isRecorded = result.data === true;
      if (isRecorded || isEligibleForNewView) {
        cache[entityId] = now;
        try {
          localStorage.setItem(STORAGE_KEY, JSON.stringify(cache));
        } catch {}
      }

      return {
        viewsCount: 0,
        isNew: isRecorded,
      };
    }
  } catch (error) {
    console.warn('Failed to record view on backend:', error);
  }

  return { viewsCount: 0, isNew: false };
}
