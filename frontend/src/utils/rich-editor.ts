import { showImageDialog, uploadImage } from './image-upload';
import { showToast } from './ui-dialog';

const IMAGE_CLASS = 'rounded-2xl my-4 max-w-full h-auto mx-auto block';

function escapeAttr(value: string): string {
  return value.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

/**
 * Wires the RichTextToolbar buttons to a contenteditable editor: formatting (H1–H3, paragraph, quote, lists, code),
 * links, and inserting images at the caret via upload, URL, clipboard paste, or drag-and-drop.
 */
export function initRichEditor(editor: HTMLElement | null) {
  if (!editor) return;

  // Remember the caret so toolbar clicks and file dialogs (which steal focus) insert at the right place
  let savedRange: Range | null = null;
  document.addEventListener('selectionchange', () => {
    const sel = window.getSelection();
    if (sel && sel.rangeCount > 0 && editor.contains(sel.getRangeAt(0).commonAncestorContainer)) {
      savedRange = sel.getRangeAt(0).cloneRange();
    }
  });

  const restoreSelection = () => {
    editor.focus();
    const sel = window.getSelection();
    if (!sel) return;
    sel.removeAllRanges();
    if (savedRange) {
      sel.addRange(savedRange);
    } else {
      const range = document.createRange();
      range.selectNodeContents(editor);
      range.collapse(false);
      sel.addRange(range);
    }
  };

  const insertImage = (url: string, alt = '') => {
    restoreSelection();
    document.execCommand(
      'insertHTML',
      false,
      `<img src="${escapeAttr(url)}" alt="${escapeAttr(alt)}" class="${IMAGE_CLASS}" /><p><br></p>`
    );
  };

  const uploadAndInsert = async (file: File) => {
    showToast('در حال آپلود تصویر...', 'info', 2000);
    try {
      const url = await uploadImage(file);
      insertImage(url, file.name.replace(/\.[^.]+$/, ''));
      showToast('تصویر در متن درج شد.', 'success');
    } catch (err: any) {
      showToast(err?.message || 'آپلود تصویر با خطا مواجه شد.', 'error');
    }
  };

  document.querySelectorAll<HTMLButtonElement>('#editor-toolbar .toolbar-btn[data-command]').forEach((btn) => {
    // mousedown + preventDefault keeps the editor selection intact while clicking the toolbar
    btn.addEventListener('mousedown', (e) => e.preventDefault());
    btn.addEventListener('click', () => {
      const command = btn.dataset.command!;
      const val = btn.dataset.val ? `<${btn.dataset.val}>` : '';
      restoreSelection();
      document.execCommand(command, false, val);
    });
  });

  document.getElementById('btn-insert-link')?.addEventListener('click', () => {
    const url = prompt('آدرس اینترنتی لینک را وارد کنید (https://...):');
    if (url) {
      restoreSelection();
      document.execCommand('createLink', false, url);
    }
  });

  document.getElementById('btn-insert-image')?.addEventListener('click', async () => {
    const result = await showImageDialog({ withAlt: true });
    if (result) insertImage(result.url, result.alt);
  });

  editor.addEventListener('paste', (e) => {
    const file = Array.from(e.clipboardData?.files || []).find((f) => f.type.startsWith('image/'));
    if (file) {
      e.preventDefault();
      uploadAndInsert(file);
    }
  });

  editor.addEventListener('dragover', (e) => {
    if (e.dataTransfer?.types.includes('Files')) e.preventDefault();
  });
  editor.addEventListener('drop', (e) => {
    const file = Array.from(e.dataTransfer?.files || []).find((f) => f.type.startsWith('image/'));
    if (!file) return;
    e.preventDefault();
    // Move the caret to the drop point so the image lands where it was dropped
    const range = (document as any).caretRangeFromPoint?.(e.clientX, e.clientY) as Range | undefined;
    if (range) savedRange = range;
    uploadAndInsert(file);
  });
}
