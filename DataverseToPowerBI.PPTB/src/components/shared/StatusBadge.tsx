/**
 * StatusBadge - Displays a colored badge based on status type
 */

import { Badge } from '@fluentui/react-components';

type StatusType = 'success' | 'error' | 'warning' | 'info' | 'neutral';

interface StatusBadgeProps {
  status: StatusType;
  label: string;
}

const colorMap: Record<StatusType, 'success' | 'danger' | 'warning' | 'informative' | 'subtle'> = {
  success: 'success',
  error: 'danger',
  warning: 'warning',
  info: 'informative',
  neutral: 'subtle',
};

export function StatusBadge({ status, label }: StatusBadgeProps) {
  return (
    <Badge appearance="filled" color={colorMap[status]} size="small">
      {label}
    </Badge>
  );
}
