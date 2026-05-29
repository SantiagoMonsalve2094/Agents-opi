import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AgentState } from './agent.reducer';

export const selectAgentState = createFeatureSelector<AgentState>('agent');

export const selectConversations = createSelector(
  selectAgentState,
  state => state.conversations
);

export const selectSelectedConversationId = createSelector(
  selectAgentState,
  state => state.selectedConversationId
);
