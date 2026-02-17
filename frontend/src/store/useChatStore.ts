import { create } from 'zustand';
import type { ChatRoom, Message } from '../types';

interface ChatState {
    activeRoom: ChatRoom | null;
    messages: Message[];
    rooms: ChatRoom[];
    onlineUserIds: Set<string>;

    setActiveRoom: (room: ChatRoom | null) => void;
    setMessages: (messages: Message[]) => void;
    addMessage: (message: Message) => void;
    setRooms: (rooms: ChatRoom[]) => void;
    addRoom: (room: ChatRoom) => void;
    setOnlineUsers: (userIds: string[]) => void;
    addOnlineUser: (userId: string) => void;
    removeOnlineUser: (userId: string) => void;
    incrementUnreadCount: (roomId: string) => void;
    resetUnreadCount: (roomId: string) => void;
    reset: () => void;
}

export const useChatStore = create<ChatState>((set) => ({
    activeRoom: null,
    messages: [],
    rooms: [],
    onlineUserIds: new Set<string>(),

    setActiveRoom: (room) => set({ activeRoom: room }),
    setMessages: (messages) => set({ messages }),
    addMessage: (message) => set((state) => {
        if (state.messages.some(m => m.id === message.id)) {
            return state;
        }
        return { messages: [...state.messages, message] };
    }),
    setRooms: (rooms) => set({ rooms }),
    addRoom: (room) => set((state) => ({ rooms: [...state.rooms, room] })),
    setOnlineUsers: (userIds) => set({ onlineUserIds: new Set(userIds) }),
    addOnlineUser: (userId) => set((state) => {
        const next = new Set(state.onlineUserIds);
        next.add(userId);
        return { onlineUserIds: next };
    }),
    removeOnlineUser: (userId) => set((state) => {
        const next = new Set(state.onlineUserIds);
        next.delete(userId);
        return { onlineUserIds: next };
    }),
    incrementUnreadCount: (roomId) => set((state) => ({
        rooms: state.rooms.map(room =>
            room.id === roomId
                ? { ...room, unreadCount: (room.unreadCount || 0) + 1 }
                : room
        )
    })),
    resetUnreadCount: (roomId) => set((state) => ({
        rooms: state.rooms.map(room =>
            room.id === roomId
                ? { ...room, unreadCount: 0 }
                : room
        )
    })),
    reset: () => set({ activeRoom: null, messages: [], rooms: [], onlineUserIds: new Set() }),
}));
