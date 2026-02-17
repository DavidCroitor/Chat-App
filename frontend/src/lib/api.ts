import axios from 'axios';
import type { AuthResponse, ChatHistory, ChatRoom, Message, SearchUser } from '../types';

const API_URL = 'http://localhost:5000/api';

const api = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Add a request interceptor to attach the token
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Helper to extract a displayable error message from backend responses
export function getErrorMessage(error: any, fallback = 'Something went wrong'): string {
    const data = error?.response?.data;
    if (!data) return error?.message || fallback;
    if (typeof data === 'string') return data;
    if (typeof data.message === 'string') return data.message;
    if (Array.isArray(data.errors) && data.errors.length > 0) {
        return data.errors.map((e: any) => e.errorMessage || e).join(', ');
    }
    return fallback;
}

// Add a response interceptor to handle auth errors globally
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            // Token expired or invalid — clear stale auth
            localStorage.removeItem('token');
            localStorage.removeItem('user');
        }
        return Promise.reject(error);
    }
);

export const auth = {
    register: (data: any) => api.post<AuthResponse>('/auth/register', data),
    login: (data: any) => api.post<AuthResponse>('/auth/login', data),
};

export const chat = {
    // Room Management
    createPrivateRoom: (targetUserId: string) => api.post<ChatRoom>('/chat/rooms/private', { targetUserId }),
    createPublicRoom: (name: string) => api.post<ChatRoom>('/chat/rooms/public', { name }),
    joinRoom: (roomId: string) => api.post<void>('/chat/rooms/join', { roomId }),

    // Messages
    sendMessage: (roomId: string, Text: string) => api.post<Message>(`/chat/rooms/${roomId}/messages`, { Text }),
    getHistory: (roomId: string) => api.get<ChatHistory>(`/chat/rooms/${roomId}/history`),

    // Users
    searchUsers: (term: string) => api.get<SearchUser[]>('/chat/users/search', { params: { term } }),
    getRooms: () => api.get<ChatRoom[]>('/chat/rooms'),

    // Admin: Add user to room
    addUserToRoom: (roomId: string, userId: string) => api.post<void>(`/chat/rooms/${roomId}/users`, { userId }),

    // Read receipts
    markRoomAsRead: (roomId: string) => api.post<void>(`/chat/rooms/${roomId}/read`),
};

export default api;
