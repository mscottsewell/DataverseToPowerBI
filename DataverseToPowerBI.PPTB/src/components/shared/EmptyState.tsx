/**
 * EmptyState - Placeholder for empty data states
 */

import {
  makeStyles,
  Text,
  tokens,
} from '@fluentui/react-components';
import type { ReactNode } from 'react';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    padding: '48px 24px',
    gap: '12px',
    textAlign: 'center',
    color: tokens.colorNeutralForeground3,
  },
  icon: {
    fontSize: '48px',
    marginBottom: '8px',
  },
});

interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  description?: string;
  action?: ReactNode;
}

export function EmptyState({ icon, title, description, action }: EmptyStateProps) {
  const styles = useStyles();

  return (
    <div className={styles.container}>
      {icon && <div className={styles.icon}>{icon}</div>}
      <Text size={400} weight="semibold">{title}</Text>
      {description && <Text size={300}>{description}</Text>}
      {action}
    </div>
  );
}
