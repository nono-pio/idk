import { EditableMathField } from 'react-mathquill';

export default function MathInput({ latex="", className="", setLatex }) {
    return (
        <EditableMathField 
            className={"form-control " + className} 
            style={{border: "var(--bs-border-width) solid var(--bs-border-color)"}} 
            latex={latex}
            onChange={(mathField) => setLatex(mathField.latex())}
        />
    )
}