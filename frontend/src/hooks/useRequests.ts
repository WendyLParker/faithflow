import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  requestService,
  type RequestCreateDto,
  type RequestScope,
  type RequestUpdateDto,
} from '@/services/requestService';

export const requestKeys = {
  sent: ['requests', 'sent'] as const,
  received: ['requests', 'received'] as const,
  detail: (id: number) => ['requests', id] as const,
};

export function useSentRequests() {
  return useQuery({
    queryKey: requestKeys.sent,
    queryFn: () => requestService.getAll('sent'),
  });
}

export function useReceivedRequests() {
  return useQuery({
    queryKey: requestKeys.received,
    queryFn: () => requestService.getAll('received'),
  });
}

export function useRequestInbox() {
  const sent = useSentRequests();
  const received = useReceivedRequests();

  return {
    sent: sent.data ?? [],
    received: received.data ?? [],
    isLoading: sent.isLoading || received.isLoading,
    error: sent.error ?? received.error,
  };
}

/** @deprecated Prefer useSentRequests or useRequestInbox */
export function useRequests() {
  return useSentRequests();
}

export function useRequest(id: number) {
  return useQuery({
    queryKey: requestKeys.detail(id),
    queryFn: () => requestService.getById(id),
    enabled: id > 0,
  });
}

function invalidateRequestLists(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: ['requests', 'sent'] });
  queryClient.invalidateQueries({ queryKey: ['requests', 'received'] });
}

export function useCreateRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: RequestCreateDto) => requestService.create(data),
    onSuccess: () => {
      invalidateRequestLists(queryClient);
    },
  });
}

export function useUpdateRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: RequestUpdateDto }) =>
      requestService.update(id, data),
    onSuccess: (_, { id }) => {
      invalidateRequestLists(queryClient);
      queryClient.invalidateQueries({ queryKey: requestKeys.detail(id) });
    },
  });
}

export function useDeleteRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => requestService.delete(id),
    onSuccess: () => {
      invalidateRequestLists(queryClient);
    },
  });
}

export function useMarkRequestCompleted() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => requestService.close(id),
    onSuccess: (_, id) => {
      invalidateRequestLists(queryClient);
      queryClient.invalidateQueries({ queryKey: requestKeys.detail(id) });
    },
  });
}

export function useFulfillRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => requestService.fulfill(id),
    onSuccess: (_, id) => {
      invalidateRequestLists(queryClient);
      queryClient.invalidateQueries({ queryKey: requestKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}

export function useCloseRequest() {
  return useMarkRequestCompleted();
}

export type { RequestScope };
