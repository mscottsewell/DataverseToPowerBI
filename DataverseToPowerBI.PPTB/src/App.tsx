import { useEffect } from 'react';
import { useConnectionStore } from './stores';
import { AppLayout } from './components/layout';

function App() {
  const setConnection = useConnectionStore((s) => s.setConnection);
  const setApiAvailable = useConnectionStore((s) => s.setApiAvailable);
  const setStatus = useConnectionStore((s) => s.setStatus);

  useEffect(() => {
    // Check if PPTB APIs are available
    if (typeof window !== 'undefined' && 'toolboxAPI' in window && 'dataverseAPI' in window) {
      setApiAvailable(true);

      try {
        window.toolboxAPI?.events?.on?.((event: string, payload: any) => {
          if (event === 'connection:changed') {
            setConnection(payload ? {
              name: payload.name,
              url: payload.url,
              environment: payload.environment,
              id: payload.id,
            } : null);
          }
        });

        setStatus('connecting');
        window.toolboxAPI?.connections?.getActiveConnection?.()
          .then((conn: any) => {
            if (conn) {
              setConnection({
                name: conn.name,
                url: conn.url,
                environment: conn.environment,
                id: conn.id,
              });
            } else {
              setConnection(null);
            }
          })
          .catch(() => setConnection(null));
      } catch {
        setConnection(null);
      }
    } else {
      setApiAvailable(false);
    }
  }, [setConnection, setApiAvailable, setStatus]);

  return <AppLayout />;
}

export default App;
