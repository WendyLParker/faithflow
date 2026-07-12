import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, Check, Loader2 } from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { useMyRole, useUpdateMyProfile } from '@/hooks/useUserRole';

const PALETTE: { label: string; value: string }[] = [
  { label: 'Green', value: '#34c759' },
  { label: 'Blue', value: '#0a84ff' },
  { label: 'Purple', value: '#bf5af2' },
  { label: 'Orange', value: '#ff9f0a' },
  { label: 'Red', value: '#ff3b30' },
  { label: 'Pink', value: '#ff2d78' },
  { label: 'Teal', value: '#5ac8fa' },
  { label: 'Yellow', value: '#ffd60a' },
  { label: 'White', value: '#e5e5e5' },
];

export default function Profile() {
  const { user } = useAuth();
  const { data: myRole, isLoading } = useMyRole();
  const { mutateAsync: updateProfile, isPending } = useUpdateMyProfile();

  const [displayName, setDisplayName] = useState('');
  const [selectedColor, setSelectedColor] = useState('');
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (myRole) {
      setDisplayName(myRole.displayName ?? '');
      setSelectedColor(myRole.profileColor ?? '#34c759');
    }
  }, [myRole]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSaved(false);
    try {
      await updateProfile({
        displayName: displayName.trim() || undefined,
        profileColor: selectedColor || undefined,
      });
      setSaved(true);
      setTimeout(() => setSaved(false), 3000);
    } catch {
      setError('Failed to save. Please try again.');
    }
  };

  if (isLoading) {
    return (
      <div className="page-container">
        <div className="flex justify-center py-20">
          <Loader2 className="animate-spin text-[#34C759]" size={32} />
        </div>
      </div>
    );
  }

  const avatarColor = selectedColor || myRole?.profileColor || '#34c759';
  const initials =
    (displayName || user?.email || '?')
      .replace(/@.*/, '')
      .slice(0, 2)
      .toUpperCase();

  return (
    <div className="page-container max-w-md">
      <Link to="/dashboard" className="back-link">
        <ArrowLeft size={16} />
        Back to dashboard
      </Link>

      <h1 className="page-title">Manage Profile</h1>
      <p className="page-subtitle">Customize how you appear in the app.</p>

      {/* Avatar preview */}
      <div className="flex flex-col items-center gap-3 mb-8">
        <div
          className="w-20 h-20 rounded-full flex items-center justify-center text-2xl font-bold text-black transition-colors duration-200"
          style={{ backgroundColor: avatarColor }}
        >
          {initials}
        </div>
        <p className="text-sm text-neutral-400">{user?.email}</p>
      </div>

      <form onSubmit={handleSave} className="space-y-6">
        <div>
          <label htmlFor="displayName" className="form-label">
            Display name
          </label>
          <input
            id="displayName"
            type="text"
            value={displayName}
            onChange={(e) => setDisplayName(e.target.value)}
            placeholder="Your name (optional)"
            maxLength={100}
            className="form-input"
          />
          <p className="mt-1 text-xs text-neutral-500">
            Shown in the greeting banner and group management.
          </p>
        </div>

        <div>
          <p className="form-label mb-3">Profile color</p>
          <div className="flex flex-wrap gap-3">
            {PALETTE.map((color) => (
              <button
                key={color.value}
                type="button"
                aria-label={color.label}
                onClick={() => setSelectedColor(color.value)}
                className="relative w-9 h-9 rounded-full transition-transform hover:scale-110 focus:outline-none focus:ring-2 focus:ring-white/50"
                style={{ backgroundColor: color.value }}
              >
                {selectedColor === color.value && (
                  <Check
                    size={16}
                    strokeWidth={3}
                    className="absolute inset-0 m-auto text-black"
                  />
                )}
              </button>
            ))}
          </div>
        </div>

        {error && <p className="message-error">{error}</p>}

        {saved && (
          <p className="message-success flex items-center gap-1.5">
            <Check size={14} />
            Profile saved!
          </p>
        )}

        <button
          type="submit"
          disabled={isPending}
          className="btn-apple w-full py-3.5 rounded-xl font-semibold disabled:opacity-50"
        >
          {isPending ? (
            <span className="flex items-center justify-center gap-2">
              <Loader2 size={16} className="animate-spin" />
              Saving…
            </span>
          ) : (
            'Save changes'
          )}
        </button>
      </form>
    </div>
  );
}
