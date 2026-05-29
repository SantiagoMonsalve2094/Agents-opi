import { createAction, props } from '@ngrx/store';
import { ConversationSummary } from '../models/conversation.model';

export const setConversations = createAction(
  '[Agent] Set Conversations',
  props<{ conversations: ConversationSummary[] }>()
);

export const selectConversation = createAction(
  '[Agent] Select Conversation',
  props<{ conversationId?: string }>()
);
