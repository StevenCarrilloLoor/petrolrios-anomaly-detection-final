import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import path from "path";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    host: '0.0.0.0',
    port: 5173,
    allowedHosts: [
      'petrolrios-deteccion-sistema.com',
      'api-petrolrios-sistema-deteccion.site',
      'localhost',
      '127.0.0.1'
    ],
    proxy: {
      "/api": {
        target: "http://localhost:5170",
        changeOrigin: true,
      },
      "/hubs": {
        target: "http://localhost:5170",
        ws: true,
      },
    },
  },
});