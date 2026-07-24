import { useState } from 'react';
import {
  Plus,
  Home,
  List,
  LogOut,
  Search,
  LifeBuoy,
  Bell,
  User,
  Settings,
} from 'lucide-react';
import {
  BrowserRouter as Router,
  Routes,
  Route,
  useNavigate,
  useLocation,
} from 'react-router-dom';

import Register from './pages/Register';
import Confirm from './pages/Confirm';
import Login from './pages/Login';
import { ProtectedDashboard } from './pages/Dashboard';
import PrayerList from './pages/PrayerList';
import CreatePrayer from './pages/CreatePrayer';
import PrayerDetail from './pages/PrayerDetail';
import Departments from './pages/Departments';
import GroupManagement from './pages/GroupManagement';
import SearchRequests from './pages/SearchRequests';
import FAQ from './pages/FAQ';
import Profile from './pages/Profile';
import ProtectedRoute from './components/ProtectedRoute';
import RootRedirect from './components/RootRedirect';
import NotificationPanel from './components/NotificationPanel';
import { AuthProvider, useAuth } from './hooks/useAuth';
import { useUnreadCount } from './hooks/useNotifications';
import { useMyRole } from './hooks/useUserRole';

type NavItem = {
  path: string;
  label: string;
  icon: typeof Home;
  match: (pathname: string) => boolean;
};

const loggedOutNav: NavItem[] = [
  {
    path: '/login',
    label: 'Home',
    icon: Home,
    match: (p) => p === '/login' || p === '/register' || p === '/confirm',
  },
];

const loggedInNav: NavItem[] = [
  {
    path: '/dashboard',
    label: 'Home',
    icon: Home,
    match: (p) => p === '/dashboard',
  },
  {
    path: '/requests',
    label: 'Requests',
    icon: List,
    match: (p) => p.startsWith('/requests'),
  },
  {
    path: '/add',
    label: 'New',
    icon: Plus,
    match: (p) => p === '/add',
  },
  {
    path: '/search',
    label: 'Search',
    icon: Search,
    match: (p) => p === '/search',
  },
  {
    path: '/faq',
    label: 'FAQ',
    icon: LifeBuoy,
    match: (p) => p === '/faq' || p === '/departments' || p === '/group-management',
  },
];

function BottomNavItem({
  item,
  active,
  onClick,
}: {
  item: NavItem;
  active: boolean;
  onClick: () => void;
}) {
  const Icon = item.icon;

  return (
    <button
      type="button"
      onClick={onClick}
      aria-current={active ? 'page' : undefined}
      className={`flex flex-1 flex-col items-center justify-center gap-0.5 py-2 min-h-[3.25rem] transition-colors ${
        active ? 'text-[#d12a44]' : 'text-neutral-500 active:text-white/80'
      }`}
    >
      <Icon size={22} strokeWidth={active ? 2.5 : 1.75} aria-hidden />
      <span className={`text-[10px] leading-none ${active ? 'font-semibold' : 'font-normal'}`}>
        {item.label}
      </span>
    </button>
  );
}

function AppContent() {
  const navigate = useNavigate();
  const location = useLocation();
  const { isLoggedIn, logout, user } = useAuth();
  const [profileOpen, setProfileOpen] = useState(false);
  const [notifOpen, setNotifOpen] = useState(false);

  const { data: unreadCount = 0 } = useUnreadCount();
  const { data: myRole } = useMyRole();
  const hasUnread = isLoggedIn && unreadCount > 0;

  const greeting = isLoggedIn
    ? myRole?.displayName
      ? `Hello, ${myRole.displayName}`
      : user?.email
        ? `Hello, ${user.email}`
        : null
    : null;

  const handleLogout = () => {
    setProfileOpen(false);
    logout();
    navigate('/login');
  };

  const navItems = isLoggedIn ? loggedInNav : loggedOutNav;
  const showBottomNav = isLoggedIn;

  return (
    <div className="min-h-screen w-full bg-[var(--bg)]">
      <header className="sticky top-0 inset-x-0 z-50 w-full bg-black border-b border-white/10 pt-[env(safe-area-inset-top,0px)]">
        <div className="relative flex items-center justify-between h-14 px-4 w-full">
          <button
            type="button"
            onClick={() => navigate(isLoggedIn ? '/dashboard' : '/login')}
            className="min-w-0 text-left"
          >
            <span className="text-xl font-black tracking-tight text-white uppercase">
              Request Manager
            </span>
          </button>

          {isLoggedIn ? (
            <div className="flex items-center gap-0.5 shrink-0">
              {/* User greeting */}
              {greeting && (
                <span className="hidden sm:block text-xs text-neutral-400 mr-1 max-w-[160px] truncate">
                  {greeting}
                </span>
              )}

              {/* Notification bell */}
              <button
                type="button"
                aria-label={hasUnread ? `${unreadCount} unread notifications` : 'Notifications'}
                onClick={() => setNotifOpen(true)}
                className="relative p-2.5 text-white hover:text-white/70 transition-colors"
              >
                <Bell size={22} strokeWidth={2} />
                {hasUnread && (
                  <span
                    className="absolute top-0.5 right-0.5 min-w-[1.1rem] h-[1.1rem] px-[3px] flex items-center justify-center bg-[#d12a44] rounded-full ring-2 ring-black text-[10px] font-bold leading-none text-white"
                    aria-hidden
                  >
                    {unreadCount > 9 ? '9+' : unreadCount}
                  </span>
                )}
              </button>

              {/* Profile menu */}
              <div className="relative">
                <button
                  type="button"
                  aria-label="Account menu"
                  aria-expanded={profileOpen}
                  onClick={() => setProfileOpen((open) => !open)}
                  className="p-2.5 text-white hover:text-white/70 transition-colors"
                >
                  <User size={22} strokeWidth={2} />
                </button>

                {profileOpen && (
                  <>
                    <button
                      type="button"
                      aria-label="Close account menu"
                      className="fixed inset-0 z-40 cursor-default"
                      onClick={() => setProfileOpen(false)}
                    />
                    <div className="absolute right-0 top-full mt-1 z-50 min-w-[12rem] rounded-md bg-[var(--surface)] border border-[var(--trim)] shadow-xl overflow-hidden">
                      {greeting && (
                        <div className="px-4 py-2.5 border-b border-[var(--trim-soft)]">
                          <p className="text-xs text-neutral-500 truncate">{greeting}</p>
                        </div>
                      )}
                      <button
                        type="button"
                        onClick={() => { setProfileOpen(false); navigate('/profile'); }}
                        className="flex w-full items-center gap-2 px-4 py-3 text-sm text-[var(--ink)] hover:bg-neutral-800 transition-colors"
                      >
                        <Settings size={16} />
                        Manage profile
                      </button>
                      <button
                        type="button"
                        onClick={handleLogout}
                        className="flex w-full items-center gap-2 px-4 py-3 text-sm text-[var(--ink)] hover:bg-neutral-800 transition-colors border-t border-[var(--trim-soft)]"
                      >
                        <LogOut size={16} />
                        Sign out
                      </button>
                    </div>
                  </>
                )}
              </div>
            </div>
          ) : (
            <button
              type="button"
              onClick={() => navigate('/login')}
              className="text-sm font-semibold text-white hover:text-white/70 transition-colors"
            >
              Sign In
            </button>
          )}
        </div>
      </header>

      <main className="w-full pb-[calc(3.75rem+env(safe-area-inset-bottom,0px))] min-h-[calc(100vh-3.5rem-env(safe-area-inset-top,0px))]">
        <Routes>
          <Route path="/register" element={<Register />} />
          <Route path="/confirm" element={<Confirm />} />
          <Route path="/login" element={<Login />} />
          <Route path="/dashboard" element={<ProtectedDashboard />} />
          <Route
            path="/requests"
            element={
              <ProtectedRoute>
                <PrayerList />
              </ProtectedRoute>
            }
          />
          <Route
            path="/requests/:id"
            element={
              <ProtectedRoute>
                <PrayerDetail />
              </ProtectedRoute>
            }
          />
          <Route
            path="/add"
            element={
              <ProtectedRoute>
                <CreatePrayer />
              </ProtectedRoute>
            }
          />
          <Route
            path="/departments"
            element={
              <ProtectedRoute>
                <Departments />
              </ProtectedRoute>
            }
          />
          <Route
            path="/group-management"
            element={
              <ProtectedRoute>
                <GroupManagement />
              </ProtectedRoute>
            }
          />
          <Route
            path="/search"
            element={
              <ProtectedRoute>
                <SearchRequests />
              </ProtectedRoute>
            }
          />
          <Route
            path="/faq"
            element={
              <ProtectedRoute>
                <FAQ />
              </ProtectedRoute>
            }
          />
          <Route
            path="/profile"
            element={
              <ProtectedRoute>
                <Profile />
              </ProtectedRoute>
            }
          />
          <Route path="/" element={<RootRedirect />} />
          <Route path="*" element={<RootRedirect />} />
        </Routes>
      </main>

      {showBottomNav && (
        <nav
          aria-label="Main navigation"
          className="fixed bottom-0 inset-x-0 z-50 w-full bg-black border-t border-white/10 pb-[env(safe-area-inset-bottom,0px)]"
        >
          <div className="flex w-full max-w-lg mx-auto">
            {navItems.map((item) => (
              <BottomNavItem
                key={item.path}
                item={item}
                active={item.match(location.pathname)}
                onClick={() => navigate(item.path)}
              />
            ))}
          </div>
        </nav>
      )}

      {/* Notification slide-in panel */}
      <NotificationPanel open={notifOpen} onClose={() => setNotifOpen(false)} />
    </div>
  );
}

function App() {
  return (
    <Router>
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </Router>
  );
}

export default App;
