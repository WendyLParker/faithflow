import { Plus, Home, List, Bot, Flame, LogOut } from 'lucide-react';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate } from 'react-router-dom';

import Register from './pages/Register';
import Confirm from './pages/Confirm';
import Login from './pages/Login';
import { ProtectedDashboard } from './pages/Dashboard';
import PrayerList from './pages/PrayerList';
import CreatePrayer from './pages/CreatePrayer';
import PrayerDetail from './pages/PrayerDetail';
import ProtectedRoute from './components/ProtectedRoute';

function AppContent() {
  const navigate = useNavigate();
  const isLoggedIn = !!localStorage.getItem('accessToken');

  const handleLogout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('idToken');
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-gradient-to-r from-indigo-600 to-violet-600 text-white p-5 shadow-md">
        <div className="flex items-center justify-between">
          <div
            className="flex items-center gap-3 cursor-pointer"
            onClick={() => navigate(isLoggedIn ? '/dashboard' : '/login')}
          >
            <Flame className="w-8 h-8" />
            <h1 className="text-3xl font-bold tracking-tight">FaithFlow</h1>
          </div>

          {isLoggedIn && (
            <button
              onClick={handleLogout}
              className="flex items-center gap-2 px-4 py-2 bg-white/20 hover:bg-white/30 rounded-lg text-sm transition"
            >
              <LogOut size={18} />
              Logout
            </button>
          )}
        </div>

        <p className="text-center text-indigo-100 mt-1 text-sm">
          Grow closer to God, one prayer at a time
        </p>
      </header>

      <main className="pb-24 min-h-[calc(100vh-180px)]">
        <Routes>
          <Route path="/register" element={<Register />} />
          <Route path="/confirm" element={<Confirm />} />
          <Route path="/login" element={<Login />} />
          <Route path="/dashboard" element={<ProtectedDashboard />} />
          <Route
            path="/prayers"
            element={
              <ProtectedRoute>
                <PrayerList />
              </ProtectedRoute>
            }
          />
          <Route
            path="/prayers/:id"
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
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </main>

      <nav className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 shadow">
        <div className="max-w-md mx-auto flex justify-around items-center py-1">
          <button
            onClick={() => navigate(isLoggedIn ? '/dashboard' : '/login')}
            className="flex flex-col items-center py-3 px-4 text-indigo-600"
          >
            <Home size={26} />
            <span className="text-[10px] mt-1">Home</span>
          </button>

          {isLoggedIn && (
            <>
              <button
                onClick={() => navigate('/prayers')}
                className="flex flex-col items-center py-3 px-4 text-gray-500"
              >
                <List size={26} />
                <span className="text-[10px] mt-1">Prayer Requests</span>
              </button>

              <button
                onClick={() => navigate('/add')}
                className="flex flex-col items-center -mt-8 bg-indigo-600 text-white p-5 rounded-3xl shadow-2xl active:scale-95 transition"
              >
                <Plus size={36} strokeWidth={3} />
              </button>
            </>
          )}

          <button
            onClick={() => navigate(isLoggedIn ? '/dashboard' : '/login')}
            className="flex flex-col items-center py-3 px-4 text-gray-500"
          >
            <Bot size={26} />
            <span className="text-[10px] mt-1">AI</span>
          </button>
        </div>
      </nav>
    </div>
  );
}

function App() {
  return (
    <Router>
      <AppContent />
    </Router>
  );
}

export default App;
