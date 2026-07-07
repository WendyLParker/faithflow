import { useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { format } from 'date-fns';
import { ArrowLeft, Loader2, Sparkles, Trash2, CheckCircle } from 'lucide-react';
import {
  usePrayer,
  useMarkPrayerAnswered,
  useDeletePrayer,
} from '@/hooks/usePrayers';

export default function PrayerDetail() {
  const { id } = useParams<{ id: string }>();
  const prayerId = Number(id);
  const navigate = useNavigate();

  const { data: prayer, isLoading, error } = usePrayer(prayerId);
  const markAnswered = useMarkPrayerAnswered();
  const deletePrayer = useDeletePrayer();
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const handleMarkAnswered = async () => {
    try {
      await markAnswered.mutateAsync(prayerId);
    } catch {
      // shown via mutation state
    }
  };

  const handleDelete = async () => {
    try {
      await deletePrayer.mutateAsync(prayerId);
      navigate('/prayers');
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

  if (error || !prayer) {
    return (
      <div className="page-container">
        <Link to="/prayers" className="back-link">
          <ArrowLeft size={16} />
          Back to requests
        </Link>
        <div className="alert-error">Request not found.</div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <Link to="/prayers" className="back-link">
        <ArrowLeft size={16} />
        Back to requests
      </Link>

      <div className="content-card">
        {prayer.isAnswered && (
          <div className="flex items-center gap-2 alert-info mb-5">
            <Sparkles size={20} className="text-[#9bada3] shrink-0" />
            <div>
              <p className="font-medium text-neutral-100">Completed</p>
              {prayer.answeredDate && (
                <p className="text-sm text-neutral-400">
                  Answered on {format(new Date(prayer.answeredDate), 'MMMM d, yyyy')}
                </p>
              )}
            </div>
          </div>
        )}

        <h1 className="text-2xl font-bold text-white">{prayer.title}</h1>

        <div className="flex flex-wrap items-center gap-2 mt-3">
          {prayer.requestTypeName && (
            <span className="badge">{prayer.requestTypeName}</span>
          )}
        </div>

        <p className="text-sm text-neutral-500 mt-2">
          Posted {format(new Date(prayer.prayerDate), 'MMMM d, yyyy')}
        </p>

        {prayer.categories.length > 0 && (
          <div className="flex flex-wrap gap-2 mt-4">
            {prayer.categories.map((cat) => (
              <span key={cat} className="badge">
                {cat}
              </span>
            ))}
          </div>
        )}

        {prayer.content && (
          <p className="text-neutral-300 mt-5 leading-relaxed whitespace-pre-wrap">
            {prayer.content}
          </p>
        )}
      </div>

      <div className="mt-6 space-y-3">
        {!prayer.isAnswered && (
          <button
            onClick={handleMarkAnswered}
            disabled={markAnswered.isPending}
            className="btn-apple w-full flex items-center justify-center gap-2 py-3.5 rounded-xl font-semibold disabled:opacity-50"
          >
            {markAnswered.isPending ? (
              <Loader2 size={20} className="animate-spin" />
            ) : (
              <CheckCircle size={20} />
            )}
            Mark as Completed
          </button>
        )}

        {markAnswered.isError && (
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
                disabled={deletePrayer.isPending}
                className="flex-1 py-2 text-sm font-medium text-white bg-red-700 rounded-lg hover:bg-red-600 disabled:opacity-50 border border-red-600"
              >
                {deletePrayer.isPending ? 'Deleting...' : 'Delete'}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
