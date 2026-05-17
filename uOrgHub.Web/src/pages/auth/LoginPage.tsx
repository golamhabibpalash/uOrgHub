import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Eye, EyeOff, Lock, User, Building2, CheckCircle } from 'lucide-react';
import { login } from '../../api/auth';
import { useAuthStore } from '../../store/authStore';

export default function LoginPage() {
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  const [form, setForm] = useState({ username: '', password: '' });
  const [showPw, setShowPw] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const setupComplete = new URLSearchParams(window.location.search).get('setup') === 'complete';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const result = await login({ username: form.username, password: form.password });
      if (result.requiresTwoFactor) {
        navigate('/auth/2fa', { state: { tempToken: result.tempToken, maskedEmail: result.maskedEmail, maskedPhone: result.maskedPhone, methods: result.twoFactorMethods } });
        return;
      }
      if (result.accessToken && result.refreshToken && result.user) {
        setAuth(result.accessToken, result.refreshToken, result.user);
        navigate('/dashboard');
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Login failed. Please try again.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-900 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="flex items-center justify-center gap-2 mb-3">
            <Building2 className="text-primary-400" size={32} />
            <span className="text-white text-2xl font-bold">uOrgHub</span>
          </div>
          <p className="text-slate-400 text-sm">Civil Construction ERP</p>
        </div>

        <div className="bg-slate-800 rounded-xl border border-slate-700 p-8">
          <h2 className="text-white text-xl font-semibold mb-6">Sign in to your account</h2>

          {setupComplete && (
            <div className="bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-sm rounded-lg px-4 py-3 mb-4 flex items-center gap-2">
              <CheckCircle size={16} />
              Company setup complete! Sign in with your admin username or email.
            </div>
          )}

          {error && (
            <div className="bg-red-500/10 border border-red-500/30 text-red-400 text-sm rounded-lg px-4 py-3 mb-4">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-slate-300 text-sm font-medium mb-1.5">Username or Email</label>
              <div className="relative">
                <User size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                <input
                  type="text"
                  required
                  value={form.username}
                  onChange={e => setForm(f => ({ ...f, username: e.target.value }))}
                  placeholder="Enter username or email"
                  className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500"
                />
              </div>
            </div>

            <div>
              <label className="block text-slate-300 text-sm font-medium mb-1.5">Password</label>
              <div className="relative">
                <Lock size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                <input
                  type={showPw ? 'text' : 'password'}
                  required
                  value={form.password}
                  onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
                  placeholder="Enter password"
                  className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-10 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500"
                />
                <button type="button" onClick={() => setShowPw(v => !v)} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-200">
                  {showPw ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            <div className="flex items-center justify-between">
              <label className="flex items-center gap-2 text-slate-400 text-sm cursor-pointer">
                <input type="checkbox" className="rounded bg-slate-700 border-slate-600" />
                Remember me
              </label>
              <button type="button" onClick={() => navigate('/auth/forgot-password')} className="text-primary-400 text-sm hover:text-primary-300">
                Forgot password?
              </button>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full bg-primary-600 hover:bg-primary-500 disabled:bg-primary-800 disabled:cursor-not-allowed text-white font-medium text-sm rounded-lg py-2.5 transition-colors"
            >
              {loading ? 'Signing in...' : 'Sign in'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
