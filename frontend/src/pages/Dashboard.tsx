import ProtectedRoute from '@/components/ProtectedRoute';
import ActionCard from '@/components/ActionCard';
import { dashboardActions } from '@/config/dashboardActions';

export default function Dashboard() {
  return (
    <div className="page-container">
      <header className="mb-6">
        <h1 className="page-title mt-2">What would you like to do?</h1>
        <p className="text-sm text-neutral-400 mt-2 leading-relaxed">
          Submit and track requests across types — prayer requests, supply requests, and more.
        </p>
      </header>

      <nav
        aria-label="Dashboard actions"
        className="grid grid-cols-2 gap-3 sm:gap-4 max-w-md mx-auto"
      >
        {dashboardActions.map((action) => (
          <ActionCard key={action.id} action={action} />
        ))}
      </nav>
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
