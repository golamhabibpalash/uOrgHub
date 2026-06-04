import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { createUser, getRoles, type RoleDto } from '../../api/auth';

export default function UserCreatePage() {
  const navigate = useNavigate();
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [form, setForm] = useState({
    username: '', email: '', firstName: '', lastName: '',
    password: '', confirmPassword: '', roleIds: [] as string[],
  });

  useEffect(() => {
    getRoles().then(setRoles).catch(() => {});
  }, []);

  const toggleRole = (id: string) => {
    setForm(f => ({
      ...f,
      roleIds: f.roleIds.includes(id)
        ? f.roleIds.filter(x => x !== id)
        : [...f.roleIds, id],
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (form.password !== form.confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    setLoading(true);
    try {
      await createUser({
        username: form.username,
        email: form.email,
        firstName: form.firstName,
        lastName: form.lastName,
        password: form.password,
        roleIds: form.roleIds,
      });
      navigate('/admin/users');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to create user.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const field = (label: string, key: string, type = 'text') => (
    <div>
      <label className="block text-slate-300 text-xs font-medium mb-1.5">{label}</label>
      <input type={type} required value={(form as any)[key] ?? ''}
        onChange={e => setForm(f => ({ ...f, [key]: e.target.value }))}
        className="w-full bg-slate-800 border border-slate-700 text-white text-sm rounded-lg px-3 py-2 focus:outline-none focus:border-primary-500"
      />
    </div>
  );

  return (
    <div className="p-6 max-w-2xl">
      <button onClick={() => navigate('/admin/users')}
        className="flex items-center gap-2 text-slate-400 hover:text-slate-200 text-sm mb-4">
        <ArrowLeft size={16} /> Back to Users
      </button>

      <h1 className="text-white text-xl font-semibold mb-6">Create New User</h1>

      {error && (
        <div className="bg-red-500/10 border border-red-500/20 text-red-400 text-sm rounded-lg px-4 py-3 mb-4">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          {field('First Name', 'firstName')}
          {field('Last Name', 'lastName')}
        </div>
        {field('Username', 'username')}
        {field('Email', 'email', 'email')}

        <div className="grid grid-cols-2 gap-4">
          {field('Password', 'password', 'password')}
          {field('Confirm Password', 'confirmPassword', 'password')}
        </div>

        <div>
          <label className="block text-slate-300 text-xs font-medium mb-2">Roles</label>
          <div className="flex flex-wrap gap-2">
            {roles.map(role => (
              <button key={role.id} type="button" onClick={() => toggleRole(role.id)}
                className={`px-3 py-1.5 rounded-lg border text-sm transition-all ${form.roleIds.includes(role.id)
                  ? 'bg-primary-600/20 border-primary-500/50 text-primary-300'
                  : 'bg-slate-800 border-slate-700 text-slate-400 hover:border-slate-500'}`}>
                {role.name}
              </button>
            ))}
            {roles.length === 0 && <span className="text-slate-500 text-sm">No roles available</span>}
          </div>
        </div>

        <button type="submit" disabled={loading}
          className="flex items-center gap-2 bg-primary-600 hover:bg-primary-500 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors disabled:opacity-50">
          <Save size={16} /> {loading ? 'Creating...' : 'Create User'}
        </button>
      </form>
    </div>
  );
}
