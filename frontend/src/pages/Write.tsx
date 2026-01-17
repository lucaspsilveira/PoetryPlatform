import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { poemService } from '../services/api';
import { RichTextEditor } from '../components/RichTextEditor';
import './Write.css';

export function Write() {
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [isPublished, setIsPublished] = useState(true);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    const strippedContent = content.replace(/<[^>]*>/g, '').trim();
    if (!strippedContent) {
      setError('Please write some content for your poem.');
      return;
    }

    setLoading(true);

    try {
      await poemService.create({ title, content, isPublished });
      navigate('/my-poems');
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      setError(error.response?.data?.message || 'Failed to save poem. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="write-page">
      <div className="write-container">
        <h1>Compose Your Verse</h1>
        <p className="write-subtitle">Let your thoughts flow onto the page</p>

        {error && <div className="write-error">{error}</div>}

        <form onSubmit={handleSubmit} className="write-form">
          <div className="form-group">
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              placeholder="Title of your poem"
              maxLength={200}
              className="title-input"
            />
          </div>

          <div className="form-group">
            <RichTextEditor
              value={content}
              onChange={setContent}
              placeholder="Write your poem here... Use the toolbar to add formatting like bold, italic, headers, and more."
            />
          </div>

          <div className="form-options">
            <label className="publish-option">
              <input
                type="checkbox"
                checked={isPublished}
                onChange={(e) => setIsPublished(e.target.checked)}
              />
              <span>Publish to feed</span>
            </label>
          </div>

          <div className="form-actions">
            <button type="submit" className="save-button" disabled={loading}>
              {loading ? 'Saving...' : isPublished ? 'Publish Poem' : 'Save as Draft'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
