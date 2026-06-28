import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Loader2 } from 'lucide-react';
import { useCreatePrayer } from '@/hooks/usePrayers';

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

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [categories, setCategories] = useState<string[]>([]);

  const toggleCategory = (cat: string) => {
    setCategories((prev) =>
      prev.includes(cat) ? prev.filter((c) => c !== cat) : [...prev, cat]
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      const created = await createPrayer.mutateAsync({
        title: title.trim(),
        content: content.trim() || undefined,
        categories,
      });
      navigate(`/prayers/${created.id}`);
    } catch {
      // error shown below
    }
  };

  return (
    <div className="p-4 max-w-2xl mx-auto">
      <button
        onClick={() => navigate(-1)}
        className="flex items-center gap-1 text-gray-500 hover:text-gray-700 mb-4 text-sm"
      >
        <ArrowLeft size={18} />
        Back
      </button>

      <h1 className="text-2xl font-bold text-gray-900 mb-6">New Prayer Request</h1>

      <form onSubmit={handleSubmit} className="space-y-5">
        <div>
          <label htmlFor="title" className="block text-sm font-medium text-gray-700 mb-1.5">
            Title
          </label>
          <input
            id="title"
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="What are you praying for?"
            required
            maxLength={200}
            className="w-full p-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
          />
        </div>

        <div>
          <label htmlFor="content" className="block text-sm font-medium text-gray-700 mb-1.5">
            Details <span className="text-gray-400 font-normal">(optional)</span>
          </label>
          <textarea
            id="content"
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder="Share more about your request..."
            rows={5}
            maxLength={2000}
            className="w-full p-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
          />
        </div>

        <div>
          <p className="text-sm font-medium text-gray-700 mb-2">Categories</p>
          <div className="flex flex-wrap gap-2">
            {PRESET_CATEGORIES.map((cat) => (
              <button
                key={cat}
                type="button"
                onClick={() => toggleCategory(cat)}
                className={`text-sm px-3 py-1.5 rounded-full border transition ${
                  categories.includes(cat)
                    ? 'bg-indigo-600 text-white border-indigo-600'
                    : 'bg-white text-gray-600 border-gray-200 hover:border-indigo-300'
                }`}
              >
                {cat}
              </button>
            ))}
          </div>
        </div>

        {createPrayer.isError && (
          <p className="text-red-600 text-sm">Failed to create prayer request. Please try again.</p>
        )}

        <button
          type="submit"
          disabled={createPrayer.isPending || !title.trim()}
          className="w-full bg-indigo-600 text-white py-3 rounded-xl font-medium hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
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
