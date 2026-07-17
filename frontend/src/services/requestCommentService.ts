import api from '@/lib/api';

export interface RequestCommentDto {
  id: number;
  requestId: number;
  authorName: string;
  content: string;
  createdAt: string;
  isOwnComment: boolean;
}

export const requestCommentService = {
  async getByRequest(requestId: number) {
    const response = await api.get<RequestCommentDto[]>(`/api/request/${requestId}/comments`);
    return response.data;
  },

  async add(requestId: number, content: string) {
    const response = await api.post<RequestCommentDto>(`/api/request/${requestId}/comments`, {
      content,
    });
    return response.data;
  },
};
