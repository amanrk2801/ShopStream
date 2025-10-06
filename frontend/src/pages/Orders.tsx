import { useState, useEffect } from 'react'
import { ordersApi, Order } from '../api/orders'
import './Orders.css'

export default function Orders() {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    loadOrders()
  }, [])

  const loadOrders = async () => {
    try {
      const response = await ordersApi.getAll()
      setOrders(response.data)
    } catch (error) {
      console.error('Failed to load orders:', error)
    } finally {
      setLoading(false)
    }
  }

  if (loading) return <div className="loading">Loading orders...</div>

  if (orders.length === 0) {
    return (
      <div className="empty-orders">
        <div className="container">
          <h1>No orders yet</h1>
          <p>Your order history will appear here</p>
        </div>
      </div>
    )
  }

  return (
    <div className="orders-page">
      <div className="container">
        <h1>My Orders</h1>
        
        <div className="orders-list">
          {orders.map((order) => (
            <div key={order.id} className="order-card">
              <div className="order-header">
                <div>
                  <h3>Order #{order.orderNumber}</h3>
                  <p className="order-date">
                    {new Date(order.createdAt).toLocaleDateString()}
                  </p>
                </div>
                <div className="order-status">{order.status}</div>
              </div>

              <div className="order-items">
                {order.items.map((item) => (
                  <div key={item.id} className="order-item">
                    <span>{item.productName}</span>
                    <span>Qty: {item.quantity}</span>
                    <span>₹{item.totalPrice.toFixed(2)}</span>
                  </div>
                ))}
              </div>

              <div className="order-total">
                Total: ₹{order.totalAmount.toFixed(2)}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
