'use client';

import { __styles, mergeClasses, shorthands } from '@griffel/react';
/**
 * Static CSS class names used internally for the component slots.
 */
export const cardFooterClassNames = {
  root: 'fui-CardFooter',
  action: 'fui-CardFooter__action'
};
const useStyles = /*#__PURE__*/__styles({
  root: {
    mc9l5x: "f22iagw",
    Beiy3e4: "f1063pyq",
    i8kkvl: 0,
    Belr9w4: 0,
    rmohyg: "fsbu5mz"
  },
  action: {
    Frg6f3: ["fcgxt0o", "f1ujusj6"],
    rjrqlh: "fs9eatd",
    Boue1pl: ["fxoo9ru", "f1g0ekvh"],
    Bhz1vi0: "f1m6zfxz",
    etxrgc: ["f1g0ekvh", "fxoo9ru"],
    Bdua9ef: "f1sret3r",
    cbfxhg: "fs4gbcv"
  }
}, {
  d: [".f22iagw{display:flex;}", ".f1063pyq{flex-direction:row;}", [".fsbu5mz{gap:12px;}", {
    p: -1
  }], ".fcgxt0o{margin-left:auto;}", ".f1ujusj6{margin-right:auto;}"],
  m: [["@media (forced-colors: active){.fs9eatd .fui-Button,.fs9eatd .fui-Link{border-top-color:currentColor;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f1g0ekvh .fui-Button,.f1g0ekvh .fui-Link{border-left-color:currentColor;}.fxoo9ru .fui-Button,.fxoo9ru .fui-Link{border-right-color:currentColor;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f1m6zfxz .fui-Button,.f1m6zfxz .fui-Link{border-bottom-color:currentColor;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f1sret3r .fui-Button,.f1sret3r .fui-Link{color:currentColor;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.fs4gbcv .fui-Button,.fs4gbcv .fui-Link{outline-color:currentColor;}}", {
    m: "(forced-colors: active)"
  }]]
});
/**
 * Apply styling to the CardFooter slots based on the state.
 */
export const useCardFooterStyles_unstable = state => {
  'use no memo';

  const styles = useStyles();
  state.root.className = mergeClasses(cardFooterClassNames.root, styles.root, state.root.className);
  if (state.action) {
    state.action.className = mergeClasses(cardFooterClassNames.action, styles.action, state.action.className);
  }
  return state;
};