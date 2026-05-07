import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import path from "node:path";

export default defineConfig(({ mode }) => {
  // loadEnv ensures .env, .env.development, etc. are read when Vite runs
  const env = loadEnv(mode, process.cwd(), "");

  // Prefer explicit VITE_PROXY_TARGET from env files; fall back to sane default
  const rawProxy = env.VITE_PROXY_TARGET || process.env.VITE_PROXY_TARGET || "127.0.0.1:5278";
  const proxyTarget = rawProxy.includes("://") ? rawProxy : `http://${rawProxy}`;

  const proxyCommon = {
    target: proxyTarget,
    changeOrigin: true,
    secure: false,
  };

  return {
    plugins: [react()],
    resolve: {
      alias: { "@": path.resolve(__dirname, "src") },
    },
    server: {
      port: Number(env.VITE_PORT) || 5373,
      proxy: {
        "/api": proxyCommon,
        "/hubs": { ...proxyCommon, ws: true },
      },
    },
  };
});
