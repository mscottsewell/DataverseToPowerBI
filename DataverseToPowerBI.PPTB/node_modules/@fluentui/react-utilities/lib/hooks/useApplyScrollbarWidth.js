'use client';
import * as React from 'react';
import { measureScrollbarWidth } from '../utils/measureScrollBarWidth';
const cache = new WeakMap();
/**
 * A React hook that provides a ref for applying the browser's scrollbar width as a CSS property.
 *
 * This hook is SSR-safe and caches measurements per document to avoid redundant calculations.
 * When the ref is attached to an element, the hook automatically applies the measured scrollbar
 * width to the specified CSS property (defaults to 'width').
 *
 * @example
 * ```tsx
 * const scrollbarRef = useApplyScrollbarWidth({ targetDocument: document });
 * return <div ref={scrollbarRef} />;
 * ```
 */ export function useApplyScrollbarWidth(options = {}) {
    const { force, property = 'width' } = options;
    const applyScrollbarWidth = React.useCallback((element)=>{
        if (!element) {
            return;
        }
        // If we have a cached value, use it
        if (!force && cache.has(element.ownerDocument)) {
            const cachedWidth = cache.get(element.ownerDocument);
            if (cachedWidth !== undefined) {
                element.style.setProperty(property, `${cachedWidth}px`);
                return;
            }
        }
        // Measure the scrollbar width and apply it to the element
        const scrollbarWidth = measureScrollbarWidth(element.ownerDocument);
        cache.set(element.ownerDocument, scrollbarWidth);
        element.style.setProperty(property, `${scrollbarWidth}px`);
    }, [
        force,
        property
    ]);
    return applyScrollbarWidth;
}
