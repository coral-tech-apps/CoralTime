import { defineConfig } from 'vite';

export default defineConfig({
  optimizeDeps: {
    include: ['marked']
  },
  build: {
    commonjsOptions: {
      include: [/marked/, /node_modules/]
    }
  }
});
