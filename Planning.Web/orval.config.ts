import { defineConfig } from 'orval'

export default defineConfig({
  planning: {
    input: {
      target: process.env.OPENAPI_URL ?? 'http://localhost:5264/swagger/v1/swagger.json',
    },
    output: {
      mode: 'tags-split',
      target: './app/generated/api',
      schemas: './app/generated/models',
      client: 'fetch',
      clean: true,
      prettier: true,
      override: {
        fetch: {
          includeHttpResponseReturnType: false,
        },
        mutator: {
          path: './app/utils/apiClient.ts',
          name: 'customFetch',
        },
      },
    },
  },
})
