/**
 * SchemaTab - Star-schema configuration: fact table picker and dimension selector
 */

import { useMemo } from 'react';
import {
  makeStyles,
  Card,
  CardHeader,
  Title2,
  Text,
  Radio,
  RadioGroup,
  Switch,
  Badge,
  tokens,
} from '@fluentui/react-components';
import { Organization24Regular } from '@fluentui/react-icons';
import { useConfigStore, useMetadataStore } from '../../stores';
import { EmptyState } from '../shared';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
  },
  columns: {
    display: 'grid',
    gridTemplateColumns: '1fr 1fr',
    gap: '16px',
  },
  card: {
    padding: '20px',
  },
  list: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
    maxHeight: 'calc(100vh - 320px)',
    overflow: 'auto',
    marginTop: '12px',
  },
  relGroup: {
    marginLeft: '24px',
    padding: '8px 0',
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
  },
  relItem: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
    padding: '4px 8px',
    borderRadius: '4px',
    ':hover': {
      backgroundColor: tokens.colorNeutralBackground1Hover,
    },
  },
});

export function SchemaTab() {
  const styles = useStyles();
  const selectedTables = useConfigStore((s) => s.selectedTables);
  const factTable = useConfigStore((s) => s.factTable);
  const setFactTable = useConfigStore((s) => s.setFactTable);
  const relationships = useConfigStore((s) => s.relationships);
  const toggleRelationshipActive = useConfigStore((s) => s.toggleRelationshipActive);
  const getTableDisplayName = useMetadataStore((s) => s.getTableDisplayName);

  // Group relationships by target table
  const groupedRelationships = useMemo(() => {
    const groups: Record<string, typeof relationships> = {};
    for (const rel of relationships) {
      const key = rel.targetTable;
      if (!groups[key]) groups[key] = [];
      groups[key].push(rel);
    }
    return groups;
  }, [relationships]);

  if (selectedTables.length === 0) {
    return (
      <EmptyState
        icon={<Organization24Regular />}
        title="No Tables Selected"
        description="Select tables in the Tables tab first."
      />
    );
  }

  return (
    <div className={styles.container}>
      <Title2>Star Schema Configuration</Title2>

      <div className={styles.columns}>
        {/* Fact Table Picker */}
        <Card className={styles.card}>
          <CardHeader header={<Text weight="semibold" size={400}>Fact Table</Text>} />
          <Text size={200}>Select the central fact table for your star schema.</Text>
          <RadioGroup
            value={factTable ?? ''}
            onChange={(_, data) => setFactTable(data.value || null)}
          >
            <div className={styles.list}>
              {selectedTables.map((table) => (
                <Radio
                  key={table}
                  value={table}
                  label={getTableDisplayName(table)}
                />
              ))}
            </div>
          </RadioGroup>
        </Card>

        {/* Dimension Tables */}
        <Card className={styles.card}>
          <CardHeader header={<Text weight="semibold" size={400}>Dimensions & Relationships</Text>} />
          <Text size={200}>
            {factTable
              ? `Relationships from ${getTableDisplayName(factTable)}`
              : 'Select a fact table to see relationships'}
          </Text>

          <div className={styles.list}>
            {Object.entries(groupedRelationships).map(([targetTable, rels]) => (
              <div key={targetTable}>
                <Text weight="semibold" size={300}>
                  {getTableDisplayName(targetTable)}
                  <Badge appearance="tint" size="tiny" style={{ marginLeft: 8 }}>
                    {rels.length}
                  </Badge>
                </Text>
                <div className={styles.relGroup}>
                  {rels.map((rel) => (
                    <div key={`${rel.sourceAttribute}-${rel.targetTable}`} className={styles.relItem}>
                      <Switch
                        checked={rel.isActive}
                        onChange={() =>
                          toggleRelationshipActive(rel.sourceTable, rel.sourceAttribute, rel.targetTable)
                        }
                        label={`${rel.sourceAttribute} → ${rel.targetTable}`}
                      />
                      {!rel.isActive && (
                        <Badge appearance="tint" color="subtle" size="tiny">inactive</Badge>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            ))}

            {Object.keys(groupedRelationships).length === 0 && factTable && (
              <EmptyState
                title="No Relationships Found"
                description="No lookup relationships detected from the fact table."
              />
            )}
          </div>
        </Card>
      </div>
    </div>
  );
}
