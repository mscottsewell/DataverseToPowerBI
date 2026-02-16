/**
 * AttributesTab - Per-table attribute selection with form/view pickers and display name overrides
 */

import { useMemo } from 'react';
import {
  makeStyles,
  Card,
  Title2,
  Text,
  Checkbox,
  Select,
  Button,
  Input,
  Badge,
  tokens,
} from '@fluentui/react-components';
import { TextColumnOne24Regular } from '@fluentui/react-icons';
import { useConfigStore, useMetadataStore, useUIStore } from '../../stores';
import { SearchInput, EmptyState, LoadingOverlay } from '../shared';
import { StorageMode } from '../../types/Constants';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
  },
  columns: {
    display: 'grid',
    gridTemplateColumns: '240px 1fr',
    gap: '16px',
    maxHeight: 'calc(100vh - 200px)',
  },
  tableList: {
    display: 'flex',
    flexDirection: 'column',
    gap: '2px',
    overflow: 'auto',
  },
  tableItem: {
    padding: '8px 12px',
    borderRadius: '4px',
    cursor: 'pointer',
    ':hover': {
      backgroundColor: tokens.colorNeutralBackground1Hover,
    },
  },
  tableItemSelected: {
    padding: '8px 12px',
    borderRadius: '4px',
    cursor: 'pointer',
    backgroundColor: tokens.colorBrandBackground2,
  },
  attrToolbar: {
    display: 'flex',
    gap: '8px',
    alignItems: 'center',
    flexWrap: 'wrap',
    marginBottom: '12px',
  },
  attrList: {
    display: 'flex',
    flexDirection: 'column',
    gap: '2px',
    overflow: 'auto',
    maxHeight: 'calc(100vh - 340px)',
  },
  attrRow: {
    display: 'grid',
    gridTemplateColumns: '32px 1fr 180px 100px',
    alignItems: 'center',
    gap: '8px',
    padding: '4px 8px',
    borderRadius: '4px',
    ':hover': {
      backgroundColor: tokens.colorNeutralBackground1Hover,
    },
  },
  attrHeader: {
    display: 'grid',
    gridTemplateColumns: '32px 1fr 180px 100px',
    alignItems: 'center',
    gap: '8px',
    padding: '4px 8px',
    fontWeight: tokens.fontWeightSemibold,
    fontSize: '12px',
    color: tokens.colorNeutralForeground3,
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    marginBottom: '4px',
  },
  overridden: {
    fontStyle: 'italic',
  },
});

export function AttributesTab() {
  const styles = useStyles();
  const selectedTables = useConfigStore((s) => s.selectedTables);
  const tableAttributes = useConfigStore((s) => s.tableAttributes);
  const toggleAttribute = useConfigStore((s) => s.toggleAttribute);
  const setTableAttributes = useConfigStore((s) => s.setTableAttributes);
  const tableStorageModes = useConfigStore((s) => s.tableStorageModes);
  const setTableStorageMode = useConfigStore((s) => s.setTableStorageMode);
  const attributeDisplayNameOverrides = useConfigStore((s) => s.attributeDisplayNameOverrides);
  const setAttributeDisplayNameOverride = useConfigStore((s) => s.setAttributeDisplayNameOverride);
  const clearAttributeDisplayNameOverride = useConfigStore((s) => s.clearAttributeDisplayNameOverride);

  const metaAttributes = useMetadataStore((s) => s.tableAttributes);
  const getTableDisplayName = useMetadataStore((s) => s.getTableDisplayName);
  const loadingAttrs = useMetadataStore((s) => s.loading.attributes);

  const selectedAttributeTable = useUIStore((s) => s.selectedAttributeTable);
  const setSelectedAttributeTable = useUIStore((s) => s.setSelectedAttributeTable);
  const searchText = useUIStore((s) => s.attributeSearchText);
  const setSearchText = useUIStore((s) => s.setAttributeSearchText);

  const currentAttrs = useMemo(() => {
    if (!selectedAttributeTable) return [];
    const attrs = metaAttributes[selectedAttributeTable] ?? [];
    if (!searchText) return attrs;
    const lower = searchText.toLowerCase();
    return attrs.filter(
      (a) =>
        a.logicalName.toLowerCase().includes(lower) ||
        (a.displayName?.toLowerCase().includes(lower) ?? false)
    );
  }, [selectedAttributeTable, metaAttributes, searchText]);

  const selectedAttrs = selectedAttributeTable ? (tableAttributes[selectedAttributeTable] ?? []) : [];

  const handleSelectAll = () => {
    if (!selectedAttributeTable) return;
    setTableAttributes(selectedAttributeTable, currentAttrs.map((a) => a.logicalName));
  };

  const handleDeselectAll = () => {
    if (!selectedAttributeTable) return;
    setTableAttributes(selectedAttributeTable, []);
  };

  if (selectedTables.length === 0) {
    return (
      <EmptyState
        icon={<TextColumnOne24Regular />}
        title="No Tables Selected"
        description="Select tables in the Tables tab first."
      />
    );
  }

  return (
    <div className={styles.container}>
      <Title2>Attribute Configuration</Title2>

      <div className={styles.columns}>
        {/* Table list sidebar */}
        <Card>
          <div className={styles.tableList}>
            {selectedTables.map((table) => (
              <div
                key={table}
                className={selectedAttributeTable === table ? styles.tableItemSelected : styles.tableItem}
                onClick={() => setSelectedAttributeTable(table)}
              >
                <Text size={300} weight={selectedAttributeTable === table ? 'semibold' : 'regular'}>
                  {getTableDisplayName(table)}
                </Text>
                <br />
                <Text size={200} style={{ color: tokens.colorNeutralForeground3 }}>
                  {(tableAttributes[table] ?? []).length} selected
                </Text>
              </div>
            ))}
          </div>
        </Card>

        {/* Attribute grid */}
        <Card>
          {!selectedAttributeTable ? (
            <EmptyState title="Select a table" description="Click a table to configure its attributes." />
          ) : loadingAttrs[selectedAttributeTable] ? (
            <LoadingOverlay inline message="Loading attributes..." />
          ) : (
            <>
              <div className={styles.attrToolbar}>
                <Text weight="semibold" size={400}>
                  {getTableDisplayName(selectedAttributeTable)}
                </Text>
                <Badge appearance="filled" color="brand" size="small">
                  {selectedAttrs.length} / {(metaAttributes[selectedAttributeTable] ?? []).length}
                </Badge>
                <SearchInput value={searchText} onChange={setSearchText} placeholder="Filter attributes..." />
                <Button size="small" appearance="secondary" onClick={handleSelectAll}>Select All</Button>
                <Button size="small" appearance="secondary" onClick={handleDeselectAll}>Deselect All</Button>
                <Select
                  size="small"
                  value={tableStorageModes[selectedAttributeTable] ?? ''}
                  onChange={(_, d) => {
                    if (d.value) setTableStorageMode(selectedAttributeTable!, d.value as StorageMode);
                  }}
                >
                  <option value="">Default Storage</option>
                  <option value={StorageMode.DirectQuery}>DirectQuery</option>
                  <option value={StorageMode.Import}>Import</option>
                  <option value={StorageMode.Dual}>Dual</option>
                </Select>
              </div>

              <div className={styles.attrHeader}>
                <span />
                <span>Attribute</span>
                <span>Display Name</span>
                <span>Type</span>
              </div>

              <div className={styles.attrList}>
                {currentAttrs.map((attr) => {
                  const override = attributeDisplayNameOverrides[selectedAttributeTable!]?.[attr.logicalName];
                  return (
                    <div key={attr.logicalName} className={styles.attrRow}>
                      <Checkbox
                        checked={selectedAttrs.includes(attr.logicalName)}
                        onChange={() => toggleAttribute(selectedAttributeTable!, attr.logicalName)}
                      />
                      <div>
                        <Text size={300}>{attr.displayName ?? attr.logicalName}</Text>
                        <br />
                        <Text size={200} style={{ color: tokens.colorNeutralForeground3 }}>
                          {attr.logicalName}
                        </Text>
                      </div>
                      <Input
                        size="small"
                        className={override ? styles.overridden : undefined}
                        value={override ?? attr.displayName ?? ''}
                        onChange={(_, d) => {
                          if (d.value === (attr.displayName ?? '')) {
                            clearAttributeDisplayNameOverride(selectedAttributeTable!, attr.logicalName);
                          } else {
                            setAttributeDisplayNameOverride(selectedAttributeTable!, attr.logicalName, d.value);
                          }
                        }}
                        placeholder={attr.displayName ?? attr.logicalName}
                      />
                      <Badge appearance="tint" size="small">{attr.attributeType ?? 'Unknown'}</Badge>
                    </div>
                  );
                })}
              </div>
            </>
          )}
        </Card>
      </div>
    </div>
  );
}
