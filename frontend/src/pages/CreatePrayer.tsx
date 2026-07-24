import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Loader2, ArrowLeft, Sparkles } from 'lucide-react';
import { useCreateRequest, useEstimateCost } from '@/hooks/useRequests';
import { useRequestTypes } from '@/hooks/useRequestTypes';
import { useGroups } from '@/hooks/useGroups';

export default function CreatePrayer() {
  const navigate = useNavigate();
  const createRequest = useCreateRequest();
  const estimateCost = useEstimateCost();
  const { data: requestTypes = [], isLoading: typesLoading } = useRequestTypes();
  const { data: groups = [], isLoading: groupsLoading } = useGroups();

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [selectedGroupIds, setSelectedGroupIds] = useState<number[]>([]);
  const [requestTypeId, setRequestTypeId] = useState<number | null>(null);

  const hasGroups = groups.length > 0;
  const selectedType = requestTypes.find((t) => t.id === requestTypeId);
  const canEstimate =
    !!selectedType && title.trim().length > 0 && !estimateCost.isPending;

  const handleEstimate = async () => {
    if (!selectedType) return;
    try {
      await estimateCost.mutateAsync({
        requestType: selectedType.name,
        title: title.trim(),
        description: content.trim(),
      });
    } catch {
      // error shown below
    }
  };

  const estimate = estimateCost.data;
  const currencyFmt = (value: number) =>
    new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: estimate?.currency || 'USD',
      maximumFractionDigits: 0,
    }).format(value);

  const toggleGroup = (groupId: number) => {
    setSelectedGroupIds((prev) =>
      prev.includes(groupId) ? prev.filter((id) => id !== groupId) : [...prev, groupId],
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (requestTypeId == null) return;
    if (hasGroups && selectedGroupIds.length === 0) return;

    try {
      await createRequest.mutateAsync({
        title: title.trim(),
        content: content.trim() || undefined,
        requestTypeId,
        groupIds: hasGroups ? selectedGroupIds : [],
      });
      navigate('/requests');
    } catch {
      // error shown below
    }
  };

  const canSubmit =
    title.trim().length > 0 &&
    requestTypeId != null &&
    (!hasGroups || selectedGroupIds.length > 0) &&
    !createRequest.isPending;

  return (
    <div className="page-container">
      <Link
        to="/dashboard"
        className="btn-apple w-full py-3.5 rounded-xl font-semibold flex items-center justify-center gap-2"
      >
        <ArrowLeft size={20} />
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
          <button
            type="button"
            onClick={handleEstimate}
            disabled={!canEstimate}
            className="w-full flex items-center justify-center gap-2 py-3 rounded-xl font-medium border border-[#3d4a44] text-[#9bada3] hover:bg-[#2f3834]/40 transition disabled:opacity-50"
          >
            {estimateCost.isPending ? (
              <Loader2 size={18} className="animate-spin" />
            ) : (
              <Sparkles size={18} />
            )}
            Estimate Cost
          </button>
          <p className="text-xs text-neutral-500 mt-1.5">
            Optional AI estimate. Select a request type and enter a title first.
          </p>

          {estimateCost.isError && (
            <p className="message-error mt-3">
              Couldn&apos;t generate an estimate right now. Please try again.
            </p>
          )}

          {estimate && (
            <div className="content-card mt-3 space-y-3">
              <div className="flex items-center justify-between gap-2">
                <div className="flex items-center gap-2">
                  <Sparkles size={16} className="text-[#9bada3]" />
                  <span className="font-semibold text-white">Estimated cost</span>
                </div>
                <span className="badge capitalize">{estimate.confidence} confidence</span>
              </div>

              <div className="grid grid-cols-3 gap-2 text-center">
                <div className="rounded-xl border border-neutral-700 bg-neutral-800/30 py-2">
                  <p className="text-xs text-neutral-500">Low</p>
                  <p className="text-sm font-semibold text-neutral-200">
                    {currencyFmt(estimate.low_estimate)}
                  </p>
                </div>
                <div className="rounded-xl border border-[#3d4a44] bg-[#2f3834]/40 py-2">
                  <p className="text-xs text-neutral-400">Most likely</p>
                  <p className="text-sm font-bold text-white">
                    {currencyFmt(estimate.most_likely)}
                  </p>
                </div>
                <div className="rounded-xl border border-neutral-700 bg-neutral-800/30 py-2">
                  <p className="text-xs text-neutral-500">High</p>
                  <p className="text-sm font-semibold text-neutral-200">
                    {currencyFmt(estimate.high_estimate)}
                  </p>
                </div>
              </div>

              {estimate.reasoning && (
                <p className="text-sm text-neutral-400">{estimate.reasoning}</p>
              )}
              <p className="text-xs text-neutral-600">
                AI-generated estimate. Actual costs may vary.
              </p>
            </div>
          )}
        </div>

        {hasGroups && (
          <div>
            <p className="form-label mb-2">Assign to</p>
            {groupsLoading ? (
              <div className="flex justify-center py-4">
                <Loader2 className="animate-spin text-[#34C759]" size={24} />
              </div>
            ) : (
              <div className="flex flex-wrap gap-2">
                {groups.map((group) => (
                  <button
                    key={group.id}
                    type="button"
                    onClick={() => toggleGroup(group.id)}
                    className={selectedGroupIds.includes(group.id) ? 'chip chip-selected' : 'chip'}
                  >
                    {group.name}
                  </button>
                ))}
              </div>
            )}
          </div>
        )}

        {createRequest.isError && (
          <p className="message-error">Failed to create request. Please try again.</p>
        )}

        <button
          type="submit"
          disabled={!canSubmit}
          className="btn-apple w-full py-3.5 rounded-xl font-semibold flex items-center justify-center gap-2"
        >
          {createRequest.isPending ? (
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
