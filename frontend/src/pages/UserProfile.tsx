import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { userService } from '../services/api';
import { PoemCard } from '../components/PoemCard';
import { useAuth } from '../context/AuthContext';
import type { UserProfile as UserProfileType, Poem } from '../types';
import './UserProfile.css';

export function UserProfile() {
  const { userId } = useParams<{ userId: string }>();
  const { isAuthenticated } = useAuth();

  const [profile, setProfile] = useState<UserProfileType | null>(null);
  const [allPoems, setAllPoems] = useState<Poem[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState('');
  const [showAllPoems, setShowAllPoems] = useState(false);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  useEffect(() => {
    if (userId) {
      loadProfile();
      setShowAllPoems(false);
      setAllPoems(null);
    }
  }, [userId]);

  const loadProfile = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await userService.getProfile(userId!);
      setProfile(data);
    } catch {
      setError('Failed to load profile. User may not exist.');
    } finally {
      setLoading(false);
    }
  };

  const loadAllPoems = async (pageNum: number = 1) => {
    setLoadingMore(true);
    try {
      const response = await userService.getUserPoems(userId!, pageNum, pageSize);
      setAllPoems(response.poems);
      setTotalCount(response.totalCount);
      setPage(pageNum);
      setShowAllPoems(true);
    } catch {
      setError('Failed to load poems.');
    } finally {
      setLoadingMore(false);
    }
  };

  const handleLikeChange = (updatedPoem: Poem) => {
    if (profile) {
      setProfile({
        ...profile,
        topPoems: profile.topPoems.map((p) =>
          p.id === updatedPoem.id ? updatedPoem : p
        ),
      });
    }
    if (allPoems) {
      setAllPoems(
        allPoems.map((p) => (p.id === updatedPoem.id ? updatedPoem : p))
      );
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
    });
  };

  const totalPages = Math.ceil(totalCount / pageSize);
  const displayedPoems = showAllPoems ? allPoems : profile?.topPoems;

  if (loading) {
    return (
      <div className="profile-page">
        <div className="loading">Loading profile...</div>
      </div>
    );
  }

  if (error || !profile) {
    return (
      <div className="profile-page">
        <div className="profile-error">{error || 'Profile not found'}</div>
      </div>
    );
  }

  return (
    <div className="profile-page">
      <div className="profile-container">
        <header className="profile-header">
          <h1 className="profile-name">{profile.displayName}</h1>
          <p className="profile-joined">
            Member since {formatDate(profile.createdAt)}
          </p>
          <p className="profile-stats">
            {profile.totalPoemCount} published{' '}
            {profile.totalPoemCount === 1 ? 'poem' : 'poems'}
          </p>
        </header>

        <section className="profile-poems">
          <div className="poems-section-header">
            <h2>{showAllPoems ? 'All Poems' : 'Top Poems'}</h2>
            {!showAllPoems && profile.totalPoemCount > 10 && (
              <button
                onClick={() => loadAllPoems(1)}
                className="view-all-btn"
                disabled={loadingMore}
              >
                {loadingMore ? 'Loading...' : 'View All Poems'}
              </button>
            )}
            {showAllPoems && (
              <button
                onClick={() => setShowAllPoems(false)}
                className="view-top-btn"
              >
                View Top Poems
              </button>
            )}
          </div>

          {displayedPoems && displayedPoems.length === 0 ? (
            <div className="empty-poems">
              <p>This poet hasn't published any poems yet.</p>
            </div>
          ) : (
            <>
              <div className="poems-grid">
                {displayedPoems?.map((poem) => (
                  <PoemCard
                    key={poem.id}
                    poem={poem}
                    isAuthenticated={isAuthenticated}
                    onLikeChange={handleLikeChange}
                  />
                ))}
              </div>

              {showAllPoems && totalPages > 1 && (
                <div className="pagination">
                  <button
                    onClick={() => loadAllPoems(page - 1)}
                    disabled={page === 1 || loadingMore}
                    className="page-btn"
                  >
                    Previous
                  </button>
                  <span className="page-info">
                    Page {page} of {totalPages}
                  </span>
                  <button
                    onClick={() => loadAllPoems(page + 1)}
                    disabled={page === totalPages || loadingMore}
                    className="page-btn"
                  >
                    Next
                  </button>
                </div>
              )}
            </>
          )}
        </section>
      </div>
    </div>
  );
}
