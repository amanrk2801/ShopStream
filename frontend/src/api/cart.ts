import api from './client'

export interface CartItem {
  id: string
  productId: string
  productName: string
  quantity: number
  unitPrice: number
  totalPrice: number
  imageUrl?: string
}

export interface Cart {
  id: string
  items: CartItem[]
  totalAmount: number
}

export const cartApi = {
  get: () => api.get<Cart>('/cart'),
  
  addItem: (productId: string, quantity: number) =>
    api.post<Cart>('/cart/items', { productId, quantity }),
  
  updateItem: (cartItemId: string, quantity: number) =>
    api.put<Cart>(`/cart/items/${cartItemId}`, { quantity }),
  
  removeItem: (cartItemId: string) =>
    api.delete(`/cart/items/${cartItemId}`),
  
  clear: () => api.delete('/cart'),
}
