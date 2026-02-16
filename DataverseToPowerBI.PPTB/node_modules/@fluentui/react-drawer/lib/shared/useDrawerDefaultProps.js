export function useDrawerDefaultProps(props) {
    const { open = false, size = 'small', position = 'start', unmountOnClose = true } = props;
    return {
        size,
        position,
        open,
        unmountOnClose
    };
}
