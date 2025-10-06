import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { useAuthStore } from './store/authStore'
import Layout from './components/Layout'
import Home from './pages/Home'
import Products from './pages/Products'
import ProductDetail from './pages/ProductDetail'
import Cart from './pages/Cart'
import Checkout from './pages/Checkout'
import Orders from './pages/Orders'
import Login from './pages/Login'
import Register from './pages/Register'
import AdminDashboard from './pages/admin/Dashboard'
import AdminProducts from './pages/admin/Products'
import AdminOrders from './pages/admin/Orders'

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore()
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" />
}

function AdminRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, user } = useAuthStore()
  return isAuthenticated && user?.role === 'Admin' ? <>{children}</> : <Navigate to="/" />
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="products" element={<Products />} />
          <Route path="products/:id" element={<ProductDetail />} />
          <Route path="cart" element={<Cart />} />
          <Route path="login" element={<Login />} />
          <Route path="register" element={<Register />} />
          
          <Route path="checkout" element={
            <PrivateRoute><Checkout /></PrivateRoute>
          } />
          <Route path="orders" element={
            <PrivateRoute><Orders /></PrivateRoute>
          } />
          
          <Route path="admin" element={
            <AdminRoute><AdminDashboard /></AdminRoute>
          } />
          <Route path="admin/products" element={
            <AdminRoute><AdminProducts /></AdminRoute>
          } />
          <Route path="admin/orders" element={
            <AdminRoute><AdminOrders /></AdminRoute>
          } />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
