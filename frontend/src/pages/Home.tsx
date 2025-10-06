import { Link } from 'react-router-dom'
import './Home.css'

export default function Home() {
  return (
    <div className="home">
      <section className="hero">
        <div className="container">
          <div className="hero-content">
            <h1 className="hero-title">
              Welcome to ShopStream
            </h1>
            <p className="hero-subtitle">
              Your one-stop shop for electronics, clothing, and books
            </p>
            <div className="hero-actions">
              <Link to="/products" className="btn btn-primary btn-lg">
                Shop Now
              </Link>
              <Link to="/products" className="btn btn-secondary btn-lg">
                Browse Categories
              </Link>
            </div>
          </div>
        </div>
      </section>

      <section className="features">
        <div className="container">
          <div className="features-grid">
            <div className="feature-card">
              <div className="feature-icon">🚚</div>
              <h3>Fast Delivery</h3>
              <p>Get your orders delivered quickly and safely</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">🔒</div>
              <h3>Secure Payment</h3>
              <p>Your transactions are safe and encrypted</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">💯</div>
              <h3>Quality Products</h3>
              <p>Only the best products for our customers</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">🎁</div>
              <h3>Great Deals</h3>
              <p>Amazing prices on all your favorite items</p>
            </div>
          </div>
        </div>
      </section>
    </div>
  )
}
