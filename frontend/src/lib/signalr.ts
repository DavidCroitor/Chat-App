import * as signalR from '@microsoft/signalr';

const HUB_URL = 'http://localhost:5000/chatHub';

export class ChatHub {
    private connection: signalR.HubConnection;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL, {
                accessTokenFactory: () => localStorage.getItem('token') || '',
            })
            .withAutomaticReconnect()
            .build();
    }

    public async start() {
        try {
            if (this.connection.state === signalR.HubConnectionState.Disconnected) {
                await this.connection.start();
                console.log('SignalR Connected');
            }
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
            setTimeout(() => this.start(), 5000);
        }
    }

    public stop() {
        this.connection.stop();
    }

    public async joinRoom(roomId: string) {
        if (this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('JoinRoom', roomId);
        }
    }

    public async leaveRoom(roomId: string) {
        if (this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('LeaveRoom', roomId);
        }
    }

    public onMessageReceived(callback: (message: any) => void) {
        this.connection.on('ReceiveMessage', callback);
    }

    public offMessageReceived(callback: (message: any) => void) {
        this.connection.off('ReceiveMessage', callback);
    }
}

export const chatHub = new ChatHub();
