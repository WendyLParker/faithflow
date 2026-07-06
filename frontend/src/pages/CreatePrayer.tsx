import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Loader2, ArrowLeft } from 'lucide-react';
import { useCreatePrayer } from '@/hooks/usePrayers';
import { useRequestTypes } from '@/hooks/useRequestTypes';

const PRESET_CATEGORIES = [
  'Health',
  'Family',
  'Work',
  'Relationships',
  'Finances',
  'Spiritual Growth',
  'Other',
];

export default function CreatePrayer() {
  const navigate = useNavigate();
  const createPrayer = useCreatePrayer();
  const { data: requestTypes = [], isLoading: typesLoading } = useRequestTypes();

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [categories, setCategories] = useState<string[]>([]);
  const [requestTypeId, setRequestTypeId] = useState<number | null>(null);

  const toggleCategory = (cat: string) => {
    setCategories((prev) =>
      prev.includes(cat) ? prev.filter((c) => c !== cat) : [...prev, cat]
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (requestTypeId == null) return;

    try {
      await createPrayer.mutateAsync({
        title: title.trim(),
        content: content.trim() || undefined,
        categories,
        requestTypeId,
      });
      navigate('/prayers');
    } catch {
      // error shown below
    }
  };

  const canSubmit = title.trim().length > 0 && requestTypeId != null && !createPrayer.isPending;

  return (
    <div className="page-container">
      <Link to="/dashboard" className="back-link">
        <ArrowLeft size={16} />
        Back to dashboard
      </Link>
      <h1 className="page-title">Create Request</h1>

      <form onSubmit={handleSubmit} className="space-y-5">
        <div>
          <p className="form-label mb-2">Request type</p>
          {typesLoading ? (
            <div className="flex justify-center py-4">
              <Loader2 className="animate-spin text-[#34C759]" size={24} />
            </div>
          ) : (
            <div className="flex flex-wrap gap-2">
              {requestTypes.map((type) => (
                <button
                  key={type.id}
                  type="button"
                  onClick={() => setRequestTypeId(type.id)}
                  className={requestTypeId === type.id ? 'chip chip-selected' : 'chip'}
                >
                  {type.name}
                </button>
              ))}
            </div>
          )}
        </div>

        <div>
          <label htmlFor="title" className="form-label">
            Title
          </label>
          <input
            id="title"
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="Brief summary of your request"
            required
            maxLength={200}
            className="form-input"
          />
        </div>

        <div>
          <label htmlFor="content" className="form-label">
            Details <span className="text-neutral-500 font-normal">(optional)</span>
          </label>
          <textarea
            id="content"
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder="Share more about your request..."
            rows={5}
            maxLength={2000}
            className="form-input resize-none"
          />
        </div>

        <div>
          <p className="form-label mb-2">Categories</p>
          <div className="flex flex-wrap gap-2">
            {PRESET_CATEGORIES.map((cat) => (
              <button
                key={cat}
                type="button"
                onClick={() => toggleCategory(cat)}
                className={categories.includes(cat) ? 'chip chip-selected' : 'chip'}
              >
                {cat}
              </button>
            ))}
          </div>
        </div>

        {createPrayer.isError && (
          <p className="message-error">Failed to create request. Please try again.</p>
        )}

        <button
          type="submit"
          disabled={!canSubmit}
          className="btn-apple w-full py-3.5 rounded-xl font-semibold flex items-center justify-center gap-2"
        >
          {createPrayer.isPending ? (
            <>
              <Loader2 size={20} className="animate-spin" />
              Submitting...
            </>
          ) : (
            'Submit Request'
          )}
        </button>
      </form>
    </div>
  );
}
