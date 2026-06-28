import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { format } from 'date-fns';
import {
  ArrowLeft,
  Loader2,
  Sparkles,
  Trash2,
  CheckCircle,
} from 'lucide-react';
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
        <Loader2 className="animate-spin text-indigo-600" size={32} />
      </div>
    );
  }

  if (error || !prayer) {
    return (
      <div className="p-4 max-w-2xl mx-auto">
        <button
          onClick={() => navigate('/prayers')}
          className="flex items-center gap-1 text-gray-500 hover:text-gray-700 mb-4 text-sm"
        >
          <ArrowLeft size={18} />
          Back to requests
        </button>
        <div className="bg-red-50 text-red-700 p-4 rounded-xl text-sm">
          Prayer request not found.
        </div>
      </div>
    );
  }

  return (
    <div className="p-4 max-w-2xl mx-auto">
      <button
        onClick={() => navigate('/prayers')}
        className="flex items-center gap-1 text-gray-500 hover:text-gray-700 mb-4 text-sm"
      >
        <ArrowLeft size={18} />
        Back to requests
      </button>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
        {prayer.isAnswered && (
          <div className="flex items-center gap-2 bg-green-50 text-green-800 px-4 py-3 rounded-xl mb-5">
            <Sparkles size={20} />
            <div>
              <p className="font-medium">Praise Report</p>
              {prayer.answeredDate && (
                <p className="text-sm text-green-700">
                  Answered on {format(new Date(prayer.answeredDate), 'MMMM d, yyyy')}
                </p>
              )}
            </div>
          </div>
        )}

        <h1 className="text-2xl font-bold text-gray-900">{prayer.title}</h1>

        <p className="text-sm text-gray-400 mt-2">
          Posted {format(new Date(prayer.prayerDate), 'MMMM d, yyyy')}
        </p>

        {prayer.categories.length > 0 && (
          <div className="flex flex-wrap gap-2 mt-4">
            {prayer.categories.map((cat) => (
              <span
                key={cat}
                className="text-xs bg-indigo-50 text-indigo-700 px-2.5 py-1 rounded-full"
              >
                {cat}
              </span>
            ))}
          </div>
        )}

        {prayer.content && (
          <p className="text-gray-700 mt-5 leading-relaxed whitespace-pre-wrap">
            {prayer.content}
          </p>
        )}
      </div>

      <div className="mt-6 space-y-3">
        {!prayer.isAnswered && (
          <button
            onClick={handleMarkAnswered}
            disabled={markAnswered.isPending}
            className="w-full flex items-center justify-center gap-2 bg-green-600 text-white py-3 rounded-xl font-medium hover:bg-green-700 disabled:opacity-50 transition"
          >
            {markAnswered.isPending ? (
              <Loader2 size={20} className="animate-spin" />
            ) : (
              <CheckCircle size={20} />
            )}
            Mark as Answered
          </button>
        )}

        {markAnswered.isError && (
          <p className="text-red-600 text-sm text-center">
            Failed to mark as answered. Please try again.
          </p>
        )}

        {!showDeleteConfirm ? (
          <button
            onClick={() => setShowDeleteConfirm(true)}
            className="w-full flex items-center justify-center gap-2 text-red-600 py-3 rounded-xl font-medium hover:bg-red-50 transition"
          >
            <Trash2 size={18} />
            Delete Request
          </button>
        ) : (
          <div className="bg-red-50 rounded-xl p-4 space-y-3">
            <p className="text-sm text-red-800 text-center">
              Are you sure? This cannot be undone.
            </p>
            <div className="flex gap-3">
              <button
                onClick={() => setShowDeleteConfirm(false)}
                className="flex-1 py-2 text-sm font-medium text-gray-600 bg-white rounded-lg border border-gray-200"
              >
                Cancel
              </button>
              <button
                onClick={handleDelete}
                disabled={deletePrayer.isPending}
                className="flex-1 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 disabled:opacity-50"
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
