import { create } from 'zustand';
import type { ChatRoom, Message } from '../types';

interface ChatState {
    activeRoom: ChatRoom | null;
    messages: Message[];
    rooms: ChatRoom[];

    setActiveRoom: (room: ChatRoom | null) => void;
    setMessages: (messages: Message[]) => void;
    addMessage: (message: Message) => void;
    setRooms: (rooms: ChatRoom[]) => void;
    addRoom: (room: ChatRoom) => void;
    reset: () => void;
}

export const useChatStore = create<ChatState>((set) => ({
    activeRoom: null,
    messages: [],
    rooms: [],

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
    reset: () => set({ activeRoom: null, messages: [], rooms: [] }),
}));
