import { getApiBaseUrl } from './api';
import { showToast } from './ui-dialog';

const MAX_IMAGE_BYTES = 5 * 1024 * 1024;
const ACCEPTED_TYPES = 'image/jpeg,image/png,image/webp,image/gif';

/**
 * Uploads an image file to the backend (/api/Uploads/image) and resolves with its public URL.
 * Throws an Error with a user-facing Persian message on failure.
 */
export async function uploadImage(file: File): Promise<string> {
  if (!file.type.startsWith('image/')) {
    throw new Error('فقط فایل‌های تصویری قابل آپلود هستند.');
  }
  if (file.size > MAX_IMAGE_BYTES) {
    throw new Error('حجم تصویر نباید بیشتر از ۵ مگابایت باشد.');
  }

  const form = new FormData();
  form.append('file', file);

  let response: Response;
  try {
    // Content-Type is intentionally omitted so the browser sets the multipart boundary
    response = await fetch(`${getApiBaseUrl()}/Uploads/image`, {
      method: 'POST',
      body: form,
      credentials: 'include',
    });
  } catch {
    throw new Error('خطا در برقراری ارتباط با سرور برای آپلود تصویر.');
  }

  const json = await response.json().catch(() => null);
  if (response.status === 401) {
    throw new Error('نشست شما منقضی شده است. لطفاً دوباره وارد شوید.');
  }
  if (!response.ok || !json?.isSuccess || !json?.data?.url) {
    throw new Error(json?.errorMessage || 'آپلود تصویر با خطا مواجه شد.');
  }
  return json.data.url as string;
}

export interface ImageDialogResult {
  url: string;
  alt: string;
}

/**
 * Opens a themed modal for choosing an image: drag-and-drop / pick / paste a file (uploaded immediately)
 * or enter an image URL, with live preview and optional alt text. Resolves with the result, or null if cancelled.
 */
export function showImageDialog(options: { title?: string; confirmText?: string; withAlt?: boolean } = {}): Promise<ImageDialogResult | null> {
  return new Promise((resolve) => {
    const title = options.title || 'درج تصویر';
    const confirmText = options.confirmText || 'درج تصویر';
    const tabBase = 'flex-1 py-2 rounded-full text-xs font-semibold transition-all';
    const tabActive = 'bg-white dark:bg-neutral-700 text-neutral-900 dark:text-white shadow-sm';
    const tabIdle = 'text-neutral-500 dark:text-neutral-400 hover:text-neutral-800 dark:hover:text-neutral-200';
    const field = 'w-full px-3.5 py-2.5 rounded-xl bg-neutral-50 dark:bg-neutral-800 border border-black/5 dark:border-white/10 text-sm focus:ring-2 focus:ring-apple-blue outline-none';

    const overlay = document.createElement('div');
    overlay.className = 'fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 dark:bg-black/60 backdrop-blur-md opacity-0 transition-opacity duration-200';
    overlay.setAttribute('role', 'dialog');
    overlay.setAttribute('aria-modal', 'true');
    overlay.innerHTML = `
      <div class="image-dialog-box w-full max-w-md rounded-3xl bg-white dark:bg-neutral-900 border border-black/10 dark:border-white/10 shadow-2xl p-6 space-y-4 transform scale-95 opacity-0 transition-all duration-200">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <div class="w-8 h-8 rounded-xl bg-apple-blue/10 text-apple-blue flex items-center justify-center">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
            </div>
            <h3 class="text-base font-bold text-neutral-900 dark:text-white">${title}</h3>
          </div>
          <button type="button" data-action="cancel" aria-label="بستن" class="p-1.5 rounded-lg text-neutral-400 hover:text-neutral-700 dark:hover:text-neutral-200 hover:bg-black/5 dark:hover:bg-white/5">
            <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
          </button>
        </div>

        <div class="flex gap-1 p-1 rounded-full bg-neutral-100 dark:bg-neutral-800">
          <button type="button" data-tab="upload" class="${tabBase} ${tabActive}">آپلود از دستگاه</button>
          <button type="button" data-tab="url" class="${tabBase} ${tabIdle}">از آدرس اینترنتی</button>
        </div>

        <div data-panel="upload">
          <label class="image-dropzone flex flex-col items-center justify-center gap-2 h-40 rounded-2xl border-2 border-dashed border-black/10 dark:border-white/15 bg-neutral-50 dark:bg-neutral-800/60 text-neutral-500 dark:text-neutral-400 cursor-pointer hover:border-apple-blue hover:text-apple-blue transition-colors text-center px-4">
            <svg class="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M4 16v2a2 2 0 002 2h12a2 2 0 002-2v-2M12 4v12m0-12l-4 4m4-4l4 4" /></svg>
            <span class="text-xs font-semibold">تصویر را اینجا رها کنید یا برای انتخاب کلیک کنید</span>
            <span class="text-[11px] opacity-75">JPEG، PNG، WebP یا GIF — حداکثر ۵ مگابایت (پیست هم پشتیبانی می‌شود)</span>
            <input type="file" accept="${ACCEPTED_TYPES}" class="image-dialog-file sr-only" aria-label="انتخاب فایل تصویر" />
          </label>
        </div>

        <div data-panel="url" class="hidden">
          <input type="url" dir="ltr" placeholder="https://..." class="image-dialog-url ${field} font-mono" aria-label="آدرس تصویر" />
        </div>

        <div class="image-dialog-preview hidden relative rounded-2xl overflow-hidden bg-neutral-100 dark:bg-neutral-800 border border-black/5 dark:border-white/10">
          <img class="w-full max-h-56 object-contain" alt="پیش‌نمایش" />
          <div class="image-dialog-busy hidden absolute inset-0 flex items-center justify-center gap-2 bg-white/70 dark:bg-neutral-900/70 text-xs font-semibold text-neutral-700 dark:text-neutral-200">
            <svg class="animate-spin w-4 h-4 text-apple-blue" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path></svg>
            <span>در حال آپلود...</span>
          </div>
        </div>

        <div class="image-dialog-error hidden p-3 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-600 dark:text-rose-400 text-xs leading-relaxed"></div>

        ${options.withAlt ? `
        <div>
          <label class="block text-xs font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">متن جایگزین تصویر (برای سئو و دسترس‌پذیری)</label>
          <input type="text" class="image-dialog-alt ${field}" placeholder="توضیح کوتاه تصویر" />
        </div>` : ''}

        <div class="flex items-center justify-end gap-2 pt-1">
          <button type="button" data-action="cancel" class="px-4 py-2 rounded-full text-xs font-semibold text-neutral-600 dark:text-neutral-400 hover:bg-black/5 dark:hover:bg-white/5">انصراف</button>
          <button type="button" data-action="confirm" disabled class="px-5 py-2 rounded-full bg-apple-blue hover:bg-blue-600 text-white text-xs font-bold shadow-md shadow-apple-blue/25 transition-all disabled:opacity-40 disabled:cursor-not-allowed">${confirmText}</button>
        </div>
      </div>
    `;
    document.body.appendChild(overlay);

    const q = <T extends Element>(sel: string) => overlay.querySelector(sel) as T;
    const box = q<HTMLElement>('.image-dialog-box');
    const fileInput = q<HTMLInputElement>('.image-dialog-file');
    const urlInput = q<HTMLInputElement>('.image-dialog-url');
    const altInput = q<HTMLInputElement | null>('.image-dialog-alt');
    const dropzone = q<HTMLElement>('.image-dropzone');
    const previewWrap = q<HTMLElement>('.image-dialog-preview');
    const previewImg = previewWrap.querySelector('img')!;
    const busy = q<HTMLElement>('.image-dialog-busy');
    const errorBox = q<HTMLElement>('.image-dialog-error');
    const confirmBtn = q<HTMLButtonElement>('[data-action="confirm"]');

    let currentUrl = '';
    let uploading = false;

    const setError = (msg = '') => {
      errorBox.textContent = msg;
      errorBox.classList.toggle('hidden', !msg);
    };
    const setUrl = (url: string) => {
      currentUrl = url;
      previewWrap.classList.toggle('hidden', !url);
      if (url) previewImg.src = url;
      confirmBtn.disabled = !url || uploading;
    };
    previewImg.addEventListener('error', () => {
      if (!currentUrl) return;
      setError('تصویر از این آدرس قابل نمایش نیست.');
      confirmBtn.disabled = true;
    });
    previewImg.addEventListener('load', () => {
      if (currentUrl && !uploading) {
        setError();
        confirmBtn.disabled = false;
      }
    });

    const handleFile = async (file: File | undefined | null) => {
      if (!file) return;
      setError();
      uploading = true;
      confirmBtn.disabled = true;
      previewWrap.classList.remove('hidden');
      previewImg.src = URL.createObjectURL(file);
      busy.classList.remove('hidden');
      if (altInput && !altInput.value) altInput.value = file.name.replace(/\.[^.]+$/, '');
      try {
        const url = await uploadImage(file);
        uploading = false;
        setUrl(url);
      } catch (err: any) {
        uploading = false;
        setUrl('');
        setError(err?.message || 'آپلود تصویر با خطا مواجه شد.');
      } finally {
        busy.classList.add('hidden');
      }
    };

    // Tabs
    overlay.querySelectorAll<HTMLButtonElement>('[data-tab]').forEach((tab) => {
      tab.addEventListener('click', () => {
        overlay.querySelectorAll<HTMLButtonElement>('[data-tab]').forEach((t) => {
          const active = t === tab;
          t.className = `${tabBase} ${active ? tabActive : tabIdle}`;
          q<HTMLElement>(`[data-panel="${t.dataset.tab}"]`).classList.toggle('hidden', !active);
        });
        setError();
        if (tab.dataset.tab === 'url') urlInput.focus();
      });
    });

    fileInput.addEventListener('change', () => handleFile(fileInput.files?.[0]));
    dropzone.addEventListener('dragover', (e) => {
      e.preventDefault();
      dropzone.classList.add('border-apple-blue', 'text-apple-blue');
    });
    dropzone.addEventListener('dragleave', () => dropzone.classList.remove('border-apple-blue', 'text-apple-blue'));
    dropzone.addEventListener('drop', (e) => {
      e.preventDefault();
      dropzone.classList.remove('border-apple-blue', 'text-apple-blue');
      handleFile(Array.from(e.dataTransfer?.files || []).find((f) => f.type.startsWith('image/')));
    });
    overlay.addEventListener('paste', (e) => {
      const file = Array.from(e.clipboardData?.files || []).find((f) => f.type.startsWith('image/'));
      if (file) {
        e.preventDefault();
        handleFile(file);
      }
    });

    urlInput.addEventListener('input', () => {
      const url = urlInput.value.trim();
      setError();
      if (!url) return setUrl('');
      if (!/^https?:\/\//i.test(url)) {
        setUrl('');
        setError('آدرس تصویر باید با http:// یا https:// شروع شود.');
        return;
      }
      setUrl(url);
      confirmBtn.disabled = true; // enabled once the preview actually loads
    });

    const close = (result: ImageDialogResult | null) => {
      document.removeEventListener('keydown', onKey);
      overlay.classList.add('opacity-0');
      box.classList.add('scale-95', 'opacity-0');
      setTimeout(() => {
        overlay.remove();
        resolve(result);
      }, 200);
    };
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') close(null);
      if (e.key === 'Enter' && !confirmBtn.disabled && document.activeElement?.tagName === 'INPUT') {
        e.preventDefault();
        confirmBtn.click();
      }
    };
    document.addEventListener('keydown', onKey);
    overlay.addEventListener('click', (e) => {
      if (e.target === overlay) close(null);
    });
    overlay.querySelectorAll('[data-action="cancel"]').forEach((b) => b.addEventListener('click', () => close(null)));
    confirmBtn.addEventListener('click', () => {
      if (currentUrl) close({ url: currentUrl, alt: altInput?.value.trim() || '' });
    });

    requestAnimationFrame(() => {
      overlay.classList.remove('opacity-0');
      box.classList.remove('scale-95', 'opacity-0');
    });
  });
}

/**
 * Adds an "upload image" button and a live preview beneath an image-URL text input,
 * so admins can either paste a URL or upload a file from their device.
 */
export function enhanceImageUrlInput(input: HTMLInputElement | null) {
  if (!input || input.dataset.uploadEnhanced) return;
  input.dataset.uploadEnhanced = 'true';

  const row = document.createElement('div');
  row.className = 'mt-2 flex items-center gap-3';
  row.innerHTML = `
    <button type="button" class="image-upload-btn inline-flex items-center gap-1.5 px-3 py-2 rounded-xl text-xs font-semibold bg-apple-blue/10 text-apple-blue hover:bg-apple-blue/20 transition-colors">
      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v2a2 2 0 002 2h12a2 2 0 002-2v-2M12 4v12m0-12l-4 4m4-4l4 4" /></svg>
      <span>انتخاب یا آپلود تصویر</span>
    </button>
    <img class="image-upload-preview hidden w-14 h-14 rounded-xl object-cover border border-black/5 dark:border-white/10" alt="پیش‌نمایش تصویر" />
  `;
  input.insertAdjacentElement('afterend', row);

  const button = row.querySelector<HTMLButtonElement>('.image-upload-btn')!;
  const preview = row.querySelector<HTMLImageElement>('.image-upload-preview')!;

  const refreshPreview = () => {
    const url = input.value.trim();
    preview.classList.toggle('hidden', !url);
    if (url) preview.src = url;
  };
  input.addEventListener('input', refreshPreview);
  preview.addEventListener('error', () => preview.classList.add('hidden'));
  refreshPreview();

  button.addEventListener('click', async () => {
    const result = await showImageDialog({ title: 'انتخاب تصویر کاور', confirmText: 'تأیید تصویر' });
    if (!result) return;
    input.value = result.url;
    input.dispatchEvent(new Event('input', { bubbles: true }));
    showToast('تصویر کاور تنظیم شد.', 'success');
  });
}
