import { mergeClasses } from '@griffel/react';
export const messageBarGroupClassNames = {
    root: 'fui-MessageBarGroup'
};
/**
 * Apply styling to the MessageBarGroup slots based on the state
 */ export const useMessageBarGroupStyles_unstable = (state)=>{
    'use no memo';
    state.root.className = mergeClasses(messageBarGroupClassNames.root, state.root.className);
    return state;
};
