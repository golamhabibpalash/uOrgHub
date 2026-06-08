import { useEffect, useState, useRef, useMemo } from 'react';
import { Building2, Globe, DollarSign, MapPin, Phone, Mail, FileText, Upload, X, Save } from 'lucide-react';
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { getMyCompany, updateCompany, uploadLogo, getLogoUrl, type CompanyDto } from '../../api/company';

const timezones = [
  'Asia/Dhaka', 'Asia/Kolkata', 'Asia/Singapore', 'Asia/Dubai',
  'America/New_York', 'America/Chicago', 'America/Denver', 'America/Los_Angeles',
  'Europe/London', 'Europe/Berlin', 'Europe/Paris', 'Australia/Sydney',
  'Pacific/Auckland', 'Asia/Tokyo', 'Asia/Shanghai', 'Asia/Hong_Kong',
];

const currencies = [
  { code: 'BDT', name: 'Bangladeshi Taka' },
  { code: 'USD', name: 'US Dollar' },
  { code: 'EUR', name: 'Euro' },
  { code: 'GBP', name: 'British Pound' },
  { code: 'INR', name: 'Indian Rupee' },
  { code: 'SGD', name: 'Singapore Dollar' },
  { code: 'AED', name: 'UAE Dirham' },
  { code: 'AUD', name: 'Australian Dollar' },
  { code: 'JPY', name: 'Japanese Yen' },
  { code: 'CNY', name: 'Chinese Yuan' },
];

const labelClass = "block text-slate-300 text-sm font-medium mb-1.5";

export default function CompanySettingsPage() {
  const fileRef = useRef<HTMLInputElement>(null);
  const [company, setCompany] = useState<CompanyDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [logoPreview, setLogoPreview] = useState<string | null>(null);

  const [form, setForm] = useState({
    name: '', tagLine: '', address: '', phone: '', email: '',
    taxId: '', currency: 'BDT', timeZone: 'Asia/Dhaka',
  });

  const load = async () => {
    setLoading(true);
    try {
      const c = await getMyCompany();
      setCompany(c);
      setForm({
        name: c.name, tagLine: c.tagLine ?? '', address: c.address ?? '',
        phone: c.phone ?? '', email: c.email ?? '', taxId: c.taxId ?? '',
        currency: c.currency, timeZone: c.timeZone,
      });
    } catch { /* handled by axios interceptor */ }
    finally { setLoading(false); }
  };

  useEffect(() => { load(); }, []);

  const update = (field: string, value: string) => setForm(f => ({ ...f, [field]: value }));

  const handleLogo = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setLogoFile(file);
    const reader = new FileReader();
    reader.onload = () => setLogoPreview(reader.result as string);
    reader.readAsDataURL(file);
  };

  const handleSave = async () => {
    if (!company) return;
    setSaving(true);
    try {
      await updateCompany(company.id, form);
      if (logoFile) await uploadLogo(company.id, logoFile);
      setLogoFile(null);
      setLogoPreview(null);
      await load();
    } catch {
      // handled by axios interceptor
    } finally { setSaving(false); }
  };

  const currencyOptions = useMemo(
    () => currencies.map((c) => ({ value: c.code, label: `${c.code} — ${c.name}` })),
    [],
  );
  const timezoneOptions = useMemo(
    () => timezones.map((tz) => ({ value: tz, label: tz.replace('_', ' ') })),
    [],
  );

  if (loading) return (
    <div className="p-6"><div className="text-slate-400 text-sm">Loading...</div></div>
  );

  if (!company) return (
    <div className="p-6"><div className="text-slate-400 text-sm">No company found.</div></div>
  );

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-white text-xl font-semibold">Company Settings</h1>
          <p className="text-slate-400 text-sm mt-0.5">Manage your organization information</p>
        </div>
      </div>

      <div className="bg-slate-800 rounded-xl border border-slate-700 p-6">
        <div className="grid grid-cols-2 gap-6">
          {/* Logo */}
          <div className="col-span-2">
            <label className={labelClass}>Company Logo</label>
            <div className="flex items-center gap-4 mt-1">
              {logoPreview ? (
                <div className="relative">
                  <img src={logoPreview} alt="preview" className="h-20 w-20 object-contain rounded-lg border border-slate-600" />
                  <button type="button" onClick={() => { setLogoFile(null); setLogoPreview(null); }} className="absolute -top-2 -right-2 w-5 h-5 bg-red-500 rounded-full flex items-center justify-center text-white">
                    <X size={12} />
                  </button>
                </div>
              ) : getLogoUrl(company.logoUrl) ? (
                <div className="relative">
                  <img src={getLogoUrl(company.logoUrl)!} alt="logo" className="h-20 w-20 object-contain rounded-lg border border-slate-600" />
                </div>
              ) : (
                <div className="h-20 w-20 rounded-lg border-2 border-dashed border-slate-600 flex items-center justify-center text-slate-500">
                  <Building2 size={28} />
                </div>
              )}
              <button type="button" onClick={() => fileRef.current?.click()} className="bg-slate-700 hover:bg-slate-600 text-slate-300 text-sm rounded-lg px-4 py-2.5 transition-colors">
                <Upload size={14} className="inline mr-1" /> Change Logo
              </button>
              <input ref={fileRef} type="file" accept="image/*" onChange={handleLogo} className="hidden" />
            </div>
          </div>

          {/* Name */}
          <div>
            <label className={labelClass}>Company Name</label>
            <div className="relative">
              <Building2 size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
              <input value={form.name} onChange={e => update('name', e.target.value)} className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>

          {/* Tagline */}
          <div>
            <label className={labelClass}>Tagline</label>
            <div className="relative">
              <FileText size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
              <input value={form.tagLine} onChange={e => update('tagLine', e.target.value)} className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>

          {/* Address */}
          <div className="col-span-2">
            <label className={labelClass}>Address</label>
            <div className="relative">
              <MapPin size={16} className="absolute left-3 top-3 text-slate-500" />
              <textarea value={form.address} onChange={e => update('address', e.target.value)} rows={2} className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500 resize-none" />
            </div>
          </div>

          {/* Phone */}
          <div>
            <label className={labelClass}>Phone</label>
            <div className="relative">
              <Phone size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
              <input value={form.phone} onChange={e => update('phone', e.target.value)} className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>

          {/* Email */}
          <div>
            <label className={labelClass}>Email</label>
            <div className="relative">
              <Mail size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
              <input value={form.email} onChange={e => update('email', e.target.value)} className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>

          {/* Tax ID */}
          <div>
            <label className={labelClass}>Tax ID / VAT</label>
            <div className="relative">
              <FileText size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
              <input value={form.taxId} onChange={e => update('taxId', e.target.value)} className="w-full bg-slate-700 border border-slate-600 text-white text-sm rounded-lg pl-9 pr-4 py-2.5 focus:outline-none focus:border-primary-500 focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>

          {/* Currency */}
          <div>
            <label className={labelClass}>Currency</label>
            <div className="relative">
              <DollarSign size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500 z-10" />
              <SearchableDropdown value={form.currency} options={currencyOptions} onChange={(v) => update('currency', v ?? 'BDT')} placeholder="Select currency" searchPlaceholder="Search currencies..." className="w-full" buttonClassName="!bg-slate-700 !border-slate-600 !text-white" />
            </div>
          </div>

          {/* TimeZone */}
          <div>
            <label className={labelClass}>Time Zone</label>
            <div className="relative">
              <Globe size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500 z-10" />
              <SearchableDropdown value={form.timeZone} options={timezoneOptions} onChange={(v) => update('timeZone', v ?? 'Asia/Dhaka')} placeholder="Select timezone" searchPlaceholder="Search timezones..." className="w-full" buttonClassName="!bg-slate-700 !border-slate-600 !text-white" />
            </div>
          </div>
        </div>

        {/* Meta */}
        <div className="mt-6 pt-6 border-t border-slate-700 flex items-center justify-between text-xs text-slate-500">
          <div className="flex items-center gap-4">
            <span>Created: {new Date(company.createdAt).toLocaleDateString()}</span>
            {company.updatedAt && <span>Updated: {new Date(company.updatedAt).toLocaleDateString()}</span>}
          </div>
          <button onClick={handleSave} disabled={saving} className="flex items-center gap-2 bg-primary-600 hover:bg-primary-500 disabled:bg-primary-800 disabled:cursor-not-allowed text-white font-medium text-sm rounded-lg px-5 py-2.5 transition-colors">
            <Save size={16} /> {saving ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </div>
    </div>
  );
}
