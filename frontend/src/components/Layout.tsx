import { Outlet, Link, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import { useCartStore } from '../store/cartStore'
import './Layout.css'

export default function Layout() {
  const { isAuthenticated, user, logout } = useAuthStore()
  const { items } = useCartStore()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/')
  }

  return (
    <div className="layout">
      <header className="header">
        <div className="container">
          <Link to="/" className="logo">
            <span className="logo-icon">🛒</span>
            ShopStream
          </Link>
          
          <nav className="nav">
            <Link to="/products" className="nav-link">Products</Link>
            {isAuthenticated && (
              <Link to="/orders" className="nav-link">Orders</Link>
            )}
            {user?.role === 'Admin' && (
              <Link to="/admin" className="nav-link">Admin</Link>
            )}
          </nav>

          <div className="header-actions">
            <Link to="/cart" className="cart-button">
              🛒 Cart
              {items.length > 0 && (
                <span className="cart-badge">{items.length}</span>
              )}
            </Link>
            
            {isAuthenticated ? (
              <div className="user-menu">
                <span className="user-name">👤 {user?.firstName}</span>
                <button onClick={handleLogout} className="btn btn-secondary">
                  Logout
                </button>
              </div>
            ) : (
              <div className="auth-buttons">
                <Link to="/login" className="btn btn-secondary">Login</Link>
                <Link to="/register" className="btn btn-primary">Sign Up</Link>
              </div>
            )}
          </div>
        </div>
      </header>

      <main className="main">
        <Outlet />
      </main>

      <footer className="footer">
        <div className="container">
          <p>&copy; 2024 ShopStream. Built with ASP.NET Core & React.</p>
        </div>
      </footer>
    </div>
  )
}
