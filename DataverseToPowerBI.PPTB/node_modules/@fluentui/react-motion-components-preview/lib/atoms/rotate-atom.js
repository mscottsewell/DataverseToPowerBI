import { motionTokens } from '@fluentui/react-motion';
const createRotateValue = (axis, angle)=>{
    return `${axis.toLowerCase()} ${angle}deg`;
};
/**
 * Generates a motion atom object for a rotation around a single axis.
 * @param direction - The functional direction of the motion: 'enter' or 'exit'.
 * @param duration - The duration of the motion in milliseconds.
 * @param easing - The easing curve for the motion. Defaults to `motionTokens.curveLinear`.
 * @param axis - The axis of rotation: 'x', 'y', or 'z'. Defaults to 'y'.
 * @param outAngle - Rotation angle for the out state (exited) in degrees. Defaults to -90.
 * @param inAngle - Rotation angle for the in state (entered) in degrees. Defaults to 0.
 * @param delay - Time (ms) to delay the animation. Defaults to 0.
 * @returns A motion atom object with rotate keyframes and the supplied duration and easing.
 */ export const rotateAtom = ({ direction, duration, easing = motionTokens.curveLinear, delay = 0, axis = 'z', outAngle = -90, inAngle = 0 })=>{
    const keyframes = [
        {
            rotate: createRotateValue(axis, outAngle)
        },
        {
            rotate: createRotateValue(axis, inAngle)
        }
    ];
    if (direction === 'exit') {
        keyframes.reverse();
    }
    return {
        keyframes,
        duration,
        easing,
        delay
    };
};
