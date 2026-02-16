"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
function _export(target, all) {
    for(var name in all)Object.defineProperty(target, name, {
        enumerable: true,
        get: all[name]
    });
}
_export(exports, {
    Collapse: function() {
        return Collapse;
    },
    CollapseDelayed: function() {
        return CollapseDelayed;
    },
    CollapseRelaxed: function() {
        return CollapseRelaxed;
    },
    CollapseSnappy: function() {
        return CollapseSnappy;
    }
});
const _reactmotion = require("@fluentui/react-motion");
const _collapseatoms = require("./collapse-atoms");
const _fadeatom = require("../../atoms/fade-atom");
/**
 * Define a presence motion for collapse/expand
 *
 * @param element - The element to apply the collapse motion to
 * @param duration - Time (ms) for the enter transition (expand). Defaults to the `durationNormal` value (200 ms)
 * @param easing - Easing curve for the enter transition. Defaults to the `curveEasyEaseMax` value
 * @param delay - Time (ms) to delay the entire enter transition. Defaults to 0
 * @param exitDuration - Time (ms) for the exit transition (collapse). Defaults to the `duration` param for symmetry
 * @param exitEasing - Easing curve for the exit transition. Defaults to the `easing` param for symmetry
 * @param exitDelay - Time (ms) to delay the entire exit transition. Defaults to the `delay` param for symmetry
 * @param staggerDelay - Time (ms) offset between the size and opacity animations. Defaults to 0
 * @param exitStaggerDelay - Time (ms) offset between the size and opacity animations on exit. Defaults to the `staggerDelay` param for symmetry
 * @param sizeDuration - Time (ms) for the size animation during enter. Defaults to `duration` for unified timing
 * @param opacityDuration - Time (ms) for the opacity animation during enter. Defaults to `sizeDuration` for synchronized timing
 * @param exitSizeDuration - Time (ms) for the size animation during exit. Defaults to `exitDuration` for unified timing
 * @param exitOpacityDuration - Time (ms) for the opacity animation during exit. Defaults to `exitSizeDuration` for synchronized timing
 * @param animateOpacity - Whether to animate the opacity. Defaults to `true`
 * @param orientation - The orientation of the size animation. Defaults to `'vertical'` to expand/collapse the height
 * @param outSize - Size for the out state (collapsed). Defaults to `'0px'`
 */ const collapsePresenceFn = ({ element, // Primary duration controls (simple API)
duration = _reactmotion.motionTokens.durationNormal, exitDuration = duration, // Granular duration controls with smart defaults (advanced API)
sizeDuration = duration, opacityDuration = sizeDuration, exitSizeDuration = exitDuration, exitOpacityDuration = exitSizeDuration, // Other timing controls
easing = _reactmotion.motionTokens.curveEasyEaseMax, delay = 0, exitEasing = easing, exitDelay = delay, staggerDelay = 0, exitStaggerDelay = staggerDelay, // Animation controls
animateOpacity = true, orientation = 'vertical', outSize = '0px' })=>{
    // ----- ENTER -----
    // The enter transition is an array of up to 3 motion atoms: size, whitespace and opacity.
    // For enter: size expands first, then opacity fades in after staggerDelay
    const enterAtoms = [
        // Apply global delay to size atom - size expansion starts first
        (0, _collapseatoms.sizeEnterAtom)({
            orientation,
            duration: sizeDuration,
            easing,
            element,
            outSize,
            delay
        }),
        (0, _collapseatoms.whitespaceAtom)({
            direction: 'enter',
            orientation,
            duration: sizeDuration,
            easing,
            delay
        })
    ];
    // Fade in only if animateOpacity is true. Otherwise, leave opacity unaffected.
    if (animateOpacity) {
        enterAtoms.push((0, _fadeatom.fadeAtom)({
            direction: 'enter',
            duration: opacityDuration,
            easing,
            delay: delay + staggerDelay
        }));
    }
    // ----- EXIT -----
    // The exit transition is an array of up to 3 motion atoms: opacity, size and whitespace.
    // For exit: opacity fades out first, then size collapses after exitStaggerDelay
    const exitAtoms = [];
    // Fade out only if animateOpacity is true. Otherwise, leave opacity unaffected.
    if (animateOpacity) {
        exitAtoms.push((0, _fadeatom.fadeAtom)({
            direction: 'exit',
            duration: exitOpacityDuration,
            easing: exitEasing,
            delay: exitDelay
        }));
    }
    exitAtoms.push((0, _collapseatoms.sizeExitAtom)({
        orientation,
        duration: exitSizeDuration,
        easing: exitEasing,
        element,
        delay: exitDelay + exitStaggerDelay,
        outSize
    }), (0, _collapseatoms.whitespaceAtom)({
        direction: 'exit',
        orientation,
        duration: exitSizeDuration,
        easing: exitEasing,
        delay: exitDelay + exitStaggerDelay
    }));
    return {
        enter: enterAtoms,
        exit: exitAtoms
    };
};
const Collapse = (0, _reactmotion.createPresenceComponent)(collapsePresenceFn);
const CollapseSnappy = (0, _reactmotion.createPresenceComponentVariant)(Collapse, {
    duration: _reactmotion.motionTokens.durationFast
});
const CollapseRelaxed = (0, _reactmotion.createPresenceComponentVariant)(Collapse, {
    duration: _reactmotion.motionTokens.durationSlower
});
const CollapseDelayed = (0, _reactmotion.createPresenceComponentVariant)(Collapse, {
    // Enter timing per motion design spec
    sizeDuration: _reactmotion.motionTokens.durationNormal,
    opacityDuration: _reactmotion.motionTokens.durationSlower,
    staggerDelay: _reactmotion.motionTokens.durationNormal,
    // Exit timing per motion design spec
    exitSizeDuration: _reactmotion.motionTokens.durationNormal,
    exitOpacityDuration: _reactmotion.motionTokens.durationSlower,
    exitStaggerDelay: _reactmotion.motionTokens.durationSlower,
    // Easing per motion design spec
    easing: _reactmotion.motionTokens.curveEasyEase,
    exitEasing: _reactmotion.motionTokens.curveEasyEase
});
