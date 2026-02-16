/**
 * LoadingOverlay - Full-screen or inline loading indicator
 */

import {
  makeStyles,
  Spinner,
  Text,
  tokens,
} from '@fluentui/react-components';

const useStyles = makeStyles({
  overlay: {
    position: 'absolute',
    inset: '0',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: 'rgba(255, 255, 255, 0.85)',
    zIndex: 1000,
    gap: '12px',
  },
  inline: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    padding: '40px',
    gap: '12px',
    color: tokens.colorNeutralForeground3,
  },
});

interface LoadingOverlayProps {
  message?: string;
  /** If true, renders inline instead of as an overlay */
  inline?: boolean;
}

export function LoadingOverlay({ message, inline = false }: LoadingOverlayProps) {
  const styles = useStyles();

  return (
    <div className={inline ? styles.inline : styles.overlay}>
      <Spinner size="medium" />
      {message && <Text size={300}>{message}</Text>}
    </div>
  );
}
