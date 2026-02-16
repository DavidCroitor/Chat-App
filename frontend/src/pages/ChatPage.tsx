import { Sidebar } from '../components/Sidebar';
import { ChatArea } from '../components/ChatArea';
import { ErrorBoundary } from '../components/ErrorBoundary';

export const ChatPage = () => {
    return (
        <div className="flex h-screen bg-gray-100 overflow-hidden">
            <ErrorBoundary>
                <Sidebar />
            </ErrorBoundary>
            <ErrorBoundary>
                <ChatArea />
            </ErrorBoundary>
        </div>
    );
};
