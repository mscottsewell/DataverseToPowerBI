'use client';

import { __resetStyles, __styles, mergeClasses } from '@griffel/react';
import { tokens } from '@fluentui/react-theme';
/**
 * Styles for the backdrop slot
 */
const useBackdropResetStyles = /*#__PURE__*/__resetStyles("rl76ifk", null, [".rl76ifk{inset:0px;position:fixed;background-color:var(--colorBackgroundOverlay);}"]);
const useBackdropStyles = /*#__PURE__*/__styles({
  nested: {
    De3pzq: "f1c21dwh"
  },
  drawerHidden: {
    Bkecrkj: "f1aehjj5"
  }
}, {
  d: [".f1c21dwh{background-color:var(--colorTransparentBackground);}", ".f1aehjj5{pointer-events:none;}"]
});
/**
 * Apply styling to the OverlayDrawerSurface slots based on the state
 */
export const useOverlayDrawerSurfaceStyles_unstable = state => {
  'use no memo';

  const {
    treatBackdropAsNested,
    backdrop,
    open,
    unmountOnClose
  } = state;
  const backdropResetStyles = useBackdropResetStyles();
  const backdropStyles = useBackdropStyles();
  const mountedAndClosed = !unmountOnClose && !open;
  if (backdrop) {
    backdrop.className = mergeClasses(backdropResetStyles, treatBackdropAsNested && backdropStyles.nested, mountedAndClosed && backdropStyles.drawerHidden, backdrop.className);
  }
  return state;
};