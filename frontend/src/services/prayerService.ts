import api from '@/lib/api';

export interface RequestTypeDto {
  id: number;
  name: string;
}

export interface PrayerCreateDto {
  title: string;
  content?: string;
  categories: string[];
  requestTypeId: number;
}

export interface PrayerUpdateDto {
  content?: string;
  isAnswered?: boolean;
}

export interface PrayerResponseDto {
  id: number;
  title: string;
  content?: string;
  prayerDate: string;
  isAnswered: boolean;
  answeredDate?: string;
  categories: string[];
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

export const prayerService = {
  async getAll() {
    const response = await api.get<PrayerResponseDto[]>('/api/prayer');
    return response.data;
  },

  async getById(id: number) {
    const response = await api.get<PrayerResponseDto>(`/api/prayer/${id}`);
    return response.data;
  },

  async create(data: PrayerCreateDto) {
    const response = await api.post<PrayerResponseDto>('/api/prayer', data);
    return response.data;
  },

  async update(id: number, data: PrayerUpdateDto) {
    const response = await api.put<PrayerResponseDto>(`/api/prayer/${id}`, data);
    return response.data;
  },

  async delete(id: number) {
    await api.delete(`/api/prayer/${id}`);
  },

  async markAsAnswered(id: number) {
    return prayerService.update(id, { isAnswered: true });
  },
};
