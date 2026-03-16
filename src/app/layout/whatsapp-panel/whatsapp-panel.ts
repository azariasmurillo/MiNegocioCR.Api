import { Component, OnInit } from '@angular/core';
import { WhatsAppService } from '../../../core/services/whatsapp.service';
import { Conversation, ConversationDTO } from '../../../core/models/conversation.model';

@Component({
  selector: 'app-whatsapp-panel',
  templateUrl: './whatsapp-panel.html',
  styleUrls: ['./whatsapp-panel.scss'],
})
export class WhatsappPanelComponent implements OnInit {
  view: 'conversations' | 'chat' = 'conversations';
  conversations: Conversation[] = [];
  selectedConversation: Conversation | null = null;

  constructor(private whatsappService: WhatsAppService) {}

  ngOnInit(): void {
    this.loadConversations();
  }

  loadConversations(): void {
    this.whatsappService.getConversations().subscribe({
      next: (data: ConversationDTO[] | null) => {
        const raw = data ?? [];
        if (raw.length === 0) {
          this.conversations = this.getMockConversations();
          return;
        }
        this.conversations = raw.map((c) => this.mapDtoToConversation(c));
      },
      error: () => {
        this.conversations = this.getMockConversations();
      },
    });
  }

  private mapDtoToConversation(c: ConversationDTO): Conversation {
    return {
      id: c.phoneNumber,
      name: c.customerName ?? c.phoneNumber,
      phone: c.phoneNumber,
      avatar: 'https://i.pravatar.cc/40',
      lastMessage: c.lastMessage ?? '',
      time: this.formatTime(c.lastMessageAt),
      unread: c.unreadCount ?? 0,
      messages: [],
    };
  }

  private formatTime(lastMessageAt: string | null): string {
    if (!lastMessageAt) return '';
    try {
      const d = new Date(lastMessageAt);
      return isNaN(d.getTime()) ? '' : d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } catch {
      return '';
    }
  }

  private getMockConversations(): Conversation[] {
    return [
      {
        id: 'mock-1',
        name: 'Usuario demo',
        phone: '+50600000000',
        avatar: 'https://i.pravatar.cc/40',
        lastMessage: 'Sin conversaciones aún',
        time: '',
        unread: 0,
        messages: [],
      },
    ];
  }

  selectConversation(conversation: Conversation): void {
    this.selectedConversation = conversation;
    this.view = 'chat';
    // signalr.joinConversation(conversation.id) etc. sin cambios
  }

  sendMessage(_text: string): void {
    // Sin cambios
  }
}
