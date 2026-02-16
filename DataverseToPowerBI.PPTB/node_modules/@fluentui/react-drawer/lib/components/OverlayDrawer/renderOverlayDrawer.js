  import { jsx as _jsx } from "@fluentui/react-jsx-runtime/jsx-runtime";
import { assertSlots } from '@fluentui/react-utilities';
import { DrawerProvider } from '../../contexts/drawerContext';
import { DialogBackdropProvider } from '@fluentui/react-dialog';
/**
 * Render the final JSX of OverlayDrawer
 */ export const renderOverlayDrawer_unstable = (state, contextValue)=>{
    assertSlots(state);
    return /*#__PURE__*/ _jsx(DrawerProvider, {
        value: contextValue,
        children: /*#__PURE__*/ _jsx(DialogBackdropProvider, {
            value: false,
            children: /*#__PURE__*/ _jsx(state.dialog, {
                children: /*#__PURE__*/ _jsx(state.root, {})
            })
        })
    });
};
