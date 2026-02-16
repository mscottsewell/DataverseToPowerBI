/* eslint-disable @typescript-eslint/explicit-module-boundary-types */ "use strict";
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
    getCollapseDurationInfo: function() {
        return getCollapseDurationInfo;
    },
    getCollapseMotionValidation: function() {
        return getCollapseMotionValidation;
    },
    getCollapseOrientationInfo: function() {
        return getCollapseOrientationInfo;
    },
    getCollapseTimingInfo: function() {
        return getCollapseTimingInfo;
    },
    getOpacityComparisonInfo: function() {
        return getOpacityComparisonInfo;
    },
    getSizeAtomInfo: function() {
        return getSizeAtomInfo;
    },
    getWhitespaceAtomInfo: function() {
        return getWhitespaceAtomInfo;
    }
});
function getCollapseMotionValidation(motion) {
    return {
        enterCount: motion.enter.length,
        exitCount: motion.exit.length,
        hasEnterOpacity: motion.enter.length === 3 && 'opacity' in (motion.enter[2].keyframes[0] || {}),
        hasExitOpacity: motion.exit.length === 3 && 'opacity' in (motion.exit[0].keyframes[0] || {}),
        enterStructure: motion.enter.map((atom)=>({
                hasKeyframes: Array.isArray(atom.keyframes),
                hasDuration: typeof atom.duration === 'number',
                hasEasing: typeof atom.easing === 'string',
                hasDelay: typeof atom.delay === 'number'
            })),
        exitStructure: motion.exit.map((atom)=>({
                hasKeyframes: Array.isArray(atom.keyframes),
                hasDuration: typeof atom.duration === 'number',
                hasEasing: typeof atom.easing === 'string',
                hasDelay: typeof atom.delay === 'number'
            }))
    };
}
function getCollapseTimingInfo(motion, animateOpacity = true) {
    var _motion_enter_, _motion_enter_1, _motion_enter_2, _motion_exit_, _motion_exit_1, _motion_exit_2, _motion_exit_3, _motion_exit_4;
    var _motion_enter__delay, _motion_enter__delay1, _motion_enter__delay2;
    const enterDelays = {
        size: (_motion_enter__delay = (_motion_enter_ = motion.enter[0]) === null || _motion_enter_ === void 0 ? void 0 : _motion_enter_.delay) !== null && _motion_enter__delay !== void 0 ? _motion_enter__delay : 0,
        whitespace: (_motion_enter__delay1 = (_motion_enter_1 = motion.enter[1]) === null || _motion_enter_1 === void 0 ? void 0 : _motion_enter_1.delay) !== null && _motion_enter__delay1 !== void 0 ? _motion_enter__delay1 : 0,
        opacity: animateOpacity ? (_motion_enter__delay2 = (_motion_enter_2 = motion.enter[2]) === null || _motion_enter_2 === void 0 ? void 0 : _motion_enter_2.delay) !== null && _motion_enter__delay2 !== void 0 ? _motion_enter__delay2 : 0 : undefined
    };
    var _motion_exit__delay, _motion_exit__delay1, _motion_exit__delay2, _motion_exit__delay3, _motion_exit__delay4;
    const exitDelays = animateOpacity ? {
        opacity: (_motion_exit__delay = (_motion_exit_ = motion.exit[0]) === null || _motion_exit_ === void 0 ? void 0 : _motion_exit_.delay) !== null && _motion_exit__delay !== void 0 ? _motion_exit__delay : 0,
        size: (_motion_exit__delay1 = (_motion_exit_1 = motion.exit[1]) === null || _motion_exit_1 === void 0 ? void 0 : _motion_exit_1.delay) !== null && _motion_exit__delay1 !== void 0 ? _motion_exit__delay1 : 0,
        whitespace: (_motion_exit__delay2 = (_motion_exit_2 = motion.exit[2]) === null || _motion_exit_2 === void 0 ? void 0 : _motion_exit_2.delay) !== null && _motion_exit__delay2 !== void 0 ? _motion_exit__delay2 : 0
    } : {
        size: (_motion_exit__delay3 = (_motion_exit_3 = motion.exit[0]) === null || _motion_exit_3 === void 0 ? void 0 : _motion_exit_3.delay) !== null && _motion_exit__delay3 !== void 0 ? _motion_exit__delay3 : 0,
        whitespace: (_motion_exit__delay4 = (_motion_exit_4 = motion.exit[1]) === null || _motion_exit_4 === void 0 ? void 0 : _motion_exit_4.delay) !== null && _motion_exit__delay4 !== void 0 ? _motion_exit__delay4 : 0
    };
    return {
        enter: enterDelays,
        exit: exitDelays
    };
}
function getCollapseDurationInfo(motion, animateOpacity = true) {
    var _motion_enter_, _motion_enter_1, _motion_enter_2, _motion_exit_, _motion_exit_1, _motion_exit_2, _motion_exit_3, _motion_exit_4;
    var _motion_enter__duration, _motion_enter__duration1, _motion_enter__duration2;
    const enterDurations = {
        size: (_motion_enter__duration = (_motion_enter_ = motion.enter[0]) === null || _motion_enter_ === void 0 ? void 0 : _motion_enter_.duration) !== null && _motion_enter__duration !== void 0 ? _motion_enter__duration : 0,
        whitespace: (_motion_enter__duration1 = (_motion_enter_1 = motion.enter[1]) === null || _motion_enter_1 === void 0 ? void 0 : _motion_enter_1.duration) !== null && _motion_enter__duration1 !== void 0 ? _motion_enter__duration1 : 0,
        opacity: animateOpacity ? (_motion_enter__duration2 = (_motion_enter_2 = motion.enter[2]) === null || _motion_enter_2 === void 0 ? void 0 : _motion_enter_2.duration) !== null && _motion_enter__duration2 !== void 0 ? _motion_enter__duration2 : 0 : undefined
    };
    var _motion_exit__duration, _motion_exit__duration1, _motion_exit__duration2, _motion_exit__duration3, _motion_exit__duration4;
    const exitDurations = animateOpacity ? {
        opacity: (_motion_exit__duration = (_motion_exit_ = motion.exit[0]) === null || _motion_exit_ === void 0 ? void 0 : _motion_exit_.duration) !== null && _motion_exit__duration !== void 0 ? _motion_exit__duration : 0,
        size: (_motion_exit__duration1 = (_motion_exit_1 = motion.exit[1]) === null || _motion_exit_1 === void 0 ? void 0 : _motion_exit_1.duration) !== null && _motion_exit__duration1 !== void 0 ? _motion_exit__duration1 : 0,
        whitespace: (_motion_exit__duration2 = (_motion_exit_2 = motion.exit[2]) === null || _motion_exit_2 === void 0 ? void 0 : _motion_exit_2.duration) !== null && _motion_exit__duration2 !== void 0 ? _motion_exit__duration2 : 0
    } : {
        size: (_motion_exit__duration3 = (_motion_exit_3 = motion.exit[0]) === null || _motion_exit_3 === void 0 ? void 0 : _motion_exit_3.duration) !== null && _motion_exit__duration3 !== void 0 ? _motion_exit__duration3 : 0,
        whitespace: (_motion_exit__duration4 = (_motion_exit_4 = motion.exit[1]) === null || _motion_exit_4 === void 0 ? void 0 : _motion_exit_4.duration) !== null && _motion_exit__duration4 !== void 0 ? _motion_exit__duration4 : 0
    };
    return {
        enter: enterDurations,
        exit: exitDurations
    };
}
function getCollapseOrientationInfo(motion, animateOpacity = true) {
    const enterSizeAtom = motion.enter[0];
    const enterWhitespaceAtom = motion.enter[1];
    const exitOffset = animateOpacity ? 1 : 0;
    const exitSizeAtom = motion.exit[exitOffset];
    const exitWhitespaceAtom = motion.exit[exitOffset + 1];
    return {
        enter: {
            sizeProperties: Object.keys((enterSizeAtom === null || enterSizeAtom === void 0 ? void 0 : enterSizeAtom.keyframes[0]) || {}),
            whitespaceProperties: Object.keys((enterWhitespaceAtom === null || enterWhitespaceAtom === void 0 ? void 0 : enterWhitespaceAtom.keyframes[0]) || {})
        },
        exit: {
            sizeProperties: Object.keys((exitSizeAtom === null || exitSizeAtom === void 0 ? void 0 : exitSizeAtom.keyframes[0]) || {}),
            whitespaceProperties: Object.keys((exitWhitespaceAtom === null || exitWhitespaceAtom === void 0 ? void 0 : exitWhitespaceAtom.keyframes[0]) || {})
        }
    };
}
function getSizeAtomInfo(sizeAtom, direction) {
    const keyframes = sizeAtom.keyframes;
    const properties = Object.keys(keyframes[0] || {});
    return {
        keyframeCount: keyframes.length,
        properties,
        hasOffset: direction === 'enter' ? 'offset' in (keyframes[1] || {}) : false,
        hasFill: 'fill' in sizeAtom,
        fillValue: sizeAtom.fill,
        firstFrameValues: keyframes[0] || {},
        lastFrameValues: keyframes[keyframes.length - 1] || {}
    };
}
function getWhitespaceAtomInfo(whitespaceAtom, direction) {
    const keyframe = whitespaceAtom.keyframes[0] || {};
    return {
        properties: Object.keys(keyframe),
        offset: keyframe.offset,
        expectedOffset: direction === 'enter' ? 0 : 1,
        hasFill: 'fill' in whitespaceAtom,
        fillValue: whitespaceAtom.fill,
        isVertical: 'paddingBlockStart' in keyframe,
        isHorizontal: 'paddingInlineStart' in keyframe
    };
}
function getOpacityComparisonInfo(withOpacity, withoutOpacity) {
    return {
        withOpacity: {
            enterCount: withOpacity.enter.length,
            exitCount: withOpacity.exit.length,
            hasEnterOpacity: withOpacity.enter.length === 3,
            hasExitOpacity: withOpacity.exit.length === 3
        },
        withoutOpacity: {
            enterCount: withoutOpacity.enter.length,
            exitCount: withoutOpacity.exit.length,
            hasEnterOpacity: false,
            hasExitOpacity: false
        }
    };
}
