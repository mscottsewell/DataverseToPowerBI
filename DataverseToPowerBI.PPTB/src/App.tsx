import { useState, useEffect } from 'react';
import {
  makeStyles,
  Card,
  Title1,
  Title2,
  Title3,
  Text,
  tokens,
} from '@fluentui/react-components';
import {
  CheckmarkCircle24Filled,
  DismissCircle24Filled,
  Info24Filled,
} from '@fluentui/react-icons';

const useStyles = makeStyles({
  container: {
    padding: '24px',
    maxWidth: '1200px',
    margin: '0 auto',
  },
  header: {
    marginBottom: '24px',
  },
  card: {
    marginBottom: '16px',
    padding: '20px',
  },
  statusContainer: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    padding: '16px',
    borderRadius: '8px',
  },
  statusConnected: {
    backgroundColor: tokens.colorPaletteGreenBackground1,
    border: `1px solid ${tokens.colorPaletteGreenBorder1}`,
  },
  statusDisconnected: {
    backgroundColor: tokens.colorPaletteRedBackground1,
    border: `1px solid ${tokens.colorPaletteRedBorder1}`,
  },
  statusWarning: {
    backgroundColor: tokens.colorPaletteYellowBackground1,
    border: `1px solid ${tokens.colorPaletteYellowBorder1}`,
  },
  list: {
    marginTop: '12px',
    marginLeft: '20px',
  },
});

function App() {
  const styles = useStyles();
  const [connection, setConnection] = useState<any>(null);
  const [apiAvailable, setApiAvailable] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check if PPTB APIs are available
    if (typeof window !== 'undefined' && 'toolboxAPI' in window && 'dataverseAPI' in window) {
      setApiAvailable(true);

      // Subscribe to connection changes via events
      try {
        window.toolboxAPI?.events?.on?.((event: string, payload: any) => {
          if (event === 'connection:changed') {
            console.log('Connection changed:', payload);
            setConnection(payload);
          }
        });

        // Get current active connection
        window.toolboxAPI?.connections?.getActiveConnection?.()
          .then((conn: any) => {
            console.log('Current connection:', conn);
            setConnection(conn);
            setLoading(false);
          })
          .catch((err: Error) => {
            console.error('Failed to get current connection:', err);
            setLoading(false);
          });
      } catch (error) {
        console.error('Error setting up connection:', error);
        setLoading(false);
      }
    } else {
      setApiAvailable(false);
      setLoading(false);
    }
  }, []);

  if (!apiAvailable) {
    return (
      <div className={styles.container}>
        <div className={styles.header}>
          <Title1>Dataverse to Power BI Semantic Model Generator</Title1>
        </div>

        <Card className={styles.card}>
          <div className={`${styles.statusContainer} ${styles.statusWarning}`}>
            <Info24Filled />
            <div>
              <Title3>⚠️ PPTB APIs Not Available</Title3>
              <Text>This tool must be loaded within PowerPlatformToolBox.</Text>
            </div>
          </div>
        </Card>

        <Card className={styles.card}>
          <Title2>Development Mode</Title2>
          <Text>
            To test this tool, you need to:
          </Text>
          <ul className={styles.list}>
            <li>Build the tool: <code>npm run build</code></li>
            <li>Install PowerPlatformToolBox desktop application</li>
            <li>Load this tool in PPTB</li>
          </ul>
        </Card>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <Title1>Dataverse to Power BI Semantic Model Generator</Title1>
        <Text>Generate optimized Power BI semantic models from Dataverse metadata</Text>
      </div>

      <Card className={styles.card}>
        <Title2>Connection Status</Title2>
        {loading ? (
          <Text>Loading connection info...</Text>
        ) : connection ? (
          <div className={`${styles.statusContainer} ${styles.statusConnected}`}>
            <CheckmarkCircle24Filled />
            <div>
              <Title3>✅ Connected</Title3>
              <Text>
                <strong>{connection.name}</strong>
                <br />
                {connection.url}
                <br />
                Environment: {connection.environment}
              </Text>
            </div>
          </div>
        ) : (
          <div className={`${styles.statusContainer} ${styles.statusDisconnected}`}>
            <DismissCircle24Filled />
            <div>
              <Title3>❌ No Connection</Title3>
              <Text>Please create a Dataverse connection in PowerPlatformToolBox.</Text>
            </div>
          </div>
        )}
      </Card>

      <Card className={styles.card}>
        <Title2>Implementation Status</Title2>
        <Text>This is the PowerPlatformToolBox port of the Dataverse to Power BI Semantic Model Generator.</Text>
        <ul className={styles.list}>
          <li>✅ Phase 0: Foundation - Project setup complete</li>
          <li>🔄 Phase 1: Type definitions - In progress</li>
          <li>⏳ Phase 2: Service layer - Pending</li>
          <li>⏳ Phase 3: Core business logic - Pending</li>
          <li>⏳ Phase 4-17: UI and features - Pending</li>
        </ul>
      </Card>

      <Card className={styles.card}>
        <Title2>Next Steps</Title2>
        <ol className={styles.list}>
          <li>Port data models from C# to TypeScript</li>
          <li>Implement Dataverse adapter wrapping window.dataverseAPI</li>
          <li>Port SemanticModelBuilder core logic</li>
          <li>Build React UI components</li>
          <li>Implement end-to-end workflow</li>
        </ol>
      </Card>
    </div>
  );
}

export default App;
