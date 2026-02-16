'use client';

import { __styles, mergeClasses } from '@griffel/react';
import { useButtonStyles_unstable } from '@fluentui/react-button';
import { navItemTokens } from '../sharedNavStyles.styles';
export const hamburgerClassNames = {
  root: 'fui-Hamburger',
  icon: 'fui-Hamburger__icon'
};
/**
 * Styles for the root slot
 */
const useStyles = /*#__PURE__*/__styles({
  root: {
    w71qe1: "f1iuv45f",
    De3pzq: "f1ctqxl6",
    Bgfg5da: 0,
    B9xav0g: 0,
    oivjwe: 0,
    Bn0qgzm: 0,
    B4g9neb: 0,
    zhjwy3: 0,
    wvpqe5: 0,
    ibv6hh: 0,
    u1mtju: 0,
    h3c5rm: 0,
    vrafjx: 0,
    Bekrc4i: 0,
    i8vvqc: 0,
    g2u3we: 0,
    icvyot: 0,
    B4j52fo: 0,
    irswps: "f3bhgqh",
    Jwef8y: "f11oyicx",
    ecr2s2: "f9fof1w"
  }
}, {
  d: [".f1iuv45f{text-decoration-line:none;}", ".f1ctqxl6{background-color:var(--colorNeutralBackground4);}", [".f3bhgqh{border:none;}", {
    p: -2
  }]],
  h: [".f11oyicx:hover{background-color:var(--colorNeutralBackground4Hover);}"],
  a: [".f9fof1w:active{background-color:var(--colorNeutralBackground4Pressed);}"]
});
/**
 * Apply styling to the Hamburger slots based on the state
 */
export const useHamburgerStyles_unstable = state => {
  'use no memo';

  useButtonStyles_unstable(state);
  const styles = useStyles();
  state.root.className = mergeClasses(hamburgerClassNames.root, styles.root, state.root.className);
  if (state.icon) {
    state.icon.className = mergeClasses(hamburgerClassNames.icon, state.icon.className);
  }
  return state;
};