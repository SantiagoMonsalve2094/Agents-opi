export const HttpMethod = {
  post: 'POST'
} as const;

export const HttpHeaderName = {
  authorization: 'Authorization',
  contentType: 'Content-Type',
  conversationId: 'X-Conversation-Id',
  azureUserId: 'X-Azure-User-Id',
  azureUserEmail: 'X-Azure-User-Email',
  azureUserName: 'X-Azure-User-Name'
} as const;

export const HttpAuthScheme = {
  bearer: 'Bearer'
} as const;

export const HttpContentType = {
  json: 'application/json'
} as const;

export const HttpErrorMessage = {
  defaultGenerationError: 'No se pudo generar la respuesta.'
} as const;
