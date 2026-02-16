import * as React from 'react';
import { useIconState } from './useIconState';
/**
 *
 * Wraps custom Svg Component with Fluent Icon behaviour
 *
 * @example
 * ```tsx
 const CustomSvg = (iconProps: FluentIconsProps) =>
  React.createElement(
    'svg',
    {
      width: '20',
      height: '20',
      viewBox: '0 0 20 20',
      xmlns: 'http://www.w3.org/2000/svg',
      ...iconProps
    },
    React.createElement('path', {
      fill: 'currentColor',
      d: 'M10 2l6 16H4l6-16z'
    })
  );

  const CustomIcon = wrapIcon(CustomSvg, 'CustomIcon');
  ```
 */
export const wrapIcon = (Icon, displayName, options) => {
    const WrappedIcon = React.forwardRef((props, ref) => {
        const state = {
            ...useIconState(props, { flipInRtl: options === null || options === void 0 ? void 0 : options.flipInRtl }),
            ref,
        };
        return React.createElement(Icon, Object.assign({}, state));
    });
    WrappedIcon.displayName = displayName;
    return WrappedIcon;
};
