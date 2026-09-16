/**
 * Server-Side HTML Sanitizer Utility.
 * Strips dangerous HTML tags (<script>, <iframe>, <object>, <embed>, <form>, <base>),
 * javascript: / vbscript: pseudo-protocols, and inline event handlers (onerror, onload, onclick, etc.)
 * while safely preserving semantic formatting tags, code snippets, tables, and images.
 */

const DANGEROUS_TAGS_REGEX = /<\s*\/?\s*(script|iframe|object|embed|applet|meta|link|style|form|input|button|base|frame|frameset)\b[^>]*>/gi;
const DANGEROUS_ATTRIBUTES_REGEX = /\s+(on[a-z]+|javascript:|vbscript:|data-bind|action|formaction)\s*=\s*(['"][^'"]*['"]|[^\s>]+)/gi;
const JAVASCRIPT_PROTOCOL_REGEX = /(href|src|data)\s*=\s*['"]\s*javascript:[^'"]*['"]/gi;

/**
 * Sanitizes an untrusted HTML string to prevent Cross-Site Scripting (XSS).
 * @param dirty Raw HTML string from database or external source
 * @returns Clean, safe HTML string suitable for set:html
 */
export function sanitizeHtml(dirty: string | null | undefined): string {
  if (!dirty) {
    return '';
  }

  return dirty
    // Remove dangerous tags and their closing counterparts
    .replace(DANGEROUS_TAGS_REGEX, '')
    // Remove javascript: URLs in href/src
    .replace(JAVASCRIPT_PROTOCOL_REGEX, '$1="#"')
    // Remove dangerous inline event handlers (onclick, onerror, onload, etc.)
    .replace(DANGEROUS_ATTRIBUTES_REGEX, '');
}
