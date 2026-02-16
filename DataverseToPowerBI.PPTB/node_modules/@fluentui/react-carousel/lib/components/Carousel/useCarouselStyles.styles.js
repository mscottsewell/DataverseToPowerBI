'use client';

import { __styles, mergeClasses } from '@griffel/react';
import { tokens } from '@fluentui/react-theme';
export const carouselClassNames = {
  root: 'fui-Carousel'
};
/**
 * Styles for the root slot
 */
const useStyles = /*#__PURE__*/__styles({
  root: {
    B68tc82: "f1p9o1ba",
    Eiaeu8: "f1115ve7",
    qhf8xq: "f10pi13n"
  },
  elevated: {
    jrapky: 0,
    Frg6f3: 0,
    t21cq0: 0,
    B6of3ja: 0,
    B74szlk: "flfurxc",
    Byoj8tv: 0,
    uwmqm3: 0,
    z189sj: 0,
    z8tnut: 0,
    B0ocmuz: "f16dxoy8"
  }
}, {
  d: [".f1p9o1ba{overflow-x:hidden;}", ".f1115ve7{overflow-anchor:none;}", ".f10pi13n{position:relative;}", [".flfurxc{margin:var(--spacingVerticalL);}", {
    p: -1
  }], [".f16dxoy8{padding:var(--spacingVerticalL);}", {
    p: -1
  }]]
});
/**
 * Apply styling to the Carousel slots based on the state
 */
export const useCarouselStyles_unstable = state => {
  'use no memo';

  const styles = useStyles();
  const {
    appearance
  } = state;
  state.root.className = mergeClasses(carouselClassNames.root, styles.root, appearance === 'elevated' && styles.elevated, state.root.className);
  return state;
};