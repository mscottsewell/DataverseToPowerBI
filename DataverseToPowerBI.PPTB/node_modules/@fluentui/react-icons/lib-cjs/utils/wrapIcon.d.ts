import * as React from 'react';
import type { FluentIconsProps } from './FluentIconsProps.types';
import type { CreateFluentIconOptions } from './createFluentIcon';
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
export declare const wrapIcon: (Icon: (iconProps: FluentIconsProps) => React.ReactElement, displayName?: string | undefined, options?: CreateFluentIconOptions | undefined) => React.FC<FluentIconsProps<React.SVGAttributes<SVGElement>, SVGSVGElement>>;
