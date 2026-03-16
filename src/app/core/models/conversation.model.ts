/**
 * DTO returned by GET /api/whatsapp/conversations/{businessId}
 * Matches API camelCase: phoneNumber, customerName, lastMessage, lastMessageAt, unreadCount
 */
export interface ConversationDTO {
  customerName: string | null;
  phoneNumber: string;
  lastMessage: string | null;
  lastMessageAt: string | null;
  unreadCount: number;
}

/**
 * Model used by the UI for the conversation list and chat.
 */
export interface Conversation {
  id: string | number;
  name: string;
  phone: string;
  avatar?: string;
  lastMessage: string;
  time: string;
  unread: number;
  messages?: any[];
}
