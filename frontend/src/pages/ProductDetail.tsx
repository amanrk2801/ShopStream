import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { productsApi, Product } from '../api/products'
import { cartApi } from '../api/cart'
import { useAuthStore } from '../store/authStore'
import { useCartStore } from '../store/cartStore'
import './ProductDetail.css'

export default function ProductDetail() {
  const { id } = useParams<{ id: string }>()
  const [product, setProduct] = useState<Product | null>(null)
  const [quantity, setQuantity] = useState(1)
  const [loading, setLoading] = useState(true)
  const [adding, setAdding] = useState(false)
  const navigate = useNavigate()
  const isAuthenticated = useAuthStore(state => state.isAuthenticated)
  const setCart = useCartStore(state => state.setCart)

  useEffect(() => {
    if (id) loadProduct()
  }, [id])

  const loadProduct = async () => {
    try {
      const response = await productsApi.getById(id!)
      setProduct(response.data)
    } catch (error) {
      console.error('Failed to load product:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleAddToCart = async () => {
    if (!isAuthenticated) {
      navigate('/login')
      return
    }

    if (!product || product.stockQuantity < quantity) {
      alert('Not enough stock available')
      return
    }

    setAdding(true)
    try {
      console.log('Adding to cart:', { productId: id, quantity })
      const response = await cartApi.addItem(id!, quantity)
      console.log('Cart response:', response.data)
      setCart(response.data.items, response.data.totalAmount)
      alert(`Added ${quantity} ${quantity === 1 ? 'item' : 'items'} to cart!`)
    } catch (error: any) {
      console.error('Add to cart error:', error)
      console.error('Error response:', error.response?.data)
      const message = error.response?.data?.message || error.message || 'Failed to add to cart. Please try again.'
      alert(message)
    } finally {
      setAdding(false)
    }
  }

  if (loading) return <div className="loading">Loading...</div>
  if (!product) return <div className="loading">Product not found</div>

  return (
    <div className="product-detail">
      <div className="container">
        <div className="product-layout">
          <div className="product-images">
            {product.images[0] ? (
              <img src={product.images[0].url} alt={product.name} />
            ) : (
              <div className="image-placeholder">📦</div>
            )}
          </div>

          <div className="product-details">
            <h1>{product.name}</h1>
            <p className="product-category">{product.categoryName}</p>
            <div className="product-price">₹{product.price.toFixed(2)}</div>
            
            <div className="product-stock">
              {product.stockQuantity > 0 ? (
                <span className="in-stock">✓ In Stock ({product.stockQuantity} available)</span>
              ) : (
                <span className="out-of-stock">✗ Out of Stock</span>
              )}
            </div>

            <p className="product-description">{product.description}</p>

            {product.stockQuantity > 0 && (
              <div className="product-actions">
                <div className="quantity-selector">
                  <label>Quantity:</label>
                  <input
                    type="number"
                    min="1"
                    max={product.stockQuantity}
                    value={quantity}
                    onChange={(e) => setQuantity(parseInt(e.target.value))}
                  />
                </div>

                <button
                  onClick={handleAddToCart}
                  disabled={adding}
                  className="btn btn-primary btn-lg"
                >
                  {adding ? 'Adding...' : 'Add to Cart'}
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
