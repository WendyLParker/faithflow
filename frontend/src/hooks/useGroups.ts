import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  groupService,
  type AddGroupMemberDto,
  type CreateGroupDto,
  type SetGroupManagersDto,
} from '@/services/notificationService';

export const groupKeys = {
  all: ['groups'] as const,
  managed: ['groups', 'managed'] as const,
  members: (id: number) => ['groups', id, 'members'] as const,
  mine: ['groups', 'mine'] as const,
  managerAssignments: (userId: string) => ['groups', 'manager-assignments', userId] as const,
};

export function useGroups() {
  return useQuery({
    queryKey: groupKeys.all,
    queryFn: groupService.getAll,
  });
}

export function useManagedGroups() {
  return useQuery({
    queryKey: groupKeys.managed,
    queryFn: groupService.getManaged,
  });
}

export function useCreateGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateGroupDto) => groupService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: groupKeys.all });
      queryClient.invalidateQueries({ queryKey: groupKeys.managed });
    },
  });
}

export function useDeleteGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (groupId: number) => groupService.delete(groupId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: groupKeys.all });
      queryClient.invalidateQueries({ queryKey: groupKeys.managed });
      queryClient.invalidateQueries({ queryKey: groupKeys.mine });
    },
  });
}

export function useGroupMembers(groupId: number, enabled = true) {
  return useQuery({
    queryKey: groupKeys.members(groupId),
    queryFn: () => groupService.getMembers(groupId),
    enabled: enabled && groupId > 0,
  });
}

export function useMyMemberships() {
  return useQuery({
    queryKey: groupKeys.mine,
    queryFn: groupService.getMyMemberships,
  });
}

export function useAddGroupMember(groupId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AddGroupMemberDto) =>
      groupService.addMember(groupId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: groupKeys.members(groupId) });
      queryClient.invalidateQueries({ queryKey: groupKeys.mine });
    },
  });
}

export function useUpdateEmailPreference() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ membershipId, enabled }: { membershipId: number; enabled: boolean }) =>
      groupService.updateEmailPreference(membershipId, enabled),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: groupKeys.mine });
    },
  });
}

export function useRemoveGroupMember(groupId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (membershipId: number) => groupService.removeMember(membershipId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: groupKeys.members(groupId) });
      queryClient.invalidateQueries({ queryKey: groupKeys.mine });
    },
  });
}

export function useUpdateGroupManager(groupId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ membershipId, canManage }: { membershipId: number; canManage: boolean }) =>
      groupService.updateManager(membershipId, canManage),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: groupKeys.members(groupId) });
      queryClient.invalidateQueries({ queryKey: groupKeys.managed });
    },
  });
}

export function useGroupManagerAssignments(userId: string | null) {
  return useQuery({
    queryKey: groupKeys.managerAssignments(userId ?? ''),
    queryFn: () => groupService.getManagerAssignments(userId as string),
    enabled: !!userId,
  });
}

export function useSetGroupManagers() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: SetGroupManagersDto) => groupService.setManagerAssignments(data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: groupKeys.managerAssignments(variables.userId) });
      queryClient.invalidateQueries({ queryKey: groupKeys.all });
      queryClient.invalidateQueries({ queryKey: groupKeys.managed });
    },
  });
}
