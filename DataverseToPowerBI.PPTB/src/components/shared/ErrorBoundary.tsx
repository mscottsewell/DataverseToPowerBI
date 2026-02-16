/**
 * ErrorBoundary - Catches React rendering errors and displays fallback UI
 */

import { Component, type ReactNode } from 'react';
import { ErrorCircle24Regular } from '@fluentui/react-icons';

const styles: Record<string, React.CSSProperties> = {
  container: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    padding: '40px',
    gap: '16px',
    textAlign: 'center',
  },
  icon: {
    color: '#d13438',
    fontSize: '48px',
  },
  details: {
    maxWidth: '600px',
    padding: '16px',
    backgroundColor: '#fdf3f4',
    borderRadius: '4px',
    fontFamily: 'monospace',
    fontSize: '12px',
    textAlign: 'left',
    overflow: 'auto',
    maxHeight: '200px',
    whiteSpace: 'pre-wrap',
    wordBreak: 'break-word',
  },
};

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('ErrorBoundary caught:', error, errorInfo);
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) return this.props.fallback;

      return (
        <div style={styles.container}>
          <ErrorCircle24Regular style={styles.icon} />
          <h3>Something went wrong</h3>
          <p>An unexpected error occurred. Please try again.</p>
          {this.state.error && (
            <div style={styles.details}>
              {this.state.error.message}
            </div>
          )}
          <button onClick={this.handleReset}>
            Try Again
          </button>
        </div>
      );
    }

    return this.props.children;
  }
}
