import { useState, useRef, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { ShieldCheck } from 'lucide-react';
import { verifyOTP, sendOTP } from '../../api/auth';
import { useAuthStore } from '../../store/authStore';

interface LocationState {
  tempToken: string;
  maskedEmail?: string;
  maskedPhone?: string;
  methods?: string[];
}

export default function TwoFactorPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as LocationState;
  const { setAuth } = useAuthStore();
  const [digits, setDigits] = useState(['', '', '', '', '', '']);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [resendCountdown, setResendCountdown] = useState(60);
  const refs = [useRef<HTMLInputElement>(null), useRef<HTMLInputElement>(null), useRef<HTMLInputElement>(null),
                useRef<HTMLInputElement>(null), useRef<HTMLInputElement>(null), useRef<HTMLInputElement>(null)];

  useEffect(() => {
    if (!state?.tempToken) navigate('/login');
  }, [state, navigate]);

  useEffect(() => {
    if (resendCountdown <= 0) return;
    const timer = setTimeout(() => setResendCountdown(c => c - 1), 1000);
    return () => clearTimeout(timer);
  }, [resendCountdown]);

  const handleDigit = (idx: number, val: string) => {
    if (!/^\d?$/.test(val)) return;
    const next = [...digits];
    next[idx] = val;
    setDigits(next);
    if (val && idx < 5) refs[idx + 1].current?.focus();
    if (next.every(d => d)) handleVerify(next.join(''));
  };

  const handleKeyDown = (idx: number, e: React.KeyboardEvent) => {
    if (e.key === 'Backspace' && !digits[idx] && idx > 0) refs[idx - 1].current?.focus();
  };

  const handleVerify = async (code: string) => {
    setError('');
    setLoading(true);
    try {
      const result = await verifyOTP({ tempToken: state.tempToken, otpCode: code, channel: 'Email' });
      setAuth(result.accessToken, result.refreshToken, result.user);
      navigate('/dashboard');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Invalid OTP code.';
      setError(msg);
      setDigits(['', '', '', '', '', '']);
      refs[0].current?.focus();
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    if (resendCountdown > 0) return;
    try {
      await sendOTP('Login', 'Email');
      setResendCountdown(60);
    } catch { /* ignore */ }
  };

  return (
    <div className="min-h-screen bg-slate-900 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-slate-800 rounded-xl border border-slate-700 p-8">
        <div className="text-center mb-6">
          <div className="w-14 h-14 bg-primary-600/20 rounded-full flex items-center justify-center mx-auto mb-4">
            <ShieldCheck className="text-primary-400" size={28} />
          </div>
          <h2 className="text-white text-xl font-semibold">Two-Factor Authentication</h2>
          <p className="text-slate-400 text-sm mt-1">
            Enter the 6-digit code sent to{' '}
            <span className="text-slate-300">{state?.maskedEmail ?? '***'}</span>
          </p>
        </div>

        {error && (
          <div className="bg-red-500/10 border border-red-500/30 text-red-400 text-sm rounded-lg px-4 py-3 mb-4 text-center">
            {error}
          </div>
        )}

        <div className="flex gap-2 justify-center mb-6">
          {digits.map((d, i) => (
            <input
              key={i}
              ref={refs[i]}
              type="text"
              inputMode="numeric"
              maxLength={1}
              value={d}
              onChange={e => handleDigit(i, e.target.value)}
              onKeyDown={e => handleKeyDown(i, e)}
              disabled={loading}
              className="w-11 h-12 bg-slate-700 border border-slate-600 text-white text-xl font-semibold text-center rounded-lg focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500"
            />
          ))}
        </div>

        <button
          type="button"
          disabled={loading || digits.some(d => !d)}
          onClick={() => handleVerify(digits.join(''))}
          className="w-full bg-primary-600 hover:bg-primary-500 disabled:bg-primary-800 disabled:cursor-not-allowed text-white font-medium text-sm rounded-lg py-2.5 transition-colors mb-4"
        >
          {loading ? 'Verifying...' : 'Verify Code'}
        </button>

        <p className="text-center text-slate-400 text-sm">
          Didn't receive the code?{' '}
          {resendCountdown > 0 ? (
            <span className="text-slate-500">Resend in {resendCountdown}s</span>
          ) : (
            <button onClick={handleResend} className="text-primary-400 hover:text-primary-300">Resend</button>
          )}
        </p>

        <button onClick={() => navigate('/login')} className="w-full text-center text-slate-500 hover:text-slate-400 text-sm mt-3">
          Back to login
        </button>
      </div>
    </div>
  );
}
