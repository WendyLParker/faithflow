import api from '@/lib/api';

export interface RequestTypeDto {
  id: number;
  name: string;
}

export interface RequestCreateDto {
  title: string;
  content?: string;
  requestTypeId: number;
  groupIds: number[];
}

export interface RequestUpdateDto {
  content?: string;
  isCompleted?: boolean;
}

export interface CostEstimateRequestDto {
  requestType: string;
  title: string;
  description: string;
}

export interface CostEstimateResponseDto {
  low_estimate: number;
  most_likely: number;
  high_estimate: number;
  currency: string;
  confidence: 'low' | 'medium' | 'high';
  reasoning: string;
}

export type RequestScope = 'sent' | 'received';

export interface RequestResponseDto {
  id: number;
  title: string;
  content?: string;
  requestDate: string;
  isCompleted: boolean;
  completedDate?: string;
  groupNames: string[];
  requestTypeId: number;
  requestTypeName: string;
  requestStatus: string;
  fulfilledDate?: string;
  isOwnedByCurrentUser: boolean;
  canFulfill: boolean;
  canClose: boolean;
  streakDays?: number;
}

export const requestTypeService = {
  async getAll() {
    const response = await api.get<RequestTypeDto[]>('/api/requesttype');
    return response.data;
  },
};

export const requestService = {
  async getAll(scope: RequestScope = 'sent') {
    const response = await api.get<RequestResponseDto[]>('/api/request', {
      params: { scope },
    });
    return response.data;
  },

  async getById(id: number) {
    const response = await api.get<RequestResponseDto>(`/api/request/${id}`);
    return response.data;
  },

  async create(data: RequestCreateDto) {
    const response = await api.post<RequestResponseDto>('/api/request', data);
    return response.data;
  },

  async update(id: number, data: RequestUpdateDto) {
    const response = await api.put<RequestResponseDto>(`/api/request/${id}`, data);
    return response.data;
  },

  async delete(id: number) {
    await api.delete(`/api/request/${id}`);
  },

  async fulfill(id: number) {
    const response = await api.post<RequestResponseDto>(`/api/request/${id}/fulfill`);
    return response.data;
  },

  async close(id: number) {
    const response = await api.post<RequestResponseDto>(`/api/request/${id}/close`);
    return response.data;
  },

  async estimateCost(data: CostEstimateRequestDto) {
    const response = await api.post<CostEstimateResponseDto>(
      '/api/requests/estimate-cost',
      data,
    );
    return response.data;
  },

  /** @deprecated Use close() after assignee has fulfilled */
  async markAsCompleted(id: number) {
    return requestService.close(id);
  },
};
