/**
 * Apple-inspired Responsive Staggered Intersection Observer Animation Engine
 * Responsive Staggering Logic:
 * - On Desktop (>= 768px): Staggers items in batches of 3 matching the 3-column grid (Col 0: 0ms, Col 1: 120ms, Col 2: 240ms).
 * - On Mobile (< 768px): Animates items sequentially 1-by-1 as each item scrolls into view without column delays.
 */

export function initScrollAnimations(): void {
  if (typeof window === 'undefined' || !('IntersectionObserver' in window)) {
    document.querySelectorAll('.reveal-on-scroll').forEach((el) => {
      (el as HTMLElement).classList.add('revealed');
    });
    return;
  }

  const observerCallback: IntersectionObserverCallback = (entries, observer) => {
    const isDesktop = window.innerWidth >= 768;

    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        const target = entry.target as HTMLElement;
        const parent = target.parentElement;

        if (parent && isDesktop) {
          // Calculate column index (0, 1, 2) based on child position in grid
          const siblings = Array.from(parent.querySelectorAll('.reveal-on-scroll'));
          const itemIndex = siblings.indexOf(target);
          if (itemIndex !== -1) {
            const columnIndex = itemIndex % 3;
            target.style.transitionDelay = `${columnIndex * 120}ms`;
          }
        } else {
          // On mobile, trigger immediately with smooth 1-by-1 entry as each element intersects
          target.style.transitionDelay = '0ms';
        }

        target.classList.add('revealed');
        observer.unobserve(target);
      }
    });
  };

  const observerOptions: IntersectionObserverInit = {
    root: null,
    rootMargin: '0px 0px -40px 0px',
    threshold: 0.08,
  };

  const observer = new IntersectionObserver(observerCallback, observerOptions);

  const observeElements = () => {
    const elements = document.querySelectorAll('.reveal-on-scroll:not(.revealed)');
    elements.forEach((el) => observer.observe(el));
  };

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', observeElements);
  } else {
    observeElements();
  }

  document.addEventListener('astro:page-load', observeElements);
  window.addEventListener('content-loaded', observeElements);
}
