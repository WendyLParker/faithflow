import { useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { format } from 'date-fns';
import { ArrowLeft, Loader2, Sparkles, Trash2, CheckCircle } from 'lucide-react';
import {
  useRequest,
  useMarkRequestCompleted,
  useDeleteRequest,
} from '@/hooks/useRequests';

export default function PrayerDetail() {
  const { id } = useParams<{ id: string }>();
  const requestId = Number(id);
  const navigate = useNavigate();

  const { data: request, isLoading, error } = useRequest(requestId);
  const markCompleted = useMarkRequestCompleted();
  const deleteRequest = useDeleteRequest();
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const handleMarkCompleted = async () => {
    try {
      await markCompleted.mutateAsync(requestId);
    } catch {
      // shown via mutation state
    }
  };

  const handleDelete = async () => {
    try {
      await deleteRequest.mutateAsync(requestId);
      navigate('/requests');
    } catch {
      setShowDeleteConfirm(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center py-24">
        <Loader2 className="animate-spin text-[#34C759]" size={32} />
      </div>
    );
  }

  if (error || !request) {
    return (
      <div className="page-container">
        <Link to="/requests" className="back-link">
          <ArrowLeft size={16} />
          Back to requests
        </Link>
        <div className="alert-error">Request not found.</div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <Link to="/requests" className="back-link">
        <ArrowLeft size={16} />
        Back to requests
      </Link>

      <div className="content-card">
        {request.isCompleted && (
          <div className="flex items-center gap-2 alert-info mb-5">
            <Sparkles size={20} className="text-[#9bada3] shrink-0" />
            <div>
              <p className="font-medium text-neutral-100">Completed</p>
              {request.completedDate && (
                <p className="text-sm text-neutral-400">
                  Completed on {format(new Date(request.completedDate), 'MMMM d, yyyy')}
                </p>
              )}
            </div>
          </div>
        )}

        <h1 className="text-2xl font-bold text-white">{request.title}</h1>

        <div className="flex flex-wrap items-center gap-2 mt-3">
          {request.requestTypeName && (
            <span className="badge">{request.requestTypeName}</span>
          )}
          {request.groupNames.map((group) => (
            <span key={group} className="badge">
              {group}
            </span>
          ))}
        </div>

        <p className="text-sm text-neutral-500 mt-2">
          Posted {format(new Date(request.requestDate), 'MMMM d, yyyy')}
        </p>

        {request.content && (
          <p className="text-neutral-300 mt-5 leading-relaxed whitespace-pre-wrap">
            {request.content}
          </p>
        )}
      </div>

      <div className="mt-6 space-y-3">
        {!request.isCompleted && (
          <button
            onClick={handleMarkCompleted}
            disabled={markCompleted.isPending}
            className="btn-apple w-full flex items-center justify-center gap-2 py-3.5 rounded-xl font-semibold disabled:opacity-50"
          >
            {markCompleted.isPending ? (
              <Loader2 size={20} className="animate-spin" />
            ) : (
              <CheckCircle size={20} />
            )}
            Mark as Completed
          </button>
        )}

        {markCompleted.isError && (
          <p className="message-error text-center">Failed to mark as completed. Please try again.</p>
        )}

        {!showDeleteConfirm ? (
          <button
            onClick={() => setShowDeleteConfirm(true)}
            className="w-full flex items-center justify-center gap-2 text-red-400 py-3 rounded-xl font-medium border border-red-900/50 hover:bg-red-950/30 transition"
          >
            <Trash2 size={18} />
            Delete Request
          </button>
        ) : (
          <div className="alert-error space-y-3">
            <p className="text-center">Are you sure? This cannot be undone.</p>
            <div className="flex gap-3">
              <button
                onClick={() => setShowDeleteConfirm(false)}
                className="filter-btn flex-1"
              >
                Cancel
              </button>
              <button
                onClick={handleDelete}
                disabled={deleteRequest.isPending}
                className="flex-1 py-2 text-sm font-medium text-white bg-red-700 rounded-lg hover:bg-red-600 disabled:opacity-50 border border-red-600"
              >
                {deleteRequest.isPending ? 'Deleting...' : 'Delete'}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
