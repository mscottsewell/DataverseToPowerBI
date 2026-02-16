'use client';

import { __styles, mergeClasses } from '@griffel/react';
import { tokens } from '@fluentui/react-theme';
import { useCarouselContext_unstable as useCarouselContext } from '../CarouselContext';
export const carouselSliderClassNames = {
  root: 'fui-CarouselSlider'
};
/**
 * Styles for the root slot
 */
const useStyles = /*#__PURE__*/__styles({
  root: {
    mc9l5x: "f22iagw",
    Eiaeu8: "f1115ve7"
  },
  elevated: {
    i8kkvl: 0,
    Belr9w4: 0,
    rmohyg: "f1vkj2z1"
  }
}, {
  d: [".f22iagw{display:flex;}", ".f1115ve7{overflow-anchor:none;}", [".f1vkj2z1{gap:var(--spacingHorizontalXXL);}", {
    p: -1
  }]]
});
/**
 * Apply styling to the CarouselSlider slots based on the state
 */
export const useCarouselSliderStyles_unstable = state => {
  'use no memo';

  const appearance = useCarouselContext(context => context.appearance);
  const styles = useStyles();
  state.root.className = mergeClasses(carouselSliderClassNames.root, styles.root, appearance === 'elevated' && styles.elevated, state.root.className);
  return state;
};