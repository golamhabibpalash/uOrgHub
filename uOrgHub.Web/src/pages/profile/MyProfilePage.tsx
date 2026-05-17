import { useState } from 'react';
import { User, Lock, Shield, Eye, EyeOff } from 'lucide-react';
import { updateProfile, changePassword, toggle2FA } from '../../api/auth';
import { useAuthStore } from '../../store/authStore';

export default function MyProfilePage() {
  const { user, setUser } = useAuthStore();
  const [tab, setTab] = useState<'info' | 'password' | 'security'>('info');
  const [form, setForm] = useState({ firstName: user?.firstName ?? '', lastName: user?.lastName ?? '', email: user?.email ?? '', phoneNumber: user?.phoneNumber ?? '' });
  const [pw, setPw] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [showPw, setShowPw] = useState(false);
  const [loading, setLoading] = useState(false);
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const flash = (type: 'success' | 'error', text: string) => {
    setMsg({ type, text });
    setTimeout(() => setMsg(null), 3000);
  };

  const handleProfileSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const updated = await updateProfile(form);
      setUser(updated as unknown as typeof user extends null ? never : NonNullable<typeof user>);
      flash('success', 'Profile updated successfully.');
    } catch { flash('error', 'Failed to update profile.'); }
    finally { setLoading(false); }
  };

  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();
    if (pw.newPassword !== pw.confirmPassword) { flash('error', 'Passwords do not match.'); return; }
    setLoading(true);
    try {
      await changePassword(pw);
      setPw({ currentPassword: '', newPassword: '', confirmPassword: '' });
      flash('success', 'Password changed. You may need to login again on other devices.');
    } catch (err: unknown) {
      flash('error', (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to change password.');
    } finally { setLoading(false); }
  };

  const handle2FAToggle = async () => {
    if (!user) return;
    setLoading(true);
    try {
      await toggle2FA(!user.isTwoFactorEnabled, user.isTwoFactorEnabled ? undefined : 'Email');
      flash('success', `2FA ${user.isTwoFactorEnabled ? 'disabled' : 'enabled'}.`);
    } catch { flash('error', 'Failed to update 2FA settings.'); }
    finally { setLoading(false); }
  };

  if (!user) return null;

  const Field = ({ label, value, onChange, type = 'text' }: { label: string; value: string; onChange: (v: string) => void; type?: string }) => (
    <div>
      <label className="block text-slate-300 text-sm font-medium mb-1.5">{label}</label>
      <input type={type} value={value} onChange={e => onChange(e.target.value)}
        className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg px-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
    </div>
  );

  return (
    <div className="p-6 max-w-2xl">
      <div className="flex items-center gap-4 mb-6">
        <div className="w-14 h-14 rounded-full bg-primary-700 flex items-center justify-center text-white text-xl font-semibold">
          {user.firstName[0]}{user.lastName[0]}
        </div>
        <div>
          <h1 className="text-white text-xl font-semibold">{user.fullName}</h1>
          <p className="text-slate-400 text-sm">{user.email} · {user.roles.join(', ')}</p>
        </div>
      </div>

      {msg && (
        <div className={`text-sm rounded-lg px-4 py-3 mb-4 ${msg.type === 'success' ? 'bg-green-500/10 border border-green-500/30 text-green-400' : 'bg-red-500/10 border border-red-500/30 text-red-400'}`}>
          {msg.text}
        </div>
      )}

      <div className="flex gap-1 mb-6 border-b border-slate-700">
        {([['info', 'Profile', User], ['password', 'Password', Lock], ['security', 'Security', Shield]] as const).map(([key, label, Icon]) => (
          <button key={key} onClick={() => setTab(key)}
            className={`flex items-center gap-2 px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${tab === key ? 'border-primary-500 text-primary-400' : 'border-transparent text-slate-400 hover:text-slate-200'}`}>
            <Icon size={14} /> {label}
          </button>
        ))}
      </div>

      {tab === 'info' && (
        <form onSubmit={handleProfileSave} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Field label="First Name" value={form.firstName} onChange={v => setForm(f => ({ ...f, firstName: v }))} />
            <Field label="Last Name" value={form.lastName} onChange={v => setForm(f => ({ ...f, lastName: v }))} />
          </div>
          <Field label="Email" type="email" value={form.email} onChange={v => setForm(f => ({ ...f, email: v }))} />
          <Field label="Phone Number" value={form.phoneNumber} onChange={v => setForm(f => ({ ...f, phoneNumber: v }))} />
          <button type="submit" disabled={loading}
            className="bg-primary-600 hover:bg-primary-500 disabled:bg-primary-800 text-white text-sm font-medium px-5 py-2 rounded-lg transition-colors">
            {loading ? 'Saving...' : 'Save Changes'}
          </button>
        </form>
      )}

      {tab === 'password' && (
        <form onSubmit={handlePasswordChange} className="space-y-4">
          <div className="relative">
            <label className="block text-slate-300 text-sm font-medium mb-1.5">Current Password</label>
            <div className="relative">
              <input type={showPw ? 'text' : 'password'} required value={pw.currentPassword} onChange={e => setPw(p => ({ ...p, currentPassword: e.target.value }))}
                className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-4 pr-10 py-2.5 focus:outline-none focus:border-primary-500" />
              <button type="button" onClick={() => setShowPw(v => !v)} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400">
                {showPw ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>
          </div>
          <div>
            <label className="block text-slate-300 text-sm font-medium mb-1.5">New Password</label>
            <input type={showPw ? 'text' : 'password'} required value={pw.newPassword} onChange={e => setPw(p => ({ ...p, newPassword: e.target.value }))}
              className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg px-4 py-2.5 focus:outline-none focus:border-primary-500" />
          </div>
          <div>
            <label className="block text-slate-300 text-sm font-medium mb-1.5">Confirm New Password</label>
            <input type={showPw ? 'text' : 'password'} required value={pw.confirmPassword} onChange={e => setPw(p => ({ ...p, confirmPassword: e.target.value }))}
              className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg px-4 py-2.5 focus:outline-none focus:border-primary-500" />
          </div>
          <button type="submit" disabled={loading}
            className="bg-primary-600 hover:bg-primary-500 disabled:bg-primary-800 text-white text-sm font-medium px-5 py-2 rounded-lg transition-colors">
            {loading ? 'Changing...' : 'Change Password'}
          </button>
        </form>
      )}

      {tab === 'security' && (
        <div className="space-y-4">
          <div className="bg-slate-800 border border-slate-700 rounded-lg p-5 flex items-center justify-between">
            <div>
              <div className="text-white font-medium text-sm">Two-Factor Authentication</div>
              <div className="text-slate-400 text-xs mt-0.5">
                {user.isTwoFactorEnabled ? `Enabled via ${user.twoFactorMethod}` : 'Add an extra layer of security to your account'}
              </div>
            </div>
            <button onClick={handle2FAToggle} disabled={loading}
              className={`text-sm font-medium px-4 py-2 rounded-lg transition-colors ${user.isTwoFactorEnabled ? 'bg-red-600/20 text-red-400 hover:bg-red-600/30' : 'bg-primary-600 hover:bg-primary-500 text-white'}`}>
              {user.isTwoFactorEnabled ? 'Disable 2FA' : 'Enable 2FA'}
            </button>
          </div>

          <div className="bg-slate-800 border border-slate-700 rounded-lg p-5">
            <div className="text-white font-medium text-sm mb-3">Assigned Roles</div>
            <div className="flex gap-2 flex-wrap">
              {user.roles.map(r => <span key={r} className="bg-primary-600/20 text-primary-300 text-xs px-2.5 py-1 rounded-full">{r}</span>)}
            </div>
          </div>

          <div className="bg-slate-800 border border-slate-700 rounded-lg p-5">
            <div className="text-white font-medium text-sm mb-3">Permissions ({user.claims.length})</div>
            <div className="flex gap-2 flex-wrap">
              {user.claims.map(c => <span key={c} className="bg-slate-700 text-slate-300 text-xs px-2 py-0.5 rounded">{c}</span>)}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
