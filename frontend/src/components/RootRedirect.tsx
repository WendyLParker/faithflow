import { Navigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';

function RootRedirect() {
  const { isLoggedIn } = useAuth();
  return <Navigate to={isLoggedIn ? '/dashboard' : '/login'} replace />;
}

export default RootRedirect;
