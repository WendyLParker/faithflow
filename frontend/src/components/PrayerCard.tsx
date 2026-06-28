import { formatDistanceToNow } from 'date-fns';
import { ChevronRight, Sparkles } from 'lucide-react';
import { Link } from 'react-router-dom';
import type { PrayerResponseDto } from '@/services/prayerService';

interface PrayerCardProps {
  prayer: PrayerResponseDto;
}

export default function PrayerCard({ prayer }: PrayerCardProps) {
  const timeAgo = formatDistanceToNow(new Date(prayer.prayerDate), { addSuffix: true });

  return (
    <Link
      to={`/prayers/${prayer.id}`}
      className="block bg-white rounded-xl p-4 shadow-sm border border-gray-100 hover:border-indigo-200 hover:shadow-md transition"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <h3 className="font-semibold text-gray-900 truncate">{prayer.title}</h3>
            {prayer.isAnswered && (
              <span className="inline-flex items-center gap-1 text-xs font-medium text-green-700 bg-green-50 px-2 py-0.5 rounded-full shrink-0">
                <Sparkles size={12} />
                Answered
              </span>
            )}
          </div>

          {prayer.content && (
            <p className="text-gray-600 text-sm mt-1 line-clamp-2">{prayer.content}</p>
          )}

          <div className="flex flex-wrap items-center gap-2 mt-3">
            <span className="text-xs text-gray-400">{timeAgo}</span>
            {prayer.categories.map((cat) => (
              <span
                key={cat}
                className="text-xs bg-indigo-50 text-indigo-700 px-2 py-0.5 rounded-full"
              >
                {cat}
              </span>
            ))}
          </div>
        </div>

        <ChevronRight size={20} className="text-gray-300 shrink-0 mt-1" />
      </div>
    </Link>
  );
}
