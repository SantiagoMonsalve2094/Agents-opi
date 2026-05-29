import { AgentPhase } from './agent-phase.model';

export interface ConversationSummary {
  id: string;
  title: string;
  initialInput: string;
  createdAt: string;
  updatedAt: string;
}

export interface ConversationDetail extends ConversationSummary {
  phaseOutputs: ConversationPhaseOutput[];
  messages: ConversationMessage[];
}

export interface ConversationPhaseOutput {
  id: string;
  phase: AgentPhase;
  input: string;
  previousOutput: string;
  output: string;
  createdAt: string;
  completedAt: string;
  isStale: boolean;
}

export type ConversationMessageRole = 'User' | 'Agent' | 'System';
export type ConversationMessageType = 'InitialInput' | 'PhaseOutput' | 'Feedback' | 'RefinedPhaseOutput' | 'StaleDecision';

export interface ConversationMessage {
  id: string;
  phase?: AgentPhase;
  role: ConversationMessageRole;
  type: ConversationMessageType;
  content: string;
  createdAt: string;
}
