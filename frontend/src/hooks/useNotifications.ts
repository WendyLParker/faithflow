import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationService } from '@/services/notificationService';
import { useAuth } from '@/hooks/useAuth';

export const notificationKeys = {
  all: (userId?: string) => ['notifications', userId] as const,
};

export function useNotifications() {
  const { isLoggedIn, user } = useAuth();

  return useQuery({
    queryKey: notificationKeys.all(user?.sub),
    queryFn: notificationService.getAll,
    enabled: isLoggedIn,
    refetchInterval: isLoggedIn ? 30_000 : false,
  });
}

export function useUnreadCount() {
  const query = useNotifications();

  return {
    ...query,
    data: query.data?.length ?? 0,
  };
}

export function useAcknowledgeNotification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => notificationService.acknowledge(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}

export function useDismissNotification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => notificationService.dismiss(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}
