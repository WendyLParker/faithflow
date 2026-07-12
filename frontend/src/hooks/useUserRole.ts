import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userRoleService, type SetUserRoleDto, type UpdateProfileDto } from '@/services/userRoleService';

export const userRoleKeys = {
  mine: ['userRole', 'mine'] as const,
  all: ['userRole', 'all'] as const,
};

export function useMyRole() {
  return useQuery({
    queryKey: userRoleKeys.mine,
    queryFn: userRoleService.getMine,
  });
}

export function useAllRoles(enabled = true) {
  return useQuery({
    queryKey: userRoleKeys.all,
    queryFn: userRoleService.getAll,
    enabled,
  });
}

export function useSetUserRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: SetUserRoleDto) => userRoleService.setRole(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userRoleKeys.all });
      queryClient.invalidateQueries({ queryKey: userRoleKeys.mine });
    },
  });
}

export function useUpdateMyProfile() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateProfileDto) => userRoleService.updateMyProfile(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userRoleKeys.mine });
    },
  });
}
