import { X, CheckCircle, BellOff, Loader2, Bell } from 'lucide-react';
import { useNotifications, useAcknowledgeNotification, useDismissNotification } from '@/hooks/useNotifications';
import type { NotificationDto } from '@/services/notificationService';
import { formatDistanceToNow } from 'date-fns';

interface Props {
  open: boolean;
  onClose: () => void;
}

export default function NotificationPanel({ open, onClose }: Props) {
  const { data: notifications = [], isLoading } = useNotifications();
  const acknowledge = useAcknowledgeNotification();
  const dismiss = useDismissNotification();

  return (
    <>
      {/* Backdrop */}
      {open && (
        <button
          type="button"
          aria-label="Close notifications"
          className="fixed inset-0 z-40 bg-black/50 cursor-default"
          onClick={onClose}
        />
      )}

      {/* Drawer */}
      <aside
        aria-label="Notifications"
        className={`fixed top-0 right-0 z-50 h-full w-full max-w-sm bg-neutral-900 border-l border-neutral-700 shadow-2xl flex flex-col transition-transform duration-300 ${
          open ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-4 py-4 border-b border-neutral-700">
          <div className="flex items-center gap-2">
            <Bell size={18} className="text-neutral-400" />
            <h2 className="font-semibold text-white">Notifications</h2>
            {notifications.length > 0 && (
              <span className="text-xs bg-[#34C759] text-black font-bold px-2 py-0.5 rounded-full">
                {notifications.length}
              </span>
            )}
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Close panel"
            className="p-1.5 text-neutral-400 hover:text-white transition-colors rounded-lg hover:bg-neutral-800"
          >
            <X size={20} />
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto">
          {isLoading && (
            <div className="flex justify-center py-16">
              <Loader2 className="animate-spin text-[#34C759]" size={28} />
            </div>
          )}

          {!isLoading && notifications.length === 0 && (
            <div className="flex flex-col items-center justify-center py-20 px-6 text-center">
              <BellOff size={40} className="text-neutral-600 mb-4" />
              <p className="text-neutral-400 text-sm">You're all caught up!</p>
              <p className="text-neutral-600 text-xs mt-1">No new notifications.</p>
            </div>
          )}

          {!isLoading && notifications.length > 0 && (
            <ul className="divide-y divide-neutral-800">
              {notifications.map((n) => (
                <li key={n.id}>
                  <NotificationCard
                    notification={n}
                    onAcknowledge={() => acknowledge.mutate(n.id)}
                    onDismiss={() => dismiss.mutate(n.id)}
                    isActing={
                      (acknowledge.isPending && acknowledge.variables === n.id) ||
                      (dismiss.isPending && dismiss.variables === n.id)
                    }
                  />
                </li>
              ))}
            </ul>
          )}
        </div>
      </aside>
    </>
  );
}

function NotificationCard({
  notification: n,
  onAcknowledge,
  onDismiss,
  isActing,
}: {
  notification: NotificationDto;
  onAcknowledge: () => void;
  onDismiss: () => void;
  isActing: boolean;
}) {
  const timeAgo = formatDistanceToNow(new Date(n.createdAt), { addSuffix: true });
  const isNewRequest = n.type === 'NewRequest';

  return (
    <div className="px-4 py-4 hover:bg-neutral-800/50 transition-colors">
      {/* Type badge + time */}
      <div className="flex items-center justify-between mb-2">
        <span
          className={`text-xs font-medium px-2 py-0.5 rounded-full ${
            isNewRequest
              ? 'bg-amber-950/60 text-amber-400 border border-amber-800/50'
              : 'bg-[#2f3834] text-[#9bada3] border border-[#3d4a44]'
          }`}
        >
          {isNewRequest ? `New ${n.requestTypeName} Request` : 'Request Acknowledged'}
        </span>
        <span className="text-xs text-neutral-500">{timeAgo}</span>
      </div>

      {/* Request title */}
      <p className="text-sm font-semibold text-neutral-100 mb-1 leading-snug">{n.prayerTitle}</p>

      {/* Preview */}
      {n.prayerContent && (
        <p className="text-xs text-neutral-400 line-clamp-2 mb-3">{n.prayerContent}</p>
      )}

      {/* Status pill */}
      <div className="flex items-center justify-between gap-2">
        <span className="text-xs text-neutral-500">
          Status: <span className="text-neutral-300">{n.requestStatus}</span>
        </span>

        {isNewRequest ? (
          <button
            type="button"
            onClick={onAcknowledge}
            disabled={isActing}
            className="inline-flex items-center gap-1.5 btn-apple text-xs px-3 py-1.5 rounded-lg font-medium disabled:opacity-50"
          >
            {isActing ? (
              <Loader2 size={12} className="animate-spin" />
            ) : (
              <CheckCircle size={14} />
            )}
            Acknowledge
          </button>
        ) : (
          <button
            type="button"
            onClick={onDismiss}
            disabled={isActing}
            className="inline-flex items-center gap-1.5 text-xs px-3 py-1.5 rounded-lg font-medium text-neutral-400 hover:text-white border border-neutral-700 hover:border-neutral-500 transition disabled:opacity-50"
          >
            {isActing ? <Loader2 size={12} className="animate-spin" /> : 'Dismiss'}
          </button>
        )}
      </div>
    </div>
  );
}
