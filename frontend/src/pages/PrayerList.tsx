import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Loader2 } from 'lucide-react';
import { usePrayers } from '@/hooks/usePrayers';
import PrayerCard from '@/components/PrayerCard';

type Filter = 'active' | 'answered';

export default function PrayerList() {
  const { data: prayers = [], isLoading, error } = usePrayers();
  const [filter, setFilter] = useState<Filter>('active');

  const filtered = prayers.filter((p) =>
    filter === 'active' ? !p.isAnswered : p.isAnswered
  );

  return (
    <div className="p-4 max-w-2xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Prayer Requests</h1>
        <Link
          to="/add"
          className="flex items-center gap-1.5 bg-indigo-600 text-white text-sm px-4 py-2 rounded-lg hover:bg-indigo-700 transition"
        >
          <Plus size={18} />
          New
        </Link>
      </div>

      <div className="flex gap-2 mb-6">
        <button
          onClick={() => setFilter('active')}
          className={`flex-1 py-2 text-sm font-medium rounded-lg transition ${
            filter === 'active'
              ? 'bg-indigo-600 text-white'
              : 'bg-white text-gray-600 border border-gray-200'
          }`}
        >
          Active ({prayers.filter((p) => !p.isAnswered).length})
        </button>
        <button
          onClick={() => setFilter('answered')}
          className={`flex-1 py-2 text-sm font-medium rounded-lg transition ${
            filter === 'answered'
              ? 'bg-green-600 text-white'
              : 'bg-white text-gray-600 border border-gray-200'
          }`}
        >
          Praise Reports ({prayers.filter((p) => p.isAnswered).length})
        </button>
      </div>

      {isLoading && (
        <div className="flex justify-center py-16">
          <Loader2 className="animate-spin text-indigo-600" size={32} />
        </div>
      )}

      {error && (
        <div className="bg-red-50 text-red-700 p-4 rounded-xl text-sm">
          Failed to load prayers. Please try again.
        </div>
      )}

      {!isLoading && !error && filtered.length === 0 && (
        <div className="text-center py-16">
          <p className="text-gray-500 mb-4">
            {filter === 'active'
              ? 'No active prayer requests yet.'
              : 'No praise reports yet.'}
          </p>
          {filter === 'active' && (
            <Link
              to="/add"
              className="inline-flex items-center gap-2 text-indigo-600 font-medium hover:text-indigo-700"
            >
              <Plus size={18} />
              Share your first request
            </Link>
          )}
        </div>
      )}

      <div className="space-y-3">
        {filtered.map((prayer) => (
          <PrayerCard key={prayer.id} prayer={prayer} />
        ))}
      </div>
    </div>
  );
}
