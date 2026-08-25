# Personal Branding & Educational Platform

> A modern, high-performance personal branding and educational platform featuring an Apple-inspired minimalist design, Clean Architecture, and seamless Telegram integration.

## 🌟 Overview & Architecture
- **Frontend:** [Astro.js](https://astro.build/) in Hybrid mode (`output: 'hybrid'`), [TailwindCSS](https://tailwindcss.com/), Vazirmatn Persian typography, Swiper.js Hero slider.
- **Backend API (Phase 2):** .NET 8/9 Web API implementing Clean Architecture, DDD, TDD, EF Core, SQL Server.
- **Telegram Bot Service (Phase 3):** Python microservice (`python-telegram-bot`) for Contact Us notifications and interactive management.
- **Infrastructure:** Docker, Nginx.

## 🚀 Key Features (Frontend - Phase 1)
- **Apple-Inspired Design Language:** Ultra-clean layouts, generous whitespace, frosted glass (`backdrop-blur`) effects, crisp typography, and vivid Apple Royal Blue accents (`#0071E3`).
- **RTL & Persian Typography:** Native Right-to-Left (RTL) support with the Google Font `Vazirmatn`.
- **Smart IRST Dark Mode:** Theme switcher with automatic Dark Mode activation after 18:00 (6:00 PM) Iran Standard Time (IRST/UTC+3:30).
- **Hero Slider:** Apple-inspired banner slider powered by Swiper.js with frosted-glass pagination and smooth transitions.
- **Sticky Header & Reading Progress:** Frosted header with a 2px–4px reading progress bar at the bottom.
- **Mobile Navigation:** Collapsed mobile header displaying only the Logo and Theme Switcher with an animated frosted drawer.
- **Infinite Scroll:** Real-time dynamic pagination for articles.
- **Full-Screen 'About Me' Presentation:** Smooth scrollable slide presentation with keynote-style cards.
- **Recursive Comments:** Unlimited recursive depth comment hierarchy.
- **View Counter:** IP/Client-based 24-hour rate-limited view tracker.

## 🛠️ Getting Started (Frontend)

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start development server on port 4500
npm run dev
```

Visit `http://localhost:4500` in your browser.

## 📄 License
This project is licensed under the [MIT License](LICENSE).
