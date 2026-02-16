import { useEffect, useState } from 'react';
import { useAuthStore } from '../store/useAuthStore';
import { useChatStore } from '../store/useChatStore';
import { chat, getErrorMessage } from '../lib/api';
import { LogOut, Plus, Search, MessageSquare, Users, Shield, X } from 'lucide-react';
import type { SearchUser } from '../types';
import toast from 'react-hot-toast';

export const Sidebar = () => {
    const { logout, user } = useAuthStore();
    const { rooms, setActiveRoom, activeRoom, addRoom } = useChatStore();
    const [view, setView] = useState<'chats' | 'search'>('chats');
    const [searchTerm, setSearchTerm] = useState('');
    const [searchResults, setSearchResults] = useState<SearchUser[]>([]);
    const [isSearching, setIsSearching] = useState(false);
    const [showCreateGroup, setShowCreateGroup] = useState(false);
    const [groupName, setGroupName] = useState('');

    useEffect(() => {
        const fetchRooms = async () => {
            try {
                const response = await chat.getRooms();
                useChatStore.getState().setRooms(response.data);
            } catch (error) {
                console.error('Failed to fetch rooms', error);
            }
        };

        fetchRooms();
    }, []);

    const handleSearch = async () => {
        if (!searchTerm.trim()) return;
        setIsSearching(true);
        try {
            const response = await chat.searchUsers(searchTerm);
            setSearchResults(response.data);
        } catch (error) {
            toast.error(getErrorMessage(error, 'Search failed'));
        } finally {
            setIsSearching(false);
        }
    };

    const startPrivateChat = async (targetUserId: string) => {
        try {
            const response = await chat.createPrivateRoom(targetUserId);
            const room = response.data;
            if (!rooms.find(r => r.id === room.id)) {
                addRoom(room);
            }
            setActiveRoom(room);
            setView('chats');
            setSearchTerm('');
            setSearchResults([]);
        } catch (error) {
            toast.error(getErrorMessage(error, 'Failed to start chat'));
        }
    };

    const createGroup = async () => {
        if (!groupName.trim()) return;
        try {
            const response = await chat.createPublicRoom(groupName);
            const room = response.data;
            if (!rooms.find(r => r.id === room.id)) {
                addRoom(room);
            }
            setActiveRoom(room);
            setShowCreateGroup(false);
            setGroupName('');
            toast.success('Group created');
        } catch (error) {
            toast.error(getErrorMessage(error, 'Failed to create group'));
        }
    };

    const getRoomName = (room: any, currentUserId: string | undefined) => {
        if (!room.isPrivate) return room.name;
        const otherParticipant = room.participants?.find((p: any) => p.id !== currentUserId);
        return otherParticipant ? otherParticipant.username : 'Private Chat';
    };

    const isGroupAdmin = (room: any, currentUserId: string | undefined) => {
        return !room.isPrivate && room.creatorId && room.creatorId === currentUserId;
    };

    return (
        <div className="w-80 bg-white border-r border-gray-200 flex flex-col h-full font-sans">
            <div className="p-4 border-b border-gray-200 bg-gray-50">
                <div className="flex justify-between items-center mb-4">
                    <div className="flex items-center space-x-2">
                        <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center text-white font-bold">
                            {user?.username?.charAt(0)?.toUpperCase() ?? '?'}
                        </div>
                        <span className="font-semibold text-gray-700">{user?.username ?? 'User'}</span>
                    </div>
                    <button onClick={logout} className="text-gray-500 hover:text-red-500 transition-colors" title="Logout">
                        <LogOut size={18} />
                    </button>
                </div>

                <div className="flex space-x-2 bg-gray-200 p-1 rounded-lg">
                    <button
                        onClick={() => setView('chats')}
                        className={`flex-1 flex items-center justify-center py-1.5 text-sm font-medium rounded-md transition-all ${view === 'chats' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-600 hover:text-gray-800'}`}
                    >
                        <MessageSquare size={16} className="mr-2" /> Chats
                    </button>
                    <button
                        onClick={() => setView('search')}
                        className={`flex-1 flex items-center justify-center py-1.5 text-sm font-medium rounded-md transition-all ${view === 'search' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-600 hover:text-gray-800'}`}
                    >
                        <Users size={16} className="mr-2" /> Users
                    </button>
                </div>
            </div>

            <div className="flex-1 overflow-y-auto">
                {view === 'chats' ? (
                    <div className="p-2 space-y-2">
                        {showCreateGroup && (
                            <div className="p-3 bg-blue-50 rounded-lg border border-blue-200">
                                <div className="flex justify-between items-center mb-2">
                                    <span className="text-sm font-semibold text-blue-700">New Group</span>
                                    <button onClick={() => setShowCreateGroup(false)} className="text-gray-400 hover:text-gray-600">
                                        <X size={16} />
                                    </button>
                                </div>
                                <div className="flex space-x-2">
                                    <input
                                        type="text"
                                        placeholder="Group name..."
                                        className="flex-1 px-3 py-1.5 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm text-gray-900"
                                        value={groupName}
                                        onChange={(e) => setGroupName(e.target.value)}
                                        onKeyDown={(e) => e.key === 'Enter' && createGroup()}
                                    />
                                    <button
                                        onClick={createGroup}
                                        className="bg-blue-600 text-white px-3 py-1.5 rounded-md hover:bg-blue-700 transition-colors text-sm"
                                    >
                                        Create
                                    </button>
                                </div>
                            </div>
                        )}
                        {rooms.length === 0 && !showCreateGroup ? (
                            <div className="text-center text-gray-400 mt-10 p-4">
                                <p>No active chats.</p>
                                <p className="text-sm">Search for users or create a group to start messaging.</p>
                            </div>
                        ) : (
                            rooms.map(room => (
                                <div
                                    key={room.id}
                                    onClick={() => setActiveRoom(room)}
                                    className={`p-3 rounded-lg cursor-pointer flex items-center space-x-3 transition-colors ${activeRoom?.id === room.id ? 'bg-blue-50 border-l-4 border-blue-500' : 'hover:bg-gray-50'}`}
                                >
                                    <div className={`w-10 h-10 rounded-full flex items-center justify-center ${room.isPrivate ? 'bg-gray-200 text-gray-600' : 'bg-green-100 text-green-600'}`}>
                                        {room.isPrivate ? <Users size={20} /> : <Users size={20} />}
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <div className="flex items-center space-x-1">
                                            <p className="font-medium text-gray-800 truncate">{getRoomName(room, user?.id)}</p>
                                            {isGroupAdmin(room, user?.id) && (
                                                <Shield size={14} className="text-amber-500 flex-shrink-0" title="Admin" />
                                            )}
                                        </div>
                                        <p className="text-xs text-gray-500">
                                            {room.isPrivate ? 'Private' : `Group · ${room.participantIds?.length ?? 0} members`}
                                        </p>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                ) : (
                    <div className="p-4">
                        <div className="flex space-x-2 mb-4">
                            <input
                                type="text"
                                placeholder="Search username..."
                                className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm text-gray-900"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                            />
                            <button
                                onClick={handleSearch}
                                className="bg-blue-600 text-white p-2 rounded-md hover:bg-blue-700 transition-colors"
                            >
                                <Search size={18} />
                            </button>
                        </div>

                        <div className="space-y-2">
                            {isSearching && <div className="text-center text-gray-500">Searching...</div>}
                            {searchResults.map(u => (
                                <div key={u.id} className="flex justify-between items-center p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors">
                                    <span className="font-medium text-gray-700">{u.username}</span>
                                    {u.id !== user?.id && (
                                        <button
                                            onClick={() => startPrivateChat(u.id)}
                                            className="text-blue-600 hover:text-blue-800 p-1.5 bg-blue-100 rounded-full hover:bg-blue-200 transition-colors"
                                        >
                                            <MessageSquare size={16} />
                                        </button>
                                    )}
                                </div>
                            ))}
                            {searchResults.length === 0 && !isSearching && searchTerm && (
                                <div className="text-center text-gray-500 text-sm">No users found.</div>
                            )}
                        </div>
                    </div>
                )}
            </div>

            <div className="p-4 border-t border-gray-200 space-y-2">
                <button
                    onClick={() => { setShowCreateGroup(true); setView('chats'); }}
                    className="w-full flex items-center justify-center space-x-2 bg-green-600 text-white py-2 rounded-md hover:bg-green-700 transition-colors shadow-sm"
                >
                    <Plus size={18} /> <span>New Group</span>
                </button>
                <button
                    onClick={() => setView('search')}
                    className="w-full flex items-center justify-center space-x-2 bg-gray-800 text-white py-2 rounded-md hover:bg-gray-900 transition-colors shadow-sm"
                >
                    <MessageSquare size={18} /> <span>New Chat</span>
                </button>
            </div>
        </div>
    );
};
