import { useState, useEffect } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { cartApi, CartItem } from '../api/cart'
import { useCartStore } from '../store/cartStore'
import './Cart.css'

export default function Cart() {
  const [items, setItems] = useState<CartItem[]>([])
  const [totalAmount, setTotalAmount] = useState(0)
  const [loading, setLoading] = useState(true)
  const navigate = useNavigate()
  const setCart = useCartStore(state => state.setCart)

  useEffect(() => {
    loadCart()
  }, [])

  const loadCart = async () => {
    try {
      const response = await cartApi.get()
      setItems(response.data.items)
      setTotalAmount(response.data.totalAmount)
      setCart(response.data.items, response.data.totalAmount)
    } catch (error) {
      console.error('Failed to load cart:', error)
    } finally {
      setLoading(false)
    }
  }

  const updateQuantity = async (itemId: string, quantity: number) => {
    try {
      const response = await cartApi.updateItem(itemId, quantity)
      setItems(response.data.items)
      setTotalAmount(response.data.totalAmount)
      setCart(response.data.items, response.data.totalAmount)
    } catch (error) {
      alert('Failed to update quantity')
    }
  }

  const removeItem = async (itemId: string) => {
    try {
      await cartApi.removeItem(itemId)
      await loadCart()
    } catch (error) {
      alert('Failed to remove item')
    }
  }

  if (loading) return <div className="loading">Loading cart...</div>

  if (items.length === 0) {
    return (
      <div className="empty-cart">
        <div className="container">
          <h1>Your cart is empty</h1>
          <p>Add some products to get started!</p>
          <Link to="/products" className="btn btn-primary">
            Browse Products
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="cart-page">
      <div className="container">
        <h1>Shopping Cart</h1>
        
        <div className="cart-layout">
          <div className="cart-items">
            {items.map((item) => (
              <div key={item.id} className="cart-item">
                <div className="item-image">
                  {item.imageUrl ? (
                    <img src={item.imageUrl} alt={item.productName} />
                  ) : (
                    <div className="image-placeholder">📦</div>
                  )}
                </div>
                
                <div className="item-details">
                  <h3>{item.productName}</h3>
                  <p className="item-price">₹{item.unitPrice.toFixed(2)}</p>
                </div>

                <div className="item-quantity">
                  <input
                    type="number"
                    min="1"
                    value={item.quantity}
                    onChange={(e) => updateQuantity(item.id, parseInt(e.target.value))}
                  />
                </div>

                <div className="item-total">
                  ₹{item.totalPrice.toFixed(2)}
                </div>

                <button
                  onClick={() => removeItem(item.id)}
                  className="btn-remove"
                >
                  ✕
                </button>
              </div>
            ))}
          </div>

          <div className="cart-summary">
            <h2>Order Summary</h2>
            <div className="summary-row">
              <span>Subtotal</span>
              <span>₹{totalAmount.toFixed(2)}</span>
            </div>
            <div className="summary-row">
              <span>Shipping</span>
              <span>Free</span>
            </div>
            <div className="summary-total">
              <span>Total</span>
              <span>₹{totalAmount.toFixed(2)}</span>
            </div>
            <button
              onClick={() => navigate('/checkout')}
              className="btn btn-primary btn-block"
            >
              Proceed to Checkout
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
