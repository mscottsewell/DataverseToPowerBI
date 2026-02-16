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
    getStaggerTotalDuration: function() {
        return getStaggerTotalDuration;
    },
    staggerItemsVisibilityAtTime: function() {
        return staggerItemsVisibilityAtTime;
    }
});
const _constants = require("./constants");
function getStaggerTotalDuration({ itemCount, itemDelay = _constants.DEFAULT_ITEM_DELAY, itemDuration = _constants.DEFAULT_ITEM_DURATION }) {
    if (itemCount <= 0) {
        return 0;
    }
    if (itemCount <= 1) {
        return Math.max(0, itemDuration);
    }
    const staggerDuration = itemDelay * (itemCount - 1);
    return Math.max(0, staggerDuration + itemDuration);
}
function staggerItemsVisibilityAtTime({ itemCount, elapsed, itemDelay = _constants.DEFAULT_ITEM_DELAY, itemDuration = _constants.DEFAULT_ITEM_DURATION, direction = 'enter', reversed = false }) {
    // If no items, return the empty state
    if (itemCount <= 0) {
        return {
            itemsVisibility: [],
            totalDuration: 0
        };
    }
    const totalDuration = getStaggerTotalDuration({
        itemCount,
        itemDelay,
        itemDuration
    });
    // Calculate progression through the stagger sequence
    let completedSteps;
    if (itemDelay <= 0) {
        // When itemDelay is 0 or negative, all steps complete immediately
        completedSteps = itemCount;
    } else {
        // Both enter and exit should start their first item immediately, but handle t=0 differently
        if (elapsed === 0) {
            // At exactly t=0, for enter we want first item visible, for exit we want all items visible
            completedSteps = direction === 'enter' ? 1 : 0;
        } else {
            // After t=0, both directions should progress at the same rate
            const stepsFromElapsedTime = Math.floor(elapsed / itemDelay) + 1;
            completedSteps = Math.min(itemCount, stepsFromElapsedTime);
        }
    }
    const itemsVisibility = Array.from({
        length: itemCount
    }, (_, idx)=>{
        // Calculate based on progression through the sequence (enter pattern)
        const fromStart = idx < completedSteps;
        const fromEnd = idx >= itemCount - completedSteps;
        let itemVisible = reversed ? fromEnd : fromStart;
        // For exit, invert the enter pattern
        if (direction === 'exit') {
            itemVisible = !itemVisible;
        }
        return itemVisible;
    });
    return {
        itemsVisibility,
        totalDuration
    };
}
