"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "Blur", {
    enumerable: true,
    get: function() {
        return Blur;
    }
});
const _reactmotion = require("@fluentui/react-motion");
const _fadeatom = require("../../atoms/fade-atom");
const _bluratom = require("../../atoms/blur-atom");
/**
 * Define a presence motion for blur in/out
 *
 * @param duration - Time (ms) for the enter transition (blur-in). Defaults to the `durationSlow` value (300 ms).
 * @param easing - Easing curve for the enter transition (blur-in). Defaults to the `curveDecelerateMin` value.
 * @param delay - Time (ms) to delay the enter transition. Defaults to 0.
 * @param exitDuration - Time (ms) for the exit transition (blur-out). Defaults to the `duration` param for symmetry.
 * @param exitEasing - Easing curve for the exit transition (blur-out). Defaults to the `curveAccelerateMin` value.
 * @param exitDelay - Time (ms) to delay the exit transition. Defaults to the `delay` param for symmetry.
 * @param outRadius - Blur radius for the out state (exited). Defaults to `'10px'`.
 * @param inRadius - Blur radius for the in state (entered). Defaults to `'0px'`.
 * @param animateOpacity - Whether to animate the opacity. Defaults to `true`.
 */ const blurPresenceFn = ({ duration = _reactmotion.motionTokens.durationSlow, easing = _reactmotion.motionTokens.curveDecelerateMin, delay = 0, exitDuration = duration, exitEasing = _reactmotion.motionTokens.curveAccelerateMin, exitDelay = delay, outRadius = '10px', inRadius = '0px', animateOpacity = true })=>{
    const enterAtoms = [
        (0, _bluratom.blurAtom)({
            direction: 'enter',
            duration,
            easing,
            delay,
            outRadius,
            inRadius
        })
    ];
    const exitAtoms = [
        (0, _bluratom.blurAtom)({
            direction: 'exit',
            duration: exitDuration,
            easing: exitEasing,
            delay: exitDelay,
            outRadius,
            inRadius
        })
    ];
    // Only add fade atoms if animateOpacity is true.
    if (animateOpacity) {
        enterAtoms.push((0, _fadeatom.fadeAtom)({
            direction: 'enter',
            duration,
            easing,
            delay
        }));
        exitAtoms.push((0, _fadeatom.fadeAtom)({
            direction: 'exit',
            duration: exitDuration,
            easing: exitEasing,
            delay: exitDelay
        }));
    }
    return {
        enter: enterAtoms,
        exit: exitAtoms
    };
};
const Blur = (0, _reactmotion.createPresenceComponent)(blurPresenceFn);
