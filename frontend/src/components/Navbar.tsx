import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Navbar.css';

export function Navbar() {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <Link to="/">Verses</Link>
      </div>
      <div className="navbar-menu">
        {isAuthenticated ? (
          <>
            <Link to="/write" className="nav-link">Write</Link>
            <Link to="/feed" className="nav-link">Feed</Link>
            <Link to="/my-poems" className="nav-link">My Poems</Link>
            <span className="nav-user">Hello, {user?.displayName}</span>
            <button onClick={handleLogout} className="nav-button">Logout</button>
          </>
        ) : (
          <>
            <Link to="/login" className="nav-link">Login</Link>
            <Link to="/register" className="nav-button primary">Get Started</Link>
          </>
        )}
      </div>
    </nav>
  );
}
