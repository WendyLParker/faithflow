import { useQuery } from '@tanstack/react-query';
import { requestTypeService } from '@/services/requestService';

export const requestTypeKeys = {
  all: ['requestTypes'] as const,
};

export function useRequestTypes() {
  return useQuery({
    queryKey: requestTypeKeys.all,
    queryFn: requestTypeService.getAll,
  });
}
