import { useState, useEffect } from 'react';
import { poemService } from '../services/api';
import { PoemCard } from '../components/PoemCard';
import type { Poem } from '../types';
import './Feed.css';

export function Feed() {
  const [poems, setPoems] = useState<Poem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  useEffect(() => {
    loadPoems();
  }, [page]);

  const loadPoems = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await poemService.getFeed(page, pageSize);
      setPoems(response.poems);
      setTotalCount(response.totalCount);
    } catch {
      setError('Failed to load poems. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  if (loading && poems.length === 0) {
    return (
      <div className="feed-page">
        <div className="loading">Loading poems...</div>
      </div>
    );
  }

  return (
    <div className="feed-page">
      <div className="feed-container">
        <header className="feed-header">
          <h1>Poetry Feed</h1>
          <p>Discover verses from our community of poets</p>
        </header>

        {error && <div className="feed-error">{error}</div>}

        {poems.length === 0 ? (
          <div className="empty-feed">
            <p>No poems yet. Be the first to share your verse!</p>
          </div>
        ) : (
          <>
            <div className="poems-mural">
              {poems.map((poem) => (
                <PoemCard key={poem.id} poem={poem} />
              ))}
            </div>

            {totalPages > 1 && (
              <div className="pagination">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="page-btn"
                >
                  Previous
                </button>
                <span className="page-info">
                  Page {page} of {totalPages}
                </span>
                <button
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                  className="page-btn"
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
