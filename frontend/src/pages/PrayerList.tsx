import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { Plus, Loader2 } from 'lucide-react';
import { usePrayers } from '@/hooks/usePrayers';
import PrayerCard from '@/components/PrayerCard';

type Filter = 'active' | 'answered';

export default function PrayerList() {
  const { data: prayers = [], isLoading, error } = usePrayers();
  const [searchParams] = useSearchParams();
  const scope = searchParams.get('scope');
  const status = searchParams.get('status');
  const isTeamOpenView = scope === 'team' && status === 'open';

  const [filter, setFilter] = useState<Filter>('active');

  useEffect(() => {
    if (status === 'open') {
      setFilter('active');
    }
  }, [status]);

  const filtered = prayers.filter((p) =>
    filter === 'active' ? !p.isAnswered : p.isAnswered
  );

  const pageTitle = isTeamOpenView ? 'Open Requests — My Team' : 'My Requests';
  const activeLabel = isTeamOpenView ? 'Open' : 'Active';

  return (
    <div className="page-container">
      <div className="flex items-center justify-between mb-2">
        <h1 className="page-title mb-0">{pageTitle}</h1>
        <Link
          to="/add"
          className="flex items-center gap-1.5 btn-apple text-sm px-4 py-2 rounded-lg"
        >
          <Plus size={18} />
          New
        </Link>
      </div>
      <p className="page-subtitle">View and manage your submitted requests.</p>

      {isTeamOpenView && (
        <div className="alert-info mb-6">
          Team-wide filtering is coming soon. Showing your open requests for now.
        </div>
      )}

      <div className="flex gap-2 mb-6">
        <button
          onClick={() => setFilter('active')}
          className={filter === 'active' ? 'filter-btn-active' : 'filter-btn'}
        >
          {activeLabel} ({prayers.filter((p) => !p.isAnswered).length})
        </button>
        <button
          onClick={() => setFilter('answered')}
          className={filter === 'answered' ? 'filter-btn-active' : 'filter-btn'}
        >
          Completed ({prayers.filter((p) => p.isAnswered).length})
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
          <p className="text-neutral-400 mb-4">
            {filter === 'active'
              ? 'No open requests yet.'
              : 'No completed requests yet.'}
          </p>
          {filter === 'active' && (
            <Link to="/add" className="inline-flex items-center gap-2 link-accent">
              <Plus size={18} />
              Create your first request
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
