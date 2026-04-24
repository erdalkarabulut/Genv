import type { Config } from "tailwindcss";

export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  darkMode: "class",
  theme: {
    extend: {
      fontFamily: {
        sans: [
          "InterVariable",
          "Inter",
          "ui-sans-serif",
          "system-ui",
          "-apple-system",
          "Segoe UI",
          "Roboto",
          "sans-serif",
        ],
      },
      colors: {
        bg: {
          DEFAULT: "rgb(var(--color-bg) / <alpha-value>)",
          subtle: "rgb(var(--color-bg-subtle) / <alpha-value>)",
          card: "rgb(var(--color-bg-card) / <alpha-value>)",
          elevated: "rgb(var(--color-bg-elevated) / <alpha-value>)",
        },
        line: "rgb(var(--color-line) / <alpha-value>)",
        ink: {
          DEFAULT: "rgb(var(--color-ink) / <alpha-value>)",
          muted: "rgb(var(--color-ink-muted) / <alpha-value>)",
          dim: "rgb(var(--color-ink-dim) / <alpha-value>)",
        },
        brand: {
          50: "#eef2ff",
          100: "#e0e7ff",
          400: "#818cf8",
          500: "#6366f1",
          600: "#4f46e5",
          700: "#4338ca",
        },
        accent: {
          mint: "#5eead4",
          rose: "#fb7185",
          amber: "#fbbf24",
          sky: "#38bdf8",
        },
      },
      boxShadow: {
        glow: "0 0 0 1px rgba(99,102,241,0.25), 0 8px 32px -8px rgba(99,102,241,0.45)",
        soft: "0 1px 2px rgba(0,0,0,0.04), 0 8px 24px -12px rgba(0,0,0,0.45)",
      },
      backgroundImage: {
        "grid-fade":
          "radial-gradient(circle at 50% 0%, rgba(99,102,241,0.18), transparent 60%)",
        "noise":
          "url(\"data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='160' height='160'><filter id='n'><feTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='2' stitchTiles='stitch'/></filter><rect width='100%' height='100%' filter='url(%23n)' opacity='0.5'/></svg>\")",
      },
      keyframes: {
        shimmer: {
          "0%": { backgroundPosition: "-200% 0" },
          "100%": { backgroundPosition: "200% 0" },
        },
        pulseGlow: {
          "0%, 100%": { boxShadow: "0 0 0 0 rgba(99,102,241,0.4)" },
          "50%": { boxShadow: "0 0 0 8px rgba(99,102,241,0)" },
        },
      },
      animation: {
        shimmer: "shimmer 2.5s linear infinite",
        pulseGlow: "pulseGlow 2s ease-in-out infinite",
      },
    },
  },
  plugins: [],
} satisfies Config;
