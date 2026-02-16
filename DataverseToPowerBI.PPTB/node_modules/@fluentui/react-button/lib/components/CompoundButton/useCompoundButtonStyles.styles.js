'use client';

import { tokens } from '@fluentui/react-theme';
import { mergeClasses, __styles } from '@griffel/react';
import { useButtonStyles_unstable } from '../Button/useButtonStyles.styles';
export const compoundButtonClassNames = {
  root: 'fui-CompoundButton',
  icon: 'fui-CompoundButton__icon',
  contentContainer: 'fui-CompoundButton__contentContainer',
  secondaryContent: 'fui-CompoundButton__secondaryContent'
};
const useRootStyles = /*#__PURE__*/__styles({
  base: {
    Bqenvij: "f11ysow2",
    J657lq: "f1um431h",
    Jlnjib: "fte7hqw",
    Bo7webf: "frw79jk"
  },
  highContrast: {
    m3fafd: "ffcivv0",
    pus4l6: "f1wchxtt"
  },
  outline: {},
  primary: {
    J657lq: "foe7gw6",
    Jlnjib: "fvxlz81",
    Bo7webf: "f16twlsn",
    D126e9: "fsglouz"
  },
  secondary: {},
  subtle: {
    J657lq: "f1um431h",
    Jlnjib: "fte7hqw",
    Bo7webf: "frw79jk",
    m3fafd: "f13lnigs",
    pus4l6: "f1ba77l5"
  },
  transparent: {
    J657lq: "f1um431h",
    Jlnjib: "f1wn9xqz",
    Bo7webf: "f1juxwb4"
  },
  small: {
    Byoj8tv: 0,
    uwmqm3: 0,
    z189sj: 0,
    z8tnut: 0,
    B0ocmuz: "f1ge6w2w",
    Be2twd7: "fkhj508",
    Bg96gwp: "f1i3iumi"
  },
  medium: {
    Byoj8tv: 0,
    uwmqm3: 0,
    z189sj: 0,
    z8tnut: 0,
    B0ocmuz: "fnnf4v2",
    Be2twd7: "fkhj508",
    Bg96gwp: "f1i3iumi"
  },
  large: {
    Byoj8tv: 0,
    uwmqm3: 0,
    z189sj: 0,
    z8tnut: 0,
    B0ocmuz: "f14s4sho",
    Be2twd7: "fod5ikn",
    Bg96gwp: "faaz57k"
  },
  disabled: {
    J657lq: "f1rlv8bf",
    Jlnjib: "fd1dbtm",
    Bo7webf: "f1x3eb98"
  },
  disabledHighContrast: {
    D126e9: "fbqh1p7",
    m3fafd: "fu2tbix",
    pus4l6: "f1g2tosm"
  }
}, {
  d: [".f11ysow2{height:auto;}", ".f1um431h .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForeground2);}", ".foe7gw6 .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForegroundOnBrand);}", [".f1ge6w2w{padding:var(--spacingHorizontalS) var(--spacingHorizontalS) var(--spacingHorizontalMNudge) var(--spacingHorizontalS);}", {
    p: -1
  }], ".fkhj508{font-size:var(--fontSizeBase300);}", ".f1i3iumi{line-height:var(--lineHeightBase300);}", [".fnnf4v2{padding:14px var(--spacingHorizontalM) var(--spacingHorizontalL) var(--spacingHorizontalM);}", {
    p: -1
  }], [".f14s4sho{padding:18px var(--spacingHorizontalL) var(--spacingHorizontalXL) var(--spacingHorizontalL);}", {
    p: -1
  }], ".fod5ikn{font-size:var(--fontSizeBase400);}", ".faaz57k{line-height:var(--lineHeightBase400);}", ".f1rlv8bf .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForegroundDisabled);}"],
  h: [".fte7hqw:hover .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForeground2Hover);}", ".frw79jk:hover:active .fui-CompoundButton__secondaryContent,.frw79jk:active:focus-visible .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForeground2Pressed);}", ".fvxlz81:hover .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForegroundOnBrand);}", ".f16twlsn:hover:active .fui-CompoundButton__secondaryContent,.f16twlsn:active:focus-visible .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForegroundOnBrand);}", ".f1wn9xqz:hover .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForeground2BrandHover);}", ".f1juxwb4:hover:active .fui-CompoundButton__secondaryContent,.f1juxwb4:active:focus-visible .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForeground2BrandPressed);}", ".fd1dbtm:hover .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForegroundDisabled);}", ".f1x3eb98:hover:active .fui-CompoundButton__secondaryContent,.f1x3eb98:active:focus-visible .fui-CompoundButton__secondaryContent{color:var(--colorNeutralForegroundDisabled);}"],
  m: [["@media (forced-colors: active){.ffcivv0:hover .fui-CompoundButton__secondaryContent{color:Highlight;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f1wchxtt:hover:active .fui-CompoundButton__secondaryContent,.f1wchxtt:active:focus-visible .fui-CompoundButton__secondaryContent{color:Highlight;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.fsglouz .fui-CompoundButton__secondaryContent{color:HighlightText;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f13lnigs:hover .fui-CompoundButton__secondaryContent{color:Canvas;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f1ba77l5:hover:active .fui-CompoundButton__secondaryContent,.f1ba77l5:active:focus-visible .fui-CompoundButton__secondaryContent{color:Canvas;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.fbqh1p7 .fui-CompoundButton__secondaryContent{color:GrayText;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.fu2tbix:hover .fui-CompoundButton__secondaryContent{color:GrayText;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f1g2tosm:hover:active .fui-CompoundButton__secondaryContent,.f1g2tosm:active:focus-visible .fui-CompoundButton__secondaryContent{color:GrayText;}}", {
    m: "(forced-colors: active)"
  }]]
});
const useRootIconOnlyStyles = /*#__PURE__*/__styles({
  small: {
    Byoj8tv: 0,
    uwmqm3: 0,
    z189sj: 0,
    z8tnut: 0,
    B0ocmuz: "f1t35pdg",
    B2u0y6b: "ft5vyj6",
    Bf4jedk: "f17suaiq"
  },
  medium: {
    Byoj8tv: 0,
    uwmqm3: 0,
    z189sj: 0,
    z8tnut: 0,
    B0ocmuz: "f1r1wyb6",
    B2u0y6b: "fdczgix",
    Bf4jedk: "fjdcg9m"
  },
  large: {
    Byoj8tv: 0,
    uwmqm3: 0,
    z189sj: 0,
    z8tnut: 0,
    B0ocmuz: "f1bnz8pu",
    B2u0y6b: "fww51uw",
    Bf4jedk: "f1qhsl2h"
  }
}, {
  d: [[".f1t35pdg{padding:var(--spacingHorizontalXS);}", {
    p: -1
  }], ".ft5vyj6{max-width:48px;}", ".f17suaiq{min-width:48px;}", [".f1r1wyb6{padding:var(--spacingHorizontalSNudge);}", {
    p: -1
  }], ".fdczgix{max-width:52px;}", ".fjdcg9m{min-width:52px;}", [".f1bnz8pu{padding:var(--spacingHorizontalS);}", {
    p: -1
  }], ".fww51uw{max-width:56px;}", ".f1qhsl2h{min-width:56px;}"]
});
const useIconStyles = /*#__PURE__*/__styles({
  base: {
    Be2twd7: "fndrnj9",
    Bqenvij: "fbhnoac",
    a9b677: "feqmc2u"
  },
  before: {
    t21cq0: ["fkujibs", "f199hnxi"]
  },
  after: {
    Frg6f3: ["f199hnxi", "fkujibs"]
  }
}, {
  d: [".fndrnj9{font-size:40px;}", ".fbhnoac{height:40px;}", ".feqmc2u{width:40px;}", ".fkujibs{margin-right:var(--spacingHorizontalM);}", ".f199hnxi{margin-left:var(--spacingHorizontalM);}"]
});
const useContentContainerStyles = /*#__PURE__*/__styles({
  base: {
    mc9l5x: "f22iagw",
    Beiy3e4: "f1vx9l62",
    fsow6f: ["f1o700av", "fes3tcz"]
  }
}, {
  d: [".f22iagw{display:flex;}", ".f1vx9l62{flex-direction:column;}", ".f1o700av{text-align:left;}", ".fes3tcz{text-align:right;}"]
});
const useSecondaryContentStyles = /*#__PURE__*/__styles({
  base: {
    Bg96gwp: "flkuc6h",
    Bhrd7zp: "figsok6"
  },
  small: {
    Be2twd7: "fy9rknc"
  },
  medium: {
    Be2twd7: "fy9rknc"
  },
  large: {
    Be2twd7: "fkhj508"
  }
}, {
  d: [".flkuc6h{line-height:100%;}", ".figsok6{font-weight:var(--fontWeightRegular);}", ".fy9rknc{font-size:var(--fontSizeBase200);}", ".fkhj508{font-size:var(--fontSizeBase300);}"]
});
export const useCompoundButtonStyles_unstable = state => {
  'use no memo';

  const rootStyles = useRootStyles();
  const rootIconOnlyStyles = useRootIconOnlyStyles();
  const iconStyles = useIconStyles();
  const contentContainerStyles = useContentContainerStyles();
  const secondaryContentStyles = useSecondaryContentStyles();
  const {
    appearance,
    disabled,
    disabledFocusable,
    iconOnly,
    iconPosition,
    size
  } = state;
  state.root.className = mergeClasses(compoundButtonClassNames.root,
  // Root styles
  rootStyles.base, rootStyles.highContrast, appearance && rootStyles[appearance], rootStyles[size],
  // Disabled styles
  (disabled || disabledFocusable) && rootStyles.disabled, (disabled || disabledFocusable) && rootStyles.disabledHighContrast,
  // Icon-only styles
  iconOnly && rootIconOnlyStyles[size],
  // User provided class name
  state.root.className);
  state.contentContainer.className = mergeClasses(compoundButtonClassNames.contentContainer, contentContainerStyles.base, state.contentContainer.className);
  if (state.icon) {
    state.icon.className = mergeClasses(compoundButtonClassNames.icon, iconStyles.base, state.root.children !== undefined && state.root.children !== null && iconStyles[iconPosition], state.icon.className);
  }
  if (state.secondaryContent) {
    state.secondaryContent.className = mergeClasses(compoundButtonClassNames.secondaryContent, secondaryContentStyles.base, secondaryContentStyles[size], state.secondaryContent.className);
  }
  useButtonStyles_unstable(state);
  return state;
};