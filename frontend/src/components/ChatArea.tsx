import { useEffect, useState, useRef } from 'react';
import { useChatStore } from '../store/useChatStore';
import { useAuthStore } from '../store/useAuthStore';
import { chat, getErrorMessage } from '../lib/api';
import { chatHub } from '../lib/signalr';
import { Send, MessageSquare, Shield, Users, UserPlus, X, Search } from 'lucide-react';
import type { Message, SearchUser } from '../types';
import toast from 'react-hot-toast';

export const ChatArea = () => {
    const { activeRoom, messages, setMessages, addMessage } = useChatStore();
    const { user } = useAuthStore();
    const [newMessage, setNewMessage] = useState('');
    const [isLoadingHistory, setIsLoadingHistory] = useState(false);
    const messagesEndRef = useRef<HTMLDivElement>(null);
    const previousRoomIdRef = useRef<string | null>(null);

    // Add user modal state
    const [showAddUserModal, setShowAddUserModal] = useState(false);
    const [userSearchTerm, setUserSearchTerm] = useState('');
    const [searchResults, setSearchResults] = useState<SearchUser[]>([]);
    const [isSearching, setIsSearching] = useState(false);
    const [isAddingUser, setIsAddingUser] = useState(false);

    const isAdmin = activeRoom && !activeRoom.isPrivate && activeRoom.creatorId === user?.id;

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    };

    useEffect(() => {
        scrollToBottom();
    }, [messages]);

    // Connect to SignalR
    useEffect(() => {
        chatHub.start();

        const handleReceiveMessage = (message: Message) => {
            console.log('Received message:', message);
            addMessage(message);
        };

        chatHub.onMessageReceived(handleReceiveMessage);

        return () => {
            chatHub.offMessageReceived(handleReceiveMessage);
        };
    }, []);

    // Fetch history when activeRoom changes
    useEffect(() => {
        if (!activeRoom) return;

        const fetchHistory = async () => {
            setIsLoadingHistory(true);
            try {
                // Leave the previous room's SignalR group
                if (previousRoomIdRef.current && previousRoomIdRef.current !== activeRoom.id) {
                    await chatHub.leaveRoom(previousRoomIdRef.current);
                }

                const response = await chat.getHistory(activeRoom.id);
                const sortedMessages = response.data.messages.sort((a, b) =>
                    new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
                );
                setMessages(sortedMessages);

                // Join the SignalR group for this room
                await chatHub.joinRoom(activeRoom.id);
                previousRoomIdRef.current = activeRoom.id;
            } catch (error) {
                console.error('Failed to fetch history', error);
                toast.error(getErrorMessage(error, 'Could not load chat history'));
            } finally {
                setIsLoadingHistory(false);
            }
        };

        fetchHistory();

    }, [activeRoom, setMessages]);


    const handleSendMessage = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!newMessage.trim() || !activeRoom) return;

        try {
            await chat.sendMessage(activeRoom.id, newMessage);
            setNewMessage('');
        } catch (error) {
            toast.error(getErrorMessage(error, 'Failed to send message'));
        }
    };

    const handleSearchUsers = async (term: string) => {
        setUserSearchTerm(term);
        if (term.length < 2) {
            setSearchResults([]);
            return;
        }

        setIsSearching(true);
        try {
            const response = await chat.searchUsers(term);
            // Filter out users already in the room
            const filteredUsers = response.data.filter(
                u => !activeRoom?.participantIds.includes(u.id)
            );
            setSearchResults(filteredUsers);
        } catch (error) {
            toast.error(getErrorMessage(error, 'Failed to search users'));
        } finally {
            setIsSearching(false);
        }
    };

    const handleAddUser = async (userId: string) => {
        if (!activeRoom) return;

        setIsAddingUser(true);
        try {
            await chat.addUserToRoom(activeRoom.id, userId);
            toast.success('User added to room');
            setShowAddUserModal(false);
            setUserSearchTerm('');
            setSearchResults([]);
        } catch (error) {
            toast.error(getErrorMessage(error, 'Failed to add user'));
        } finally {
            setIsAddingUser(false);
        }
    };

    const getRoomName = (room: any, currentUserId: string | undefined) => {
        if (!room || !room.isPrivate) return room?.name || 'Chat';
        const otherParticipant = room.participants?.find((p: any) => p.id !== currentUserId);
        return otherParticipant ? otherParticipant.username : 'Private Chat';
    };

    if (!activeRoom) {
        return (
            <div className="flex-1 flex items-center justify-center bg-gray-50 text-gray-500 flex-col">
                <MessageSquare className="w-16 h-16 text-gray-300 mb-4" />
                <p className="text-xl font-medium text-gray-400">Select a chat to start messaging</p>
            </div>
        );
    }

    return (
        <div className="flex-1 flex flex-col h-full bg-gray-50 overflow-hidden relative">
            <div className="p-4 bg-white border-b border-gray-200 shadow-sm flex items-center justify-between z-10 w-full">
                <div>
                    <h2 className="font-bold text-gray-800 text-lg flex items-center">
                        {getRoomName(activeRoom, user?.id)}
                        {!activeRoom.isPrivate && (
                            <span className="ml-2 text-xs font-normal bg-green-100 text-green-700 px-2 py-0.5 rounded-full">Group</span>
                        )}
                    </h2>
                    {!activeRoom.isPrivate && (
                        <div className="flex items-center text-xs text-gray-500 mt-0.5">
                            <Users size={12} className="mr-1" />
                            <span>{activeRoom.participantIds?.length ?? 0} members</span>
                            {activeRoom.creatorId && (
                                <span className="ml-2 flex items-center">
                                    <Shield size={12} className="mr-0.5 text-amber-500" />
                                    Admin: {activeRoom.participants.find(p => p.id === activeRoom.creatorId)?.username ?? 'Unknown'}
                                </span>
                            )}
                        </div>
                    )}
                </div>
                <div className="text-sm text-gray-500">
                    {isAdmin && (
                        <button
                            onClick={() => setShowAddUserModal(true)}
                            className="flex items-center gap-1 px-3 py-1.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm font-medium"
                        >
                            <UserPlus size={16} />
                            Add User
                        </button>
                    )}
                </div>
            </div>

            {/* Add User Modal */}
            {showAddUserModal && (
                <div className="absolute inset-0 bg-black/50 flex items-center justify-center z-20">
                    <div className="bg-white rounded-lg shadow-xl w-96 max-h-[80vh] flex flex-col">
                        <div className="p-4 border-b flex items-center justify-between">
                            <h3 className="font-semibold text-gray-800">Add User to Room</h3>
                            <button
                                onClick={() => {
                                    setShowAddUserModal(false);
                                    setUserSearchTerm('');
                                    setSearchResults([]);
                                }}
                                className="text-gray-500 hover:text-gray-700"
                            >
                                <X size={20} />
                            </button>
                        </div>
                        <div className="p-4">
                            <div className="relative">
                                <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                                <input
                                    type="text"
                                    placeholder="Search users by username..."
                                    value={userSearchTerm}
                                    onChange={(e) => handleSearchUsers(e.target.value)}
                                    className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg text-gray-900 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                                />
                            </div>
                        </div>
                        <div className="flex-1 overflow-y-auto px-4 pb-4 max-h-64">
                            {isSearching ? (
                                <div className="flex justify-center py-4">
                                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-500"></div>
                                </div>
                            ) : searchResults.length > 0 ? (
                                <div className="space-y-2">
                                    {searchResults.map((searchUser) => (
                                        <div
                                            key={searchUser.id}
                                            className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                                        >
                                            <div className="flex items-center gap-3">
                                                <div className="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center text-sm font-bold text-gray-600">
                                                    {searchUser.username.charAt(0).toUpperCase()}
                                                </div>
                                                <span className="font-medium text-gray-800">{searchUser.username}</span>
                                            </div>
                                            <button
                                                onClick={() => handleAddUser(searchUser.id)}
                                                disabled={isAddingUser}
                                                className="px-3 py-1 bg-blue-600 text-white rounded-md text-sm hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                                            >
                                                {isAddingUser ? 'Adding...' : 'Add'}
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            ) : userSearchTerm.length >= 2 ? (
                                <p className="text-center text-gray-500 py-4">No users found</p>
                            ) : (
                                <p className="text-center text-gray-500 py-4">Type at least 2 characters to search</p>
                            )}
                        </div>
                    </div>
                </div>
            )}

            <div className="flex-1 overflow-y-auto p-4 space-y-4 bg-gray-50">
                {isLoadingHistory ? (
                    <div className="flex justify-center items-center h-full">
                        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
                    </div>
                ) : (
                    messages.map((msg, index) => {
                        const isMe = msg.senderId === user?.id;
                        return (
                            <div key={msg.id || index} className={`flex ${isMe ? 'justify-end' : 'justify-start'}`}>
                                <div className={`flex max-w-xs md:max-w-md ${isMe ? 'flex-row-reverse' : 'flex-row'} items-end space-x-2`}>
                                    {!isMe && (
                                        <div className="w-8 h-8 bg-gray-300 rounded-full flex-shrink-0 flex items-center justify-center text-xs font-bold text-gray-600 ml-2">
                                            {msg.senderUsername?.charAt(0)?.toUpperCase() ?? '?'}
                                        </div>
                                    )}
                                    <div className={`px-4 py-2 rounded-lg shadow-sm ${isMe ? 'bg-blue-600 text-white rounded-br-none mr-2' : 'bg-white text-gray-800 rounded-bl-none ml-2'}`}>
                                        {!isMe && <p className="text-xs text-gray-500 mb-1 font-bold">{msg.senderUsername}</p>}
                                        <p className="text-sm break-words">{msg.content}</p>
                                        <p className={`text-[10px] mt-1 text-right ${isMe ? 'text-blue-200' : 'text-gray-400'}`}>
                                            {new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                        </p>
                                    </div>
                                </div>
                            </div>
                        );
                    })
                )}
                <div ref={messagesEndRef} />
            </div>

            <div className="p-4 bg-white border-t border-gray-200 w-full">
                <form onSubmit={handleSendMessage} className="flex items-center space-x-2">
                    <input
                        type="text"
                        value={newMessage}
                        onChange={(e) => setNewMessage(e.target.value)}
                        placeholder="Type a message..."
                        className="flex-1 border border-gray-300 rounded-full px-4 py-2 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-shadow text-gray-900"
                    />
                    <button
                        type="submit"
                        disabled={!newMessage.trim()}
                        className="bg-blue-600 text-white p-2.5 rounded-full hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                        <Send size={20} />
                    </button>
                </form>
            </div>
        </div>
    );
};
