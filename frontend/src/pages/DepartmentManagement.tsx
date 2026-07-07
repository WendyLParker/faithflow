import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, UserPlus, Trash2, Mail, BellOff, Loader2, ChevronDown, ChevronUp } from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import {
  useDepartments,
  useDepartmentMembers,
  useAddDepartmentMember,
  useRemoveDepartmentMember,
  useUpdateEmailPreference,
  useMyMemberships,
} from '@/hooks/useDepartments';

export default function DepartmentManagement() {
  const { user } = useAuth();
  const { data: departments = [], isLoading: depsLoading } = useDepartments();
  const { data: myMemberships = [] } = useMyMemberships();
  const [expandedDept, setExpandedDept] = useState<number | null>(null);

  if (depsLoading) {
    return (
      <div className="page-container">
        <div className="flex justify-center py-20">
          <Loader2 className="animate-spin text-[#34C759]" size={32} />
        </div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <Link to="/dashboard" className="back-link">
        <ArrowLeft size={16} />
        Back to dashboard
      </Link>

      <h1 className="page-title">Department Management</h1>
      <p className="page-subtitle">Assign users to departments to control notification routing.</p>

      {user && (
        <div className="alert-info mb-6">
          <p className="text-xs text-neutral-500 mb-1">Your user ID</p>
          <p className="font-mono text-xs text-neutral-300 break-all select-all">{user.sub}</p>
          <p className="text-xs text-neutral-500 mt-2">Share this ID with an admin to be added to a department, or add yourself below.</p>
        </div>
      )}

      <div className="space-y-4">
        {departments.map((dept) => {
          const isExpanded = expandedDept === dept.id;
          const myMembership = myMemberships.find((m) => m.departmentId === dept.id);

          return (
            <div key={dept.id} className="content-card">
              {/* Department header */}
              <button
                type="button"
                onClick={() => setExpandedDept(isExpanded ? null : dept.id)}
                className="w-full flex items-center justify-between gap-3 text-left"
              >
                <div className="min-w-0">
                  <h2 className="font-semibold text-neutral-100">{dept.name}</h2>
                  {dept.description && (
                    <p className="text-xs text-neutral-500 mt-0.5">{dept.description}</p>
                  )}
                  <div className="flex flex-wrap gap-1.5 mt-2">
                    {dept.requestTypeNames.map((t) => (
                      <span key={t} className="badge">{t}</span>
                    ))}
                  </div>
                </div>
                <div className="flex items-center gap-2 shrink-0">
                  {myMembership && (
                    <span className="text-xs text-[#9bada3] bg-[#2f3834] border border-[#3d4a44] px-2 py-0.5 rounded-full">
                      Joined
                    </span>
                  )}
                  {isExpanded ? (
                    <ChevronUp size={18} className="text-neutral-500" />
                  ) : (
                    <ChevronDown size={18} className="text-neutral-500" />
                  )}
                </div>
              </button>

              {/* Expanded body */}
              {isExpanded && (
                <div className="mt-4 pt-4 border-t border-neutral-700 space-y-4">
                  <DepartmentMembers
                    departmentId={dept.id}
                    currentUser={user}
                  />
                  {!myMembership && user && (
                    <JoinDepartmentForm
                      departmentId={dept.id}
                      currentUser={user}
                    />
                  )}
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ── Sub-components ────────────────────────────────────────────────────────────

function DepartmentMembers({
  departmentId,
  currentUser,
}: {
  departmentId: number;
  currentUser: { sub: string } | null;
}) {
  const { data: members = [], isLoading } = useDepartmentMembers(departmentId);
  const removeMember = useRemoveDepartmentMember(departmentId);
  const updateEmail = useUpdateEmailPreference();

  if (isLoading) {
    return <div className="flex justify-center py-4"><Loader2 className="animate-spin text-[#34C759]" size={20} /></div>;
  }

  if (members.length === 0) {
    return <p className="text-sm text-neutral-500 text-center py-2">No members yet.</p>;
  }

  return (
    <ul className="space-y-2">
      {members.map((m) => {
        const isMe = currentUser?.sub === m.userId;
        return (
          <li key={m.id} className="flex items-center justify-between gap-3 bg-neutral-900/60 rounded-xl px-3 py-2.5">
            <div className="min-w-0">
              <p className="text-sm font-medium text-neutral-100 truncate">
                {m.displayName}
                {isMe && <span className="ml-1.5 text-[#34C759] text-xs">(you)</span>}
              </p>
              <p className="text-xs text-neutral-500 truncate">{m.userEmail}</p>
            </div>
            <div className="flex items-center gap-1.5 shrink-0">
              <button
                type="button"
                title={m.emailNotificationsEnabled ? 'Email notifications on' : 'Email notifications off'}
                onClick={() =>
                  updateEmail.mutate({
                    membershipId: m.id,
                    enabled: !m.emailNotificationsEnabled,
                  })
                }
                disabled={updateEmail.isPending}
                className="p-1.5 rounded-lg transition text-neutral-400 hover:text-white hover:bg-neutral-700"
              >
                {m.emailNotificationsEnabled ? <Mail size={16} /> : <BellOff size={16} />}
              </button>
              <button
                type="button"
                title="Remove from department"
                onClick={() => removeMember.mutate(m.id)}
                disabled={removeMember.isPending}
                className="p-1.5 rounded-lg text-neutral-500 hover:text-red-400 hover:bg-red-950/30 transition"
              >
                <Trash2 size={16} />
              </button>
            </div>
          </li>
        );
      })}
    </ul>
  );
}

function JoinDepartmentForm({
  departmentId,
  currentUser,
}: {
  departmentId: number;
  currentUser: { sub: string; email: string };
}) {
  const addMember = useAddDepartmentMember(departmentId);
  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState(currentUser.email);
  const [emailNotifs, setEmailNotifs] = useState(true);
  const [addOther, setAddOther] = useState(false);
  const [otherUserId, setOtherUserId] = useState('');
  const [otherName, setOtherName] = useState('');
  const [otherEmail, setOtherEmail] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const userId = addOther ? otherUserId.trim() : currentUser.sub;
    const name = addOther ? otherName.trim() : displayName.trim();
    const userEmail = addOther ? otherEmail.trim() : email.trim();
    if (!userId || !name) return;

    await addMember.mutateAsync({
      userId,
      displayName: name,
      userEmail,
      emailNotificationsEnabled: emailNotifs,
    });

    setDisplayName('');
    setOtherUserId('');
    setOtherName('');
    setOtherEmail('');
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-3 pt-2">
      <div className="flex items-center gap-3">
        <button
          type="button"
          onClick={() => setAddOther(false)}
          className={!addOther ? 'chip chip-selected' : 'chip'}
        >
          Add myself
        </button>
        <button
          type="button"
          onClick={() => setAddOther(true)}
          className={addOther ? 'chip chip-selected' : 'chip'}
        >
          Add another user
        </button>
      </div>

      {!addOther ? (
        <>
          <div>
            <label className="form-label">Display name</label>
            <input
              className="form-input"
              placeholder="How your name appears in the system"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              required
            />
          </div>
          <div>
            <label className="form-label">Email</label>
            <input
              className="form-input"
              type="email"
              placeholder="you@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>
        </>
      ) : (
        <>
          <div>
            <label className="form-label">User ID (Cognito sub)</label>
            <input
              className="form-input font-mono text-sm"
              placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
              value={otherUserId}
              onChange={(e) => setOtherUserId(e.target.value)}
              required
            />
          </div>
          <div>
            <label className="form-label">Display name</label>
            <input
              className="form-input"
              placeholder="Full name"
              value={otherName}
              onChange={(e) => setOtherName(e.target.value)}
              required
            />
          </div>
          <div>
            <label className="form-label">Email</label>
            <input
              className="form-input"
              type="email"
              placeholder="user@example.com"
              value={otherEmail}
              onChange={(e) => setOtherEmail(e.target.value)}
            />
          </div>
        </>
      )}

      <label className="flex items-center gap-2.5 cursor-pointer">
        <input
          type="checkbox"
          checked={emailNotifs}
          onChange={(e) => setEmailNotifs(e.target.checked)}
          className="w-4 h-4 accent-[#34C759]"
        />
        <span className="text-sm text-neutral-300">Receive email notifications</span>
      </label>

      {addMember.isError && (
        <p className="message-error">
          {(addMember.error as Error)?.message?.includes('409') ||
          String(addMember.error).includes('409')
            ? 'This user is already a member of this department.'
            : 'Failed to add member. Please try again.'}
        </p>
      )}

      <button
        type="submit"
        disabled={addMember.isPending}
        className="btn-apple flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-sm disabled:opacity-50"
      >
        {addMember.isPending ? (
          <Loader2 size={16} className="animate-spin" />
        ) : (
          <UserPlus size={16} />
        )}
        {addOther ? 'Add user' : 'Join department'}
      </button>
    </form>
  );
}
