import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Landing.css';

export function Landing() {
  const { isAuthenticated } = useAuth();

  return (
    <div className="landing">
      <section className="hero">
        <div className="hero-content">
          <h1 className="hero-title">
            Where Words<br />
            <span className="highlight">Find Their Wings</span>
          </h1>
          <p className="hero-subtitle">
            A sanctuary for poets and dreamers. Share your verses with a community
            that celebrates the art of expression.
          </p>
          {isAuthenticated ? (
            <Link to="/write" className="cta-button">Start Writing</Link>
          ) : (
            <Link to="/register" className="cta-button">Begin Your Journey</Link>
          )}
        </div>
        <div className="hero-decoration">
          <div className="floating-quote">
            <p>"Poetry is when an emotion has found its thought and the thought has found words."</p>
            <span>— Robert Frost</span>
          </div>
        </div>
      </section>

      <section className="features">
        <div className="feature">
          <div className="feature-icon">&#9998;</div>
          <h3>Write Freely</h3>
          <p>A distraction-free space to craft your poems, with no judgment—only inspiration.</p>
        </div>
        <div className="feature">
          <div className="feature-icon">&#9829;</div>
          <h3>Share & Connect</h3>
          <p>Publish your work to a community of fellow poets who appreciate the written word.</p>
        </div>
        <div className="feature">
          <div className="feature-icon">&#9733;</div>
          <h3>Discover</h3>
          <p>Explore a feed of beautiful poetry from writers around the world.</p>
        </div>
      </section>

      <section className="inspiration">
        <h2>Let the muse guide you</h2>
        <div className="quote-carousel">
          <blockquote>
            "Fill your paper with the breathings of your heart."
            <cite>— William Wordsworth</cite>
          </blockquote>
        </div>
      </section>

      <footer className="landing-footer">
        <p>Verses — Where every word matters</p>
      </footer>
    </div>
  );
}
