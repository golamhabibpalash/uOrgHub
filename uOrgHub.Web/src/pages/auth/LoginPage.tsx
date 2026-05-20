import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Eye, EyeOff, Lock, User, CheckCircle, Hexagon } from 'lucide-react';
import { login } from '../../api/auth';
import { useAuthStore } from '../../store/authStore';

function StarField({ count = 200 }) {
  const stars = useMemo(() => {
    return Array.from({ length: count }, (_, i) => ({
      id: i,
      x: Math.random() * 100,
      y: Math.random() * 100,
      size: Math.random() * 2.5 + 0.5,
      opacity: Math.random() * 0.8 + 0.2,
      delay: Math.random() * 4,
      duration: Math.random() * 3 + 2,
    }));
  }, [count]);

  return (
    <div className="absolute inset-0 pointer-events-none overflow-hidden">
      {stars.map((s) => (
        <div
          key={s.id}
          className="absolute rounded-full bg-white animate-pulse"
          style={{
            left: `${s.x}%`,
            top: `${s.y}%`,
            width: `${s.size}px`,
            height: `${s.size}px`,
            opacity: s.opacity,
            animationDelay: `${s.delay}s`,
            animationDuration: `${s.duration}s`,
          }}
        />
      ))}
    </div>
  );
}

function WireframeLines() {
  return (
    <div className="absolute inset-0 pointer-events-none overflow-hidden opacity-20">
      <svg className="w-full h-full" viewBox="0 0 1440 900" preserveAspectRatio="none">
        <polyline points="0,200 120,180 200,300 350,250 500,400" fill="none" stroke="#00f2fe" strokeWidth="0.5" />
        <polyline points="1440,150 1320,130 1240,280 1090,220 940,370" fill="none" stroke="#00f2fe" strokeWidth="0.5" />
        <polyline points="0,600 100,620 180,550 300,580 420,500" fill="none" stroke="#4facfe" strokeWidth="0.5" />
        <polyline points="1440,700 1300,680 1200,750 1080,720 950,800" fill="none" stroke="#4facfe" strokeWidth="0.5" />
        <line x1="100" y1="0" x2="100" y2="900" stroke="#00f2fe" strokeWidth="0.3" strokeDasharray="4,8" opacity="0.4" />
        <line x1="1340" y1="0" x2="1340" y2="900" stroke="#00f2fe" strokeWidth="0.3" strokeDasharray="4,8" opacity="0.4" />
        <polygon points="200,300 350,250 400,380 250,420" fill="none" stroke="#00f2fe" strokeWidth="0.4" opacity="0.3" />
        <polygon points="1240,280 1090,220 1040,350 1190,390" fill="none" stroke="#00f2fe" strokeWidth="0.4" opacity="0.3" />
      </svg>
    </div>
  );
}

function NebulaGlows() {
  return (
    <div className="absolute inset-0 pointer-events-none overflow-hidden">
      <div className="absolute -top-40 -left-40 w-[600px] h-[600px] rounded-full bg-teal-500/10 blur-[120px]" />
      <div className="absolute -bottom-40 -right-40 w-[600px] h-[600px] rounded-full bg-cyan-500/10 blur-[120px]" />
      <div className="absolute top-1/3 -right-20 w-[300px] h-[300px] rounded-full bg-blue-500/8 blur-[100px]" />
    </div>
  );
}

function HexLogo() {
  return (
    <div className="flex flex-col items-center gap-2">
      <div className="relative">
        <Hexagon size={48} className="text-transparent" stroke="url(#logoGrad)" strokeWidth={1.5} />
        <svg width="0" height="0" className="absolute">
          <defs>
            <linearGradient id="logoGrad" x1="0%" y1="0%" x2="100%" y2="100%">
              <stop offset="0%" stopColor="#2563eb" />
              <stop offset="50%" stopColor="#11998e" />
              <stop offset="100%" stopColor="#38ef7d" />
            </linearGradient>
            <linearGradient id="btnGrad" x1="0%" y1="0%" x2="100%" y2="0%">
              <stop offset="0%" stopColor="#2563eb" />
              <stop offset="100%" stopColor="#11998e" />
            </linearGradient>
            <linearGradient id="btnGradHover" x1="0%" y1="0%" x2="100%" y2="0%">
              <stop offset="0%" stopColor="#1d4ed8" />
              <stop offset="100%" stopColor="#0d9488" />
            </linearGradient>
          </defs>
        </svg>
        <div className="absolute inset-0 flex items-center justify-center">
          <span className="text-transparent bg-clip-text bg-gradient-to-br from-blue-500 via-teal-400 to-emerald-400 font-bold text-lg">uH</span>
        </div>
      </div>
      <h1 className="text-white text-2xl font-bold tracking-tight">uOrgHub</h1>
      <p className="text-[10px] text-slate-500 tracking-[0.25em] font-medium">ENTERPRISE RESOURCE PLANNING</p>
    </div>
  );
}

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
    <div className="min-h-screen bg-[radial-gradient(ellipse_at_center,_#001a2e_0%,_#000610_70%)] relative overflow-hidden">

      <StarField count={300} />
      <NebulaGlows />
      <WireframeLines />

      <div className="relative z-10 min-h-screen flex items-center justify-center p-4">
        <div className="w-full max-w-sm">
          <div className="text-center mb-10">
            <HexLogo />
          </div>

          <div
            className="rounded-2xl p-8 relative"
            style={{
              background: 'rgba(15, 23, 42, 0.45)',
              backdropFilter: 'blur(16px) saturate(120%)',
              WebkitBackdropFilter: 'blur(16px) saturate(120%)',
              border: '1px solid rgba(255, 255, 255, 0.08)',
              boxShadow: '0 8px 32px 0 rgba(0, 0, 0, 0.37)',
            }}
          >
            <div className="absolute left-0 top-4 bottom-4 w-px bg-gradient-to-b from-transparent via-cyan-500/30 to-transparent" />
            <div className="absolute right-0 top-8 bottom-8 w-px bg-gradient-to-b from-transparent via-teal-500/20 to-transparent" />

            <div className="relative">
              <h2 className="text-white text-xl font-bold mb-1">Welcome Back</h2>
              <p className="text-slate-400 text-sm mb-6">Sign In to Your Account</p>

              {setupComplete && (
                <div className="bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-sm rounded-lg px-4 py-3 mb-4 flex items-center gap-2">
                  <CheckCircle size={16} />
                  Company setup complete! Sign in with your admin credentials.
                </div>
              )}

              {error && (
                <div className="bg-red-500/10 border border-red-500/20 text-red-400 text-sm rounded-lg px-4 py-3 mb-4">
                  {error}
                </div>
              )}

              <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                  <label className="block text-slate-300 text-xs font-medium mb-1.5">Username or Email</label>
                  <div className="relative group">
                    <User size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500 group-focus-within:text-cyan-400 transition-colors" />
                    <input
                      type="text"
                      required
                      value={form.username}
                      onChange={e => setForm(f => ({ ...f, username: e.target.value }))}
                      placeholder="Enter username or email"
                      className="w-full bg-[rgba(0,0,0,0.4)] border border-slate-700/60 text-white text-sm rounded-xl pl-9 pr-4 py-2.5 focus:outline-none focus:border-cyan-500/50 focus:ring-1 focus:ring-cyan-500/20 transition-all placeholder:text-slate-600"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-slate-300 text-xs font-medium mb-1.5">Password</label>
                  <div className="relative group">
                    <Lock size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500 group-focus-within:text-cyan-400 transition-colors" />
                    <input
                      type={showPw ? 'text' : 'password'}
                      required
                      value={form.password}
                      onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
                      placeholder="Enter password"
                      className="w-full bg-[rgba(0,0,0,0.4)] border border-slate-700/60 text-white text-sm rounded-xl pl-9 pr-10 py-2.5 focus:outline-none focus:border-cyan-500/50 focus:ring-1 focus:ring-cyan-500/20 transition-all placeholder:text-slate-600"
                    />
                    <button type="button" onClick={() => setShowPw(v => !v)} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-500 hover:text-slate-300 transition-colors">
                      {showPw ? <EyeOff size={15} /> : <Eye size={15} />}
                    </button>
                  </div>
                </div>

                <div className="flex items-center justify-end">
                  <button type="button" onClick={() => navigate('/auth/forgot-password')} className="text-cyan-400 text-xs hover:text-cyan-300 transition-colors">
                    Forgot Password?
                  </button>
                </div>

                <button
                  type="submit"
                  disabled={loading}
                  className="relative w-full py-2.5 rounded-full text-white text-sm font-semibold tracking-wider overflow-hidden group transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  style={{
                    background: 'linear-gradient(90deg, #2563eb, #11998e)',
                    boxShadow: '0 4px 20px rgba(37, 99, 235, 0.3)',
                  }}
                >
                  <span
                    className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-300"
                    style={{ background: 'linear-gradient(90deg, #1d4ed8, #0d9488)' }}
                  />
                  <span className="absolute inset-0 bg-gradient-to-b from-white/15 to-transparent rounded-full" />
                  <span className="relative z-10 flex items-center justify-center gap-2">
                    {loading ? 'SIGNING IN...' : 'SIGN IN'}
                  </span>
                </button>
              </form>

              <p className="text-center text-slate-500 text-xs mt-6">
                Don&apos;t have an account?{' '}
                <button type="button" onClick={() => navigate('/register')} className="text-cyan-400 hover:text-cyan-300 transition-colors font-medium">
                  Register Now
                </button>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
