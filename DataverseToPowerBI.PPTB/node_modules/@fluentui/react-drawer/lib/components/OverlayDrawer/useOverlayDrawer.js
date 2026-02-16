'use client';
import * as React from 'react';
import { Dialog } from '@fluentui/react-dialog';
import { slot } from '@fluentui/react-utilities';
import { useFluent_unstable as useFluent } from '@fluentui/react-shared-contexts';
import { toMountNodeProps } from '@fluentui/react-portal';
import { OverlayDrawerMotion, OverlaySurfaceBackdropMotion } from '../../shared/drawerMotions';
import { useDrawerDefaultProps } from '../../shared/useDrawerDefaultProps';
import { OverlayDrawerSurface } from './OverlayDrawerSurface';
import { mergePresenceSlots } from '../../shared/drawerMotionUtils';
const STATIC_MOTION = {
    active: true,
    canRender: true,
    ref: /*#__PURE__*/ React.createRef(),
    type: 'idle'
};
/**
 * Create the state required to render OverlayDrawer.
 *
 * The returned state can be modified with hooks such as useOverlayDrawerStyles_unstable,
 * before being passed to renderOverlayDrawer_unstable.
 *
 * @param props - props from this instance of OverlayDrawer
 * @param ref - reference to root HTMLElement of OverlayDrawer
 */ export const useOverlayDrawer_unstable = (props, ref)=>{
    const { open, size, position, unmountOnClose } = useDrawerDefaultProps(props);
    const { modalType = 'modal', inertTrapFocus, onOpenChange, backdropMotion, surfaceMotion, mountNode } = props;
    const { dir, targetDocument } = useFluent();
    const { element: mountNodeElement } = toMountNodeProps(mountNode);
    const hasMountNodeElement = Boolean(mountNodeElement && (targetDocument === null || targetDocument === void 0 ? void 0 : targetDocument.body) !== mountNodeElement);
    const backdropProps = slot.resolveShorthand(props.backdrop);
    const hasCustomBackdrop = modalType !== 'non-modal' && backdropProps !== null;
    const root = slot.always({
        ...props,
        ref,
        unmountOnClose,
        backdrop: hasCustomBackdrop ? {
            ...backdropProps
        } : null,
        backdropMotion: mergePresenceSlots(backdropMotion, OverlaySurfaceBackdropMotion, {
            size
        })
    }, {
        elementType: OverlayDrawerSurface
    });
    const dialog = slot.always({
        open,
        onOpenChange,
        inertTrapFocus,
        modalType,
        unmountOnClose,
        surfaceMotion: mergePresenceSlots(surfaceMotion, OverlayDrawerMotion, {
            position,
            size,
            dir
        }),
        /**
       * children is not needed here because we construct the children in the render function,
       * but it's required by DialogProps
       */ children: null
    }, {
        elementType: Dialog
    });
    return {
        components: {
            root: OverlayDrawerSurface,
            dialog: Dialog
        },
        root,
        dialog,
        open,
        size,
        position,
        hasMountNodeElement,
        unmountOnClose,
        // Deprecated props
        mountNode,
        motion: STATIC_MOTION
    };
};
