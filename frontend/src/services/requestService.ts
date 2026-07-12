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
  streakDays?: number;
}

export const requestTypeService = {
  async getAll() {
    const response = await api.get<RequestTypeDto[]>('/api/requesttype');
    return response.data;
  },
};

export const requestService = {
  async getAll() {
    const response = await api.get<RequestResponseDto[]>('/api/request');
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

  async markAsCompleted(id: number) {
    return requestService.update(id, { isCompleted: true });
  },
};
