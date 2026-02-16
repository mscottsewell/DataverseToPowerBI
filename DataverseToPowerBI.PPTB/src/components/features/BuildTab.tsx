/**
 * BuildTab - TMDL preview, generation, and change detection
 */

import { useState, useCallback } from 'react';
import {
  makeStyles,
  Card,
  CardHeader,
  Title2,
  Text,
  Button,
  Badge,
  tokens,
} from '@fluentui/react-components';
import {
  BuildingFactory24Regular,
  Eye24Regular,
  ArrowDownload24Regular,
  ArrowSync24Regular,
} from '@fluentui/react-icons';
import { useConfigStore, useUIStore } from '../../stores';
import { EmptyState, LoadingOverlay } from '../shared';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
  },
  card: {
    padding: '20px',
  },
  actions: {
    display: 'flex',
    gap: '12px',
    flexWrap: 'wrap',
  },
  previewArea: {
    marginTop: '16px',
  },
  fileTree: {
    display: 'flex',
    flexDirection: 'column',
    gap: '2px',
    marginBottom: '12px',
    padding: '12px',
    backgroundColor: tokens.colorNeutralBackground3,
    borderRadius: '4px',
    fontFamily: 'monospace',
    fontSize: '13px',
  },
  fileItem: {
    padding: '2px 8px',
    cursor: 'pointer',
    borderRadius: '4px',
    ':hover': {
      backgroundColor: tokens.colorNeutralBackground1Hover,
    },
  },
  fileItemSelected: {
    padding: '2px 8px',
    cursor: 'pointer',
    borderRadius: '4px',
    backgroundColor: tokens.colorBrandBackground2,
  },
  codeBlock: {
    fontFamily: 'Consolas, "Courier New", monospace',
    fontSize: '12px',
    lineHeight: '1.5',
    whiteSpace: 'pre',
    overflow: 'auto',
    maxHeight: '400px',
    padding: '16px',
    backgroundColor: tokens.colorNeutralBackground3,
    borderRadius: '4px',
    border: `1px solid ${tokens.colorNeutralStroke2}`,
  },
  summary: {
    display: 'flex',
    gap: '12px',
    flexWrap: 'wrap',
    marginBottom: '12px',
  },
});

export function BuildTab() {
  const styles = useStyles();
  const selectedTables = useConfigStore((s) => s.selectedTables);
  const projectName = useConfigStore((s) => s.projectName);
  const [previewFiles] = useState<Record<string, string>>({});
  const [selectedFile, setSelectedFile] = useState<string | null>(null);
  const [building] = useState(false);
  const [buildResult] = useState<'success' | 'error' | null>(null);

  const openPreview = useUIStore((s) => s.openDialog);
  const openChangePreview = useCallback(() => openPreview('changePreview'), [openPreview]);

  if (selectedTables.length === 0) {
    return (
      <EmptyState
        icon={<BuildingFactory24Regular />}
        title="No Tables Selected"
        description="Select tables and configure your schema before building."
      />
    );
  }

  const fileNames = Object.keys(previewFiles);

  return (
    <div className={styles.container}>
      <Title2>Build & Preview</Title2>

      <Card className={styles.card}>
        <CardHeader header={<Text weight="semibold" size={400}>Generation Summary</Text>} />
        <div className={styles.summary}>
          <Badge appearance="filled" color="brand">{selectedTables.length} tables</Badge>
          <Badge appearance="tint">{projectName || 'Unnamed Project'}</Badge>
          {buildResult === 'success' && <Badge appearance="filled" color="success">Build Successful</Badge>}
          {buildResult === 'error' && <Badge appearance="filled" color="danger">Build Failed</Badge>}
        </div>

        <div className={styles.actions}>
          <Button
            appearance="primary"
            icon={<Eye24Regular />}
            disabled={building}
          >
            Preview TMDL
          </Button>
          <Button
            appearance="primary"
            icon={<ArrowDownload24Regular />}
            disabled={building || selectedTables.length === 0}
          >
            Generate & Save
          </Button>
          <Button
            appearance="secondary"
            icon={<ArrowSync24Regular />}
            disabled={building}
            onClick={openChangePreview}
          >
            Preview Changes
          </Button>
        </div>
      </Card>

      {fileNames.length > 0 && (
        <Card className={styles.card}>
          <CardHeader header={<Text weight="semibold" size={400}>TMDL Preview</Text>} />
          <div className={styles.previewArea}>
            <div className={styles.fileTree}>
              {fileNames.map((name) => (
                <div
                  key={name}
                  className={selectedFile === name ? styles.fileItemSelected : styles.fileItem}
                  onClick={() => setSelectedFile(name)}
                >
                  📄 {name}
                </div>
              ))}
            </div>
            {selectedFile && previewFiles[selectedFile] && (
              <>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                  <Text weight="semibold">{selectedFile}</Text>
                  <Button
                    size="small"
                    appearance="subtle"
                    onClick={() => navigator.clipboard.writeText(previewFiles[selectedFile])}
                  >
                    Copy
                  </Button>
                </div>
                <div className={styles.codeBlock}>
                  {previewFiles[selectedFile]}
                </div>
              </>
            )}
          </div>
        </Card>
      )}

      {building && <LoadingOverlay message="Generating semantic model..." />}
    </div>
  );
}
