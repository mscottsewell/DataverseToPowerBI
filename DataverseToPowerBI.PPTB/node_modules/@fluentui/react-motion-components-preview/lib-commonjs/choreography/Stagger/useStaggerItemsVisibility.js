'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useStaggerItemsVisibility", {
    enumerable: true,
    get: function() {
        return useStaggerItemsVisibility;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reactutilities = require("@fluentui/react-utilities");
const _utils = require("./utils");
function useStaggerItemsVisibility({ childMapping, itemDelay, itemDuration = _utils.DEFAULT_ITEM_DURATION, direction, reversed = false, onMotionFinish, hideMode = 'visibleProp' }) {
    const [requestAnimationFrame, cancelAnimationFrame] = (0, _reactutilities.useAnimationFrame)();
    // Stabilize the callback reference to avoid re-triggering effects on every render
    const handleMotionFinish = (0, _reactutilities.useEventCallback)(onMotionFinish !== null && onMotionFinish !== void 0 ? onMotionFinish : ()=>{
        return;
    });
    // Track animation state independently of child changes
    const [animationKey, setAnimationKey] = _react.useState(0);
    const prevDirection = _react.useRef(direction);
    // Only trigger new animation when direction actually changes, not when children change
    _react.useEffect(()=>{
        if (prevDirection.current !== direction) {
            setAnimationKey((prev)=>prev + 1);
            prevDirection.current = direction;
        }
    }, [
        direction
    ]);
    // State: visibility mapping for all items by key
    const [itemsVisibility, setItemsVisibility] = _react.useState(()=>{
        const initial = {};
        // All hide modes start in final state: visible for 'enter', hidden for 'exit'
        const initialState = direction === 'enter';
        Object.keys(childMapping).forEach((key)=>{
            initial[key] = initialState;
        });
        return initial;
    });
    // Update visibility mapping when childMapping changes
    _react.useEffect(()=>{
        setItemsVisibility((prev)=>{
            const next = {};
            const targetState = direction === 'enter';
            // Add or update items from new mapping
            Object.keys(childMapping).forEach((key)=>{
                if (key in prev) {
                    // Existing item - preserve its visibility state
                    next[key] = prev[key];
                } else {
                    // New item - set to target state
                    next[key] = targetState;
                }
            });
            // Note: Items that were in prev but not in childMapping are automatically removed
            // because we only iterate over keys in childMapping
            return next;
        });
    }, [
        childMapping,
        direction
    ]);
    // Refs: animation timing and control
    const startTimeRef = _react.useRef(null);
    const frameRef = _react.useRef(null);
    const finishedRef = _react.useRef(false);
    const isFirstRender = _react.useRef(true);
    // Use ref to avoid re-running the animation when child mapping changes
    const childMappingRef = _react.useRef(childMapping);
    // Update childMapping ref whenever it changes
    _react.useEffect(()=>{
        childMappingRef.current = childMapping;
    }, [
        childMapping
    ]);
    // Use ref for reversed to avoid re-running animation when it changes
    const reversedRef = _react.useRef(reversed);
    // Update reversed ref whenever it changes
    _react.useEffect(()=>{
        reversedRef.current = reversed;
    }, [
        reversed
    ]);
    // ====== ANIMATION EFFECT ======
    _react.useEffect(()=>{
        let cancelled = false;
        startTimeRef.current = null;
        finishedRef.current = false;
        // All hide modes skip animation on first render - items are already in their final state
        if (isFirstRender.current) {
            isFirstRender.current = false;
            // Items are already in their final state from useState, no animation needed
            handleMotionFinish();
            return; // No cleanup needed for first render
        }
        // For animations after first render, start from the opposite of the final state
        // - Enter animation: start hidden (false), animate to visible (true)
        // - Exit animation: start visible (true), animate to hidden (false)
        const startState = direction === 'exit';
        // Use childMappingRef.current to avoid adding childMapping to dependencies
        const initialVisibility = {};
        Object.keys(childMappingRef.current).forEach((key)=>{
            initialVisibility[key] = startState;
        });
        setItemsVisibility(initialVisibility);
        // Animation loop: update visibility on each frame until complete
        const tick = (now)=>{
            if (cancelled) {
                return;
            }
            if (startTimeRef.current === null) {
                startTimeRef.current = now;
            }
            const elapsed = now - startTimeRef.current;
            const childKeys = Object.keys(childMappingRef.current);
            const itemCount = childKeys.length;
            const result = (0, _utils.staggerItemsVisibilityAtTime)({
                itemCount,
                elapsed,
                itemDelay,
                itemDuration,
                direction,
                reversed: reversedRef.current
            });
            // Convert boolean array to keyed object
            const nextVisibility = {};
            childKeys.forEach((key, idx)=>{
                nextVisibility[key] = result.itemsVisibility[idx];
            });
            setItemsVisibility(nextVisibility);
            if (elapsed < result.totalDuration) {
                frameRef.current = requestAnimationFrame(tick);
            } else if (!finishedRef.current) {
                finishedRef.current = true;
                handleMotionFinish();
            }
        };
        frameRef.current = requestAnimationFrame(tick);
        return ()=>{
            cancelled = true;
            if (frameRef.current) {
                cancelAnimationFrame();
            }
        };
    }, [
        animationKey,
        itemDelay,
        itemDuration,
        direction,
        requestAnimationFrame,
        cancelAnimationFrame,
        handleMotionFinish
    ]);
    return {
        itemsVisibility
    };
}
