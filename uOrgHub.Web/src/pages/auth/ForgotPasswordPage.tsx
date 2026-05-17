import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Mail, Lock, KeyRound, Eye, EyeOff } from 'lucide-react';
import { forgotPassword, resetPassword } from '../../api/auth';

type Step = 'email' | 'otp' | 'newpass';

export default function ForgotPasswordPage() {
  const navigate = useNavigate();
  const [step, setStep] = useState<Step>('email');
  const [email, setEmail] = useState('');
  const [otp, setOtp] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [showPw, setShowPw] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleEmail = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(''); setLoading(true);
    try {
      await forgotPassword(email);
      setSuccess(`If an account with that email exists, a reset code has been sent.`);
      setStep('otp');
    } catch { setError('Something went wrong. Try again.'); }
    finally { setLoading(false); }
  };

  const handleReset = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirm) { setError('Passwords do not match.'); return; }
    setError(''); setLoading(true);
    try {
      await resetPassword({ email, otpCode: otp, newPassword, confirmPassword: confirm });
      setSuccess('Password reset successfully! Redirecting to login...');
      setTimeout(() => navigate('/login'), 2000);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Reset failed.';
      setError(msg);
    } finally { setLoading(false); }
  };

  return (
    <div className="min-h-screen bg-slate-900 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-slate-800 rounded-xl border border-slate-700 p-8">
        <h2 className="text-white text-xl font-semibold mb-2">Reset Password</h2>
        <p className="text-slate-400 text-sm mb-6">
          {step === 'email' && 'Enter your email to receive a reset code.'}
          {step === 'otp' && 'Enter the 6-digit code sent to your email.'}
          {step === 'newpass' && 'Enter your new password.'}
        </p>

        {error && <div className="bg-red-500/10 border border-red-500/30 text-red-400 text-sm rounded-lg px-4 py-3 mb-4">{error}</div>}
        {success && <div className="bg-green-500/10 border border-green-500/30 text-green-400 text-sm rounded-lg px-4 py-3 mb-4">{success}</div>}

        {step === 'email' && (
          <form onSubmit={handleEmail} className="space-y-4">
            <div>
              <label className="block text-slate-300 text-sm font-medium mb-1.5">Email Address</label>
              <div className="relative">
                <Mail size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                <input type="email" required value={email} onChange={e => setEmail(e.target.value)}
                  placeholder="you@example.com"
                  className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
              </div>
            </div>
            <button type="submit" disabled={loading}
              className="w-full bg-primary-600 hover:bg-primary-500 disabled:bg-primary-800 text-white font-medium text-sm rounded-lg py-2.5 transition-colors">
              {loading ? 'Sending...' : 'Send Reset Code'}
            </button>
          </form>
        )}

        {step === 'otp' && (
          <div className="space-y-4">
            <div>
              <label className="block text-slate-300 text-sm font-medium mb-1.5">Reset Code</label>
              <div className="relative">
                <KeyRound size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                <input type="text" inputMode="numeric" maxLength={6} value={otp} onChange={e => setOtp(e.target.value)}
                  placeholder="6-digit code"
                  className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
              </div>
            </div>
            <button type="button" onClick={() => { if (otp.length === 6) setStep('newpass'); else setError('Enter the 6-digit code.'); }}
              className="w-full bg-primary-600 hover:bg-primary-500 text-white font-medium text-sm rounded-lg py-2.5 transition-colors">
              Continue
            </button>
          </div>
        )}

        {step === 'newpass' && (
          <form onSubmit={handleReset} className="space-y-4">
            <div>
              <label className="block text-slate-300 text-sm font-medium mb-1.5">New Password</label>
              <div className="relative">
                <Lock size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                <input type={showPw ? 'text' : 'password'} required value={newPassword} onChange={e => setNewPassword(e.target.value)}
                  placeholder="New password"
                  className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-10 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
                <button type="button" onClick={() => setShowPw(v => !v)} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400">
                  {showPw ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>
            <div>
              <label className="block text-slate-300 text-sm font-medium mb-1.5">Confirm Password</label>
              <div className="relative">
                <Lock size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                <input type={showPw ? 'text' : 'password'} required value={confirm} onChange={e => setConfirm(e.target.value)}
                  placeholder="Confirm password"
                  className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
              </div>
            </div>
            <button type="submit" disabled={loading}
              className="w-full bg-primary-600 hover:bg-primary-500 disabled:bg-primary-800 text-white font-medium text-sm rounded-lg py-2.5 transition-colors">
              {loading ? 'Resetting...' : 'Reset Password'}
            </button>
          </form>
        )}

        <button onClick={() => navigate('/login')} className="w-full text-center text-slate-500 hover:text-slate-400 text-sm mt-4">
          Back to login
        </button>
      </div>
    </div>
  );
}
