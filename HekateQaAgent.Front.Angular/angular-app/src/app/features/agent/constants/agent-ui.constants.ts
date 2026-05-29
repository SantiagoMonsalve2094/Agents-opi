export const AgentUiText = {
  agentTitle: 'HEKATE-QA',
  agentName: 'Agente HEKATE-QA',
  historySearch: 'Buscar en historial...',
  inputPlaceholder: 'Escribe la historia, requisito, flujo o feedback para refinar esta fase...',
  feedbackPlaceholder: 'Escribe el feedback para refinar esta fase...',
  copyInput: 'Copiar entrada',
  copyOutput: 'Copiar resultado',
  copied: 'Copiado',
  loadingUser: 'Espera un momento mientras se carga el usuario de Azure DevOps.',
  missingInput: 'Pega primero una historia de usuario, requisito o flujo.',
  generationError: 'No se pudo generar la respuesta.',
  openConversationError: 'No se pudo abrir la conversacion.',
  degeneratedOutputError: 'La IA genero texto repetido sin sentido. Intenta de nuevo con Generar ISTQB.',
  thinking: 'Pensando...',
  writing: 'Escribiendo...',
  errorStatus: 'Error',
  approveAndContinue: 'Aprobar y continuar',
  requestAdjustment: 'Solicitar ajuste',
  regenerateWithFeedback: 'Regenerar con el refinamiento anterior',
  keepCurrentPhase: 'Dejarla igual',
  stalePhaseMessage: 'Esta fase fue generada antes de un refinamiento anterior.',
  currentPhase: 'Fase actual',
  noHistory: 'Sin conversaciones.',
  loadingHistory: 'Cargando...',
  newConversation: 'Nuevo',
  available: 'Disponible',
  blocked: 'Bloqueado',
  completed: 'Completado',
  inProgress: 'En curso',
  stale: 'Desactualizado'
} as const;

export const AgentTiming = {
  copyResetDelayMs: 1400
} as const;

export const DegeneratedOutputRules = {
  repeatedLetterThreshold: 120,
  repeatedSymbolThreshold: 200,
  repeatedWordThreshold: 30,
  repeatedWordMaxLength: 24,
  tailLength: 2000,
  compactTailMinLength: 800,
  repeatedCharacterPattern: /([A-Za-z])\1{120,}/,
  repeatedSymbolPattern: /([-_=])\1{200,}/,
  repeatedWordPattern: /\b([\p{L}]{1,24})\b(?:\s+\1\b){30,}/u,
  compactTailPattern: /^[-|:=_]+$/
} as const;
