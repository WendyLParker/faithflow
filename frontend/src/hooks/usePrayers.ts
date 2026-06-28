import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  prayerService,
  type PrayerCreateDto,
  type PrayerUpdateDto,
} from '@/services/prayerService';

export const prayerKeys = {
  all: ['prayers'] as const,
  detail: (id: number) => ['prayers', id] as const,
};

export function usePrayers() {
  return useQuery({
    queryKey: prayerKeys.all,
    queryFn: prayerService.getAll,
  });
}

export function usePrayer(id: number) {
  return useQuery({
    queryKey: prayerKeys.detail(id),
    queryFn: () => prayerService.getById(id),
    enabled: id > 0,
  });
}

export function useCreatePrayer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: PrayerCreateDto) => prayerService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: prayerKeys.all });
    },
  });
}

export function useUpdatePrayer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: PrayerUpdateDto }) =>
      prayerService.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: prayerKeys.all });
      queryClient.invalidateQueries({ queryKey: prayerKeys.detail(id) });
    },
  });
}

export function useDeletePrayer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => prayerService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: prayerKeys.all });
    },
  });
}

export function useMarkPrayerAnswered() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => prayerService.markAsAnswered(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: prayerKeys.all });
      queryClient.invalidateQueries({ queryKey: prayerKeys.detail(id) });
    },
  });
}
