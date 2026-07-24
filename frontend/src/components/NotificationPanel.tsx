import { useEffect } from 'react';
import { X, CheckCircle, BellOff, Loader2, Bell } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useNotifications, useAcknowledgeNotification, useDismissNotification } from '@/hooks/useNotifications';
import type { NotificationDto } from '@/services/notificationService';
import { formatDistanceToNow } from 'date-fns';

interface Props {
  open: boolean;
  onClose: () => void;
}

export default function NotificationPanel({ open, onClose }: Props) {
  const { data: notifications = [], isLoading, isError, refetch } = useNotifications();
  const acknowledge = useAcknowledgeNotification();
  const dismiss = useDismissNotification();

  useEffect(() => {
    if (open) {
      void refetch();
    }
  }, [open, refetch]);

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
            <h2 className="font-semibold text-[var(--ink)]">Notifications</h2>
            {notifications.length > 0 && (
              <span className="text-xs bg-[#9e1b32] text-white font-bold px-2 py-0.5 rounded-full">
                {notifications.length}
              </span>
            )}
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Close panel"
            className="p-1.5 text-neutral-400 hover:text-[var(--ink)] transition-colors rounded-lg hover:bg-neutral-800"
          >
            <X size={20} />
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto">
          {isLoading && (
            <div className="flex justify-center py-16">
              <Loader2 className="animate-spin text-[#9e1b32]" size={28} />
            </div>
          )}

          {isError && (
            <div className="alert-error m-4">Failed to load notifications. Please try again.</div>
          )}

          {!isLoading && !isError && notifications.length === 0 && (
            <div className="flex flex-col items-center justify-center py-20 px-6 text-center">
              <BellOff size={40} className="text-neutral-600 mb-4" />
              <p className="text-neutral-400 text-sm">You're all caught up!</p>
              <p className="text-neutral-600 text-xs mt-1">No new notifications.</p>
            </div>
          )}

          {!isLoading && !isError && notifications.length > 0 && (
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
  const isFulfilled = n.type === 'RequestFulfilled';
  const isComment = n.type === 'RequestComment';

  const typeLabel = isNewRequest
    ? `New ${n.requestTypeName} Request`
    : isFulfilled
      ? 'Request Part Complete'
      : isComment
        ? 'New Comment'
        : 'Request Acknowledged';

  return (
    <div className="px-4 py-4 hover:bg-neutral-800/50 transition-colors">
      {/* Type badge + time */}
      <div className="flex items-center justify-between mb-2">
        <span
          className={`text-xs font-medium px-2 py-0.5 rounded-full ${
            isNewRequest
              ? 'bg-amber-100 text-amber-800 border border-amber-300'
              : 'bg-[#9e1b32]/10 text-[#9e1b32] border border-[#9e1b32]/30'
          }`}
        >
          {typeLabel}
        </span>
        <span className="text-xs text-neutral-500">{timeAgo}</span>
      </div>

      {/* Request title */}
      <p className="text-sm font-semibold text-neutral-100 mb-1 leading-snug">{n.requestTitle}</p>

      {/* Preview */}
      {isComment && n.commentContent ? (
        <p className="text-xs text-neutral-400 line-clamp-3 mb-3">
          {n.commentAuthorName && (
            <span className="text-neutral-300">{n.commentAuthorName}: </span>
          )}
          {n.commentContent}
        </p>
      ) : (
        n.requestContent && (
          <p className="text-xs text-neutral-400 line-clamp-2 mb-3">{n.requestContent}</p>
        )
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
        ) : isFulfilled ? (
          <Link
            to={`/requests/${n.requestId}`}
            onClick={onDismiss}
            className="inline-flex items-center gap-1.5 btn-apple text-xs px-3 py-1.5 rounded-lg font-medium"
          >
            View & close
          </Link>
        ) : isComment ? (
          <Link
            to={`/requests/${n.requestId}`}
            onClick={onDismiss}
            className="inline-flex items-center gap-1.5 btn-apple text-xs px-3 py-1.5 rounded-lg font-medium"
          >
            View request
          </Link>
        ) : (
          <button
            type="button"
            onClick={onDismiss}
            disabled={isActing}
            className="inline-flex items-center gap-1.5 text-xs px-3 py-1.5 rounded-lg font-medium text-neutral-400 hover:text-[var(--ink)] border border-neutral-700 hover:border-neutral-500 transition disabled:opacity-50"
          >
            {isActing ? <Loader2 size={12} className="animate-spin" /> : 'Dismiss'}
          </button>
        )}
      </div>
    </div>
  );
}
