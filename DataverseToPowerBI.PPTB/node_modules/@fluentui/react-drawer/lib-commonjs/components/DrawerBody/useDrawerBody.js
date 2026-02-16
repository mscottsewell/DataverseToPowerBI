'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useDrawerBody_unstable", {
    enumerable: true,
    get: function() {
        return useDrawerBody_unstable;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reactutilities = require("@fluentui/react-utilities");
const _drawerContext = require("../../contexts/drawerContext");
const _reactsharedcontexts = require("@fluentui/react-shared-contexts");
/**
 * @internal
 *
 * Get the current scroll state of the DrawerBody.
 *
 * @param element - HTMLElement to check scroll state of
 */ const getScrollState = ({ scrollTop, scrollHeight, clientHeight })=>{
    if (scrollHeight <= clientHeight) {
        return 'none';
    }
    if (scrollTop === 0) {
        return 'top';
    }
    if (scrollTop + clientHeight === scrollHeight) {
        return 'bottom';
    }
    return 'middle';
};
const useDrawerBody_unstable = (props, ref)=>{
    const { targetDocument } = (0, _reactsharedcontexts.useFluent_unstable)();
    const win = targetDocument === null || targetDocument === void 0 ? void 0 : targetDocument.defaultView;
    const { setScrollState } = (0, _drawerContext.useDrawerContext_unstable)();
    const scrollRef = _react.useRef(null);
    const mergedRef = (0, _reactutilities.useMergedRefs)(ref, scrollRef);
    const [setScrollAnimationFrame, cancelScrollAnimationFrame] = (0, _reactutilities.useAnimationFrame)();
    const [setResizeAnimationFrame, cancelResizeAnimationFrame] = (0, _reactutilities.useAnimationFrame)();
    const updateScrollState = _react.useCallback(()=>{
        if (!scrollRef.current) {
            return;
        }
        setScrollState(getScrollState(scrollRef.current));
    }, [
        setScrollState
    ]);
    const onScroll = _react.useCallback(()=>{
        cancelScrollAnimationFrame();
        setScrollAnimationFrame(updateScrollState);
    }, [
        cancelScrollAnimationFrame,
        setScrollAnimationFrame,
        updateScrollState
    ]);
    // Update scroll state on children change
    (0, _reactutilities.useIsomorphicLayoutEffect)(updateScrollState, [
        props.children,
        updateScrollState
    ]);
    // Update scroll state on mount and when resize occurs
    (0, _reactutilities.useIsomorphicLayoutEffect)(()=>{
        if (!scrollRef.current || !(win === null || win === void 0 ? void 0 : win.ResizeObserver)) {
            return;
        }
        const observer = new win.ResizeObserver(()=>setResizeAnimationFrame(updateScrollState));
        observer.observe(scrollRef.current);
        return ()=>{
            observer.disconnect();
            cancelResizeAnimationFrame();
        };
    }, [
        setResizeAnimationFrame,
        cancelResizeAnimationFrame,
        updateScrollState,
        win
    ]);
    return {
        components: {
            root: 'div'
        },
        root: _reactutilities.slot.always((0, _reactutilities.getIntrinsicElementProps)('div', {
            ref: mergedRef,
            ...props,
            onScroll: (0, _reactutilities.mergeCallbacks)(props.onScroll, onScroll)
        }), {
            elementType: 'div'
        })
    };
};
