import EpiFormElement from '@/types/EpiFormElement';

export default interface SubmitButtonElementProps extends EpiFormElement {
    onClick?: (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => void;
    onKeyDown?: (event: React.KeyboardEvent<HTMLButtonElement>) => void;
}
