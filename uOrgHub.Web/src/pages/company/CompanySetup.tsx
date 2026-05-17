import { useState, useEffect, useRef } from 'react';
import { Building2, Globe, DollarSign, MapPin, Phone, Mail, FileText, User, Lock, Eye, EyeOff, Sparkles, PartyPopper, Upload, X } from 'lucide-react';
import { setupCompany, uploadLogo } from '../../api/company';

const timezones = [
  'Asia/Dhaka', 'Asia/Kolkata', 'Asia/Singapore', 'Asia/Dubai',
  'America/New_York', 'America/Chicago', 'America/Denver', 'America/Los_Angeles',
  'Europe/London', 'Europe/Berlin', 'Europe/Paris', 'Australia/Sydney',
  'Pacific/Auckland', 'Asia/Tokyo', 'Asia/Shanghai', 'Asia/Hong_Kong',
];

const currencies = [
  { code: 'BDT', name: 'Bangladeshi Taka', symbol: '৳' },
  { code: 'USD', name: 'US Dollar', symbol: '$' },
  { code: 'EUR', name: 'Euro', symbol: '€' },
  { code: 'GBP', name: 'British Pound', symbol: '£' },
  { code: 'INR', name: 'Indian Rupee', symbol: '₹' },
  { code: 'SGD', name: 'Singapore Dollar', symbol: 'S$' },
  { code: 'AED', name: 'UAE Dirham', symbol: 'د.إ' },
  { code: 'AUD', name: 'Australian Dollar', symbol: 'A$' },
  { code: 'JPY', name: 'Japanese Yen', symbol: '¥' },
  { code: 'CNY', name: 'Chinese Yuan', symbol: '¥' },
];

function goToLogin() {
  window.location.href = '/login?setup=complete';
}

export default function CompanySetup() {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [step, setStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showPw, setShowPw] = useState(false);
  const [setupComplete, setSetupComplete] = useState(false);
  const [countdown, setCountdown] = useState(5);
  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [formData, setFormData] = useState({
    companyName: '',
    tagLine: '',
    address: '',
    phone: '',
    email: '',
    taxId: '',
    currency: 'BDT',
    timeZone: 'Asia/Dhaka',
    adminUsername: '',
    adminEmail: '',
    adminFirstName: '',
    adminLastName: '',
    adminPassword: '',
  });

  const update = (field: string, value: string) => setFormData(f => ({ ...f, [field]: value }));

  const handleLogoSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setLogoFile(file);
    const reader = new FileReader();
    reader.onload = () => setLogoPreview(reader.result as string);
    reader.readAsDataURL(file);
  };

  const clearLogo = () => {
    setLogoFile(null);
    setLogoPreview(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const result = await setupCompany({
        companyName: formData.companyName,
        tagLine: formData.tagLine || undefined,
        address: formData.address || undefined,
        phone: formData.phone || undefined,
        email: formData.email || undefined,
        taxId: formData.taxId || undefined,
        currency: formData.currency,
        timeZone: formData.timeZone,
        adminUsername: formData.adminUsername,
        adminEmail: formData.adminEmail,
        adminFirstName: formData.adminFirstName,
        adminLastName: formData.adminLastName,
        adminPassword: formData.adminPassword,
      });

      if (logoFile) {
        try {
          await uploadLogo(result.companyId, logoFile);
        } catch {
          // logo upload is non-critical
        }
      }

      setSetupComplete(true);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Setup failed. Please try again.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!setupComplete) return;
    if (countdown <= 0) {
      goToLogin();
      return;
    }
    const t = setTimeout(() => setCountdown(c => c - 1), 1000);
    return () => clearTimeout(t);
  }, [setupComplete, countdown]);

  const inputClass = "w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500";
  const labelClass = "block text-slate-300 text-sm font-medium mb-1.5";

  if (setupComplete) {
    return (
      <div className="min-h-screen bg-slate-900 flex items-center justify-center p-4">
        <div className="text-center max-w-md">
          {logoPreview ? (
            <div className="flex justify-center mb-4">
              <img src={logoPreview} alt="logo" className="h-16 object-contain" />
            </div>
          ) : (
            <div className="flex items-center justify-center gap-2 mb-4">
              <div className="w-10 h-10 rounded-lg bg-primary-500 flex items-center justify-center">
                <Building2 size={22} className="text-white" />
              </div>
              <span className="text-white text-xl font-bold">{formData.companyName}</span>
            </div>
          )}

          {formData.tagLine && (
            <p className="text-slate-500 text-sm mb-6">{formData.tagLine}</p>
          )}

          <div className="relative mb-8">
            <div className="w-24 h-24 rounded-full bg-emerald-500/20 flex items-center justify-center mx-auto animate-bounce">
              <PartyPopper size={48} className="text-emerald-400" />
            </div>
            <Sparkles size={24} className="text-yellow-400 absolute top-0 right-1/4 animate-pulse" />
            <Sparkles size={20} className="text-primary-400 absolute bottom-0 left-1/4 animate-pulse" />
          </div>

          <h1 className="text-3xl font-bold text-white mb-3">
            Welcome to <span className="text-primary-400">{formData.companyName}</span>
          </h1>
          <p className="text-slate-400 text-base mb-2">
            Your company has been set up successfully!
          </p>
          <p className="text-slate-500 text-sm mb-8">
            You'll be redirected to the login page in <span className="text-primary-400 font-semibold">{countdown}</span> seconds.
          </p>

          <button
            onClick={goToLogin}
            className="bg-primary-600 hover:bg-primary-500 text-white font-medium text-sm rounded-lg px-8 py-3 transition-colors"
          >
            Go to Login Now
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-900 flex items-center justify-center p-4">
      <div className="w-full max-w-2xl">
        <div className="text-center mb-8">
          <div className="flex items-center justify-center gap-2 mb-3">
            <Building2 className="text-primary-400" size={32} />
            <span className="text-white text-2xl font-bold">uOrgHub</span>
          </div>
          <p className="text-slate-400 text-sm">Civil Construction ERP — Initial Setup</p>
        </div>

        <div className="bg-slate-800 rounded-xl border border-slate-700 p-8">
          <div className="flex items-center justify-between mb-8">
            <h2 className="text-white text-xl font-semibold">Configure Your Company</h2>
            <div className="flex items-center gap-2 text-sm">
              <span className={`w-8 h-8 rounded-full flex items-center justify-center text-xs font-medium ${step >= 1 ? 'bg-primary-600 text-white' : 'bg-slate-700 text-slate-400'}`}>1</span>
              <span className="text-slate-500">—</span>
              <span className={`w-8 h-8 rounded-full flex items-center justify-center text-xs font-medium ${step >= 2 ? 'bg-primary-600 text-white' : 'bg-slate-700 text-slate-400'}`}>2</span>
            </div>
          </div>

          {error && (
            <div className="bg-red-500/10 border border-red-500/30 text-red-400 text-sm rounded-lg px-4 py-3 mb-4">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            {step === 1 && (
              <div className="space-y-4">
                <p className="text-slate-400 text-sm mb-4">Enter your organization details</p>

                {/* Logo Upload */}
                <div>
                  <label className={labelClass}>Company Logo</label>
                  <div className="flex items-center gap-4">
                    {logoPreview ? (
                      <div className="relative">
                        <img src={logoPreview} alt="preview" className="h-20 w-20 object-contain rounded-lg border border-slate-600" />
                        <button type="button" onClick={clearLogo} className="absolute -top-2 -right-2 w-5 h-5 bg-red-500 rounded-full flex items-center justify-center text-white">
                          <X size={12} />
                        </button>
                      </div>
                    ) : (
                      <div className="h-20 w-20 rounded-lg border-2 border-dashed border-slate-600 flex items-center justify-center text-slate-500">
                        <Building2 size={28} />
                      </div>
                    )}
                    <button type="button" onClick={() => fileInputRef.current?.click()} className="bg-slate-700 hover:bg-slate-600 text-slate-300 text-sm rounded-lg px-4 py-2.5 transition-colors">
                      <Upload size={14} className="inline mr-1" />
                      Upload Logo
                    </button>
                    <input ref={fileInputRef} type="file" accept="image/*" onChange={handleLogoSelect} className="hidden" />
                  </div>
                </div>

                <div>
                  <label className={labelClass}>Company Name *</label>
                  <div className="relative">
                    <Building2 size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                    <input type="text" required value={formData.companyName} onChange={e => update('companyName', e.target.value)} placeholder="Your Company Ltd." className={inputClass} />
                  </div>
                </div>

                <div>
                  <label className={labelClass}>Tagline / Slogan</label>
                  <div className="relative">
                    <FileText size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                    <input type="text" value={formData.tagLine} onChange={e => update('tagLine', e.target.value)} placeholder="Building the future together" className={inputClass} />
                  </div>
                </div>

                <div>
                  <label className={labelClass}>Address</label>
                  <div className="relative">
                    <MapPin size={16} className="absolute left-3 top-3 text-slate-500" />
                    <textarea value={formData.address} onChange={e => update('address', e.target.value)} placeholder="123 Business Avenue, City" rows={2} className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500 resize-none" />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className={labelClass}>Phone</label>
                    <div className="relative">
                      <Phone size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                      <input type="tel" value={formData.phone} onChange={e => update('phone', e.target.value)} placeholder="+880 1234 567890" className={inputClass} />
                    </div>
                  </div>
                  <div>
                    <label className={labelClass}>Email</label>
                    <div className="relative">
                      <Mail size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                      <input type="email" value={formData.email} onChange={e => update('email', e.target.value)} placeholder="info@company.com" className={inputClass} />
                    </div>
                  </div>
                </div>

                <div>
                  <label className={labelClass}>Tax ID / VAT Number</label>
                  <div className="relative">
                    <FileText size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                    <input type="text" value={formData.taxId} onChange={e => update('taxId', e.target.value)} placeholder="VAT-123456" className={inputClass} />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className={labelClass}>Currency *</label>
                    <div className="relative">
                      <DollarSign size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500 z-10" />
                      <select value={formData.currency} onChange={e => update('currency', e.target.value)} className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500 appearance-none">
                        {currencies.map(c => (
                          <option key={c.code} value={c.code}>{c.code} — {c.name} ({c.symbol})</option>
                        ))}
                      </select>
                    </div>
                  </div>
                  <div>
                    <label className={labelClass}>Time Zone *</label>
                    <div className="relative">
                      <Globe size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500 z-10" />
                      <select value={formData.timeZone} onChange={e => update('timeZone', e.target.value)} className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500 appearance-none">
                        {timezones.map(tz => <option key={tz} value={tz}>{tz.replace('_', ' ')}</option>)}
                      </select>
                    </div>
                  </div>
                </div>

                <div className="flex justify-end pt-4">
                  <button type="button" onClick={() => setStep(2)} className="bg-primary-600 hover:bg-primary-500 text-white font-medium text-sm rounded-lg px-6 py-2.5 transition-colors">
                    Next — Admin Account
                  </button>
                </div>
              </div>
            )}

            {step === 2 && (
              <div className="space-y-4">
                <p className="text-slate-400 text-sm mb-4">Create the administrator account</p>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className={labelClass}>First Name *</label>
                    <div className="relative">
                      <User size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                      <input type="text" required value={formData.adminFirstName} onChange={e => update('adminFirstName', e.target.value)} placeholder="John" className={inputClass} />
                    </div>
                  </div>
                  <div>
                    <label className={labelClass}>Last Name *</label>
                    <div className="relative">
                      <User size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                      <input type="text" required value={formData.adminLastName} onChange={e => update('adminLastName', e.target.value)} placeholder="Doe" className={inputClass} />
                    </div>
                  </div>
                </div>

                <div>
                  <label className={labelClass}>Username *</label>
                  <div className="relative">
                    <User size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                    <input type="text" required value={formData.adminUsername} onChange={e => update('adminUsername', e.target.value)} placeholder="admin" className={inputClass} />
                  </div>
                </div>

                <div>
                  <label className={labelClass}>Email *</label>
                  <div className="relative">
                    <Mail size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                    <input type="email" required value={formData.adminEmail} onChange={e => update('adminEmail', e.target.value)} placeholder="admin@company.com" className={inputClass} />
                  </div>
                </div>

                <div>
                  <label className={labelClass}>Password *</label>
                  <div className="relative">
                    <Lock size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                    <input type={showPw ? 'text' : 'password'} required value={formData.adminPassword} onChange={e => update('adminPassword', e.target.value)} placeholder="Min 8 chars, uppercase & number" className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-10 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
                    <button type="button" onClick={() => setShowPw(v => !v)} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-200">
                      {showPw ? <EyeOff size={16} /> : <Eye size={16} />}
                    </button>
                  </div>
                </div>

                <div className="flex justify-between pt-4">
                  <button type="button" onClick={() => setStep(1)} className="text-slate-400 hover:text-slate-200 text-sm font-medium px-4 py-2.5 transition-colors">
                    ← Back
                  </button>
                  <button type="submit" disabled={loading} className="bg-primary-600 hover:bg-primary-500 disabled:bg-primary-800 disabled:cursor-not-allowed text-white font-medium text-sm rounded-lg px-6 py-2.5 transition-colors">
                    {loading ? 'Setting up...' : 'Complete Setup'}
                  </button>
                </div>
              </div>
            )}
          </form>
        </div>
      </div>
    </div>
  );
}
