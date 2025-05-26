import { defineConfig } from "vite"

export default defineConfig({
  base: "/",
  build: {
    outDir: "dist",
    assetsDir: "assets",
    rollupOptions: {
      input: {
        main: "index.html",
        catalogo: "catalogo.html",
        probador: "probador.html",
      },
    },
  },
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "https://localhost:7295",
        changeOrigin: true,
        secure: false,
      },
    },
  },
})