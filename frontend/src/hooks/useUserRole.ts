import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userRoleService, type SetUserRoleDto, type UpdateProfileDto } from '@/services/userRoleService';
import { useAuth } from '@/hooks/useAuth';

export const userRoleKeys = {
  mine: (userId?: string) => ['userRole', 'mine', userId] as const,
  all: ['userRole', 'all'] as const,
};

export function useMyRole() {
  const { isLoggedIn, user } = useAuth();

  return useQuery({
    queryKey: userRoleKeys.mine(user?.sub),
    queryFn: userRoleService.getMine,
    enabled: isLoggedIn,
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
      queryClient.invalidateQueries({ queryKey: ['userRole', 'mine'] });
    },
  });
}

export function useUpdateMyProfile() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateProfileDto) => userRoleService.updateMyProfile(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['userRole', 'mine'] });
    },
  });
}
