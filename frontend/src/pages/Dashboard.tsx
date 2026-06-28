import { Link } from 'react-router-dom';
import { Loader2, Plus, Sparkles } from 'lucide-react';
import { usePrayers } from '@/hooks/usePrayers';
import PrayerCard from '@/components/PrayerCard';
import ProtectedRoute from '@/components/ProtectedRoute';

export default function Dashboard() {
  const { data: prayers = [], isLoading } = usePrayers();

  const active = prayers.filter((p) => !p.isAnswered);
  const answered = prayers.filter((p) => p.isAnswered);
  const recentActive = active.slice(0, 3);

  return (
    <div className="p-4 max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Dashboard</h1>

      <div className="grid grid-cols-2 gap-4 mb-8">
        <div className="bg-white p-5 rounded-xl shadow-sm border border-gray-100">
          <p className="text-3xl font-bold text-indigo-600">{active.length}</p>
          <p className="text-sm text-gray-500 mt-1">Active Requests</p>
        </div>
        <div className="bg-white p-5 rounded-xl shadow-sm border border-gray-100">
          <p className="text-3xl font-bold text-green-600">{answered.length}</p>
          <p className="text-sm text-gray-500 mt-1">Praise Reports</p>
        </div>
      </div>

      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold text-gray-800">Recent Requests</h2>
        <Link to="/add" className="text-indigo-600 text-sm font-medium hover:text-indigo-700">
          <Plus size={16} className="inline mr-0.5" />
          New
        </Link>
      </div>

      {isLoading && (
        <div className="flex justify-center py-12">
          <Loader2 className="animate-spin text-indigo-600" size={28} />
        </div>
      )}

      {!isLoading && recentActive.length === 0 && (
        <div className="bg-white rounded-xl p-8 text-center border border-gray-100">
          <p className="text-gray-500 mb-4">No active requests yet.</p>
          <Link
            to="/add"
            className="inline-flex items-center gap-2 bg-indigo-600 text-white px-5 py-2.5 rounded-lg text-sm font-medium hover:bg-indigo-700"
          >
            <Plus size={18} />
            Share a prayer request
          </Link>
        </div>
      )}

      <div className="space-y-3">
        {recentActive.map((prayer) => (
          <PrayerCard key={prayer.id} prayer={prayer} />
        ))}
      </div>

      {active.length > 3 && (
        <Link
          to="/prayers"
          className="block text-center text-indigo-600 text-sm font-medium mt-4 hover:text-indigo-700"
        >
          View all {active.length} active requests
        </Link>
      )}

      {answered.length > 0 && (
        <div className="mt-8">
          <Link
            to="/prayers"
            className="flex items-center gap-2 text-green-700 bg-green-50 px-4 py-3 rounded-xl text-sm font-medium hover:bg-green-100 transition"
          >
            <Sparkles size={18} />
            {answered.length} praise report{answered.length !== 1 ? 's' : ''} — view all
          </Link>
        </div>
      )}

      <div className="mt-8 bg-gradient-to-r from-indigo-50 to-violet-50 p-6 rounded-2xl text-center">
        <p className="italic text-gray-700 text-sm">
          &ldquo;Do not be anxious about anything, but in every situation, by prayer and
          petition, with thanksgiving, present your requests to God.&rdquo;
        </p>
        <p className="text-xs text-gray-500 mt-3">— Philippians 4:6</p>
      </div>
    </div>
  );
}

export function ProtectedDashboard() {
  return (
    <ProtectedRoute>
      <Dashboard />
    </ProtectedRoute>
  );
}
