  import { jsx as _jsx } from "@fluentui/react-jsx-runtime/jsx-runtime";
import { assertSlots } from '@fluentui/react-utilities';
import { PresenceGroup } from '@fluentui/react-motion';
import { MessageBarMotion } from './MessageBarGroup.motions';
import { MotionRefForwarder } from '../MotionRefForwarder';
/**
 * Render the final JSX of MessageBarGroup
 */ export const renderMessageBarGroup_unstable = (state)=>{
    assertSlots(state);
    return /*#__PURE__*/ _jsx(state.root, {
        children: /*#__PURE__*/ _jsx(PresenceGroup, {
            children: state.children.map((child)=>/*#__PURE__*/ _jsx(MessageBarMotion, {
                    animate: state.animate,
                    unmountOnExit: true,
                    children: /*#__PURE__*/ _jsx(MotionRefForwarder, {
                        children: child
                    })
                }, child.key))
        })
    });
};
