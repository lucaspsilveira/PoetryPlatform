import { useState } from 'react';
import type { Poem } from '../types';
import { poemService } from '../services/api';
import './PoemCard.css';

interface PoemCardProps {
  poem: Poem;
  showActions?: boolean;
  onEdit?: (poem: Poem) => void;
  onDelete?: (id: number) => void;
  onLikeChange?: (poem: Poem) => void;
  isAuthenticated?: boolean;
}

export function PoemCard({ poem, showActions, onEdit, onDelete, onLikeChange, isAuthenticated }: PoemCardProps) {
  const [isLiking, setIsLiking] = useState(false);

  const handleLikeToggle = async () => {
    if (!isAuthenticated || isLiking) return;

    setIsLiking(true);
    try {
      const updatedPoem = poem.isLikedByCurrentUser
        ? await poemService.unlike(poem.id)
        : await poemService.like(poem.id);
      onLikeChange?.(updatedPoem);
    } catch (error) {
      console.error('Failed to toggle like:', error);
    } finally {
      setIsLiking(false);
    }
  };

  const formattedDate = new Date(poem.createdAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

  const isHtmlContent = poem.content.includes('<') && poem.content.includes('>');

  return (
    <article className="poem-card">
      <header className="poem-header">
        <h3 className="poem-title">{poem.title}</h3>
        {!poem.isPublished && <span className="draft-badge">Draft</span>}
      </header>
      {isHtmlContent ? (
        <div
          className="poem-content"
          dangerouslySetInnerHTML={{ __html: poem.content }}
        />
      ) : (
        <div className="poem-content">
          {poem.content.split('\n').map((line, i) => (
            <p key={i}>{line || '\u00A0'}</p>
          ))}
        </div>
      )}
      <footer className="poem-footer">
        <div className="poem-meta">
          <span className="poem-author">{poem.author.displayName}</span>
          <span className="poem-date">{formattedDate}</span>
        </div>
        <div className="poem-footer-right">
          <button
            className={`like-btn ${poem.isLikedByCurrentUser ? 'liked' : ''} ${!isAuthenticated ? 'disabled' : ''}`}
            onClick={handleLikeToggle}
            disabled={!isAuthenticated || isLiking}
            title={isAuthenticated ? (poem.isLikedByCurrentUser ? 'Unlike' : 'Like') : 'Login to like'}
          >
            <span className="like-icon">{poem.isLikedByCurrentUser ? '♥' : '♡'}</span>
            <span className="like-count">{poem.likeCount}</span>
          </button>
          {showActions && (
            <div className="poem-actions">
              <button onClick={() => onEdit?.(poem)} className="action-btn edit">
                Edit
              </button>
              <button onClick={() => onDelete?.(poem.id)} className="action-btn delete">
                Delete
              </button>
            </div>
          )}
        </div>
      </footer>
    </article>
  );
}
