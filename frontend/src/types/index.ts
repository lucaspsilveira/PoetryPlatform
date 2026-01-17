export interface User {
  id: string;
  email: string;
  displayName: string;
}

export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  displayName: string;
}

export interface Author {
  id: string;
  displayName: string;
}

export interface Poem {
  id: number;
  title: string;
  content: string;
  createdAt: string;
  updatedAt: string | null;
  isPublished: boolean;
  author: Author;
}

export interface PoemListResponse {
  poems: Poem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreatePoemRequest {
  title: string;
  content: string;
  isPublished?: boolean;
}

export interface UpdatePoemRequest {
  title?: string;
  content?: string;
  isPublished?: boolean;
}
