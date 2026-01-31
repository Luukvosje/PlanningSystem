import { defineNuxtPlugin, useRuntimeConfig } from 'nuxt/app'
import { ServiceFactory } from '../typescript/factory/service.factory'

export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()
  const baseUrl = String(config.public.apiBaseUrl || 'http://localhost:5157')

  const serviceFactory = new ServiceFactory(baseUrl)

  return {
    provide: {
      serviceFactory: serviceFactory
    }
  }
});