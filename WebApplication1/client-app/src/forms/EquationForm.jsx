import CustomForm from "./CustomForm";
import MathInput from "../components/MathInput";
import { useState } from "react";
import Fetch from "../api/FetchAPI";
import MathExpr from "../components/MathExpr";
import {FormSelect} from "react-bootstrap";

const displayToASCII = {
    '=' : '=',
    '<' : '<',
    '≤' : '<=',
    '>' : '>',
    '≥' : '>='
}

export default function EquationForm({ setResults }) {
    async function onSubmit() {

        if (lhs === "" || rhs === "" || variable === "")
            return
        
        const signAscii = displayToASCII[sign];
        let fetch_result;
        if (signAscii === '=')
            fetch_result = await Fetch("equation", {lhs, rhs, var: variable})
        else
            fetch_result = await Fetch("inequality", {lhs, rhs, var: variable, sign:signAscii})         
        const results = fetch_result.result
        
        if (results === null)
            setResults([ ])
        else
            setResults([
                {
                    domain: "Equation",
                    title: <>Solutions of <MathExpr latex={lhs + ` ${sign} ` + rhs} /></>,
                    content: <MathExpr latex={results.solutions} />,
                    fetch: fetch_result
                },
            ])
    }

    const [lhs, setLhs] = useState("")
    const [rhs, setRhs] = useState("")
    const [variable, setVariable] = useState("x")
    const [sign, setSign] = useState("=")

    return (
        <CustomForm title="Equation Solver" onSubmit={onSubmit}>
            <label>Enter the equation to solve</label>
            <div id="equation" className="mb-3 d-flex gap-1 align-items-center">
                <MathInput className="flex-grow-1" setLatex={setLhs} />
                <FormSelect 
                    style={{
                        width:75, 
                        fontFamily: "Symbola, \"Times New Roman\", serif"
                    }} 
                    defaultValue={sign} 
                    onChange={ev => setSign(ev.target.value)}
                >
                    <option>{'='}</option>
                    <option>{'<'}</option>
                    <option>{'≤'}</option>
                    <option>{'>'}</option>
                    <option>{'≥'}</option>
                </FormSelect>
                <MathInput className="flex-grow-1" setLatex={setRhs}></MathInput>
            </div>
            <label>Enter the variable</label>
            <MathInput className="mb-3" setLatex={setVariable} latex={variable} />
        </CustomForm>
    )
}