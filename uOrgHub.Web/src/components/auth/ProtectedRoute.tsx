import { Navigate, useLocation } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { useAuthStore } from '../../store/authStore';

interface Props {
  children: React.ReactNode;
  requiredClaim?: string;
  requiredClaims?: string[];
  requiredRole?: string;
}

function HydrationGuard({ children }: { children: React.ReactNode }) {
  const [hydrated, setHydrated] = useState(useAuthStore.persist.hasHydrated());

  useEffect(() => {
    const unsub = useAuthStore.persist.onFinishHydration(() => setHydrated(true));
    return () => unsub();
  }, []);

  if (!hydrated) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="w-8 h-8 border-4 border-primary-500 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return <>{children}</>;
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

  return <HydrationGuard>{children}</HydrationGuard>;
}
