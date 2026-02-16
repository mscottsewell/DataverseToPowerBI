'use client';
import { makeStyles, mergeClasses } from '@griffel/react';
import { tokens } from '@fluentui/react-theme';
export const carouselClassNames = {
    root: 'fui-Carousel'
};
/**
 * Styles for the root slot
 */ const useStyles = makeStyles({
    root: {
        // Only hide horizontal overflow to enable focus border to bleed bounds vertically
        overflowX: 'hidden',
        overflowAnchor: 'none',
        position: 'relative'
    },
    elevated: {
        // Reserve vertical space so the drop shadow has room to render
        // without being clipped by surrounding layout or overflow rules.
        margin: tokens.spacingVerticalL,
        padding: tokens.spacingVerticalL
    }
});
/**
 * Apply styling to the Carousel slots based on the state
 */ export const useCarouselStyles_unstable = (state)=>{
    'use no memo';
    const styles = useStyles();
    const { appearance } = state;
    state.root.className = mergeClasses(carouselClassNames.root, styles.root, appearance === 'elevated' && styles.elevated, state.root.className);
    return state;
};
