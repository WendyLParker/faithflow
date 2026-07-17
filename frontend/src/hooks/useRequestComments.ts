import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { requestCommentService } from '@/services/requestCommentService';

export const commentKeys = {
  byRequest: (requestId: number) => ['request-comments', requestId] as const,
};

export function useRequestComments(requestId: number) {
  return useQuery({
    queryKey: commentKeys.byRequest(requestId),
    queryFn: () => requestCommentService.getByRequest(requestId),
    enabled: requestId > 0,
  });
}

export function useAddRequestComment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ requestId, content }: { requestId: number; content: string }) =>
      requestCommentService.add(requestId, content),
    onSuccess: (_, { requestId }) => {
      queryClient.invalidateQueries({ queryKey: commentKeys.byRequest(requestId) });
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}
