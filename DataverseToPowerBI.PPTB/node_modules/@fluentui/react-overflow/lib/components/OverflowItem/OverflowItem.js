'use client';
import * as React from 'react';
import { applyTriggerPropsToChildren, getReactElementRef, getTriggerChild, useMergedRefs } from '@fluentui/react-utilities';
import { useOverflowItem } from '../../useOverflowItem';
/**
 * Attaches overflow item behavior to its child registered with the OverflowContext.
 * It does not render an element of its own.
 *
 * Behaves similarly to other `*Trigger` components in Fluent UI React.
 */ export const OverflowItem = /*#__PURE__*/ React.forwardRef((props, ref)=>{
    const { id, groupId, priority, pinned, children } = props;
    const containerRef = useOverflowItem(id, priority, groupId, pinned);
    const child = getTriggerChild(children);
    return applyTriggerPropsToChildren(children, {
        ref: useMergedRefs(containerRef, ref, getReactElementRef(child))
    });
});
// type casting here is required to ensure internal type FluentTriggerComponent is not leaked
OverflowItem.isFluentTriggerComponent = true;
