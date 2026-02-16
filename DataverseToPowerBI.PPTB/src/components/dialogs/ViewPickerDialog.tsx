/**
 * ViewPickerDialog - Modal to select a view for a table
 *
 * Displays available views and allows the user to select one to
 * apply FetchXML-based filtering during TMDL generation.
 */

import { useEffect } from 'react';
import {
  Dialog,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogContent,
  DialogActions,
  Button,
  Radio,
  RadioGroup,
  Badge,
  makeStyles,
} from '@fluentui/react-components';
import { useMetadataStore, useConfigStore, useUIStore } from '../../stores';
import { useFetchViews } from '../../hooks/useDataverse';
import { LoadingOverlay, EmptyState } from '../shared';

const useStyles = makeStyles({
  list: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
    maxHeight: '400px',
    overflow: 'auto',
    marginTop: '8px',
  },
});

export function ViewPickerDialog() {
  const styles = useStyles();
  const open = useUIStore((s) => s.dialogs.viewPicker);
  const closeDialog = useUIStore((s) => s.closeDialog);
  const tableName = useUIStore((s) => s.pickerContext.tableName);

  const views = useMetadataStore((s) => tableName ? s.tableViews[tableName] : undefined);
  const loadingViews = useMetadataStore((s) => tableName ? s.loading.views[tableName] : false);
  const fetchViews = useFetchViews();

  const setTableView = useConfigStore((s) => s.setTableView);
  const currentViewId = useConfigStore((s) => tableName ? s.tableViews[tableName] : undefined);

  useEffect(() => {
    if (open && tableName && !views) {
      fetchViews(tableName);
    }
  }, [open, tableName, views, fetchViews]);

  const handleSelect = (viewId: string) => {
    if (!tableName) return;
    const view = views?.find((v) => v.viewId === viewId);
    if (view) {
      setTableView(tableName, view.viewId, view.name);
    }
    closeDialog('viewPicker');
  };

  const handleClose = () => closeDialog('viewPicker');

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogSurface>
        <DialogTitle>Select View for {tableName}</DialogTitle>
        <DialogBody>
          <DialogContent>
            {loadingViews ? (
              <LoadingOverlay inline message="Loading views..." />
            ) : !views || views.length === 0 ? (
              <EmptyState title="No Views Available" description="No saved queries found for this table." />
            ) : (
              <RadioGroup value={currentViewId ?? ''} onChange={(_, d) => handleSelect(d.value)}>
                <div className={styles.list}>
                  {views.map((view) => (
                    <Radio
                      key={view.viewId}
                      value={view.viewId}
                      label={
                        <span>
                          {view.name}
                          {view.isDefault && (
                            <Badge appearance="tint" color="brand" size="tiny" style={{ marginLeft: 8 }}>
                              default
                            </Badge>
                          )}
                        </span>
                      }
                    />
                  ))}
                </div>
              </RadioGroup>
            )}
          </DialogContent>
          <DialogActions>
            <Button appearance="secondary" onClick={handleClose}>Cancel</Button>
          </DialogActions>
        </DialogBody>
      </DialogSurface>
    </Dialog>
  );
}
