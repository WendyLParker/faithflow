import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  requestService,
  type RequestCreateDto,
  type RequestUpdateDto,
} from '@/services/requestService';

export const requestKeys = {
  all: ['requests'] as const,
  detail: (id: number) => ['requests', id] as const,
};

export function useRequests() {
  return useQuery({
    queryKey: requestKeys.all,
    queryFn: requestService.getAll,
  });
}

export function useRequest(id: number) {
  return useQuery({
    queryKey: requestKeys.detail(id),
    queryFn: () => requestService.getById(id),
    enabled: id > 0,
  });
}

export function useCreateRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: RequestCreateDto) => requestService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: requestKeys.all });
    },
  });
}

export function useUpdateRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: RequestUpdateDto }) =>
      requestService.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: requestKeys.all });
      queryClient.invalidateQueries({ queryKey: requestKeys.detail(id) });
    },
  });
}

export function useDeleteRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => requestService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: requestKeys.all });
    },
  });
}

export function useMarkRequestCompleted() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => requestService.markAsCompleted(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: requestKeys.all });
      queryClient.invalidateQueries({ queryKey: requestKeys.detail(id) });
    },
  });
}
