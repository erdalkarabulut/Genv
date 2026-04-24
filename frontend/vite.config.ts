import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "node:path";

/**
 * API hedefi. Varsayılan 127.0.0.1 — `localhost` bazen ::1 ile IPv6’a gider;
 * Kestrel yalnızca IPv4 dinliyorsa veya yanlış süreçte “Invalid character in chunk size” proxy hatası oluşabilir.
 * Docker API: VITE_PROXY_TARGET=http://127.0.0.1:8080
 * HTTPS dev (kendi imzalı sertifika): VITE_PROXY_TARGET=https://127.0.0.1:PORT  (secure:false aşağıda)
 */
const proxyTarget =
  process.env.VITE_PROXY_TARGET ?? "http://127.0.0.1:5278";

const proxyCommon = {
  target: proxyTarget,
  changeOrigin: true,
  secure: false,
};

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { "@": path.resolve(__dirname, "src") },
  },
  server: {
    port: 5173,
    proxy: {
      "/api": proxyCommon,
      "/hubs": { ...proxyCommon, ws: true },
    },
  },
});
