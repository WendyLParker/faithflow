import { Link } from 'react-router-dom';
import type { DashboardAction } from '@/config/dashboardActions';
import { tileToneStyles } from '@/config/dashboardActions';

type ActionCardProps = {
  action: DashboardAction;
};

export default function ActionCard({ action }: ActionCardProps) {
  const Icon = action.icon;
  const styles = tileToneStyles[action.tone];

  return (
    <Link
      to={action.to}
      aria-label={`${action.title}: ${action.description}`}
      className={`flex flex-col items-center justify-center aspect-square rounded-2xl p-4 border shadow-sm transition active:scale-[0.98] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#34C759] focus-visible:ring-offset-2 focus-visible:ring-offset-[#1e1e1e] ${styles.tile}`}
    >
      <Icon size={36} strokeWidth={1.5} className={`shrink-0 ${styles.icon}`} aria-hidden />
      <p className="mt-3 text-sm font-semibold text-center leading-snug text-neutral-100">
        {action.title}
      </p>
      <p
        className={`text-[11px] text-center mt-1.5 leading-relaxed line-clamp-3 ${styles.description}`}
      >
        {action.description}
      </p>
    </Link>
  );
}
