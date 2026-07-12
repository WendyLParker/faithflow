import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  ArrowLeft,
  UserPlus,
  Trash2,
  Mail,
  BellOff,
  Loader2,
  ChevronDown,
  ChevronUp,
  Plus,
  ShieldCheck,
} from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import {
  useGroupMembers,
  useAddGroupMember,
  useRemoveGroupMember,
  useUpdateEmailPreference,
  useUpdateGroupManager,
  useMyMemberships,
  useCreateGroup,
  useManagedGroups,
  useGroupManagerAssignments,
  useSetGroupManagers,
  useDeleteGroup,
} from '@/hooks/useGroups';
import { useMyRole, useAllRoles, useSetUserRole } from '@/hooks/useUserRole';
import type { UserRoleDto } from '@/services/userRoleService';
import type { GroupManagerAssignmentDto } from '@/services/notificationService';

const EMPTY_ASSIGNMENTS: GroupManagerAssignmentDto[] = [];

function sameIdSet(a: number[], b: number[]) {
  if (a.length !== b.length) return false;
  const sortedA = [...a].sort((x, y) => x - y);
  const sortedB = [...b].sort((x, y) => x - y);
  return sortedA.every((id, i) => id === sortedB[i]);
}

function userPrimaryLabel(user: { displayName: string; userEmail: string }) {
  return user.displayName || user.userEmail || 'Unknown user';
}

export default function GroupManagement() {
  const { user } = useAuth();
  const { data: myMemberships = [] } = useMyMemberships();
  const { data: myRole } = useMyRole();
  const { data: managedGroups = [], isLoading: groupsLoading } = useManagedGroups();
  const [expandedGroup, setExpandedGroup] = useState<number | null>(null);
  const [deletingGroupId, setDeletingGroupId] = useState<number | null>(null);

  const isAdmin = !!myRole?.isAdmin;

  if (groupsLoading) {
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

      <h1 className="page-title">Users & Groups</h1>
      <p className="page-subtitle">Create groups of users so requests can be assigned.</p>

      {isAdmin && <CreateGroupForm />}

      {managedGroups.length === 0 && (
        <div className="content-card text-center py-8">
          <p className="text-neutral-400 text-sm">You don't manage any groups yet.</p>
          <p className="text-neutral-500 text-xs mt-1">Ask a global admin to grant you management access.</p>
        </div>
      )}

      <div className="space-y-4">
        {managedGroups.map((group) => {
          const isExpanded = expandedGroup === group.id;
          const myMembership = myMemberships.find((m) => m.groupId === group.id);
          const canManageGroup = true; // all visible groups are ones the user manages

          return (
            <div key={group.id} className="content-card">
              <div className="flex items-start gap-2">
                {/* Group header */}
                <button
                  type="button"
                  onClick={() => setExpandedGroup(isExpanded ? null : group.id)}
                  className="flex-1 flex items-center justify-between gap-3 text-left min-w-0"
                >
                  <div className="min-w-0">
                    <h2 className="font-semibold text-neutral-100">{group.name}</h2>
                    {group.description && (
                      <p className="text-xs text-neutral-500 mt-0.5">{group.description}</p>
                    )}
                  </div>
                  <div className="flex items-center gap-2 shrink-0">
                    {canManageGroup && (
                      <span className="badge-success">
                        <ShieldCheck size={12} /> Manager
                      </span>
                    )}
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

                {isAdmin && (
                  <button
                    type="button"
                    title="Delete group"
                    aria-label={`Delete ${group.name}`}
                    onClick={() => setDeletingGroupId(group.id)}
                    className="p-2 rounded-lg text-neutral-500 hover:text-red-400 hover:bg-red-950/30 transition shrink-0"
                  >
                    <Trash2 size={18} />
                  </button>
                )}
              </div>

              {deletingGroupId === group.id && (
                <DeleteGroupConfirm
                  groupId={group.id}
                  groupName={group.name}
                  onCancel={() => setDeletingGroupId(null)}
                  onDeleted={() => {
                    setDeletingGroupId(null);
                    if (expandedGroup === group.id) setExpandedGroup(null);
                  }}
                />
              )}

              {/* Expanded body */}
              {isExpanded && (
                <div className="mt-4 pt-4 border-t border-neutral-700 space-y-4">
                  <GroupMembers
                    groupId={group.id}
                    currentUser={user}
                    canManage={canManageGroup}
                    isAdmin={isAdmin}
                  />
                  {/* Manager: add other users */}
                  {canManageGroup && user && (
                    <div className="pt-2 border-t border-neutral-800">
                      <AddMemberForm groupId={group.id} />
                    </div>
                  )}
                </div>
              )}
            </div>
          );
        })}
      </div>

      {isAdmin && <AdminsSection currentUserId={user?.sub} />}
    </div>
  );
}

// ── Sub-components ────────────────────────────────────────────────────────────

function DeleteGroupConfirm({
  groupId,
  groupName,
  onCancel,
  onDeleted,
}: {
  groupId: number;
  groupName: string;
  onCancel: () => void;
  onDeleted: () => void;
}) {
  const deleteGroup = useDeleteGroup();
  const { data: members = [], isLoading: membersLoading } = useGroupMembers(groupId, true);

  const handleConfirm = async () => {
    await deleteGroup.mutateAsync(groupId);
    onDeleted();
  };

  const hasMembers = members.length > 0;

  return (
    <div className="mt-3 p-3 rounded-xl bg-red-950/20 border border-red-900/40 space-y-3">
      {membersLoading ? (
        <div className="flex justify-center py-2">
          <Loader2 className="animate-spin text-red-400" size={18} />
        </div>
      ) : (
        <>
          <p className="text-sm text-neutral-200">
            {hasMembers ? (
              <>
                <strong>{groupName}</strong> has {members.length} member{members.length === 1 ? '' : 's'} assigned.
                Deleting this group will remove all memberships. This cannot be undone.
              </>
            ) : (
              <>
                Delete <strong>{groupName}</strong>? This cannot be undone.
              </>
            )}
          </p>

          {deleteGroup.isError && (
            <p className="message-error text-sm">Failed to delete group. Please try again.</p>
          )}

          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={handleConfirm}
              disabled={deleteGroup.isPending}
              className="px-3 py-1.5 rounded-lg text-sm font-semibold bg-red-600 text-white hover:bg-red-500 disabled:opacity-50"
            >
              {deleteGroup.isPending ? 'Deleting…' : 'Delete group'}
            </button>
            <button
              type="button"
              onClick={onCancel}
              disabled={deleteGroup.isPending}
              className="px-3 py-1.5 rounded-lg text-sm text-neutral-400 hover:text-neutral-200"
            >
              Cancel
            </button>
          </div>
        </>
      )}
    </div>
  );
}

function CreateGroupForm() {
  const createGroup = useCreateGroup();
  const [isOpen, setIsOpen] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    await createGroup.mutateAsync({
      name: name.trim(),
      description: description.trim() || undefined,
    });

    setName('');
    setDescription('');
    setIsOpen(false);
  };

  if (!isOpen) {
    return (
      <button
        type="button"
        onClick={() => setIsOpen(true)}
        className="btn-apple flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-sm mb-6"
      >
        <Plus size={16} />
        Create Group
      </button>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="content-card mb-6 space-y-3">
      <div className="flex items-center justify-between">
        <h2 className="font-semibold text-neutral-100">Create Group</h2>
        <button
          type="button"
          onClick={() => setIsOpen(false)}
          className="text-xs text-neutral-500 hover:text-neutral-300"
        >
          Cancel
        </button>
      </div>

      <div>
        <label className="form-label">Group name</label>
        <input
          className="form-input"
          placeholder="e.g. Youth Ministry"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
        />
      </div>

      <div>
        <label className="form-label">Description</label>
        <input
          className="form-input"
          placeholder="What does this group handle?"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
      </div>

      {createGroup.isError && (
        <p className="message-error">Failed to create group. Please try again.</p>
      )}

      <button
        type="submit"
        disabled={createGroup.isPending}
        className="btn-apple flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-sm disabled:opacity-50"
      >
        {createGroup.isPending ? (
          <Loader2 size={16} className="animate-spin" />
        ) : (
          <Plus size={16} />
        )}
        Create Group
      </button>
    </form>
  );
}

function AdminsSection({ currentUserId }: { currentUserId?: string }) {
  const { data: roles = [], isLoading } = useAllRoles();
  const setRole = useSetUserRole();

  const admins = roles.filter((r) => r.role === 'Admin');
  const members = roles.filter((r) => r.role === 'Member');

  return (
    <div className="content-card mt-8">
      <div className="flex items-center gap-2 mb-1">
        <ShieldCheck size={18} className="text-[#34C759]" />
        <h2 className="font-semibold text-neutral-100">Admins</h2>
      </div>
      <p className="text-xs text-neutral-500 mb-4">
        Admins can create groups and promote other users to admin.
      </p>

      {isLoading ? (
        <div className="flex justify-center py-4"><Loader2 className="animate-spin text-[#34C759]" size={20} /></div>
      ) : (
        <ul className="space-y-2 mb-4">
          {admins.map((a) => (
            <li key={a.userId} className="flex items-center justify-between gap-3 bg-neutral-900/60 rounded-xl px-3 py-2.5">
              <div className="min-w-0">
                <p className="text-sm font-medium text-neutral-100 truncate">
                  {userPrimaryLabel(a)}
                  {a.userId === currentUserId && <span className="ml-1.5 text-[#34C759] text-xs">(you)</span>}
                </p>
                {a.displayName && a.userEmail && (
                  <p className="text-xs text-neutral-500 truncate">{a.userEmail}</p>
                )}
              </div>
              <span className="badge-success">Admin</span>
            </li>
          ))}
        </ul>
      )}

      {members.length > 0 && (
        <details className="mb-4">
          <summary className="text-xs text-neutral-500 cursor-pointer hover:text-neutral-300">
            {members.length} other user{members.length === 1 ? '' : 's'} with member access
          </summary>
          <ul className="space-y-2 mt-2">
            {members.map((m) => (
              <li key={m.userId} className="flex items-center justify-between gap-3 bg-neutral-900/60 rounded-xl px-3 py-2.5">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-neutral-100 truncate">{userPrimaryLabel(m)}</p>
                  {m.displayName && m.userEmail && (
                    <p className="text-xs text-neutral-500 truncate">{m.userEmail}</p>
                  )}
                </div>
                <button
                  type="button"
                  onClick={() =>
                    setRole.mutate({
                      userId: m.userId,
                      displayName: m.displayName,
                      userEmail: m.userEmail,
                      role: 'Admin',
                    })
                  }
                  disabled={setRole.isPending}
                  className="text-xs text-[#34C759] hover:underline disabled:opacity-50"
                >
                  Make admin
                </button>
              </li>
            ))}
          </ul>
        </details>
      )}
    </div>
  );
}

function GroupManagerAssignmentForm({ knownUsers }: { knownUsers: UserRoleDto[] }) {
  const [query, setQuery] = useState('');
  const [selectedUser, setSelectedUser] = useState<UserRoleDto | null>(null);
  const { data: assignments, isLoading: assignmentsLoading } = useGroupManagerAssignments(
    selectedUser?.userId ?? null,
  );
  const assignmentList = assignments ?? EMPTY_ASSIGNMENTS;
  const setManagers = useSetGroupManagers();
  const [checkedIds, setCheckedIds] = useState<number[]>([]);
  const [justSaved, setJustSaved] = useState(false);

  useEffect(() => {
    if (!selectedUser) {
      setCheckedIds((prev) => (prev.length === 0 ? prev : []));
      return;
    }

    const nextIds = assignmentList.filter((a) => a.canManage).map((a) => a.groupId);
    setCheckedIds((prev) => (sameIdSet(prev, nextIds) ? prev : nextIds));
  }, [selectedUser?.userId, assignmentList]);

  const q = query.trim().toLowerCase();
  const filteredUsers = q
    ? knownUsers.filter(
        (u) =>
          u.displayName.toLowerCase().includes(q) ||
          u.userEmail.toLowerCase().includes(q) ||
          u.userId.toLowerCase().includes(q),
      )
    : [];

  const handleSelectUser = (u: UserRoleDto) => {
    setSelectedUser(u);
    setQuery('');
    setJustSaved(false);
  };

  const toggleGroup = (groupId: number) => {
    setCheckedIds((prev) =>
      prev.includes(groupId) ? prev.filter((id) => id !== groupId) : [...prev, groupId],
    );
    setJustSaved(false);
  };

  const handleSave = async () => {
    if (!selectedUser) return;
    await setManagers.mutateAsync({
      userId: selectedUser.userId,
      displayName: selectedUser.displayName,
      userEmail: selectedUser.userEmail,
      groupIds: checkedIds,
    });
    setJustSaved(true);
  };

  return (
    <div className="space-y-3">
      <p className="form-label">Assign group managers</p>
      <p className="text-xs text-neutral-500 -mt-2">
        Choose a user, then select which groups they can manage.
      </p>

      {selectedUser ? (
        <div className="flex items-center justify-between gap-3 bg-neutral-900/60 rounded-xl px-3 py-2.5">
          <div className="min-w-0">
            <p className="text-sm font-medium text-neutral-100 truncate">
              {userPrimaryLabel(selectedUser)}
            </p>
            {selectedUser.displayName && selectedUser.userEmail && (
              <p className="text-xs text-neutral-500 truncate">{selectedUser.userEmail}</p>
            )}
          </div>
          <button
            type="button"
            onClick={() => setSelectedUser(null)}
            className="text-xs text-neutral-500 hover:text-neutral-300 shrink-0"
          >
            Change
          </button>
        </div>
      ) : (
        <div className="relative">
          <input
            className="form-input"
            placeholder="Start typing a name or email…"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          />
          {q && (
            <div className="absolute z-10 mt-1 w-full max-h-56 overflow-y-auto content-card-sm !p-1.5 space-y-0.5">
              {filteredUsers.length > 0 ? (
                filteredUsers.map((u) => (
                  <button
                    key={u.userId}
                    type="button"
                    onClick={() => handleSelectUser(u)}
                    className="w-full text-left px-2.5 py-2 rounded-lg hover:bg-neutral-700 transition"
                  >
                    <p className="text-sm text-neutral-100 truncate">{userPrimaryLabel(u)}</p>
                    {u.displayName && u.userEmail && (
                      <p className="text-xs text-neutral-500 truncate">{u.userEmail}</p>
                    )}
                  </button>
                ))
              ) : (
                <p className="text-xs text-neutral-500 px-2.5 py-2">No matching users found.</p>
              )}
            </div>
          )}
        </div>
      )}

      {selectedUser && (
        <>
          {assignmentsLoading ? (
            <div className="flex justify-center py-4">
              <Loader2 className="animate-spin text-[#34C759]" size={20} />
            </div>
          ) : assignmentList.length === 0 ? (
            <p className="text-sm text-neutral-500">No groups exist yet.</p>
          ) : (
            <div className="flex flex-wrap gap-2">
              {assignmentList.map((a) => (
                <button
                  key={a.groupId}
                  type="button"
                  onClick={() => toggleGroup(a.groupId)}
                  className={checkedIds.includes(a.groupId) ? 'chip chip-selected' : 'chip'}
                >
                  {a.groupName}
                </button>
              ))}
            </div>
          )}

          {setManagers.isError && (
            <p className="message-error">Failed to update group managers. Please try again.</p>
          )}

          {justSaved && !setManagers.isPending && (
            <p className="text-xs text-[#34C759]">Saved.</p>
          )}

          <button
            type="button"
            onClick={handleSave}
            disabled={setManagers.isPending || assignmentsLoading}
            className="btn-apple flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-sm disabled:opacity-50"
          >
            {setManagers.isPending ? <Loader2 size={16} className="animate-spin" /> : <ShieldCheck size={16} />}
            Save group access
          </button>
        </>
      )}
    </div>
  );
}

function GroupMembers({
  groupId,
  currentUser,
  canManage,
  isAdmin,
}: {
  groupId: number;
  currentUser: { sub: string } | null;
  canManage: boolean;
  isAdmin: boolean;
}) {
  const { data: members = [], isLoading } = useGroupMembers(groupId);
  const removeMember = useRemoveGroupMember(groupId);
  const updateEmail = useUpdateEmailPreference();
  const updateManager = useUpdateGroupManager(groupId);

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
        const canRemove = canManage || isMe;
        return (
          <li key={m.id} className="flex items-center justify-between gap-3 bg-neutral-900/60 rounded-xl px-3 py-2.5">
            <div className="min-w-0">
              <p className="text-sm font-medium text-neutral-100 truncate">
                {m.displayName}
                {isMe && <span className="ml-1.5 text-[#34C759] text-xs">(you)</span>}
                {m.canManage && <span className="ml-1.5 text-xs text-neutral-500">· manager</span>}
              </p>
              <p className="text-xs text-neutral-500 truncate">{m.userEmail}</p>
            </div>
            <div className="flex items-center gap-1.5 shrink-0">
              {isAdmin && (
                <button
                  type="button"
                  title={m.canManage ? 'Revoke manage permission' : 'Grant manage permission'}
                  onClick={() =>
                    updateManager.mutate({
                      membershipId: m.id,
                      canManage: !m.canManage,
                    })
                  }
                  disabled={updateManager.isPending}
                  className={`p-1.5 rounded-lg transition hover:bg-neutral-700 ${
                    m.canManage ? 'text-[#34C759]' : 'text-neutral-500 hover:text-white'
                  }`}
                >
                  <ShieldCheck size={16} />
                </button>
              )}
              {isMe && (
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
              )}
              {canRemove && (
                <button
                  type="button"
                  title="Remove from group"
                  onClick={() => removeMember.mutate(m.id)}
                  disabled={removeMember.isPending}
                  className="p-1.5 rounded-lg text-neutral-500 hover:text-red-400 hover:bg-red-950/30 transition"
                >
                  <Trash2 size={16} />
                </button>
              )}
            </div>
          </li>
        );
      })}
    </ul>
  );
}

function JoinGroupForm({
  groupId,
  currentUser,
}: {
  groupId: number;
  currentUser: { sub: string; email: string };
}) {
  const addMember = useAddGroupMember(groupId);
  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState(currentUser.email);
  const [emailNotifs, setEmailNotifs] = useState(true);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const name = displayName.trim();
    if (!name) return;

    await addMember.mutateAsync({
      userId: currentUser.sub,
      displayName: name,
      userEmail: email.trim(),
      emailNotificationsEnabled: emailNotifs,
    });

    setDisplayName('');
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-3">
      <p className="form-label">Join this group</p>
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
          {String(addMember.error).includes('409')
            ? 'You are already a member of this group.'
            : 'Failed to join. Please try again.'}
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
        Join group
      </button>
    </form>
  );
}

function AddMemberForm({ groupId }: { groupId: number }) {
  const addMember = useAddGroupMember(groupId);
  const { data: knownUsers = [] } = useAllRoles(true);
  const [query, setQuery] = useState('');
  const [selectedUser, setSelectedUser] = useState<UserRoleDto | null>(null);
  const [emailNotifs, setEmailNotifs] = useState(true);
  const [success, setSuccess] = useState(false);

  const q = query.trim().toLowerCase();
  const filteredUsers = q
    ? knownUsers.filter(
        (u) =>
          u.displayName.toLowerCase().includes(q) ||
          u.userEmail.toLowerCase().includes(q) ||
          u.userId.toLowerCase().includes(q),
      )
    : [];

  const handleAdd = async () => {
    if (!selectedUser) return;
    await addMember.mutateAsync({
      userId: selectedUser.userId,
      displayName: selectedUser.displayName || selectedUser.userEmail,
      userEmail: selectedUser.userEmail,
      emailNotificationsEnabled: emailNotifs,
    });
    setSelectedUser(null);
    setQuery('');
    setSuccess(true);
    setTimeout(() => setSuccess(false), 3000);
  };

  return (
    <div className="space-y-3">
      <p className="form-label">Add a member</p>

      {selectedUser ? (
        <div className="flex items-center justify-between gap-3 bg-neutral-900/60 rounded-xl px-3 py-2.5">
          <div className="min-w-0">
            <p className="text-sm font-medium text-neutral-100 truncate">
              {userPrimaryLabel(selectedUser)}
            </p>
            {selectedUser.displayName && selectedUser.userEmail && (
              <p className="text-xs text-neutral-500 truncate">{selectedUser.userEmail}</p>
            )}
          </div>
          <button
            type="button"
            onClick={() => setSelectedUser(null)}
            className="text-xs text-neutral-500 hover:text-neutral-300 shrink-0"
          >
            Change
          </button>
        </div>
      ) : (
        <div className="relative">
          <input
            className="form-input"
            placeholder="Search by name or email…"
            value={query}
            onChange={(e) => { setQuery(e.target.value); setSuccess(false); }}
          />
          {q && (
            <div className="absolute z-10 mt-1 w-full max-h-48 overflow-y-auto content-card-sm !p-1.5 space-y-0.5">
              {filteredUsers.length > 0 ? (
                filteredUsers.map((u) => (
                  <button
                    key={u.userId}
                    type="button"
                    onClick={() => { setSelectedUser(u); setQuery(''); }}
                    className="w-full text-left px-2.5 py-2 rounded-lg hover:bg-neutral-700 transition"
                  >
                    <p className="text-sm text-neutral-100 truncate">{userPrimaryLabel(u)}</p>
                    {u.displayName && u.userEmail && (
                      <p className="text-xs text-neutral-500 truncate">{u.userEmail}</p>
                    )}
                  </button>
                ))
              ) : (
                <p className="text-xs text-neutral-500 px-2.5 py-2">No matching users found.</p>
              )}
            </div>
          )}
        </div>
      )}

      {selectedUser && (
        <>
          <label className="flex items-center gap-2.5 cursor-pointer">
            <input
              type="checkbox"
              checked={emailNotifs}
              onChange={(e) => setEmailNotifs(e.target.checked)}
              className="w-4 h-4 accent-[#34C759]"
            />
            <span className="text-sm text-neutral-300">Email notifications enabled</span>
          </label>

          {addMember.isError && (
            <p className="message-error">
              {String(addMember.error).includes('409')
                ? 'This user is already a member of this group.'
                : 'Failed to add member. Please try again.'}
            </p>
          )}

          <button
            type="button"
            onClick={handleAdd}
            disabled={addMember.isPending}
            className="btn-apple flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-sm disabled:opacity-50"
          >
            {addMember.isPending ? (
              <Loader2 size={16} className="animate-spin" />
            ) : (
              <UserPlus size={16} />
            )}
            Add to group
          </button>
        </>
      )}

      {success && <p className="message-success text-sm">Member added successfully.</p>}
    </div>
  );
}
