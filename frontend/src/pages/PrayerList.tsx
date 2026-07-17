import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Loader2, ArrowLeft } from 'lucide-react';
import { useRequestInbox, type RequestScope } from '@/hooks/useRequests';
import type { RequestResponseDto } from '@/services/requestService';
import RequestCard from '@/components/RequestCard';

type Direction = RequestScope;
type StatusFilter = 'active' | 'completed';

function filterByStatus(requests: RequestResponseDto[], status: StatusFilter, direction: Direction) {
  if (status === 'completed') {
    if (direction === 'sent') {
      return requests.filter((r) => r.isCompleted);
    }
    return requests.filter((r) => r.requestStatus === 'Fulfilled' || r.isCompleted);
  }

  if (direction === 'sent') {
    return requests.filter((r) => !r.isCompleted);
  }
  return requests.filter((r) => !r.isCompleted && r.requestStatus !== 'Fulfilled');
}

function countByStatus(requests: RequestResponseDto[], status: StatusFilter, direction: Direction) {
  return filterByStatus(requests, status, direction).length;
}

export default function PrayerList() {
  const { sent, received, isLoading, error } = useRequestInbox();
  const [direction, setDirection] = useState<Direction>('sent');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('active');

  const currentList = direction === 'sent' ? sent : received;

  const filtered = useMemo(
    () => filterByStatus(currentList, statusFilter, direction),
    [currentList, statusFilter, direction],
  );

  const activeCount = countByStatus(currentList, 'active', direction);
  const completedCount = countByStatus(currentList, 'completed', direction);

  const emptyMessage =
    statusFilter === 'active'
      ? direction === 'sent'
        ? 'No active sent requests yet.'
        : 'No active received requests.'
      : direction === 'sent'
        ? 'No completed sent requests yet.'
        : 'No completed received requests.';

  return (
    <div className="page-container">
      <Link
        to="/dashboard"
        className="btn-apple w-full py-3.5 rounded-xl font-semibold flex items-center justify-center gap-2"
      >
        <ArrowLeft size={20} />
        Back to dashboard
      </Link>

      <div className="flex items-center justify-between mb-2 mt-4">
        <h1 className="page-title mb-0">Requests</h1>
      </div>
      <p className="page-subtitle">
        {direction === 'sent'
          ? 'Requests you have submitted.'
          : 'Requests assigned to your groups.'}
      </p>

      <div className="flex gap-2 mb-4">
        <button
          type="button"
          onClick={() => setDirection('sent')}
          className={`filter-btn tabular-nums${direction === 'sent' ? ' filter-btn-active' : ''}`}
        >
          Sent ({sent.length})
        </button>
        <button
          type="button"
          onClick={() => setDirection('received')}
          className={`filter-btn tabular-nums${direction === 'received' ? ' filter-btn-active' : ''}`}
        >
          Received ({received.length})
        </button>
      </div>

      <div className="flex gap-2 mb-6">
        <button
          type="button"
          onClick={() => setStatusFilter('active')}
          className={`filter-btn tabular-nums${statusFilter === 'active' ? ' filter-btn-active' : ''}`}
        >
          Active ({activeCount})
        </button>
        <button
          type="button"
          onClick={() => setStatusFilter('completed')}
          className={`filter-btn tabular-nums${statusFilter === 'completed' ? ' filter-btn-active' : ''}`}
        >
          Completed ({completedCount})
        </button>
      </div>

      {isLoading && (
        <div className="flex justify-center py-16">
          <Loader2 className="animate-spin text-[#34C759]" size={32} />
        </div>
      )}

      {error && (
        <div className="alert-error">Failed to load requests. Please try again.</div>
      )}

      {!isLoading && !error && filtered.length === 0 && (
        <div className="text-center py-16">
          <p className="text-neutral-400 mb-4">{emptyMessage}</p>
        </div>
      )}

      <div className="space-y-3">
        {filtered.map((request) => (
          <RequestCard key={request.id} request={request} direction={direction} />
        ))}
      </div>

      <br />
      <Link
        to="/add"
        className="btn-apple w-full py-3.5 rounded-xl font-semibold flex items-center justify-center gap-2"
      >
        <Plus size={20} />
        New
      </Link>
    </div>
  );
}
