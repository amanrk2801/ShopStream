import api from './client'

export interface Order {
  id: string
  orderNumber: string
  totalAmount: number
  status: string
  createdAt: string
  shippingAddress?: Address
  items: OrderItem[]
  payment?: Payment
}

export interface OrderItem {
  id: string
  productId: string
  productName: string
  quantity: number
  unitPrice: number
  totalPrice: number
}

export interface Address {
  id: string
  street: string
  city: string
  state: string
  zipCode: string
  country: string
  isDefault: boolean
}

export interface Payment {
  id: string
  provider: string
  status: string
  transactionId?: string
  amount: number
}

export const ordersApi = {
  checkout: (shippingAddressId: string, paymentProvider: string = 'Mock') =>
    api.post<Order>('/orders/checkout', { shippingAddressId, paymentProvider }),
  
  getAll: () => api.get<Order[]>('/orders'),
  
  getById: (id: string) => api.get<Order>(`/orders/${id}`),
}
