import type { ServiceFactory } from '../typescript/factory/service.factory'
declare module '#app' {
  interface NuxtApp {
    $serviceFactory: ServiceFactory;
  }
  interface PageMeta {
    title?: string,
  }
}