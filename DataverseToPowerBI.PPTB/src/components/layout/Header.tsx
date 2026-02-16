/**
 * Header - Application header with title, config name, and connection status
 */

import {
  makeStyles,
  Text,
  tokens,
  Badge,
} from '@fluentui/react-components';
import { Database24Regular } from '@fluentui/react-icons';
import { ConnectionStatusBar } from '../shared';
import { useConfigStore } from '../../stores';

const useStyles = makeStyles({
  header: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    padding: '8px 24px',
    backgroundColor: tokens.colorNeutralBackground1,
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    minHeight: '48px',
    flexShrink: 0,
  },
  titleGroup: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  title: {
    fontWeight: tokens.fontWeightSemibold,
  },
  configName: {
    display: 'flex',
    alignItems: 'center',
    gap: '6px',
  },
});

export function Header() {
  const styles = useStyles();
  const configName = useConfigStore((s) => s.configName);
  const isDirty = useConfigStore((s) => s.isDirty);

  return (
    <header className={styles.header}>
      <div className={styles.titleGroup}>
        <Database24Regular />
        <Text className={styles.title} size={400}>
          Dataverse → Power BI
        </Text>
        <div className={styles.configName}>
          <Text size={300} italic>
            {configName}{isDirty ? ' •' : ''}
          </Text>
          {isDirty && (
            <Badge appearance="tint" color="warning" size="tiny">
              unsaved
            </Badge>
          )}
        </div>
      </div>
      <ConnectionStatusBar />
    </header>
  );
}
