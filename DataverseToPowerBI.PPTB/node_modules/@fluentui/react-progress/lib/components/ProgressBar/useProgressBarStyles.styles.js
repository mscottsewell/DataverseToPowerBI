'use client';

import { __styles, mergeClasses } from '@griffel/react';
import { tokens } from '@fluentui/react-theme';
export const progressBarClassNames = {
  root: 'fui-ProgressBar',
  bar: 'fui-ProgressBar__bar'
};
// If the percentComplete is near 0, don't animate it.
// This prevents animations on reset to 0 scenarios.
const ZERO_THRESHOLD = 0.01;
const barThicknessValues = {
  medium: '2px',
  large: '4px'
};
const indeterminateProgressBar = {
  '0%': {
    left: '-33%'
  },
  '100%': {
    left: '100%'
  }
};
const indeterminateProgressBarReducedMotion = {
  '0%': {
    opacity: '.2'
  },
  '50%': {
    opacity: '1'
  },
  '100%': {
    opacity: '.2'
  }
};
/**
 * Styles for the root slot
 */
const useRootStyles = /*#__PURE__*/__styles({
  root: {
    mc9l5x: "ftgm304",
    De3pzq: "f18f03hv",
    a9b677: "fly5x3f",
    B68tc82: 0,
    Bmxbyg5: 0,
    Bpg54ce: "f1a3p1vp",
    Bomf52o: "f1skxd4g"
  },
  rounded: {
    Beyfa6y: 0,
    Bbmb7ep: 0,
    Btl43ni: 0,
    B7oj6ja: 0,
    Dimara: "ft85np5"
  },
  square: {
    Beyfa6y: 0,
    Bbmb7ep: 0,
    Btl43ni: 0,
    B7oj6ja: 0,
    Dimara: "f1fabniw"
  },
  medium: {
    Bqenvij: "f4t8t6x"
  },
  large: {
    Bqenvij: "f6ywr7j"
  }
}, {
  d: [".ftgm304{display:block;}", ".f18f03hv{background-color:var(--colorNeutralBackground6);}", ".fly5x3f{width:100%;}", [".f1a3p1vp{overflow:hidden;}", {
    p: -1
  }], [".ft85np5{border-radius:var(--borderRadiusMedium);}", {
    p: -1
  }], [".f1fabniw{border-radius:var(--borderRadiusNone);}", {
    p: -1
  }], ".f4t8t6x{height:2px;}", ".f6ywr7j{height:4px;}"],
  m: [["@media screen and (forced-colors: active){.f1skxd4g{background-color:CanvasText;}}", {
    m: "screen and (forced-colors: active)"
  }]]
});
/**
 * Styles for the ProgressBar bar
 */
const useBarStyles = /*#__PURE__*/__styles({
  base: {
    Bomf52o: "f1tnpuu0",
    Beyfa6y: 0,
    Bbmb7ep: 0,
    Btl43ni: 0,
    B7oj6ja: 0,
    Dimara: "f12b9xdw",
    Bqenvij: "f1l02sjl"
  },
  nonZeroDeterminate: {
    Bmy1vo4: "fjt6zfz",
    B3o57yi: "f1wofebd",
    Bkqvd7p: "fv71qf3"
  },
  indeterminate: {
    B2u0y6b: "fa0wk36",
    qhf8xq: "f10pi13n",
    Bcmaq0h: ["fpo0yib", "f1u5hf6c"],
    Bv12yb3: ["fwd2bol", "f14gig94"],
    vin17d: "f1a27w2r",
    Ezkn3b: "f452v7t",
    w3vfg9: "f1cpbl36",
    jpy9cc: "f3z2g5w",
    Bqo2lbl: "fz5izi4",
    B6plc1d: "fv40pdu",
    I82g5a: "f1uj6jbf"
  },
  brand: {
    De3pzq: "ftywsgz"
  },
  error: {
    De3pzq: "fdl5y0r"
  },
  warning: {
    De3pzq: "f1s438gw"
  },
  success: {
    De3pzq: "flxk52p"
  }
}, {
  m: [["@media screen and (forced-colors: active){.f1tnpuu0{background-color:Highlight;}}", {
    m: "screen and (forced-colors: active)"
  }], ["@media screen and (prefers-reduced-motion: reduce){.f3z2g5w{max-width:100%;}}", {
    m: "screen and (prefers-reduced-motion: reduce)"
  }], ["@media screen and (prefers-reduced-motion: reduce){.fz5izi4{animation-iteration-count:infinite;}}", {
    m: "screen and (prefers-reduced-motion: reduce)"
  }], ["@media screen and (prefers-reduced-motion: reduce){.fv40pdu{animation-duration:3s;}}", {
    m: "screen and (prefers-reduced-motion: reduce)"
  }], ["@media screen and (prefers-reduced-motion: reduce){.f1uj6jbf{animation-name:ftc26vs;}}", {
    m: "screen and (prefers-reduced-motion: reduce)"
  }]],
  d: [[".f12b9xdw{border-radius:inherit;}", {
    p: -1
  }], ".f1l02sjl{height:100%;}", ".fjt6zfz{transition-property:width;}", ".f1wofebd{transition-duration:0.3s;}", ".fv71qf3{transition-timing-function:ease;}", ".fa0wk36{max-width:33%;}", ".f10pi13n{position:relative;}", ".fpo0yib{background-image:linear-gradient(\n      to right,\n      var(--colorNeutralBackground6) 0%,\n      var(--colorTransparentBackground) 50%,\n      var(--colorNeutralBackground6) 100%\n    );}", ".f1u5hf6c{background-image:linear-gradient(\n      to left,\n      var(--colorNeutralBackground6) 0%,\n      var(--colorTransparentBackground) 50%,\n      var(--colorNeutralBackground6) 100%\n    );}", ".fwd2bol{animation-name:f1keuaan;}", ".f14gig94{animation-name:f10x8f8u;}", ".f1a27w2r{animation-duration:3s;}", ".f452v7t{animation-timing-function:linear;}", ".f1cpbl36{animation-iteration-count:infinite;}", ".ftywsgz{background-color:var(--colorCompoundBrandBackground);}", ".fdl5y0r{background-color:var(--colorPaletteRedBackground3);}", ".f1s438gw{background-color:var(--colorPaletteDarkOrangeBackground3);}", ".flxk52p{background-color:var(--colorPaletteGreenBackground3);}"],
  k: ["@keyframes f1keuaan{0%{left:-33%;}100%{left:100%;}}", "@keyframes f10x8f8u{0%{right:-33%;}100%{right:100%;}}", "@keyframes ftc26vs{0%{opacity:.2;}50%{opacity:1;}100%{opacity:.2;}}"]
});
/**
 * Apply styling to the ProgressBar slots based on the state
 */
export const useProgressBarStyles_unstable = state => {
  'use no memo';

  const {
    color,
    max,
    shape,
    thickness,
    value
  } = state;
  const rootStyles = useRootStyles();
  const barStyles = useBarStyles();
  state.root.className = mergeClasses(progressBarClassNames.root, rootStyles.root, rootStyles[shape], rootStyles[thickness], state.root.className);
  if (state.bar) {
    state.bar.className = mergeClasses(progressBarClassNames.bar, barStyles.base, barStyles.brand, value === undefined && barStyles.indeterminate, value !== undefined && value > ZERO_THRESHOLD && barStyles.nonZeroDeterminate, color && value !== undefined && barStyles[color], state.bar.className);
  }
  if (state.bar && value !== undefined) {
    state.bar.style = {
      width: Math.min(100, Math.max(0, value / max * 100)) + '%',
      ...state.bar.style
    };
  }
  return state;
};