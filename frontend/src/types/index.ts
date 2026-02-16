export interface User {
    id: string;
    username: string;
}

export interface AuthResponse {
    id: string;
    username: string;
    email: string;
    token: string;
}

export interface Message {
    id: string;
    content: string;
    senderId: string;
    senderUsername: string;
    sentAt: string;
}

export interface ChatRoom {
    id: string;
    name: string;
    isPrivate: boolean;
    participantIds: string[];
    participants: Participant[];
    createdAt: string;
    creatorId: string | null;
}

export interface ChatHistory {
    messages: Message[];
    participants: Participant[];
}

export interface Participant {
    id: string;
    username: string;
}

export interface SearchUser {
    id: string;
    username: string;
}
