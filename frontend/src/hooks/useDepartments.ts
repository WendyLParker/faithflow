import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { departmentService, type AddDepartmentMemberDto } from '@/services/notificationService';

export const departmentKeys = {
  all: ['departments'] as const,
  members: (id: number) => ['departments', id, 'members'] as const,
  mine: ['departments', 'mine'] as const,
};

export function useDepartments() {
  return useQuery({
    queryKey: departmentKeys.all,
    queryFn: departmentService.getAll,
  });
}

export function useDepartmentMembers(departmentId: number) {
  return useQuery({
    queryKey: departmentKeys.members(departmentId),
    queryFn: () => departmentService.getMembers(departmentId),
    enabled: departmentId > 0,
  });
}

export function useMyMemberships() {
  return useQuery({
    queryKey: departmentKeys.mine,
    queryFn: departmentService.getMyMemberships,
  });
}

export function useAddDepartmentMember(departmentId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AddDepartmentMemberDto) =>
      departmentService.addMember(departmentId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: departmentKeys.members(departmentId) });
      queryClient.invalidateQueries({ queryKey: departmentKeys.mine });
    },
  });
}

export function useUpdateEmailPreference() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ membershipId, enabled }: { membershipId: number; enabled: boolean }) =>
      departmentService.updateEmailPreference(membershipId, enabled),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: departmentKeys.mine });
    },
  });
}

export function useRemoveDepartmentMember(departmentId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (membershipId: number) => departmentService.removeMember(membershipId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: departmentKeys.members(departmentId) });
      queryClient.invalidateQueries({ queryKey: departmentKeys.mine });
    },
  });
}
