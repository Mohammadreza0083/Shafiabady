// Single source for contact channels and profile links used by the footer, contact page, CTA blocks and JSON-LD.

export const contactEmail = 'mhshafiaabadi@gmail.com';

export interface SocialLink {
  id: 'email' | 'linkedin' | 'github' | 'instagram';
  label: string;
  href: string;
}

export const socialLinks: SocialLink[] = [
  { id: 'email', label: 'ایمیل', href: `mailto:${contactEmail}` },
  { id: 'instagram', label: 'اینستاگرام', href: 'https://www.instagram.com/shafiabadi.ir' },
  { id: 'linkedin', label: 'لینکدین', href: 'https://www.linkedin.com/in/mohammad-hossein-shafiabadi' },
  { id: 'github', label: 'گیت‌هاب', href: 'https://github.com/mhshafiabadi' },
];

// Public profile URLs for schema.org `sameAs`
export const profileUrls = socialLinks.filter((link) => link.id !== 'email').map((link) => link.href);
