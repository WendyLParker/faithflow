import { formatDistanceToNow } from 'date-fns';
import { ChevronRight, Sparkles, CheckCircle } from 'lucide-react';
import { Link } from 'react-router-dom';
import type { RequestResponseDto } from '@/services/requestService';
import type { RequestScope } from '@/hooks/useRequests';

interface RequestCardProps {
  request: RequestResponseDto;
  direction?: RequestScope;
}

function formatRequestStatus(status: string) {
  if (status === 'Acknowledged') return 'Acknowledged';
  if (status === 'New') return 'New';
  if (status === 'Fulfilled') return 'Part complete';
  return status;
}

export default function RequestCard({ request, direction = 'sent' }: RequestCardProps) {
  const timeAgo = formatDistanceToNow(new Date(request.requestDate), { addSuffix: true });
  const isFulfilled = request.requestStatus === 'Fulfilled' && !request.isCompleted;
  const showReceivedStatus = direction === 'received' && !request.isCompleted && request.requestStatus;

  return (
    <Link
      to={`/requests/${request.id}`}
      className="block content-card-sm hover:border-neutral-500 transition"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 flex-1">
            {request.requestTypeName && (
              <span className="badge">{request.requestTypeName}</span>
            )}
          <div className="flex items-center gap-2">
          
            <h3 className="font-semibold text-neutral-100 truncate">{request.title}</h3>
            {request.isCompleted && (
              <span className="badge-success shrink-0">
                <Sparkles size={12} />
                Closed
              </span>
            )}
            {direction === 'sent' && isFulfilled && (
              <span className="badge-success shrink-0">
                <CheckCircle size={12} />
                Ready to close
              </span>
            )}
            {showReceivedStatus && (
              <span className="badge shrink-0">
                {formatRequestStatus(request.requestStatus)}
              </span>
            )}
          </div>

          {request.content && (
            <p className="text-neutral-400 text-sm mt-1 line-clamp-2">{request.content}</p>
          )}

          <div className="flex flex-wrap items-center gap-2 mt-3">
            <span className="text-xs text-neutral-500">{timeAgo}</span>
            {request.groupNames.map((group) => (
              <span key={group} className="badge">
                {group}
              </span>
            ))}
          </div>
        </div>

        <ChevronRight size={20} className="text-neutral-600 shrink-0 mt-1" />
      </div>
    </Link>
  );
}
