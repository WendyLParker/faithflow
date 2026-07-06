import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, Loader2, Search } from 'lucide-react';
import { usePrayers } from '@/hooks/usePrayers';
import PrayerCard from '@/components/PrayerCard';

export default function SearchRequests() {
  const { data: prayers = [], isLoading, error } = usePrayers();
  const [query, setQuery] = useState('');

  const results = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return [];

    return prayers.filter((p) => {
      const inTitle = p.title.toLowerCase().includes(q);
      const inContent = p.content?.toLowerCase().includes(q);
      const inCategories = p.categories.some((c) => c.toLowerCase().includes(q));
      return inTitle || inContent || inCategories;
    });
  }, [prayers, query]);

  return (
    <div className="page-container">
      <Link to="/dashboard" className="back-link">
        <ArrowLeft size={16} />
        Back to dashboard
      </Link>

      <h1 className="page-title">Search Requests</h1>
      <p className="page-subtitle">Search across your submitted requests.</p>

      <label className="block mb-6">
        <span className="form-label">Search</span>
        <div className="relative">
          <Search
            size={18}
            className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-500"
            aria-hidden
          />
          <input
            type="search"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Title, category, or keyword…"
            className="form-input pl-10"
          />
        </div>
      </label>

      {isLoading && (
        <div className="flex justify-center py-12">
          <Loader2 className="animate-spin text-[#34C759]" size={28} />
        </div>
      )}

      {error && (
        <div className="alert-error">Failed to load requests. Please try again.</div>
      )}

      {!isLoading && !error && query.trim() && results.length === 0 && (
        <p className="text-center text-neutral-400 py-12">No requests match your search.</p>
      )}

      {!query.trim() && !isLoading && (
        <p className="text-center text-neutral-500 text-sm py-8">
          Enter a term to search your requests.
        </p>
      )}

      <div className="space-y-3">
        {results.map((prayer) => (
          <PrayerCard key={prayer.id} prayer={prayer} />
        ))}
      </div>
    </div>
  );
}
