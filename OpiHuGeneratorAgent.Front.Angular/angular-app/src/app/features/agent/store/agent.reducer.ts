import { createReducer, on } from '@ngrx/store';
import { ConversationSummary } from '../models/conversation.model';
import { selectConversation, setConversations } from './agent.actions';

export interface AgentState {
  conversations: ConversationSummary[];
  selectedConversationId?: string;
}

export const initialAgentState: AgentState = {
  conversations: []
};

export const agentReducer = createReducer(
  initialAgentState,
  on(setConversations, (state, { conversations }) => ({ ...state, conversations })),
  on(selectConversation, (state, { conversationId }) => ({ ...state, selectedConversationId: conversationId }))
);
