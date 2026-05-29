export enum AgentPhase {
  Invest = 'invest',
  Istqb = 'istqb',
  GherkinTable = 'gherkin_table',
  GherkinCode = 'gherkin_code',
  Selenium = 'selenium'
}

export interface AgentPhaseAction {
  phase: AgentPhase;
  label: string;
  actionText: string;
}

export const AGENT_PHASE_ACTIONS: readonly AgentPhaseAction[] = [
  { phase: AgentPhase.Invest, label: 'Analisis INVEST', actionText: 'Analisis INVEST' },
  { phase: AgentPhase.Istqb, label: 'ISTQB', actionText: 'Generar ISTQB' },
  { phase: AgentPhase.GherkinTable, label: 'Gherkin tabular', actionText: 'Gherkin tabular' },
  { phase: AgentPhase.GherkinCode, label: 'Codigo Gherkin', actionText: 'Codigo Gherkin' },
  { phase: AgentPhase.Selenium, label: 'Selenium', actionText: 'Selenium' }
];
