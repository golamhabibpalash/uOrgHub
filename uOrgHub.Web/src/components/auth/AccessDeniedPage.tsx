import { useNavigate } from 'react-router-dom';
import { ShieldX } from 'lucide-react';

export default function AccessDeniedPage() {
  const navigate = useNavigate();
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] text-center p-8">
      <ShieldX size={56} className="text-red-400 mb-4" />
      <h1 className="text-white text-2xl font-semibold mb-2">Access Denied</h1>
      <p className="text-slate-400 text-sm mb-6 max-w-sm">
        You don't have permission to view this page. Contact your administrator if you believe this is an error.
      </p>
      <button
        onClick={() => navigate(-1)}
        className="bg-slate-700 hover:bg-slate-600 text-white text-sm font-medium px-5 py-2 rounded-lg transition-colors"
      >
        Go Back
      </button>
    </div>
  );
}
