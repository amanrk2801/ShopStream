import api from './client'

export interface Product {
  id: string
  name: string
  sku: string
  description: string
  price: number
  stockQuantity: number
  categoryId: string
  categoryName: string
  isActive: boolean
  images: ProductImage[]
}

export interface ProductImage {
  id: string
  url: string
  altText: string
  displayOrder: number
}

export interface ProductsResponse {
  items: Product[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export const productsApi = {
  getAll: (params?: {
    search?: string
    categoryId?: string
    page?: number
    pageSize?: number
  }) => api.get<ProductsResponse>('/products', { params }),
  
  getById: (id: string) => api.get<Product>(`/products/${id}`),
}
