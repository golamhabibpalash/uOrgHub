import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';

interface Props {
  children: React.ReactNode;
  requiredClaim?: string;
  requiredClaims?: string[];
  requiredRole?: string;
}

export default function ProtectedRoute({ children, requiredClaim, requiredClaims, requiredRole }: Props) {
  const location = useLocation();
  const { isAuthenticated, hasClaim, hasAnyClaim, hasRole } = useAuthStore();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requiredRole && !hasRole(requiredRole) && !hasRole('Admin')) {
    return <Navigate to="/access-denied" replace />;
  }

  if (requiredClaim && !hasClaim(requiredClaim) && !hasRole('Admin')) {
    return <Navigate to="/access-denied" replace />;
  }

  if (requiredClaims && requiredClaims.length > 0 && !hasAnyClaim(...requiredClaims) && !hasRole('Admin')) {
    return <Navigate to="/access-denied" replace />;
  }

  return <>{children}</>;
}
