import {EditableMathField} from 'react-mathquill';

export default function MathInput({latex = "", className = "", setLatex, style = {}}) {
    return (
        <EditableMathField
            className={"form-control " + className}
            style={{border: "var(--bs-border-width) solid var(--bs-border-color)", ...style}}
            latex={latex}
            onChange={(mathField) => setLatex(mathField.latex())}
        />
    )
}