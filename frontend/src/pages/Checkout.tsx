import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { ordersApi } from '../api/orders'
import { useCartStore } from '../store/cartStore'
import api from '../api/client'
import './Checkout.css'

interface Address {
  id: string
  street: string
  city: string
  state: string
  zipCode: string
  country: string
  isDefault: boolean
}

export default function Checkout() {
  const [loading, setLoading] = useState(false)
  const [loadingAddress, setLoadingAddress] = useState(true)
  const [addressId, setAddressId] = useState<string | null>(null)
  const [address, setAddress] = useState<Address | null>(null)
  const [showAddressForm, setShowAddressForm] = useState(false)
  const [newAddress, setNewAddress] = useState({
    street: '',
    city: '',
    state: '',
    zipCode: '',
    country: '',
    isDefault: true
  })
  const navigate = useNavigate()
  const { totalAmount, clearCart } = useCartStore()

  useEffect(() => {
    loadAddress()
  }, [])

  const loadAddress = async () => {
    setLoadingAddress(true)
    try {
      // Get user's default address
      const response = await api.get('/addresses')
      const addresses = response.data as Address[]
      const defaultAddress = addresses.find(a => a.isDefault) || addresses[0]
      
      if (defaultAddress) {
        setAddress(defaultAddress)
        setAddressId(defaultAddress.id)
      } else {
        setShowAddressForm(true)
      }
    } catch (error) {
      console.error('Failed to load address:', error)
      setShowAddressForm(true)
    } finally {
      setLoadingAddress(false)
    }
  }

  const handleAddressSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      const response = await api.post('/addresses', newAddress)
      const savedAddress = response.data as Address
      setAddress(savedAddress)
      setAddressId(savedAddress.id)
      setShowAddressForm(false)
    } catch (error) {
      console.error('Failed to save address:', error)
      alert('Failed to save address. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  const handleCheckout = async () => {
    if (!addressId) {
      alert('Please add a shipping address first')
      return
    }

    setLoading(true)
    try {
      await ordersApi.checkout(addressId, 'Mock')
      clearCart()
      alert('Order placed successfully!')
      navigate('/orders')
    } catch (error: any) {
      console.error('Checkout error:', error)
      alert(error.response?.data?.message || 'Checkout failed. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="checkout-page">
      <div className="container">
        <h1>Checkout</h1>
        
        <div className="checkout-layout">
          <div className="checkout-form">
            <div className="section">
              <h2>Shipping Address</h2>
              {loadingAddress ? (
                <p className="demo-note">Loading address...</p>
              ) : address ? (
                <div className="address-display">
                  <p>{address.street}</p>
                  <p>{address.city}, {address.state} {address.zipCode}</p>
                  <p>{address.country}</p>
                  <button 
                    onClick={() => setShowAddressForm(true)}
                    className="btn btn-secondary"
                    style={{ marginTop: '10px' }}
                  >
                    Change Address
                  </button>
                </div>
              ) : showAddressForm ? (
                <form onSubmit={handleAddressSubmit} className="address-form">
                  <div className="form-group">
                    <label>Street Address</label>
                    <input
                      type="text"
                      value={newAddress.street}
                      onChange={(e) => setNewAddress({ ...newAddress, street: e.target.value })}
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>City</label>
                    <input
                      type="text"
                      value={newAddress.city}
                      onChange={(e) => setNewAddress({ ...newAddress, city: e.target.value })}
                      required
                    />
                  </div>
                  <div className="form-row">
                    <div className="form-group">
                      <label>State</label>
                      <input
                        type="text"
                        value={newAddress.state}
                        onChange={(e) => setNewAddress({ ...newAddress, state: e.target.value })}
                        required
                      />
                    </div>
                    <div className="form-group">
                      <label>Zip Code</label>
                      <input
                        type="text"
                        value={newAddress.zipCode}
                        onChange={(e) => setNewAddress({ ...newAddress, zipCode: e.target.value })}
                        required
                      />
                    </div>
                  </div>
                  <div className="form-group">
                    <label>Country</label>
                    <input
                      type="text"
                      value={newAddress.country}
                      onChange={(e) => setNewAddress({ ...newAddress, country: e.target.value })}
                      required
                    />
                  </div>
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? 'Saving...' : 'Save Address'}
                  </button>
                </form>
              ) : (
                <p className="demo-note">No address found. Please add a shipping address.</p>
              )}
            </div>

            <div className="section">
              <h2>Payment Method</h2>
              <div className="payment-option">
                <input type="radio" id="mock" name="payment" checked readOnly />
                <label htmlFor="mock">Mock Payment (Demo)</label>
              </div>
            </div>
          </div>

          <div className="checkout-summary">
            <h2>Order Summary</h2>
            <div className="summary-row">
              <span>Total</span>
              <span>₹{totalAmount.toFixed(2)}</span>
            </div>
            <button
              onClick={handleCheckout}
              disabled={loading}
              className="btn btn-primary btn-block"
            >
              {loading ? 'Processing...' : 'Place Order'}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
