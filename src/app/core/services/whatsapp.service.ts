import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConversationDTO } from '../models/conversation.model';
import { apiConfig } from '../config/api.config';

const TENANT_ID = '24a2cf67-4494-4432-a8c7-c74294de5077';

@Injectable({ providedIn: 'root' })
export class WhatsAppService {
  private get baseUrl(): string {
    return apiConfig.baseUrl.replace(/\/$/, '');
  }

  constructor(private http: HttpClient) {}

  /**
   * GET /api/whatsapp/conversations/{tenantId}
   * Returns typed ConversationDTO[] for UC-01 Ver conversaciones.
   */
  getConversations(): Observable<ConversationDTO[]> {
    return this.http.get<ConversationDTO[]>(`${this.baseUrl}/api/whatsapp/conversations/${TENANT_ID}`);
  }
}
