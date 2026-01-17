import type { Poem } from '../types';
import './PoemCard.css';

interface PoemCardProps {
  poem: Poem;
  showActions?: boolean;
  onEdit?: (poem: Poem) => void;
  onDelete?: (id: number) => void;
}

export function PoemCard({ poem, showActions, onEdit, onDelete }: PoemCardProps) {
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
      </footer>
    </article>
  );
}
