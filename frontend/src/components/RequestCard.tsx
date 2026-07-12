import { formatDistanceToNow } from 'date-fns';
import { ChevronRight, Sparkles } from 'lucide-react';
import { Link } from 'react-router-dom';
import type { RequestResponseDto } from '@/services/requestService';

interface RequestCardProps {
  request: RequestResponseDto;
}

export default function RequestCard({ request }: RequestCardProps) {
  const timeAgo = formatDistanceToNow(new Date(request.requestDate), { addSuffix: true });

  return (
    <Link
      to={`/requests/${request.id}`}
      className="block content-card-sm hover:border-neutral-500 transition"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <h3 className="font-semibold text-neutral-100 truncate">{request.title}</h3>
            {request.isCompleted && (
              <span className="badge-success shrink-0">
                <Sparkles size={12} />
                Completed
              </span>
            )}
          </div>

          {request.content && (
            <p className="text-neutral-400 text-sm mt-1 line-clamp-2">{request.content}</p>
          )}

          <div className="flex flex-wrap items-center gap-2 mt-3">
            <span className="text-xs text-neutral-500">{timeAgo}</span>
            {request.requestTypeName && (
              <span className="badge">{request.requestTypeName}</span>
            )}
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
