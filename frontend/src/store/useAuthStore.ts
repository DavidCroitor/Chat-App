import { create } from 'zustand';
import type { User } from '../types';
import { useChatStore } from './useChatStore';
import { chatHub } from '../lib/signalr';

interface AuthState {
    user: User | null;
    token: string | null;
    isAuthenticated: boolean;
    setAuth: (user: User, token: string) => void;
    logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => {
    // Initialize from localStorage with safe parsing
    const storedToken = localStorage.getItem('token');
    let storedUser: User | null = null;
    try {
        const raw = localStorage.getItem('user');
        if (raw) storedUser = JSON.parse(raw);
    } catch {
        localStorage.removeItem('user');
    }

    return {
        user: storedUser ?? null,
        token: storedToken,
        isAuthenticated: !!storedToken && !!storedUser,
        setAuth: (user, token) => {
            localStorage.setItem('token', token);
            localStorage.setItem('user', JSON.stringify(user));
            set({ user, token, isAuthenticated: true });
        },
        logout: () => {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            // Clear chat state and disconnect SignalR
            useChatStore.getState().reset();
            chatHub.stop();
            set({ user: null, token: null, isAuthenticated: false });
        },
    };
});
