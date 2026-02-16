/**
 * SearchInput - Reusable search/filter input with clear button
 */

import {
  Input,
  makeStyles,
} from '@fluentui/react-components';
import { Search24Regular, Dismiss24Regular } from '@fluentui/react-icons';

const useStyles = makeStyles({
  root: {
    width: '100%',
    maxWidth: '400px',
  },
});

interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
}

export function SearchInput({ value, onChange, placeholder = 'Search...', disabled }: SearchInputProps) {
  const styles = useStyles();

  return (
    <Input
      className={styles.root}
      value={value}
      onChange={(_, data) => onChange(data.value)}
      placeholder={placeholder}
      disabled={disabled}
      contentBefore={<Search24Regular />}
      contentAfter={
        value ? (
          <Dismiss24Regular
            style={{ cursor: 'pointer' }}
            onClick={() => onChange('')}
          />
        ) : undefined
      }
    />
  );
}
