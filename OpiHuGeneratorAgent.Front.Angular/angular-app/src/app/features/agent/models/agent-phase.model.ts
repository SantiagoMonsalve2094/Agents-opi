export enum AgentPhase {
  Epic = 'epic',
  UserStories = 'user_stories'
}

export interface AgentPhaseAction {
  phase: AgentPhase;
  label: string;
  actionText: string;
}

export const AGENT_PHASE_ACTIONS: readonly AgentPhaseAction[] = [
  { phase: AgentPhase.Epic, label: 'Epica OPI', actionText: 'Generar epica OPI' },
  { phase: AgentPhase.UserStories, label: 'Historias de usuario', actionText: 'Generar historias de usuario' }
];
