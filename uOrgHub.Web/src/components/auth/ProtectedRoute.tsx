import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';

interface Props {
  children: React.ReactNode;
  requiredClaim?: string;
  requiredRole?: string;
}

export default function ProtectedRoute({ children, requiredClaim, requiredRole }: Props) {
  const location = useLocation();
  const { isAuthenticated, hasClaim, hasRole } = useAuthStore();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requiredRole && !hasRole(requiredRole) && !hasRole('Admin')) {
    return <Navigate to="/access-denied" replace />;
  }

  if (requiredClaim && !hasClaim(requiredClaim) && !hasRole('Admin')) {
    return <Navigate to="/access-denied" replace />;
  }

  return <>{children}</>;
}
