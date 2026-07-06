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
      className="block content-card-sm hover:border-neutral-500 transition"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <h3 className="font-semibold text-neutral-100 truncate">{prayer.title}</h3>
            {prayer.isAnswered && (
              <span className="badge-success shrink-0">
                <Sparkles size={12} />
                Answered
              </span>
            )}
          </div>

          {prayer.content && (
            <p className="text-neutral-400 text-sm mt-1 line-clamp-2">{prayer.content}</p>
          )}

          <div className="flex flex-wrap items-center gap-2 mt-3">
            <span className="text-xs text-neutral-500">{timeAgo}</span>
            {prayer.requestTypeName && (
              <span className="badge">{prayer.requestTypeName}</span>
            )}
            {prayer.categories.map((cat) => (
              <span key={cat} className="badge">
                {cat}
              </span>
            ))}
          </div>
        </div>

        <ChevronRight size={20} className="text-neutral-600 shrink-0 mt-1" />
      </div>
    </Link>
  );
}
