/** @type {import('tailwindcss').Config} */
export default {
  content: ['./src/**/*.{astro,html,js,jsx,md,mdx,svelte,ts,tsx,vue}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        apple: {
          bg: '#FFFFFF',
          'bg-dark': '#000000',
          surface: '#F5F5F7',
          'surface-dark': '#121212',
          card: '#FFFFFF',
          'card-dark': '#1C1C1E',
          border: 'rgba(0, 0, 0, 0.08)',
          'border-dark': 'rgba(255, 255, 255, 0.12)',
          blue: {
            DEFAULT: '#0071E3',
            hover: '#0077ED',
            light: '#E8F2FF',
            dark: '#0A84FF',
          },
          gray: {
            50: '#F5F5F7',
            100: '#E8E8ED',
            200: '#D2D2D7',
            300: '#86868B',
            400: '#6E6E73',
            500: '#1D1D1F',
          }
        }
      },
      fontFamily: {
        sans: ['Vazirmatn', '-apple-system', 'BlinkMacSystemFont', 'SF Pro Text', 'Helvetica Neue', 'sans-serif'],
      },
      boxShadow: {
        'apple-sm': '0 2px 8px rgba(0, 0, 0, 0.04)',
        'apple-md': '0 4px 20px rgba(0, 0, 0, 0.08)',
        'apple-lg': '0 12px 32px rgba(0, 0, 0, 0.12)',
        'apple-glow': '0 0 30px rgba(0, 113, 227, 0.25)',
      },
      backdropBlur: {
        'apple': '20px',
      },
      animation: {
        'fade-in': 'fadeIn 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards',
        'slide-up': 'slideUp 0.7s cubic-bezier(0.16, 1, 0.3, 1) forwards',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideUp: {
          '0%': { opacity: '0', transform: 'translateY(24px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        }
      }
    },
  },
  plugins: [],
};
