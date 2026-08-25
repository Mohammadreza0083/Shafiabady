/**
 * Apple-inspired Intersection Observer Animation Engine
 * Automatically attaches to all elements with `.reveal-on-scroll` class or `data-animate` attribute.
 */
export function initScrollAnimations(): void {
  if (typeof window === 'undefined' || !('IntersectionObserver' in window)) {
    // Fallback: reveal immediately if IntersectionObserver not supported
    document.querySelectorAll('.reveal-on-scroll').forEach(el => el.classList.add('revealed'));
    return;
  }

  const observerCallback: IntersectionObserverCallback = (entries, observer) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add('revealed');
        observer.unobserve(entry.target);
      }
    });
  };

  const observerOptions: IntersectionObserverInit = {
    root: null,
    rootMargin: '0px 0px -60px 0px',
    threshold: 0.1,
  };

  const observer = new IntersectionObserver(observerCallback, observerOptions);

  const observeElements = () => {
    const elements = document.querySelectorAll('.reveal-on-scroll:not(.revealed)');
    elements.forEach(el => observer.observe(el));
  };

  // Run on initial load
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', observeElements);
  } else {
    observeElements();
  }

  // Support for dynamic DOM changes (e.g., Infinite Scroll)
  window.addEventListener('content-loaded', observeElements);
}
