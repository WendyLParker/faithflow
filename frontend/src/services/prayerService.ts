import api from '@/lib/api';

export interface PrayerCreateDto {
  title: string;
  content?: string;
  categories: string[];
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
  streakDays?: number;
}

export const prayerService = {
  async getAll() {
    const response = await api.get<PrayerResponseDto[]>('/api/prayer');
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

  // Convenience method
  async markAsAnswered(id: number) {
    return await prayerService.update(id, { isAnswered: true });
  }
};