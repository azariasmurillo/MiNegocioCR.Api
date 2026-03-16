import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { apiConfig } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export class ChatSignalRService {
  private hubConnection: signalR.HubConnection | null = null;

  private get hubUrl(): string {
    const base = apiConfig.baseUrl.replace(/\/$/, '');
    return `${base}/chatHub`;
  }

  async start(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, { withCredentials: true })
      .withAutomaticReconnect()
      .build();
    await this.hubConnection.start();
  }

  async joinConversation(conversationId: string | number): Promise<void> {
    await this.start();
    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Cannot send data if the connection is not in the \'Connected\' State.');
    }
    await this.hubConnection.invoke('JoinConversation', String(conversationId));
  }

  onMessage(callback: (message: unknown) => void): void {
    this.hubConnection?.on('ReceiveMessage', callback);
  }

  onTyping(callback: (data: unknown) => void): void {
    this.hubConnection?.on('typing', callback);
  }

  stop(): void {
    this.hubConnection?.stop().catch(() => {});
    this.hubConnection = null;
  }
}
