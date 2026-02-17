import * as signalR from '@microsoft/signalr';

const HUB_URL = 'http://localhost:5000/chatHub';

export class ChatHub {
    private connection: signalR.HubConnection;
    private connectionPromise: Promise<void> | null = null;
    private joinedRooms: Set<string> = new Set();

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL, {
                accessTokenFactory: () => localStorage.getItem('token') || '',
            })
            .withAutomaticReconnect()
            .build();
    }

    public async start() {
        // Return existing connection promise if already connecting
        if (this.connectionPromise) {
            return this.connectionPromise;
        }

        // Already connected
        if (this.connection.state === signalR.HubConnectionState.Connected) {
            console.log('SignalR already connected');
            return Promise.resolve();
        }

        // Create new connection promise
        this.connectionPromise = (async () => {
            try {
                if (this.connection.state === signalR.HubConnectionState.Disconnected) {
                    await this.connection.start();
                    console.log('SignalR Connected');
                }
            } catch (err) {
                console.error('SignalR Connection Error: ', err);
                this.connectionPromise = null;
                throw err;
            } finally {
                this.connectionPromise = null;
            }
        })();

        return this.connectionPromise;
    }

    public stop() {
        this.connection.stop();
    }

    public async joinRoom(roomId: string) {
        // Prevent duplicate joins
        if (this.joinedRooms.has(roomId)) {
            console.log('[SignalR] Already in room:', roomId);
            return;
        }

        if (this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('JoinRoom', roomId);
            this.joinedRooms.add(roomId);
            console.log('[SignalR] Joined room:', roomId);
        } else {
            console.warn('[SignalR] Cannot join room, not connected. State:', this.connection.state);
        }
    }

    public async leaveRoom(roomId: string) {
        if (this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('LeaveRoom', roomId);
            this.joinedRooms.delete(roomId);
            console.log('[SignalR] Left room:', roomId);
        }
    }

    public onMessageReceived(callback: (message: any, roomId: string) => void) {
        this.connection.on('ReceiveMessage', callback);
        console.log('[SignalR] Registered ReceiveMessage handler');
    }

    public offMessageReceived(callback: (message: any, roomId: string) => void) {
        this.connection.off('ReceiveMessage', callback);
        console.log('[SignalR] Unregistered ReceiveMessage handler');
    }

    public async sendTyping(roomId: string, username: string) {
        if (this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('SendTyping', roomId, username);
        }
    }

    public onUserTyping(callback: (roomId: string, username: string) => void) {
        this.connection.on('UserTyping', callback);
        console.log('[SignalR] Registered UserTyping handler');
    }

    public offUserTyping(callback: (roomId: string, username: string) => void) {
        this.connection.off('UserTyping', callback);
        console.log('[SignalR] Unregistered UserTyping handler');
    }

    // Online presence
    public async getOnlineUsers(): Promise<string[]> {
        if (this.connection.state === signalR.HubConnectionState.Connected) {
            return await this.connection.invoke<string[]>('GetOnlineUsers');
        }
        return [];
    }

    public onUserOnline(callback: (userId: string) => void) {
        this.connection.on('UserOnline', callback);
        console.log('[SignalR] Registered UserOnline handler');
    }

    public offUserOnline(callback: (userId: string) => void) {
        this.connection.off('UserOnline', callback);
        console.log('[SignalR] Unregistered UserOnline handler');
    }

    public onUserOffline(callback: (userId: string) => void) {
        this.connection.on('UserOffline', callback);
        console.log('[SignalR] Registered UserOffline handler');
    }

    public offUserOffline(callback: (userId: string) => void) {
        this.connection.off('UserOffline', callback);
        console.log('[SignalR] Unregistered UserOffline handler');
    }
}

export const chatHub = new ChatHub();
