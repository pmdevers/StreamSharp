import createClient from 'openapi-fetch'

export type Paths = Record<string, Record<string, unknown>>

export const openApiDocumentPath = '/openapi/v1.json'

export const apiClient = createClient<Paths>({
  baseUrl: '/api',
})
