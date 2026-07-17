import { useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { format, formatDistanceToNow } from 'date-fns';
import { ArrowLeft, Loader2, Sparkles, Trash2, CheckCircle, MessageSquare, Send } from 'lucide-react';
import {
  useRequest,
  useCloseRequest,
  useFulfillRequest,
  useDeleteRequest,
} from '@/hooks/useRequests';
import { useRequestComments, useAddRequestComment } from '@/hooks/useRequestComments';

export default function PrayerDetail() {
  const { id } = useParams<{ id: string }>();
  const requestId = Number(id);
  const navigate = useNavigate();

  const { data: request, isLoading, error } = useRequest(requestId);
  const { data: comments = [], isLoading: commentsLoading } = useRequestComments(requestId);
  const closeRequest = useCloseRequest();
  const fulfillRequest = useFulfillRequest();
  const deleteRequest = useDeleteRequest();
  const addComment = useAddRequestComment();
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [commentText, setCommentText] = useState('');

  const handleClose = async () => {
    try {
      await closeRequest.mutateAsync(requestId);
    } catch {
      // shown via mutation state
    }
  };

  const handleFulfill = async () => {
    try {
      await fulfillRequest.mutateAsync(requestId);
    } catch {
      // shown via mutation state
    }
  };

  const handleDelete = async () => {
    try {
      await deleteRequest.mutateAsync(requestId);
      navigate('/requests');
    } catch {
      setShowDeleteConfirm(false);
    }
  };

  const handleAddComment = async () => {
    const content = commentText.trim();
    if (!content) return;

    try {
      await addComment.mutateAsync({ requestId, content });
      setCommentText('');
    } catch {
      // shown via mutation state
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center py-24">
        <Loader2 className="animate-spin text-[#34C759]" size={32} />
      </div>
    );
  }

  if (error || !request) {
    return (
      <div className="page-container">
         <Link
        to="/requests"
        className="btn-apple w-full py-3.5 rounded-xl font-semibold flex items-center justify-center gap-2"
      >
        <ArrowLeft size={20} />
        Back to requests
      </Link>
        <div className="alert-error">Request not found.</div>
      </div>
    );
  }

  return (
    <div className="page-container">
       <Link
        to="/requests"
        className="btn-apple w-full py-3.5 rounded-xl font-semibold flex items-center justify-center gap-2"
      >
        <ArrowLeft size={20} />
        Back to requests
      </Link>
      <br/>

      <div className="content-card">
        {request.isCompleted && (
          <div className="flex items-center gap-2 alert-info mb-5">
            <Sparkles size={20} className="text-[#9bada3] shrink-0" />
            <div>
              <p className="font-medium text-neutral-100">Closed</p>
              {request.completedDate && (
                <p className="text-sm text-neutral-400">
                  Closed on {format(new Date(request.completedDate), 'MMMM d, yyyy')}
                </p>
              )}
            </div>
          </div>
        )}

        {!request.isCompleted && request.requestStatus === 'Fulfilled' && (
          <div className="flex items-center gap-2 alert-info mb-5">
            <CheckCircle size={20} className="text-[#9bada3] shrink-0" />
            <div>
              <p className="font-medium text-neutral-100">
                {request.isOwnedByCurrentUser
                  ? 'Assignee has completed their part'
                  : 'You marked your part complete'}
              </p>
              {request.fulfilledDate && (
                <p className="text-sm text-neutral-400">
                  Completed on {format(new Date(request.fulfilledDate), 'MMMM d, yyyy')}
                </p>
              )}
              {request.isOwnedByCurrentUser && (
                <p className="text-sm text-neutral-400 mt-1">
                  Review the request and close it when you are satisfied.
                </p>
              )}
            </div>
          </div>
        )}

        <h1 className="text-2xl font-bold text-white">{request.title}</h1>

        <div className="flex flex-wrap items-center gap-2 mt-3">
          {request.requestTypeName && (
            <span className="badge">{request.requestTypeName}</span>
          )}
          {request.groupNames.map((group) => (
            <span key={group} className="badge">
              {group}
            </span>
          ))}
        </div>

        <p className="text-sm text-neutral-500 mt-2">
          Posted {format(new Date(request.requestDate), 'MMMM d, yyyy')}
        </p>

        {request.content && (
          <p className="text-neutral-300 mt-5 leading-relaxed whitespace-pre-wrap">
            {request.content}
          </p>
        )}
      </div>

      <div className="content-card mt-6">
        <div className="flex items-center gap-2 mb-4">
          <MessageSquare size={18} className="text-[#9bada3]" />
          <h2 className="font-semibold text-white">Comments</h2>
        </div>

        {commentsLoading && (
          <div className="flex justify-center py-6">
            <Loader2 className="animate-spin text-[#34C759]" size={24} />
          </div>
        )}

        {!commentsLoading && comments.length === 0 && (
          <p className="text-sm text-neutral-500 mb-4">No comments yet.</p>
        )}

        <ul className="space-y-3 mb-4">
          {comments.map((comment) => (
            <li
              key={comment.id}
              className={`rounded-xl border px-4 py-3 ${
                comment.isOwnComment
                  ? 'border-[#3d4a44] bg-[#2f3834]/40'
                  : 'border-neutral-700 bg-neutral-800/30'
              }`}
            >
              <div className="flex items-center justify-between gap-2 mb-1">
                <span className="text-sm font-medium text-neutral-200">{comment.authorName}</span>
                <span className="text-xs text-neutral-500">
                  {formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true })}
                </span>
              </div>
              <p className="text-sm text-neutral-300 whitespace-pre-wrap">{comment.content}</p>
            </li>
          ))}
        </ul>

        <div className="space-y-3">
          <textarea
            value={commentText}
            onChange={(e) => setCommentText(e.target.value)}
            placeholder="Add a comment..."
            rows={3}
            maxLength={2000}
            className="w-full rounded-xl border border-neutral-700 bg-neutral-900 px-4 py-3 text-sm text-neutral-100 placeholder:text-neutral-500 focus:outline-none focus:border-[#34C759] resize-y"
          />
          <button
            type="button"
            onClick={handleAddComment}
            disabled={addComment.isPending || !commentText.trim()}
            className="btn-apple w-full flex items-center justify-center gap-2 py-3 rounded-xl font-semibold disabled:opacity-50"
          >
            {addComment.isPending ? (
              <Loader2 size={18} className="animate-spin" />
            ) : (
              <Send size={18} />
            )}
            Post Comment
          </button>
          {addComment.isError && (
            <p className="message-error text-center text-sm">Failed to post comment. Please try again.</p>
          )}
        </div>
      </div>

      <div className="mt-6 space-y-3">
        {request.canFulfill && (
          <button
            onClick={handleFulfill}
            disabled={fulfillRequest.isPending}
            className="btn-apple w-full flex items-center justify-center gap-2 py-3.5 rounded-xl font-semibold disabled:opacity-50"
          >
            {fulfillRequest.isPending ? (
              <Loader2 size={20} className="animate-spin" />
            ) : (
              <CheckCircle size={20} />
            )}
            Mark My Part Complete
          </button>
        )}

        {fulfillRequest.isError && (
          <p className="message-error text-center">Failed to mark complete. Please try again.</p>
        )}

        {request.canClose && (
          <button
            onClick={handleClose}
            disabled={closeRequest.isPending}
            className="btn-apple w-full flex items-center justify-center gap-2 py-3.5 rounded-xl font-semibold disabled:opacity-50"
          >
            {closeRequest.isPending ? (
              <Loader2 size={20} className="animate-spin" />
            ) : (
              <Sparkles size={20} />
            )}
            Close Request
          </button>
        )}

        {closeRequest.isError && (
          <p className="message-error text-center">Failed to close request. Please try again.</p>
        )}

        {request.isOwnedByCurrentUser && !request.isCompleted && request.requestStatus !== 'Fulfilled' && (
          <p className="text-sm text-neutral-500 text-center">
            Waiting for the assignee to complete their part.
          </p>
        )}

        {request.isOwnedByCurrentUser && (
          !showDeleteConfirm ? (
            <button
              onClick={() => setShowDeleteConfirm(true)}
              className="w-full flex items-center justify-center gap-2 text-red-400 py-3 rounded-xl font-medium border border-red-900/50 hover:bg-red-950/30 transition"
            >
              <Trash2 size={18} />
              Delete Request
            </button>
          ) : (
            <div className="alert-error space-y-3">
              <p className="text-center">Are you sure? This cannot be undone.</p>
              <div className="flex gap-3">
                <button
                  onClick={() => setShowDeleteConfirm(false)}
                  className="filter-btn flex-1"
                >
                  Cancel
                </button>
                <button
                  onClick={handleDelete}
                  disabled={deleteRequest.isPending}
                  className="flex-1 py-2 text-sm font-medium text-white bg-red-700 rounded-lg hover:bg-red-600 disabled:opacity-50 border border-red-600"
                >
                  {deleteRequest.isPending ? 'Deleting...' : 'Delete'}
                </button>
              </div>
            </div>
          )
        )}
      </div>
    </div>
  );
}
