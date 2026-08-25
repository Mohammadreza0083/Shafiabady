export type ThemeMode = 'light' | 'dark' | 'auto';

/**
 * Checks whether current time in Iran Standard Time (IRST / UTC+3:30) is night (>= 18:00 or < 06:00).
 */
export function isIranNightTime(): boolean {
  try {
    const now = new Date();
    // Using Intl API for precise Asia/Tehran timezone formatting
    const tehranTimeString = new Intl.DateTimeFormat('en-US', {
      timeZone: 'Asia/Tehran',
      hour: 'numeric',
      hour12: false,
    }).format(now);

    const tehranHour = parseInt(tehranTimeString, 10);
    // 18:00 (6 PM) to 06:00 (6 AM) is dark mode
    return tehranHour >= 18 || tehranHour < 6;
  } catch {
    // Fallback manual UTC+3:30 calculation
    const now = new Date();
    const utcHours = now.getUTCHours();
    const utcMinutes = now.getUTCMinutes();
    const iranTotalMinutes = utcHours * 60 + utcMinutes + 210; // +3.5 hours = +210 mins
    const iranHour = Math.floor((iranTotalMinutes % 1440 + 1440) % 1440 / 60);
    return iranHour >= 18 || iranHour < 6;
  }
}

/**
 * Determines current theme based on user stored preference or Iran time.
 */
export function getActiveTheme(): 'light' | 'dark' {
  if (typeof window === 'undefined') return 'light';
  
  const saved = localStorage.getItem('theme') as ThemeMode | null;
  if (saved === 'dark') return 'dark';
  if (saved === 'light') return 'light';

  // If no explicit manual choice is set, apply IRST 18:00 auto-dark rule
  return isIranNightTime() ? 'dark' : 'light';
}

/**
 * Applies the given theme to documentElement and saves choice if requested.
 */
export function applyTheme(theme: 'light' | 'dark', manualChoice?: ThemeMode): void {
  if (typeof document === 'undefined') return;

  const root = document.documentElement;
  if (theme === 'dark') {
    root.classList.add('dark');
  } else {
    root.classList.remove('dark');
  }

  if (manualChoice) {
    if (manualChoice === 'auto') {
      localStorage.removeItem('theme');
    } else {
      localStorage.setItem('theme', manualChoice);
    }
  }

  // Dispatch custom event for reactive UI
  window.dispatchEvent(new CustomEvent('theme-changed', { detail: { theme } }));
}

/**
 * Toggles theme between light and dark manually.
 */
export function toggleTheme(): 'light' | 'dark' {
  const current = document.documentElement.classList.contains('dark') ? 'dark' : 'light';
  const next = current === 'dark' ? 'light' : 'dark';
  applyTheme(next, next);
  return next;
}
