/**
 * Apple-Minimalist UI Toast & Modal Alert System
 * Provides sleek, glassmorphic toast notifications and modal confirmation dialogs.
 */

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface ToastOptions {
  message: string;
  type?: ToastType;
  durationMs?: number;
}

export interface ModalAlertOptions {
  title?: string;
  message: string;
  type?: ToastType;
  confirmText?: string;
}

export interface ModalConfirmOptions {
  title?: string;
  message: string;
  type?: 'warning' | 'danger' | 'info';
  confirmText?: string;
  cancelText?: string;
  isDestructive?: boolean;
}

/**
 * Shows an Apple-style floating toast notification.
 */
export function showToast(message: string, type: ToastType = 'info', durationMs: number = 3500): void {
  if (typeof document === 'undefined') return;

  let container = document.getElementById('apple-toast-container');
  if (!container) {
    container = document.createElement('div');
    container.id = 'apple-toast-container';
    container.className = 'fixed bottom-6 right-6 z-50 flex flex-col gap-3 pointer-events-none max-w-sm w-full px-4';
    document.body.appendChild(container);
  }

  const toast = document.createElement('div');
  toast.className = `pointer-events-auto flex items-center gap-3 p-4 rounded-2xl shadow-xl backdrop-blur-xl border text-xs font-semibold transform transition-all duration-300 translate-y-4 opacity-0 ${
    type === 'success'
      ? 'bg-white/90 dark:bg-neutral-900/90 text-emerald-700 dark:text-emerald-400 border-emerald-500/20 shadow-emerald-500/10'
      : type === 'error'
      ? 'bg-white/90 dark:bg-neutral-900/90 text-rose-700 dark:text-rose-400 border-rose-500/20 shadow-rose-500/10'
      : type === 'warning'
      ? 'bg-white/90 dark:bg-neutral-900/90 text-amber-700 dark:text-amber-400 border-amber-500/20 shadow-amber-500/10'
      : 'bg-white/90 dark:bg-neutral-900/90 text-neutral-800 dark:text-neutral-100 border-apple-blue/20 shadow-apple-blue/10'
  }`;

  const iconSvg =
    type === 'success'
      ? '<svg class="w-5 h-5 flex-shrink-0 text-emerald-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" /></svg>'
      : type === 'error'
      ? '<svg class="w-5 h-5 flex-shrink-0 text-rose-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>'
      : type === 'warning'
      ? '<svg class="w-5 h-5 flex-shrink-0 text-amber-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>'
      : '<svg class="w-5 h-5 flex-shrink-0 text-apple-blue" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" /></svg>';

  toast.innerHTML = `
    ${iconSvg}
    <span class="flex-1 leading-relaxed">${message}</span>
  `;

  container.appendChild(toast);

  // Trigger enter animation
  requestAnimationFrame(() => {
    toast.classList.remove('translate-y-4', 'opacity-0');
    toast.classList.add('translate-y-0', 'opacity-100');
  });

  setTimeout(() => {
    toast.classList.remove('translate-y-0', 'opacity-100');
    toast.classList.add('translate-y-4', 'opacity-0');
    setTimeout(() => toast.remove(), 300);
  }, durationMs);
}

/**
 * Shows an Apple-style centered modal alert dialog replacing native alert().
 */
export function showAlert(options: ModalAlertOptions | string): Promise<void> {
  return new Promise((resolve) => {
    if (typeof document === 'undefined') {
      resolve();
      return;
    }

    const opts: ModalAlertOptions = typeof options === 'string' ? { message: options } : options;
    const title = opts.title || (opts.type === 'error' ? 'خطا' : opts.type === 'success' ? 'موفقیت' : 'پیام سیستم');
    const confirmText = opts.confirmText || 'متوجه شدم';
    const type = opts.type || 'info';

    const modalOverlay = document.createElement('div');
    modalOverlay.className = 'fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 dark:bg-black/60 backdrop-blur-md opacity-0 transition-opacity duration-200';

    const iconSvg =
      type === 'success'
        ? '<div class="w-12 h-12 rounded-2xl bg-emerald-500/10 text-emerald-500 flex items-center justify-center mx-auto mb-3"><svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" /></svg></div>'
        : type === 'error'
        ? '<div class="w-12 h-12 rounded-2xl bg-rose-500/10 text-rose-500 flex items-center justify-center mx-auto mb-3"><svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg></div>'
        : '<div class="w-12 h-12 rounded-2xl bg-apple-blue/10 text-apple-blue flex items-center justify-center mx-auto mb-3"><svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" /></svg></div>';

    modalOverlay.innerHTML = `
      <div class="w-full max-w-sm rounded-3xl bg-white dark:bg-neutral-900 border border-black/10 dark:border-white/10 shadow-2xl p-6 text-center transform scale-95 opacity-0 transition-all duration-200 space-y-4">
        ${iconSvg}
        <h3 class="text-base font-bold text-neutral-900 dark:text-white">${title}</h3>
        <p class="text-xs text-neutral-600 dark:text-neutral-300 leading-relaxed">${opts.message}</p>
        <div class="pt-2">
          <button type="button" id="apple-modal-btn-ok" class="w-full py-2.5 rounded-full bg-apple-blue hover:bg-blue-600 text-white text-xs font-bold shadow-md shadow-apple-blue/25 transition-all">
            ${confirmText}
          </button>
        </div>
      </div>
    `;

    document.body.appendChild(modalOverlay);

    const modalBox = modalOverlay.firstElementChild as HTMLElement;

    requestAnimationFrame(() => {
      modalOverlay.classList.remove('opacity-0');
      modalOverlay.classList.add('opacity-100');
      modalBox?.classList.remove('scale-95', 'opacity-0');
      modalBox?.classList.add('scale-100', 'opacity-100');
    });

    const close = () => {
      modalOverlay.classList.remove('opacity-100');
      modalOverlay.classList.add('opacity-0');
      modalBox?.classList.remove('scale-100', 'opacity-100');
      modalBox?.classList.add('scale-95', 'opacity-0');
      setTimeout(() => {
        modalOverlay.remove();
        resolve();
      }, 200);
    };

    modalOverlay.querySelector('#apple-modal-btn-ok')?.addEventListener('click', close);
  });
}

/**
 * Shows an Apple-style centered modal confirmation dialog replacing native confirm().
 */
export function showConfirm(options: ModalConfirmOptions | string): Promise<boolean> {
  return new Promise((resolve) => {
    if (typeof document === 'undefined') {
      resolve(false);
      return;
    }

    const opts: ModalConfirmOptions = typeof options === 'string' ? { message: options } : options;
    const title = opts.title || 'تأیید عملیات';
    const confirmText = opts.confirmText || (opts.isDestructive ? 'بله، حذف شود' : 'تأیید');
    const cancelText = opts.cancelText || 'انصراف';
    const isDestructive = opts.isDestructive ?? true;

    const modalOverlay = document.createElement('div');
    modalOverlay.className = 'fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 dark:bg-black/60 backdrop-blur-md opacity-0 transition-opacity duration-200';

    const iconSvg = isDestructive
      ? '<div class="w-12 h-12 rounded-2xl bg-rose-500/10 text-rose-500 flex items-center justify-center mx-auto mb-3"><svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg></div>'
      : '<div class="w-12 h-12 rounded-2xl bg-amber-500/10 text-amber-500 flex items-center justify-center mx-auto mb-3"><svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg></div>';

    modalOverlay.innerHTML = `
      <div class="w-full max-w-sm rounded-3xl bg-white dark:bg-neutral-900 border border-black/10 dark:border-white/10 shadow-2xl p-6 text-center transform scale-95 opacity-0 transition-all duration-200 space-y-4">
        ${iconSvg}
        <h3 class="text-base font-bold text-neutral-900 dark:text-white">${title}</h3>
        <p class="text-xs text-neutral-600 dark:text-neutral-300 leading-relaxed">${opts.message}</p>
        <div class="flex items-center justify-center gap-3 pt-2">
          <button type="button" id="apple-confirm-btn-cancel" class="flex-1 py-2.5 rounded-full border border-black/10 dark:border-white/10 text-neutral-700 dark:text-neutral-300 text-xs font-semibold hover:bg-black/5 dark:hover:bg-white/5 transition-colors">
            ${cancelText}
          </button>
          <button type="button" id="apple-confirm-btn-ok" class="flex-1 py-2.5 rounded-full ${
            isDestructive
              ? 'bg-rose-500 hover:bg-rose-600 shadow-rose-500/25'
              : 'bg-apple-blue hover:bg-blue-600 shadow-apple-blue/25'
          } text-white text-xs font-bold shadow-md transition-all">
            ${confirmText}
          </button>
        </div>
      </div>
    `;

    document.body.appendChild(modalOverlay);

    const modalBox = modalOverlay.firstElementChild as HTMLElement;

    requestAnimationFrame(() => {
      modalOverlay.classList.remove('opacity-0');
      modalOverlay.classList.add('opacity-100');
      modalBox?.classList.remove('scale-95', 'opacity-0');
      modalBox?.classList.add('scale-100', 'opacity-100');
    });

    const cleanup = (result: boolean) => {
      modalOverlay.classList.remove('opacity-100');
      modalOverlay.classList.add('opacity-0');
      modalBox?.classList.remove('scale-100', 'opacity-100');
      modalBox?.classList.add('scale-95', 'opacity-0');
      setTimeout(() => {
        modalOverlay.remove();
        resolve(result);
      }, 200);
    };

    modalOverlay.querySelector('#apple-confirm-btn-ok')?.addEventListener('click', () => cleanup(true));
    modalOverlay.querySelector('#apple-confirm-btn-cancel')?.addEventListener('click', () => cleanup(false));
  });
}

// Attach to window object for ease of access across scripts
if (typeof window !== 'undefined') {
  (window as any).showToast = showToast;
  (window as any).showAlert = showAlert;
  (window as any).showConfirm = showConfirm;
}
