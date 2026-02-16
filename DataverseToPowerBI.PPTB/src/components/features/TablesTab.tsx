/**
 * TablesTab - Table selection with search, solution filtering, and bulk operations
 */

import { useMemo } from 'react';
import {
  makeStyles,
  Card,
  Title2,
  Text,
  Checkbox,
  Button,
  tokens,
  Badge,
} from '@fluentui/react-components';
import { useConfigStore, useMetadataStore, useUIStore } from '../../stores';
import { SearchInput, EmptyState, LoadingOverlay } from '../shared';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
  },
  toolbar: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    flexWrap: 'wrap',
  },
  tableList: {
    display: 'flex',
    flexDirection: 'column',
    gap: '2px',
    maxHeight: 'calc(100vh - 280px)',
    overflow: 'auto',
  },
  tableRow: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
    padding: '6px 12px',
    borderRadius: '4px',
    ':hover': {
      backgroundColor: tokens.colorNeutralBackground1Hover,
    },
  },
  tableName: {
    flex: 1,
    minWidth: 0,
  },
  logicalName: {
    color: tokens.colorNeutralForeground3,
    fontSize: '12px',
  },
  stats: {
    display: 'flex',
    gap: '8px',
    alignItems: 'center',
  },
});

export function TablesTab() {
  const styles = useStyles();
  const tables = useMetadataStore((s) => s.tables);
  const loadingTables = useMetadataStore((s) => s.loading.tables);
  const selectedTables = useConfigStore((s) => s.selectedTables);
  const toggleTable = useConfigStore((s) => s.toggleTable);
  const setSelectedTables = useConfigStore((s) => s.setSelectedTables);
  const searchText = useUIStore((s) => s.tableSearchText);
  const setSearchText = useUIStore((s) => s.setTableSearchText);

  const filteredTables = useMemo(() => {
    if (!searchText) return tables;
    const lower = searchText.toLowerCase();
    return tables.filter(
      (t) =>
        t.logicalName.toLowerCase().includes(lower) ||
        (t.displayName?.toLowerCase().includes(lower) ?? false)
    );
  }, [tables, searchText]);

  const handleSelectAll = () => {
    const allNames = filteredTables.map((t) => t.logicalName);
    const currentSet = new Set(selectedTables);
    allNames.forEach((n) => currentSet.add(n));
    setSelectedTables(Array.from(currentSet));
  };

  const handleDeselectAll = () => {
    const filteredSet = new Set(filteredTables.map((t) => t.logicalName));
    setSelectedTables(selectedTables.filter((t) => !filteredSet.has(t)));
  };

  if (loadingTables) {
    return <LoadingOverlay inline message="Loading tables..." />;
  }

  if (tables.length === 0) {
    return (
      <EmptyState
        title="No Tables Available"
        description="Select a solution in the Setup tab to load tables."
      />
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.toolbar}>
        <Title2>Tables</Title2>
        <div className={styles.stats}>
          <Badge appearance="filled" color="brand">
            {selectedTables.length} / {tables.length} selected
          </Badge>
        </div>
      </div>

      <div className={styles.toolbar}>
        <SearchInput
          value={searchText}
          onChange={setSearchText}
          placeholder="Search tables..."
        />
        <Button appearance="secondary" size="small" onClick={handleSelectAll}>
          Select All
        </Button>
        <Button appearance="secondary" size="small" onClick={handleDeselectAll}>
          Deselect All
        </Button>
      </div>

      <Card>
        <div className={styles.tableList}>
          {filteredTables.map((table) => (
            <div key={table.logicalName} className={styles.tableRow}>
              <Checkbox
                checked={selectedTables.includes(table.logicalName)}
                onChange={() => toggleTable(table.logicalName)}
              />
              <div className={styles.tableName}>
                <Text size={300} weight="semibold">
                  {table.displayName ?? table.logicalName}
                </Text>
                <br />
                <Text className={styles.logicalName}>{table.logicalName}</Text>
              </div>
            </div>
          ))}
          {filteredTables.length === 0 && (
            <EmptyState title="No tables match your search" />
          )}
        </div>
      </Card>
    </div>
  );
}
