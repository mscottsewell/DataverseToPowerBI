/**
 * FormPickerDialog - Modal to select a form for a table
 *
 * Displays available forms and allows the user to select one to
 * use as an attribute preset (loads attributes visible on the form).
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
  Text,
  makeStyles,
  tokens,
} from '@fluentui/react-components';
import { useMetadataStore, useConfigStore, useUIStore } from '../../stores';
import { useFetchForms } from '../../hooks/useDataverse';
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

export function FormPickerDialog() {
  const styles = useStyles();
  const open = useUIStore((s) => s.dialogs.formPicker);
  const closeDialog = useUIStore((s) => s.closeDialog);
  const tableName = useUIStore((s) => s.pickerContext.tableName);

  const forms = useMetadataStore((s) => tableName ? s.tableForms[tableName] : undefined);
  const loadingForms = useMetadataStore((s) => tableName ? s.loading.forms[tableName] : false);
  const fetchForms = useFetchForms();

  const setTableForm = useConfigStore((s) => s.setTableForm);
  const setTableAttributes = useConfigStore((s) => s.setTableAttributes);
  const currentFormId = useConfigStore((s) => tableName ? s.tableForms[tableName] : undefined);

  useEffect(() => {
    if (open && tableName && !forms) {
      fetchForms(tableName);
    }
  }, [open, tableName, forms, fetchForms]);

  const handleSelect = (formId: string) => {
    if (!tableName) return;
    const form = forms?.find((f) => f.formId === formId);
    if (form) {
      setTableForm(tableName, form.formId, form.name);
      // If form has parsed fields, auto-select those attributes
      if (form.fields && form.fields.length > 0) {
        setTableAttributes(tableName, form.fields);
      }
    }
    closeDialog('formPicker');
  };

  const handleClose = () => closeDialog('formPicker');

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogSurface>
        <DialogTitle>Select Form for {tableName}</DialogTitle>
        <DialogBody>
          <DialogContent>
            {loadingForms ? (
              <LoadingOverlay inline message="Loading forms..." />
            ) : !forms || forms.length === 0 ? (
              <EmptyState title="No Forms Available" description="No main forms found for this table." />
            ) : (
              <RadioGroup value={currentFormId ?? ''} onChange={(_, d) => handleSelect(d.value)}>
                <div className={styles.list}>
                  {forms.map((form) => (
                    <Radio
                      key={form.formId}
                      value={form.formId}
                      label={
                        <span>
                          {form.name}
                          {form.fields && (
                            <Text size={200} style={{ color: tokens.colorNeutralForeground3, marginLeft: 8 }}>
                              ({form.fields.length} fields)
                            </Text>
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
