/* eslint-disable @typescript-eslint/no-deprecated */'use client';

import { createFocusOutlineStyle } from '@fluentui/react-tabster';
import { iconFilledClassName, iconRegularClassName } from '@fluentui/react-icons';
import { __styles, mergeClasses, shorthands } from '@griffel/react';
import { tokens, typographyStyles } from '@fluentui/react-theme';
/**
 * @deprecated use {@link @fluentui/react-components#InfoLabel} from `\@fluentui/react-components` or `\@fluentui/react-infolabel` instead
 */
export const infoButtonClassNames = {
  root: 'fui-InfoButton',
  // this className won't be used, but it's needed to satisfy the type checker
  popover: 'fui-InfoButton__popover',
  info: 'fui-InfoButton__info'
};
/**
 * Styles for the root slot
 *
 * @deprecated use {@link @fluentui/react-components#InfoLabel} from `\@fluentui/react-components` or `\@fluentui/react-infolabel` instead
 */
const useButtonStyles = /*#__PURE__*/__styles({
  base: {
    Bt984gj: "f122n59",
    B7ck84d: "f1ewtqcl",
    mc9l5x: "ftuwxu6",
    Brf1p80: "f4d9j23",
    w71qe1: "f1iuv45f",
    ha4doy: "fmrv4ls",
    qhf8xq: "f10pi13n",
    De3pzq: "f1c21dwh",
    sj55zd: "fkfq4zb",
    icvyot: "f1ern45e",
    vrafjx: ["f1n71otn", "f1deefiw"],
    oivjwe: "f1h8hb77",
    wvpqe5: ["f1deefiw", "f1n71otn"],
    Bbmb7ep: ["f1aa9q02", "f16jpd5f"],
    Beyfa6y: ["f16jpd5f", "f1aa9q02"],
    B7oj6ja: ["f1jar5jt", "fyu767a"],
    Btl43ni: ["fyu767a", "f1jar5jt"],
    B6of3ja: "f1hu3pq6",
    t21cq0: ["f11qmguv", "f1tyq0we"],
    jrapky: "f19f4twv",
    Frg6f3: ["f1tyq0we", "f11qmguv"],
    z8tnut: "f1ywm7hm",
    z189sj: ["f7x41pl", "fruq291"],
    Byoj8tv: "f14wxoun",
    uwmqm3: ["fruq291", "f7x41pl"],
    D0sxk3: "f16u1re",
    t6yez3: "f1rw4040",
    Jwef8y: "fjxutwb",
    Bi91k9c: "f139oj5f",
    eoavqd: "f8491dx",
    Bk3fhr4: "f1jpd6y0",
    Bmfj8id: "fuxngvv",
    iro3zm: "fwiml72",
    B2d53fq: "f1fg1p5m"
  },
  selected: {
    De3pzq: "f1q9pm1r",
    sj55zd: "f1qj7y59",
    D0sxk3: "fgzdkf0",
    t6yez3: "f15q0o9g",
    By8wz76: "f1nz3ub2",
    B7iucu3: "f1205bnn"
  },
  highContrast: {
    B7iucu3: "fslfhp6",
    Bfsdueo: "f11s1vgy",
    pb71ee: "f12b7wpn",
    ik86qs: "f1u668rv"
  },
  focusIndicator: {
    Brovlpu: "ftqa4ok",
    B486eqv: "f2hkw1w",
    B8q5s1w: "f8hki3x",
    Bci5o5g: ["f1d2448m", "ffh67wi"],
    n8qw10: "f1bjia2o",
    Bdrgwmp: ["ffh67wi", "f1d2448m"],
    Bqhya38: "f1j6vpng",
    Bwxa6fj: ["f1pniga2", "f1ffjurs"],
    Bdhvstf: "f987i1v",
    B7zbvrb: ["f1ffjurs", "f1pniga2"],
    Bm4h7ae: "f15bsgw9",
    B7ys5i9: "f14e48fq",
    Busjfv9: "f18yb2kv",
    Bhk32uz: "fd6o370",
    f6g5ot: 0,
    Boxcth7: 0,
    Bhdgwq3: 0,
    hgwjuy: 0,
    Bshpdp8: 0,
    Bsom6fd: 0,
    Blkhhs4: 0,
    Bonggc9: 0,
    Ddfuxk: 0,
    i03rao: 0,
    kclons: 0,
    clg4pj: 0,
    Bpqj9nj: 0,
    B6dhp37: 0,
    Bf4ptjt: 0,
    Bqtpl0w: 0,
    i4rwgc: "ffwy5si",
    Dah5zi: 0,
    B1tsrr9: 0,
    qqdqy8: 0,
    Bkh64rk: 0,
    e3fwne: "f3znvyf",
    J0r882: "f57olzd",
    Bule8hv: ["f4stah7", "fs1por5"],
    Bjwuhne: "f480a47",
    Ghsupd: ["fs1por5", "f4stah7"]
  },
  large: {
    z8tnut: "fclwglc",
    z189sj: ["f1w3695s", "f1b0r8ql"],
    Byoj8tv: "fywfov9",
    uwmqm3: ["f1b0r8ql", "f1w3695s"]
  }
}, {
  d: [".f122n59{align-items:center;}", ".f1ewtqcl{box-sizing:border-box;}", ".ftuwxu6{display:inline-flex;}", ".f4d9j23{justify-content:center;}", ".f1iuv45f{text-decoration-line:none;}", ".fmrv4ls{vertical-align:middle;}", ".f10pi13n{position:relative;}", ".f1c21dwh{background-color:var(--colorTransparentBackground);}", ".fkfq4zb{color:var(--colorNeutralForeground2);}", ".f1ern45e{border-top-style:none;}", ".f1n71otn{border-right-style:none;}", ".f1deefiw{border-left-style:none;}", ".f1h8hb77{border-bottom-style:none;}", ".f1aa9q02{border-bottom-right-radius:var(--borderRadiusMedium);}", ".f16jpd5f{border-bottom-left-radius:var(--borderRadiusMedium);}", ".f1jar5jt{border-top-right-radius:var(--borderRadiusMedium);}", ".fyu767a{border-top-left-radius:var(--borderRadiusMedium);}", ".f1hu3pq6{margin-top:0;}", ".f11qmguv{margin-right:0;}", ".f1tyq0we{margin-left:0;}", ".f19f4twv{margin-bottom:0;}", ".f1ywm7hm{padding-top:var(--spacingVerticalXS);}", ".f7x41pl{padding-right:var(--spacingHorizontalXS);}", ".fruq291{padding-left:var(--spacingHorizontalXS);}", ".f14wxoun{padding-bottom:var(--spacingVerticalXS);}", ".f16u1re .fui-Icon-filled{display:none;}", ".f1rw4040 .fui-Icon-regular{display:inline-flex;}", ".f1q9pm1r{background-color:var(--colorTransparentBackgroundSelected);}", ".f1qj7y59{color:var(--colorNeutralForeground2BrandSelected);}", ".fgzdkf0 .fui-Icon-filled{display:inline-flex;}", ".f15q0o9g .fui-Icon-regular{display:none;}", ".f8hki3x[data-fui-focus-visible]{border-top-color:transparent;}", ".f1d2448m[data-fui-focus-visible]{border-right-color:transparent;}", ".ffh67wi[data-fui-focus-visible]{border-left-color:transparent;}", ".f1bjia2o[data-fui-focus-visible]{border-bottom-color:transparent;}", ".f15bsgw9[data-fui-focus-visible]::after{content:\"\";}", ".f14e48fq[data-fui-focus-visible]::after{position:absolute;}", ".f18yb2kv[data-fui-focus-visible]::after{pointer-events:none;}", ".fd6o370[data-fui-focus-visible]::after{z-index:1;}", [".ffwy5si[data-fui-focus-visible]::after{border:2px solid var(--colorStrokeFocus2);}", {
    p: -2
  }], [".f3znvyf[data-fui-focus-visible]::after{border-radius:var(--borderRadiusMedium);}", {
    p: -1
  }], ".f57olzd[data-fui-focus-visible]::after{top:calc(2px * -1);}", ".f4stah7[data-fui-focus-visible]::after{right:calc(2px * -1);}", ".fs1por5[data-fui-focus-visible]::after{left:calc(2px * -1);}", ".f480a47[data-fui-focus-visible]::after{bottom:calc(2px * -1);}", ".fclwglc{padding-top:var(--spacingVerticalXXS);}", ".f1w3695s{padding-right:var(--spacingVerticalXXS);}", ".f1b0r8ql{padding-left:var(--spacingVerticalXXS);}", ".fywfov9{padding-bottom:var(--spacingVerticalXXS);}"],
  h: [".fjxutwb:hover{background-color:var(--colorTransparentBackgroundHover);}", ".f139oj5f:hover{color:var(--colorNeutralForeground2BrandHover);}", ".f8491dx:hover{cursor:pointer;}", ".f1jpd6y0:hover .fui-Icon-filled{display:inline-flex;}", ".fuxngvv:hover .fui-Icon-regular{display:none;}", ".fwiml72:hover:active{background-color:var(--colorTransparentBackgroundPressed);}", ".f1fg1p5m:hover:active{color:var(--colorNeutralForeground2BrandPressed);}"],
  m: [["@media (forced-colors: active){.f1nz3ub2{background-color:Highlight;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f1205bnn{color:Canvas;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.fslfhp6{color:CanvasText;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f11s1vgy:hover,.f11s1vgy:hover:active{forced-color-adjust:none;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f12b7wpn:hover,.f12b7wpn:hover:active{background-color:Highlight;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f1u668rv:hover,.f1u668rv:hover:active{color:Canvas;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f1j6vpng[data-fui-focus-visible]::after{border-top-color:Highlight;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f1ffjurs[data-fui-focus-visible]::after{border-left-color:Highlight;}.f1pniga2[data-fui-focus-visible]::after{border-right-color:Highlight;}}", {
    m: "(forced-colors: active)"
  }], ["@media (forced-colors: active){.f987i1v[data-fui-focus-visible]::after{border-bottom-color:Highlight;}}", {
    m: "(forced-colors: active)"
  }]],
  f: [".ftqa4ok:focus{outline-style:none;}"],
  i: [".f2hkw1w:focus-visible{outline-style:none;}"]
});
const usePopoverSurfaceStyles = /*#__PURE__*/__styles({
  smallMedium: {
    Bahqtrf: "fk6fouc",
    Be2twd7: "fy9rknc",
    Bhrd7zp: "figsok6",
    Bg96gwp: "fwrc4pm"
  },
  large: {
    Bahqtrf: "fk6fouc",
    Be2twd7: "fkhj508",
    Bhrd7zp: "figsok6",
    Bg96gwp: "f1i3iumi"
  }
}, {
  d: [".fk6fouc{font-family:var(--fontFamilyBase);}", ".fy9rknc{font-size:var(--fontSizeBase200);}", ".figsok6{font-weight:var(--fontWeightRegular);}", ".fwrc4pm{line-height:var(--lineHeightBase200);}", ".fkhj508{font-size:var(--fontSizeBase300);}", ".f1i3iumi{line-height:var(--lineHeightBase300);}"]
});
/**
 * Apply styling to the InfoButton slots based on the state
 */
export const useInfoButtonStyles_unstable = state => {
  const {
    size
  } = state;
  const {
    open
  } = state.popover;
  const buttonStyles = useButtonStyles();
  const popoverSurfaceStyles = usePopoverSurfaceStyles();
  // eslint-disable-next-line react-compiler/react-compiler
  state.info.className = mergeClasses(infoButtonClassNames.info, size === 'large' ? popoverSurfaceStyles.large : popoverSurfaceStyles.smallMedium, state.info.className);
  state.root.className = mergeClasses(infoButtonClassNames.root, buttonStyles.base, buttonStyles.highContrast, buttonStyles.focusIndicator, open && buttonStyles.selected, size === 'large' && buttonStyles.large, state.root.className);
  return state;
};