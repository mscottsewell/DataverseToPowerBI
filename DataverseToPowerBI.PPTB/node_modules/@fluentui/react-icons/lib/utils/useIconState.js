import { useIconContext } from '../contexts';
import { mergeClasses } from '@griffel/react';
import { useStyles } from './useIconStyles.styles';
export const useIconState = (props, options) => {
    const { 
    // remove unwanted props to be set on the svg/html element
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    filled, title, primaryFill = 'currentColor', ...rest } = props;
    const state = {
        ...rest,
        fill: primaryFill,
    };
    const styles = useStyles();
    const iconContext = useIconContext();
    state.className = mergeClasses(styles.root, (options === null || options === void 0 ? void 0 : options.flipInRtl) && (iconContext === null || iconContext === void 0 ? void 0 : iconContext.textDirection) === 'rtl' && styles.rtl, state.className);
    if (title) {
        state['aria-label'] = title;
    }
    if (!state['aria-label'] && !state['aria-labelledby']) {
        state['aria-hidden'] = true;
    }
    else {
        state['role'] = 'img';
    }
    return state;
};
