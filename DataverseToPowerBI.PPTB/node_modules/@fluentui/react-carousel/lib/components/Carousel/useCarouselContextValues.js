'use client';
import * as React from 'react';
export function useCarouselContextValues_unstable(state) {
    const { activeIndex, appearance, selectPageByElement, selectPageByDirection, selectPageByIndex, subscribeForValues, enableAutoplay, resetAutoplay, circular, containerRef, viewportRef } = state;
    const carousel = React.useMemo(()=>({
            activeIndex,
            appearance,
            selectPageByElement,
            selectPageByDirection,
            selectPageByIndex,
            subscribeForValues,
            enableAutoplay,
            resetAutoplay,
            circular,
            containerRef,
            viewportRef
        }), [
        activeIndex,
        appearance,
        selectPageByElement,
        selectPageByDirection,
        selectPageByIndex,
        subscribeForValues,
        enableAutoplay,
        resetAutoplay,
        circular,
        containerRef,
        viewportRef
    ]);
    return {
        carousel
    };
}
