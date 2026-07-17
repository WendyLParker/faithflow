import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, Loader2, Search } from 'lucide-react';
import { useRequests } from '@/hooks/useRequests';
import RequestCard from '@/components/RequestCard';

export default function SearchRequests() {
  const { data: requests = [], isLoading, error } = useRequests();
  const [query, setQuery] = useState('');

  const results = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return [];

    return requests.filter((r) => {
      const inTitle = r.title.toLowerCase().includes(q);
      const inContent = r.content?.toLowerCase().includes(q);
      const inGroups = r.groupNames.some((g) => g.toLowerCase().includes(q));
      return inTitle || inContent || inGroups;
    });
  }, [requests, query]);

  return (
    <div className="page-container">
       <Link
        to="/dashboard"
        className="btn-apple w-full py-3.5 rounded-xl font-semibold flex items-center justify-center gap-2"
      >
        <ArrowLeft size={20} />
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
            placeholder="Title, type, or keyword…"
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
        {results.map((request) => (
          <RequestCard key={request.id} request={request} />
        ))}
      </div>
    </div>
  );
}
