import { Link } from 'react-router-dom'
import './Admin.css'

export default function AdminDashboard() {
  return (
    <div className="admin-page">
      <div className="container">
        <h1>Admin Dashboard</h1>
        
        <div className="admin-grid">
          <Link to="/admin/products" className="admin-card">
            <div className="admin-icon">📦</div>
            <h2>Products</h2>
            <p>Manage product catalog</p>
          </Link>

          <Link to="/admin/orders" className="admin-card">
            <div className="admin-icon">📋</div>
            <h2>Orders</h2>
            <p>View and manage orders</p>
          </Link>

          <div className="admin-card">
            <div className="admin-icon">👥</div>
            <h2>Customers</h2>
            <p>Manage customers</p>
          </div>

          <div className="admin-card">
            <div className="admin-icon">📊</div>
            <h2>Reports</h2>
            <p>View sales reports</p>
          </div>
        </div>
      </div>
    </div>
  )
}
