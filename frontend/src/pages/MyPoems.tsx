import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { poemService } from '../services/api';
import { PoemCard } from '../components/PoemCard';
import { RichTextEditor } from '../components/RichTextEditor';
import type { Poem } from '../types';
import './MyPoems.css';

export function MyPoems() {
  const [poems, setPoems] = useState<Poem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [editingPoem, setEditingPoem] = useState<Poem | null>(null);
  const [editTitle, setEditTitle] = useState('');
  const [editContent, setEditContent] = useState('');
  const [editPublished, setEditPublished] = useState(true);
  const [saving, setSaving] = useState(false);
  const pageSize = 10;
  const navigate = useNavigate();

  useEffect(() => {
    loadPoems();
  }, [page]);

  const loadPoems = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await poemService.getMyPoems(page, pageSize);
      setPoems(response.poems);
      setTotalCount(response.totalCount);
    } catch {
      setError('Failed to load your poems. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (poem: Poem) => {
    setEditingPoem(poem);
    setEditTitle(poem.title);
    setEditContent(poem.content);
    setEditPublished(poem.isPublished);
  };

  const handleCancelEdit = () => {
    setEditingPoem(null);
    setEditTitle('');
    setEditContent('');
    setEditPublished(true);
  };

  const handleSaveEdit = async () => {
    if (!editingPoem) return;
    setSaving(true);
    try {
      await poemService.update(editingPoem.id, {
        title: editTitle,
        content: editContent,
        isPublished: editPublished,
      });
      handleCancelEdit();
      loadPoems();
    } catch {
      setError('Failed to update poem. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this poem?')) return;
    try {
      await poemService.delete(id);
      loadPoems();
    } catch {
      setError('Failed to delete poem. Please try again.');
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  if (loading && poems.length === 0) {
    return (
      <div className="my-poems-page">
        <div className="loading">Loading your poems...</div>
      </div>
    );
  }

  return (
    <div className="my-poems-page">
      <div className="my-poems-container">
        <header className="my-poems-header">
          <div>
            <h1>My Poems</h1>
            <p>Manage your collection of verses</p>
          </div>
          <button onClick={() => navigate('/write')} className="new-poem-btn">
            Write New Poem
          </button>
        </header>

        {error && <div className="my-poems-error">{error}</div>}

        {editingPoem && (
          <div className="edit-modal-overlay">
            <div className="edit-modal">
              <h2>Edit Poem</h2>
              <div className="edit-form">
                <input
                  type="text"
                  value={editTitle}
                  onChange={(e) => setEditTitle(e.target.value)}
                  placeholder="Title"
                  className="edit-title"
                />
                <div className="edit-editor-wrapper">
                  <RichTextEditor
                    value={editContent}
                    onChange={setEditContent}
                    placeholder="Edit your poem..."
                  />
                </div>
                <label className="edit-publish">
                  <input
                    type="checkbox"
                    checked={editPublished}
                    onChange={(e) => setEditPublished(e.target.checked)}
                  />
                  <span>Published</span>
                </label>
                <div className="edit-actions">
                  <button onClick={handleCancelEdit} className="cancel-btn">
                    Cancel
                  </button>
                  <button onClick={handleSaveEdit} className="save-btn" disabled={saving}>
                    {saving ? 'Saving...' : 'Save Changes'}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        {poems.length === 0 ? (
          <div className="empty-poems">
            <p>You haven't written any poems yet.</p>
            <button onClick={() => navigate('/write')} className="start-writing-btn">
              Start Writing
            </button>
          </div>
        ) : (
          <>
            <div className="poems-list">
              {poems.map((poem) => (
                <PoemCard
                  key={poem.id}
                  poem={poem}
                  showActions
                  onEdit={handleEdit}
                  onDelete={handleDelete}
                />
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
