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
    Slide: function() {
        return Slide;
    },
    SlideRelaxed: function() {
        return SlideRelaxed;
    },
    SlideSnappy: function() {
        return SlideSnappy;
    }
});
const _reactmotion = require("@fluentui/react-motion");
const _fadeatom = require("../../atoms/fade-atom");
const _slideatom = require("../../atoms/slide-atom");
/**
 * Define a presence motion for slide in/out
 *
 * @param duration - Time (ms) for the enter transition (slide-in). Defaults to the `durationNormal` value (200 ms).
 * @param easing - Easing curve for the enter transition (slide-in). Defaults to the `curveDecelerateMid` value.
 * @param delay - Time (ms) to delay the enter transition. Defaults to 0.
 * @param exitDuration - Time (ms) for the exit transition (slide-out). Defaults to the `duration` param for symmetry.
 * @param exitEasing - Easing curve for the exit transition (slide-out). Defaults to the `curveAccelerateMid` value.
 * @param exitDelay - Time (ms) to delay the exit transition. Defaults to the `delay` param for symmetry.
 * @param outX - X translate for the out state (exited). Defaults to `'0px'`.
 * @param outY - Y translate for the out state (exited). Defaults to `'0px'`.
 * @param inX - X translate for the in state (entered). Defaults to `'0px'`.
 * @param inY - Y translate for the in state (entered). Defaults to `'0px'`.
 * @param animateOpacity - Whether to animate the opacity. Defaults to `true`.
 */ const slidePresenceFn = ({ duration = _reactmotion.motionTokens.durationNormal, easing = _reactmotion.motionTokens.curveDecelerateMid, delay = 0, exitDuration = duration, exitEasing = _reactmotion.motionTokens.curveAccelerateMid, exitDelay = delay, outX = '0px', outY = '0px', inX = '0px', inY = '0px', animateOpacity = true })=>{
    const enterAtoms = [
        (0, _slideatom.slideAtom)({
            direction: 'enter',
            duration,
            easing,
            delay,
            outX,
            outY,
            inX,
            inY
        })
    ];
    const exitAtoms = [
        (0, _slideatom.slideAtom)({
            direction: 'exit',
            duration: exitDuration,
            easing: exitEasing,
            delay: exitDelay,
            outX,
            outY,
            inX,
            inY
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
const Slide = (0, _reactmotion.createPresenceComponent)(slidePresenceFn);
const SlideSnappy = (0, _reactmotion.createPresenceComponentVariant)(Slide, {
    easing: _reactmotion.motionTokens.curveDecelerateMax,
    exitEasing: _reactmotion.motionTokens.curveAccelerateMax
});
const SlideRelaxed = (0, _reactmotion.createPresenceComponentVariant)(Slide, {
    duration: _reactmotion.motionTokens.durationGentle
});
