/**
 * ConnectionStatusBar - Displays current Dataverse connection status
 */

import {
  makeStyles,
  tokens,
  Badge,
  Text,
  Tooltip,
} from '@fluentui/react-components';
import {
  PlugConnected24Regular,
  PlugDisconnected24Regular,
  ArrowSync24Regular,
  ErrorCircle24Regular,
} from '@fluentui/react-icons';
import { useConnectionStore } from '../../stores';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
    padding: '4px 12px',
    borderRadius: '4px',
    fontSize: '13px',
  },
  connected: {
    color: tokens.colorPaletteGreenForeground1,
  },
  disconnected: {
    color: tokens.colorNeutralForeground3,
  },
  connecting: {
    color: tokens.colorPaletteYellowForeground1,
  },
  error: {
    color: tokens.colorPaletteRedForeground1,
  },
});

export function ConnectionStatusBar() {
  const styles = useStyles();
  const { connection, status, error } = useConnectionStore();

  const icon = {
    connected: <PlugConnected24Regular className={styles.connected} />,
    disconnected: <PlugDisconnected24Regular className={styles.disconnected} />,
    connecting: <ArrowSync24Regular className={styles.connecting} />,
    error: <ErrorCircle24Regular className={styles.error} />,
  }[status];

  const label = {
    connected: connection?.name ?? 'Connected',
    disconnected: 'Not Connected',
    connecting: 'Connecting...',
    error: error ?? 'Connection Error',
  }[status];

  return (
    <Tooltip content={connection?.url ?? 'No Dataverse connection'} relationship="description">
      <div className={styles.container}>
        {icon}
        <Text size={200}>{label}</Text>
        {status === 'connected' && (
          <Badge appearance="filled" color="success" size="small">
            Live
          </Badge>
        )}
      </div>
    </Tooltip>
  );
}
